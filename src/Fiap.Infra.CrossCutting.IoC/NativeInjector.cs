using Fiap.Infra.HostedService;
using Rebus.Retry.Simple;
using Microsoft.Extensions.Logging;
using Fiap.Infra.Data.EventSourcing;

namespace Fiap.Infra.CrossCutting.IoC;

[ExcludeFromCodeCoverage]
public static class NativeInjector
{
    public static void AddCustomServices(this IServiceCollection services, IConfiguration configuration)
    {
        AddLocalHttpClients(services, configuration);
        AddLocalServices(services, configuration);
        AddRedisDatabase(services, configuration);
        AddElasticsearch(services, configuration);
        AddObservability(services, configuration, services.BuildServiceProvider().GetRequiredService<IWebHostEnvironment>());
        AddDatabase(services, configuration);
		AddMongoDb(services, configuration);
        AddServiceBus(services, configuration);
    }
    public static IServiceCollection AddServiceBus(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ServiceBus>()
          .Bind(configuration.GetSection(nameof(ServiceBus)))
          .ValidateDataAnnotations()
          .ValidateOnStart();    

        services.AddRebus(configure => configure
        .Transport(t => t.UseAmazonSQS(
            accessKeyId: configuration["ServiceBus:AccessKey"],
            secretAccessKey: configuration["ServiceBus:SecretKey"],
            regionEndpoint: Amazon.RegionEndpoint.GetBySystemName(configuration["ServiceBus:Region"]),
            inputQueueAddress: configuration["ServiceBus:FCGQueueName"]
        ))
        .Routing(r => r.TypeBased()
            .Map<GameCreatedHandler>(configuration["ServiceBus:FCGQueueName"])
            .Map<PromotionCreatedHandler>(configuration["ServiceBus:FCGQueueName"])
            .Map<PromotionUpdatedHandler>(configuration["ServiceBus:FCGQueueName"]))
        .Options(o =>
        {
            o.RetryStrategy(maxDeliveryAttempts: 3);
            o.SetNumberOfWorkers(1);
            o.SetMaxParallelism(1);
        }),
        isDefaultBus: true);

        return services;
    }

    public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ConnectionStrings>()
            .Bind(configuration.GetSection(nameof(ConnectionStrings)))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var connectionString = configuration.GetConnectionString("FCGConnection");
        
        services.AddDbContext<Context>(options =>
            options.UseSqlServer(connectionString));

		services.AddSingleton(sp =>
		{
			var conn = configuration.GetConnectionString("EventStore");
			if (string.IsNullOrWhiteSpace(conn))
				throw new InvalidOperationException("EventStore connection string is not configured.");
			var settings = EventStoreClientSettings.Create(conn);
			return new EventStoreClient(settings);
		});
	}

    public static void AddLocalHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        var urlCRM = configuration["ExternalServices:CRMUrl"];

        services
            .AddRefitClient<ICRMService>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(urlCRM));
    }

    public static void AddLocalServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<INotification, Notification>();

        services.AddScoped<IElasticSearchService, ElasticSearchService>();

        services.AddHostedService<OutboxProcessorService>();
        services.AddHostedService<DataSyncHostedService>(); 

        #region Repositories
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IPromotionRepository, PromotionRepository>();
        services.AddScoped<IGameRepository, GameRepository>();
		services.AddScoped<IEventStoreRepository, EventStoreRepository>();
		services.AddScoped<IGameMongoRepository, GameMongoRepository>();
		services.AddScoped<IPromotionMongoRepository, PromotionMongoRepository>();
		#endregion

		#region Services
		services.AddScoped<IGamesService, GamesService>();
        services.AddScoped<IPromotionsService, PromotionsService>();
        #endregion

        #region Bus Services
        services.AutoRegisterHandlersFromAssemblyOf<GameCreatedHandler>();
        services.AutoRegisterHandlersFromAssemblyOf<PromotionCreatedHandler>();
        #endregion
    }

    public static void AddRedisDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("Redis");
        
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.ConfigurationOptions = ConfigurationOptions.Parse(redisConnectionString);
            options.ConfigurationOptions.AbortOnConnectFail = false;
            options.ConfigurationOptions.ConnectTimeout = 10000;
            options.ConfigurationOptions.SyncTimeout = 10000;
            options.ConfigurationOptions.ConnectRetry = 5;
            options.ConfigurationOptions.ReconnectRetryPolicy = new ExponentialRetry(5000);
        });

        services.AddSingleton<IConnectionMultiplexer>(provider =>
        {
            return ConnectionMultiplexer.Connect(redisConnectionString);
        });

        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                LocalCacheExpiration = TimeSpan.FromMinutes(5),
                Expiration = TimeSpan.FromMinutes(60)
            };
        });
        
        services.AddSingleton<ICacheService, CacheService.Services.CacheService>();
    }

    public static IServiceCollection AddObservability(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        ConfigureSerilog(services, configuration, environment);
        ConfigureOpenTelemetry(services, configuration);
        return services;
    }

    public static void AddElasticsearch(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<ElasticsearchClient>>();
            
            var elasticsearchUri = configuration.GetConnectionString("Elasticsearch");
            
            var uri = new Uri(elasticsearchUri);

            var settings = new ElasticsearchClientSettings(uri)
                .DefaultIndex("games")
                .DefaultFieldNameInferrer(p => p.ToLowerInvariant())
                .RequestTimeout(TimeSpan.FromSeconds(60))
                .ThrowExceptions(false)
                .DisableDirectStreaming()
                .PrettyJson()
                .ServerCertificateValidationCallback(CertificateValidations.AllowAll);

            var client = new ElasticsearchClient(settings);              
             
            return client;
        });

        services.AddScoped<IElasticSearchService, ElasticSearchService>();
    }

    private static void ConfigureSerilog(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddOptions<Utils.Serilog>()
            .Bind(configuration.GetSection(nameof(Utils.Serilog)))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var lokiUri = configuration["Serilog:WriteTo:1:Args:Uri"];

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithProperty("Application", "Fiap")
            .WriteTo.Console()
            .WriteTo.GrafanaLoki(
                uri: lokiUri,
                labels:
                [
                    new() { Key = "app", Value = "dotnet-api" },
                    new() { Key = "env", Value = environment.EnvironmentName }
                ])
            .CreateLogger();
    }

    private static void ConfigureOpenTelemetry(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DistributedTracing>()
            .Bind(configuration.GetSection(nameof(DistributedTracing)))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var serviceName = configuration["DistributedTracing:Jaeger:ServiceName"];
        var endpoint = configuration["DistributedTracing:Jaeger:Endpoint"];
        var host = configuration["DistributedTracing:Jaeger:Host"];
        var port = int.Parse(configuration["DistributedTracing:Jaeger:Port"]);

        services.AddOpenTelemetry()
            .WithTracing(tracing => tracing
                .SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService(serviceName: serviceName, serviceVersion: "1.0.0"))
                .AddAspNetCoreInstrumentation()
                .AddEntityFrameworkCoreInstrumentation(options =>
                {
                    options.SetDbStatementForText = true;
                    options.SetDbStatementForStoredProcedure = true;
                })
                .AddJaegerExporter(options =>
                {
                    if (!string.IsNullOrEmpty(endpoint))
                    {
                        options.Endpoint = new Uri(endpoint);
                        options.Protocol = JaegerExportProtocol.HttpBinaryThrift;
                    }
                    else
                    {
                        options.AgentHost = host;
                        options.AgentPort = port;
                    }
                }));
    }

	public static void AddMongoDb(this IServiceCollection services, IConfiguration configuration)
	{
		var mongoSettings = configuration.GetSection("MongoDb").Get<MongoDbSettings>();

		services.AddSingleton<IMongoClient>(sp => new MongoClient(mongoSettings.ConnectionString));
		services.AddSingleton(sp =>
		{
			var client = sp.GetRequiredService<IMongoClient>();
			return client.GetDatabase(mongoSettings.Database);
		});
	}
}