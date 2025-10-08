using Fiap.Domain.GameAggregate;
using Fiap.Domain.PromotionAggregate;
using Fiap.Infra.CrossCutting.Common.Elastic.Models;

namespace Fiap.Infra.HostedService;

public class DataSyncHostedService(IServiceProvider serviceProvider, ILogger<DataSyncHostedService> logger) : BackgroundService
{
    private readonly TimeSpan _syncInterval = TimeSpan.FromMinutes(5);
    private bool _initialSyncCompleted = false;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        var maxInitialRetries = 5;
        var currentRetry = 0;

        while (!_initialSyncCompleted && currentRetry < maxInitialRetries && !stoppingToken.IsCancellationRequested)
        {
            currentRetry++;
            await PerformInitialSync(stoppingToken);

            if (_initialSyncCompleted)
                break;
            
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            await PerformPeriodicSync(stoppingToken);
            await Task.Delay(_syncInterval, stoppingToken);
        }
    }

    private async Task PerformInitialSync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested || _initialSyncCompleted) 
            return;

        using var scope = serviceProvider.CreateScope();

        var gameRepository = scope.ServiceProvider.GetRequiredService<IGameRepository>();
        var gameMongoRepository = scope.ServiceProvider.GetRequiredService<IGameMongoRepository>();
        var elasticSearchService = scope.ServiceProvider.GetService<IElasticSearchService>();

        await SyncPromotions(scope);

        var gamesFromPostgreSQL = await gameRepository.GetAllWithPromotionsAsync();

        if (gamesFromPostgreSQL?.Any() != true)
            return;

        var mongoSyncCount = 0;
        var elasticSyncCount = 0;

        foreach (var game in gamesFromPostgreSQL)
        {
            var cleanGame = new Game(game.Id, game.Name, game.Genre, game.Price.Value, game.PromotionId, game.Price.Currency)
            {
                Promotion = null
            };

            var existingGame = await gameMongoRepository.GetByIdWithPromotionAsync(game.Id);

            if (existingGame is null)
            {
                await gameMongoRepository.InsertAsync(cleanGame);
                mongoSyncCount++;
            }
            else
            {
                await gameMongoRepository.UpdateAsync(cleanGame.Id, cleanGame);
            }
        }

        if (elasticSearchService is not null)
        {
            var gameDocuments = gamesFromPostgreSQL.Select(game => new GameDocument
            {
                Id = game.Id,
                Name = game.Name,
                Genre = game.Genre,
                Price = game.Price?.Value ?? 0,
                PromotionId = game.PromotionId,
                FinalPrice = game.GetFinalPrice(),
                HasActivePromotion = game.HasActivePromotion(),
                DiscountPercentage = game.HasActivePromotion() ? game.GetDiscountPercentage() : null,
                IndexedAt = DateTime.UtcNow,
                PopularityScore = 0,
                Tags = [game.Genre.ToLowerInvariant()]
            }).ToList();

            var elasticSuccess = await elasticSearchService.IndexGamesAsync(gameDocuments);

            if (elasticSuccess)
            {
                elasticSyncCount = gameDocuments.Count;
                
                var verificationAttempts = new[] { 1000, 3000, 5000 };
                
                for (int attempt = 0; attempt < verificationAttempts.Length; attempt++)
                {
                    await Task.Delay(verificationAttempts[attempt], cancellationToken);
                    
                    var verifyGames = await elasticSearchService.GetAllGamesAsync();
                    var verifyCount = verifyGames?.Count ?? 0;
                    
                    if (verifyCount >= gameDocuments.Count)
                        break;
                }
            }
            else
            {
                return;
            }
        }
        else
        {
            return;
        }

        if (mongoSyncCount > 0 && elasticSyncCount > 0)
        {
            _initialSyncCompleted = true;
        }
    }

    private async Task PerformPeriodicSync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) 
            return;

        using var scope = serviceProvider.CreateScope();
        await CheckDataConsistency(scope);
    }

    private async Task CheckDataConsistency(IServiceScope scope)
    {
        var gameRepository = scope.ServiceProvider.GetRequiredService<IGameRepository>();
        var gameMongoRepository = scope.ServiceProvider.GetRequiredService<IGameMongoRepository>();
        var elasticSearchService = scope.ServiceProvider.GetService<IElasticSearchService>();

        var postgresGames = await gameRepository.GetAllAsync();
        var mongoGames = await gameMongoRepository.GetAllAsync();
        var elasticGames = elasticSearchService is not null ? await elasticSearchService.GetAllGamesAsync() : [];

        var postgresCount = postgresGames?.Count() ?? 0;
        var mongoCount = mongoGames?.Count ?? 0;
        var elasticCount = elasticGames?.Count ?? 0;

        if (mongoCount < postgresCount * 0.8 || elasticCount < postgresCount * 0.8)
        {
            _initialSyncCompleted = false;
            await PerformInitialSync(CancellationToken.None);
        }
    }

    private async Task SyncPromotions(IServiceScope scope)
    {
        var promotionRepository = scope.ServiceProvider.GetService<IPromotionRepository>();
        var promotionMongoRepository = scope.ServiceProvider.GetService<IPromotionMongoRepository>();

        if (promotionRepository == null || promotionMongoRepository == null)
            return;

        var promotionsFromPostgres = await promotionRepository.GetAllAsync();

        if (promotionsFromPostgres?.Any() != true)
            return;

        var promotionSyncCount = 0;

        foreach (var promotion in promotionsFromPostgres)
        {
            var cleanPromotion = new Promotion(promotion.Discount.Value, promotion.StartDate.Value, promotion.EndDate.Value)
            {
                Id = promotion.Id,
                Games = []
            };

            var existingPromotion = await promotionMongoRepository.GetByIdAsync(promotion.Id);

            if (existingPromotion is null)
            {
                await promotionMongoRepository.InsertAsync(cleanPromotion);
                promotionSyncCount++;
            }
            else
            {
                await promotionMongoRepository.UpdateAsync(promotion.Id, cleanPromotion);
            }
        }
    }
}