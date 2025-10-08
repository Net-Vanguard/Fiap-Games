
namespace Fiap.Domain.SeedWork
{
    public interface IBaseRepository<TEntity>
            where TEntity : class
    {
        Task<TEntity> InsertOrUpdateAsync(TEntity entity);

        Task<IEnumerable<TEntity>> InsertRangeAsync(IEnumerable<TEntity> entity);

        Task UpdateAsync(TEntity entity);

        IQueryable<TEntity> GetAll();

        Task<TEntity> GetOneNoTracking(Expression<Func<TEntity, bool>> expression);

        Task<TEntity> GetOneTracking(Expression<Func<TEntity, bool>> expression);
        Task<IEnumerable<TEntity>> GetNoTrackingAsync(Expression<Func<TEntity, bool>> expression);

        Task<bool> ExistAsync(Expression<Func<TEntity, bool>> expression);

        Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> expression);

        Task<TEntity> GetByIdAsync(int id, bool noTracking);

        IQueryable<TEntity> Get(Expression<Func<TEntity, bool>> expression);

        Task<IEnumerable<TEntity>> GetAllAsync();

        IQueryable<TEntity> Include<TProperty>(IQueryable<TEntity> query, Expression<Func<TEntity, TProperty>> path);

        Task DeleteAsync(TEntity entity);

        Task AddRangeAsync(IEnumerable<TEntity> entities);

        Task UpdateRangeAsync(IEnumerable<TEntity> entities);
        Task UpdateRangeNoTrackingAsync(IEnumerable<TEntity> entities);
    }
}
