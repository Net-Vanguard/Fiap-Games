using Fiap.Infra.CrossCutting.Common.Elastic.Models;

namespace Fiap.Infra.Bus.Handlers
{
	public class PromotionCreatedHandler(
		IPromotionMongoRepository promotionMongoRepository,
		IGameMongoRepository gameMongoRepository,
		IEventStoreRepository eventStoreRepository,
		IElasticSearchService elasticSearchService,
		ILogger<PromotionCreatedHandler> logger) : IHandleMessages<PromotionCreatedIntegrationEvent>
	{
		public async Task Handle(PromotionCreatedIntegrationEvent message)
		{
			var entity = new Promotion(
				message.Discount.Value,
				message.StartDate,
				message.EndDate
			)
			{
				Id = message.PromotionId
			};

			await promotionMongoRepository.InsertAsync(entity);

			if (message.GameIds is not null && message.GameIds.Count > 0)
			{
				await AssignPromotionToGames(message.PromotionId, message.GameIds);
			}

			var promotionCreatedEvent = new PromotionCreatedEvent(entity);
			await eventStoreRepository.SaveAsync(promotionCreatedEvent);
		}

		private async Task AssignPromotionToGames(int promotionId, List<int> gameIds)
		{
			var updatedGameIds = new List<int>();

			foreach (var gameId in gameIds)
			{
				var game = await gameMongoRepository.GetByIdAsync(gameId);
				if (game is not null)
				{
					game.AssignPromotion(promotionId);
					await gameMongoRepository.UpdateAsync(gameId, game);
					updatedGameIds.Add(gameId);
					logger.LogDebug("Assigned promotion {PromotionId} to game {GameId}", promotionId, gameId);
				}
			}

			if (updatedGameIds.Count > 0)
			{
				await UpdateElasticsearchPromotions(updatedGameIds);
			}
		}

		private async Task UpdateElasticsearchPromotions(List<int> gameIds)
		{
			foreach (var gameId in gameIds)
			{
				var game = await gameMongoRepository.GetByIdWithPromotionAsync(gameId);
				if (game is not null)
				{
					var gameDoc = new GameDocument
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
					};

					await elasticSearchService.IndexGamesAsync([gameDoc]);
				}
			}
		}
	}
}
