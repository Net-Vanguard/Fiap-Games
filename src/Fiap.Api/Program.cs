using Fiap.Infra.Utils;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

#if DEBUG
builder.Configuration.AddUserSecrets<Program>(optional: true, reloadOnChange: true);
#endif

builder.Configuration.AddSystemsManager(source =>
{
    source.Path = "/myapp/prod/";
    source.Optional = true;
    source.ReloadAfter = TimeSpan.FromMinutes(5);
});


builder.Services.AddOptions<ConnectionStrings>()
    .Bind(builder.Configuration.GetSection(nameof(ConnectionStrings)))
    .ValidateDataAnnotations()
    .ValidateOnStart();

var serviceProvider = builder.Services.BuildServiceProvider();
var databaseConnection = serviceProvider.GetRequiredService<IOptions<ConnectionStrings>>().Value;

builder.Services.AddCustomServices(builder.Configuration);

builder.Host.UseSerilog();

builder.Services.AddCustomMvc();

builder.Services
    .AddHealthChecks()
    .AddSqlServer(databaseConnection.FCGConnection, "games-sqlserver")
    .AddApplicationStatus()
    .AddRedis(databaseConnection.RedisConnectionString);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddGlobalCorsPolicy();

builder.Services.AddApiVersioningConfiguration();

builder.Services.AddSwaggerDocumentation();

var app = builder.Build();

app.UseCors("AllowAllOrigins");

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwaggerDocumentation();

app.UseRouting();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseMiddleware<UnauthorizedResponseMiddleware>();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.UseHttpMetrics();
app.MapMetrics();

app.Run();

[ExcludeFromCodeCoverage]
public partial class Program { }
