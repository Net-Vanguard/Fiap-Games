namespace Fiap.Domain.GameAggregate
{
	public interface IGameMongoRepository: IMongoRepository<Game>
	{
		Task<IEnumerable<Game>> GetAllWithPromotionsAsync();
		Task<Game> GetByIdWithPromotionAsync(object id);
	}
}
