using MongoDB.Bson;

namespace Fiap.Infra.MongoDb.Repositories
{
	public class GameMongoRepository(IMongoDatabase database) : BaseRepository<Game>(database, "games"), IGameMongoRepository
	{
        public async Task<IEnumerable<Game>> GetAllWithPromotionsAsync()
		{
			var pipeline = new BsonDocument[]
			{
				new("$lookup", new BsonDocument
				{
					{ "from", "promotions" },
					{ "localField", "PromotionId" },
					{ "foreignField", "_id" },
					{ "as", "promotionArray" }
				}),

				new("$addFields", new BsonDocument
				{
					{ "Promotion", new BsonDocument("$cond", new BsonDocument
						{
							{ "if", new BsonDocument("$eq", new BsonArray { new BsonDocument("$size", "$promotionArray"), 0 }) },
							{ "then", BsonNull.Value },
							{ "else", new BsonDocument("$arrayElemAt", new BsonArray { "$promotionArray", 0 }) }
						})
					}
				}),

				new("$unset", "promotionArray"),

				new("$sort", new BsonDocument("_id", 1))
			};

			var aggregationResult = await _collection.Aggregate<Game>(pipeline).ToListAsync();

			foreach (var game in aggregationResult)
			{
				if (game.Promotion != null)
				{
					game.Promotion.Games = [];
				}
			}

			return aggregationResult;
		}

		public async Task<Game> GetByIdWithPromotionAsync(object id)
		{
			var pipeline = new BsonDocument[]
			{
				new("$match", new BsonDocument("_id", BsonValue.Create(id))),

				new("$lookup", new BsonDocument
				{
					{ "from", "promotions" },
					{ "localField", "PromotionId" },
					{ "foreignField", "_id" },
					{ "as", "promotionArray" }
				}),

				new("$addFields", new BsonDocument
				{
					{ "Promotion", new BsonDocument("$cond", new BsonDocument
						{
							{ "if", new BsonDocument("$eq", new BsonArray { new BsonDocument("$size", "$promotionArray"), 0 }) },
							{ "then", BsonNull.Value },
							{ "else", new BsonDocument("$arrayElemAt", new BsonArray { "$promotionArray", 0 }) }
						})
					}
				}),

				new("$unset", "promotionArray")
			};

			var aggregationResult = await _collection.Aggregate<Game>(pipeline).FirstOrDefaultAsync();

			if (aggregationResult?.Promotion != null)
			{
				aggregationResult.Promotion.Games = [];
			}

			return aggregationResult;
		}
	}
}
