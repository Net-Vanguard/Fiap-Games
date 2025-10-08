namespace Fiap.Domain.GameAggregate
{
    public interface IGameRepository : IBaseRepository<Game>, IUnitOfWork
    {
        Task<IEnumerable<Game>> GetAllWithPromotionsAsync();
        Task<Game> GetByIdWithPromotionAsync(int id, bool noTracking = false);
    }
}
