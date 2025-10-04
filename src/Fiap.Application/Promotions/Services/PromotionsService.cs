namespace Fiap.Application.Promotions.Services
{
    public class PromotionsService(
        INotification notification,
        IPromotionRepository promotionRepository,
        IGameRepository gameRepository,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        ILogger<PromotionsService> logger,
		IPromotionMongoRepository promotionMongoRepository,
		IOutboxRepository outboxRepository
		) : BaseService(notification), IPromotionsService
    {
        public Task<PromotionResponse> CreateAsync(CreatePromotionRequest request) => ExecuteAsync(async () =>
        {
            var response = new PromotionResponse();

            Validate(request, new CreatePromotionRequestValidator());

            var promotion = (Promotion)request;

            promotion.ValidatePeriod();

            await promotionRepository.InsertOrUpdateAsync(promotion);
            await promotionRepository.SaveChangesAsync();

            var updatedGames = await CreatePromotion(request, promotion);

            await gameRepository.SaveChangesAsync();

			var gameIds = updatedGames.Select(g => g.Id).ToList();
			var integrationEvent = new PromotionCreatedIntegrationEvent(promotion.Id, promotion.Discount, promotion.StartDate, promotion.EndDate, gameIds.Count > 0 ? gameIds : null);
			var outbox = new OutboxMessage(nameof(PromotionCreatedIntegrationEvent), integrationEvent, DateTime.UtcNow);
			await outboxRepository.InsertOrUpdateAsync(outbox);
			await outboxRepository.SaveChangesAsync();

			await cache.RemoveAsync(EnumCacheTags.AllPromotions);

            response = (PromotionResponse)promotion;

            return response;

        });

        private async Task<List<Game>> CreatePromotion(CreatePromotionRequest request, Promotion promotion)
        {
            var games = new List<Game>();

            if (request.GameId is not null && request.GameId.Count is not 0)
            {
                await cache.RemoveAsync(EnumCacheTags.AllGames);

                var validIds = request.GameId
                    .Where(id => id.HasValue)
                    .Select(id => id.Value)
                    .ToList();

                foreach (var gameId in validIds)
                {
                    var game = await gameRepository.GetByIdAsync(gameId, noTracking: false);
                    if (game is null)
                    {
                        notification.AddNotification($"Game with ID {gameId} Not found", "Not Found", ENotificationType.NotFound);
                        continue; 
                    }

                    await cache.RemoveAsync(EnumCacheTags.GameId(game.Id));

                    game.PromotionId = promotion.Id;
                    games.Add(game);                    
                }

                if (games.Count is not 0)
                {
                    await gameRepository.UpdateRangeAsync(games);
                }
            }

            return games;
        }

        public Task<BaseResponse<object>> UpdateAsync(int id, UpdatePromotionRequest request) => ExecuteAsync(async () =>
        {
            var response = new PromotionResponse();

            Validate(request, new UpdatePromotionRequestValidator());

            var promotion = await promotionRepository.GetByIdAsync(id, noTracking: true);

            if (promotion is null)
            {
                notification.AddNotification("PromotionId", "Promotion not found", NotificationModel.ENotificationType.NotFound);
                return null!;
            }

            promotion.UpdateDiscount(request.Discount, request.ExpirationDate);

            await promotionRepository.UpdateAsync(promotion);
            await promotionRepository.SaveChangesAsync();

            var updatedGames = await UpdateGamesPromotion(request.GameId, promotion.Id);

            await gameRepository.SaveChangesAsync();

			var gameIds = updatedGames.Select(g => g.Id).ToList();
			var integrationEvent = new PromotionUpdatedIntegrationEvent(promotion.Id, promotion.Discount, promotion.StartDate, promotion.EndDate, gameIds.Count > 0 ? gameIds : null);
			var outbox = new OutboxMessage(nameof(PromotionUpdatedIntegrationEvent), integrationEvent, DateTime.UtcNow);
			await outboxRepository.InsertOrUpdateAsync(outbox);
			await outboxRepository.SaveChangesAsync();

			await cache.RemoveAsync(EnumCacheTags.AllPromotions);
            await cache.RemoveAsync(EnumCacheTags.PromotionId(id));

            return BaseResponse<object>.Ok(null);

        });

        private async Task<List<Game>> UpdateGamesPromotion(List<int?>? gameIds, int promotionId)
        {
            var validIds = gameIds?
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .ToList();

            if (validIds is null || validIds.Count is 0)
                return [];

            await cache.RemoveAsync(EnumCacheTags.AllGames);

            var games = new List<Game>();

            foreach (var gameId in validIds)
            {
                var game = await gameRepository.GetByIdAsync(gameId, noTracking: false);
                if (game is null)
                {
                    notification.AddNotification($"Game with ID {gameId} Not found", "Not Found", ENotificationType.NotFound);
                    continue;
                }

                await cache.RemoveAsync(EnumCacheTags.GameId(game.Id));


                game.AssignPromotion(promotionId);
                games.Add(game);                
            }

            if (games.Count > 0)
                await gameRepository.UpdateRangeAsync(games);

			return games;
        }

        public async Task<PromotionResponse> GetPromotionAsync(int id)
        {
            var response = new PromotionResponse();

            var cacheKey = EnumCacheTags.PromotionId(id);

            async Task<Promotion> fallbackFunc()
            {
				var promotion = await promotionMongoRepository.GetByIdAsync(id);
				if (promotion is not null)
				{
					logger.LogInformation("Promotion {PromotionId} found in MongoDB.", id);
					return promotion;
				}

				logger.LogInformation("Promotion {PromotionId} not found in MongoDB.", id);
				return promotion;
			}

            var cachedPromotion = await cache.GetOrSetAsync(cacheKey, fallbackFunc);

            if (cachedPromotion is null)
            {
                notification.AddNotification("Promotion not found", $"Promotion with id {id} not found", ENotificationType.NotFound);
                return response;
            }

            response = (PromotionResponse)cachedPromotion;

            return response;
        }
	}
}
