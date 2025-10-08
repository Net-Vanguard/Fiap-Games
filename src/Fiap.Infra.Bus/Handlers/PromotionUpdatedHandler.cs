namespace Fiap.Infra.Bus.Handlers
{
	public class PromotionUpdatedHandler(
		IPromotionMongoRepository promotionMongoRepository,
		IGameMongoRepository gameMongoRepository,
		IEventStoreRepository eventStoreRepository,
		IElasticSearchService elasticSearchService,
		ILogger<PromotionUpdatedHandler> logger) : IHandleMessages<PromotionUpdatedIntegrationEvent>
	{
		public async Task Handle(PromotionUpdatedIntegrationEvent message)
		{
			var promotion = new Promotion(
				message.Discount.Value,
				message.StartDate,
				message.EndDate
			)
			{
				Id = message.PromotionId
			};

			await promotionMongoRepository.UpdateAsync(message.PromotionId, promotion);

			await UpdateGamesPromotionInMongo(message.PromotionId, message.GameIds);

			var promotionUpdatedEvent = new PromotionUpdatedEvent(message.PromotionId);
			await eventStoreRepository.SaveAsync(promotionUpdatedEvent);
		}

		private async Task UpdateGamesPromotionInMongo(int promotionId, List<int>? gameIds)
		{
			var removedGameIds = await RemovePromotionFromAllGames(promotionId);
			var addedGameIds = new List<int>();

			if (gameIds is not null && gameIds.Count > 0)
			{
				addedGameIds = await AssignPromotionToGames(promotionId, gameIds);
			}

			var allAffectedGames = removedGameIds.Union(addedGameIds).Distinct().ToList();
			if (allAffectedGames.Any())
			{
				await UpdateElasticsearchPromotions(allAffectedGames);
			}

			await RemovePromotionFromAllGames(promotionId);

			if (gameIds is not null && gameIds.Count > 0)
			{
				await AssignPromotionToGames(promotionId, gameIds);
			}
		}

		private async Task<List<int>> RemovePromotionFromAllGames(int promotionId)
		{
			var allGames = await gameMongoRepository.GetAllAsync();
			var gamesToUpdate = allGames.Where(g => g.PromotionId == promotionId).ToList();
			var updatedGameIds = new List<int>();

			foreach (var game in gamesToUpdate)
			{
				game.RemovePromotion();
				await gameMongoRepository.UpdateAsync(game.Id, game);
				updatedGameIds.Add(game.Id);
			}

			return updatedGameIds;
		}

		private async Task<List<int>> AssignPromotionToGames(int promotionId, List<int> gameIds)
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
				}
			}
			return updatedGameIds;
		}

		private async Task UpdateElasticsearchPromotions(List<int> gameIds)
		{
			foreach (var gameId in gameIds)
			{
				var game = await gameMongoRepository.GetByIdWithPromotionAsync(gameId);
				if (game is not null)
				{
					var gameDoc = new Fiap.Infra.CrossCutting.Common.Elastic.Models.GameDocument
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