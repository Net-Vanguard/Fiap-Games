namespace Fiap.Infra.MongoDb.Repositories.Base
{
	public abstract class BaseRepository<TEntity>(IMongoDatabase database, string collectionName)
        where TEntity : class
	{
		protected readonly IMongoCollection<TEntity> _collection = database.GetCollection<TEntity>(collectionName);

        public virtual async Task<List<TEntity>> GetAllAsync()
		{
			return await _collection.Find(_ => true).ToListAsync();
		}

		public virtual async Task<TEntity> GetByIdAsync(object id)
		{
			var filter = Builders<TEntity>.Filter.Eq("_id", id);
			return await _collection.Find(filter).SingleOrDefaultAsync();
		}

		public virtual async Task InsertAsync(TEntity entity)
		{
			await _collection.InsertOneAsync(entity);
		}

		public virtual async Task UpdateAsync(object id, TEntity entity)
		{
			var filter = Builders<TEntity>.Filter.Eq("Id", id);
			await _collection.ReplaceOneAsync(filter, entity);
		}

		public virtual async Task DeleteAsync(object id)
		{
			var filter = Builders<TEntity>.Filter.Eq("Id", id);
			await _collection.DeleteOneAsync(filter);
		}
	}
}
