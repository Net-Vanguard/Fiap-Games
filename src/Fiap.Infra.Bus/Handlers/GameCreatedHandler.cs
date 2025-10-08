namespace Fiap.Infra.Bus.Handlers
{
	public class GameCreatedHandler(
		IGameMongoRepository gameMongoRepository,
		IEventStoreRepository eventStoreRepository,
		IElasticSearchService elasticSearchService,
		ILogger<GameCreatedHandler> logger) : IHandleMessages<GameCreatedIntegrationEvent>
	{
		public async Task Handle(GameCreatedIntegrationEvent message)
		{
			var entity = new Game(
				id: message.GameId, 
				name: message.Name,
				genre: message.Genre,
				price: message.Price,
				promotionId: message.PromotionId
			);

			var existingGame = await gameMongoRepository.GetByIdAsync(entity.Id);
			if (existingGame is null)
			{
				await gameMongoRepository.InsertAsync(entity);
			}
			else
			{
				await gameMongoRepository.UpdateAsync(entity.Id, entity);
			}

			var gameCreatedEvent = new GameCreatedEvent(entity);
			await eventStoreRepository.SaveAsync(gameCreatedEvent);

			var gameDoc = new GameDocument
			{
				Id = entity.Id, 
				Name = entity.Name,
				Genre = entity.Genre,
				Price = entity.Price.Value,
				PromotionId = entity.PromotionId,
				FinalPrice = entity.GetFinalPrice(),
				HasActivePromotion = entity.HasActivePromotion(),
				DiscountPercentage = entity.HasActivePromotion() ? entity.GetDiscountPercentage() : null,
				IndexedAt = DateTime.UtcNow,
				PopularityScore = 0,
				Tags = [entity.Genre.ToLowerInvariant()]
			};

			await elasticSearchService.IndexGamesAsync([gameDoc]);
		}
	}
}