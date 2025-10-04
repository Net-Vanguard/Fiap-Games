namespace Fiap.Application.Games.Services;
public class GamesService(
    INotification notification,
    IGameRepository gameRepository,
    IElasticSearchService elasticSearchService,
    ICacheService cache,
    ICRMService crm,
    IGameMongoRepository gameMongoRepository,
    IOutboxRepository outboxRepository,
    ILogger<GamesService> logger
    ) : BaseService(notification), IGamesService
{
    public Task<GameResponse> CreateAsync(CreateGameRequest request) => ExecuteAsync(async () =>
    {
        var response = new GameResponse();

        Validate(request, new CreateGameRequestValidator());

        var name = request.Name.ToLowerInvariant().Trim();

        var exists = await gameRepository.ExistAsync(g => g.Name.ToLower() == name);

        if (exists)
        {
            notification.AddNotification("Create Game", $"The game '{request.Name}' has already been registered.", ENotificationType.BusinessRules);
            return response;
        }

        var game = Game.Create(request.Name, request.Genre, request.Price, request.PromotionId);
        await gameRepository.InsertOrUpdateAsync(game);
        await gameRepository.SaveChangesAsync();

		var integrationEvent = new GameCreatedIntegrationEvent(game.Id,	game.Name, game.Genre, game.Price.Value, game.PromotionId);
		var outbox = new OutboxMessage(nameof(GameCreatedIntegrationEvent), integrationEvent, DateTime.UtcNow);
		await outboxRepository.InsertOrUpdateAsync(outbox);
		await outboxRepository.SaveChangesAsync();
			
		await cache.RemoveAsync(EnumCacheTags.AllGames);


		await IndexGameInElasticsearchAsync(game);


		response = (GameResponse)game;
        return response;
    });

    public Task<IEnumerable<GameResponse>> GetAllAsync() => ExecuteAsync(async () =>
    {
        var gamesFromElastic = await elasticSearchService.GetAllGamesAsync();
            
        if (gamesFromElastic?.Any() is true)
        {
            return gamesFromElastic.Select(doc => ConvertToGameResponse(doc));
        }

        async Task<IEnumerable<Game>> fallbackFunc()
        {
            var games = await gameMongoRepository.GetAllWithPromotionsAsync();
		    if (games is not null)
		    {
			    return games;
		    }

		    return games ?? [];
	    }            

        var cachedGames = await cache.GetOrSetAsync(EnumCacheTags.AllGames, fallbackFunc);
        return cachedGames.Select(game => (GameResponse)game);
    });

    public Task<GameResponse> GetAsync(int id) => ExecuteAsync(async () =>
    {
        var response = new GameResponse();

        var gameFromElastic = await elasticSearchService.GetGameByIdAsync(id);
            
        if (gameFromElastic is not null)
        {
            return ConvertToGameResponse(gameFromElastic);
        }

        var cacheKey = EnumCacheTags.GameId(id);
        var cachedGame = await cache.GetOrSetAsync(cacheKey, (async () =>
        {
            var game = await gameMongoRepository.GetByIdWithPromotionAsync(id);
			if (game is not null)
			{
				return game;
			}

			return game;
		}));

        if (cachedGame is null)
        {
            notification.AddNotification("Get game by id", $"Game not found with id {id}", ENotificationType.NotFound);
            return response;
        }

        response = (GameResponse)cachedGame;
        return response;
    });

    public Task<IEnumerable<GameResponse>> GetMostPopularGamesFromElasticsearchAsync() => ExecuteAsync(async () =>
    {
        var usersResponse = await crm.GetAllAsync();

        if (!usersResponse.Success || usersResponse.Data?.Any() is not true)
        {
            notification.AddNotification("CRM", "CRM service is not available", ENotificationType.BusinessRules);
            return [];
        }

        var gamePopularity = usersResponse.Data
            .Where(u => u.GameIds?.Any() == true)
            .SelectMany(u => u.GameIds)
            .GroupBy(gameId => gameId)
            .OrderByDescending(g => g.Count())
            .ToDictionary(g => g.Key, g => g.Count());

        if (gamePopularity.Count is 0)
        {
            return [];
        }

        await elasticSearchService.BulkUpdatePopularityAsync(gamePopularity);
            
        var allGamesFromElastic = await elasticSearchService.GetAllGamesAsync();

        if (allGamesFromElastic?.Any() is not true)
        {
            notification.AddNotification("Elasticsearch", "Elasticsearch is empty - cannot get popular games", ENotificationType.BusinessRules);
            return [];
        }

        var popularGameIds = gamePopularity.Keys.ToList();
        var popularGames = allGamesFromElastic
            .Where(g => popularGameIds.Contains(g.Id))
            .ToList();

        if (popularGames.Count is 0)
        {
            return [];
        }

        var orderedPopularGames = popularGames
            .OrderByDescending(g => gamePopularity.GetValueOrDefault(g.Id, 0))
            .ThenByDescending(g => g.HasActivePromotion)
            .ThenBy(g => g.Name)
            .Take(10)
            .ToList();

        return orderedPopularGames.Select(doc => ConvertToGameResponse(doc));
    });

    public Task<IEnumerable<GameResponse>> GetUserRecommendationsAsync(int userId) => ExecuteAsync(async () =>
    {
        var userResponse = await crm.GetAsync(userId);

        if (!userResponse.Success || userResponse.Data is null)
        {
            notification.AddNotification("CRM", $"CRM data not available for user {userId}", ENotificationType.BusinessRules);
            return [];
        }

        var targetUser = userResponse.Data;
        var userGameIds = targetUser?.GameIds ?? [];

        if (userGameIds.Count is 0)
        {
            var popularGames = await GetMostPopularGamesFromElasticsearchAsync();
            var popularAsList = popularGames.Take(5).ToList();

            return popularAsList;
        }

        var allGamesFromElastic = await elasticSearchService.GetAllGamesAsync();

        if (allGamesFromElastic?.Any() is not true)
        {
            notification.AddNotification("Elasticsearch", "Elasticsearch is not available for recommendations", ENotificationType.BusinessRules);
            return [];
        }

        var userGames = allGamesFromElastic.Where(g => userGameIds.Contains(g.Id)).ToList();

        if (userGames.Count is 0)
        {
            var allUsersForGenres = await crm.GetAllAsync();
            if (allUsersForGenres.Success && allUsersForGenres.Data?.Any() == true)
            {
                var popularGenres = allUsersForGenres.Data
                    .Where(u => u.GameIds?.Any() == true)
                    .SelectMany(u => u.GameIds)
                    .Select(gId => allGamesFromElastic.FirstOrDefault(g => g.Id == gId)?.Genre)
                    .Where(genre => !string.IsNullOrEmpty(genre))
                    .GroupBy(genre => genre)
                    .OrderByDescending(g => g.Count())
                    .Take(3)
                    .Select(g => g.Key)
                    .ToList();

                var genreRecommendations = new List<GameDocument>();
                foreach (var genre in popularGenres)
                {
                    var genreGames = allGamesFromElastic
                        .Where(g => g.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase))
                        .Where(g => !userGameIds.Contains(g.Id))
                        .OrderByDescending(g => g.HasActivePromotion)
                        .Take(3)
                        .ToList();
                    genreRecommendations.AddRange(genreGames);
                }

                var finalGenreRecs = genreRecommendations
                    .GroupBy(g => g.Id)
                    .Select(g => g.First())
                    .Take(10)
                    .ToList();

                return finalGenreRecs.Select(doc => ConvertToGameResponse(doc));
            }

            return [];
        }

        var preferredGenres = userGames
            .GroupBy(g => g.Genre)
            .OrderByDescending(g => g.Count())
            .Take(3)
            .Select(g => g.Key)
            .ToList();

        var allUsersResponse = await crm.GetAllAsync();

        if (!allUsersResponse.Success || allUsersResponse.Data?.Any() is not true)
        {
            notification.AddNotification("CRM", "CRM service not available for recommendations", ENotificationType.BusinessRules);
            return [];
        }

        var similarUsers = allUsersResponse.Data
            .Where(u => u.GameIds?.Any() == true && u.UserId != targetUser.UserId)
            .Select(u => new
            {
                User = u,
                CommonGames = u.GameIds.Intersect(userGameIds).Count(),
                TotalGames = u.GameIds.Count
            })
            .Where(u => u.CommonGames > 0)
            .OrderByDescending(u => u.CommonGames)
            .ThenByDescending(u => (double)u.CommonGames / u.TotalGames)
            .Take(5)
            .ToList();

        var recommendations = new List<GameDocument>();

        if (similarUsers.Count is not 0)
        {
            var recommendedGameIds = similarUsers
                .SelectMany(u => u.User.GameIds)
                .Where(gameId => !userGameIds.Contains(gameId))
                .GroupBy(gameId => gameId)
                .OrderByDescending(g => g.Count())
                .Take(15)
                .Select(g => g.Key)
                .ToList();

            if (recommendedGameIds.Any())
            {
                var similarUserGames = allGamesFromElastic.Where(g => recommendedGameIds.Contains(g.Id)).ToList();
                recommendations.AddRange(similarUserGames);
            }
        }

        foreach (var genre in preferredGenres)
        {
            var genreRecs = await elasticSearchService.GetRecommendationsByGenreAsync(genre, userId, 5);
            var newGenreRecs = genreRecs.Where(g => !userGameIds.Contains(g.Id) && !recommendations.Any(r => r.Id == g.Id));
            recommendations.AddRange(newGenreRecs);
        }

        var finalRecommendations = recommendations
            .GroupBy(g => g.Id)
            .Select(g => g.First())
            .OrderByDescending(g => g.HasActivePromotion)
            .ThenByDescending(g => g.PopularityScore)
            .ThenBy(g => g.Name)
            .Take(10)
            .ToList();

        return finalRecommendations.Select(doc => ConvertToGameResponse(doc));
    });

    private async Task IndexGameInElasticsearchAsync(Game game)
    {
        var gameDoc = new GameDocument
        {
            Id = game.Id,
            Name = game.Name,
            Genre = game.Genre,
            Price = game.Price.Value,
            PromotionId = game.PromotionId,
            FinalPrice = game.GetFinalPrice(),
            HasActivePromotion = game.HasActivePromotion(),
            DiscountPercentage = game.HasActivePromotion() ? game.GetDiscountPercentage() : null,
            IndexedAt = DateTime.UtcNow,
            PopularityScore = 0,
            Tags = [game.Genre.ToLowerInvariant()]
        };

        var success = await elasticSearchService.IndexGamesAsync([gameDoc]);
            
        if (success)
        {                
            await Task.Delay(2000);

            var verification = await elasticSearchService.GetGameByIdAsync(game.Id);
        }
        else
        {
            throw new InvalidOperationException($"Failed to index game {game.Id} in Elasticsearch - this is required for assignment");
        }
    }
    private static GameResponse ConvertToGameResponse(GameDocument doc)
    {
        return new GameResponse
        {
            Id = doc.Id,
            Name = doc.Name,
            Genre = doc.Genre,
            Price = doc.Price,
            PromotionId = doc.PromotionId,
            PriceWithDiscount = doc.FinalPrice
        };
    }

    public Task<IEnumerable<GameResponse>> SearchGamesAsync(Dictionary<string, string> queryParams, int limit = 50) => ExecuteAsync(async () =>
    {
        var results = await elasticSearchService.SearchWithDSLAsync(queryParams, limit);
            
        if (results?.Any() is true)
        {
            return results.Select(doc => ConvertToGameResponse(doc));
        }
            
        return [];
    });
}