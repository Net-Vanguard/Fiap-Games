namespace Fiap.Infra.MongoDb.Repositories
{
	public class PromotionMongoRepository(IMongoDatabase database) : BaseRepository<Promotion>(database, "promotions"), IPromotionMongoRepository
	{
    }
}
