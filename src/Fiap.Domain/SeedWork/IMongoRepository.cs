namespace Fiap.Domain.SeedWork
{
	public interface IMongoRepository<TEntity>
		where TEntity : class
	{
		Task<List<TEntity>> GetAllAsync();
		Task<TEntity> GetByIdAsync(object id);
		Task InsertAsync(TEntity entity);
		Task UpdateAsync(object id, TEntity entity);
		Task DeleteAsync(object id);
	}
}
