

namespace Fiap.Infra.Data.Repositories.Base
{
    [ExcludeFromCodeCoverage]
    public abstract class BaseRepository<TEntity>(Context context) : UnitOfWork(context), IBaseRepository<TEntity> where TEntity : class
    {
        protected DbSet<TEntity> dbSet = context.Set<TEntity>();

        public async Task<TEntity> InsertOrUpdateAsync(TEntity entity)
        {
            var entry = context.Entry(entity);

            if (entry.State == EntityState.Detached)
            {
                var exists = await dbSet.FindAsync(entry.Property("Id").CurrentValue);
                if (exists != null)                
                    dbSet.Update(entity);
                else
                    await dbSet.AddAsync(entity);
            }
            return entity;
        }

        public async Task<IEnumerable<TEntity>> InsertRangeAsync(IEnumerable<TEntity> entity)
        {
            await dbSet.AddRangeAsync(entity);
            return entity;
        }

        public Task UpdateAsync(TEntity entity)
        {
            dbSet.Attach(entity);
            context.Entry(entity).State = EntityState.Modified;
            return Task.CompletedTask;
        }

        public IQueryable<TEntity> GetAll() => dbSet.AsQueryable();

        public async Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> expression) => await GetAll().Where(expression).ToListAsync();

        public async Task<TEntity> GetByIdAsync(int id, bool noTracking) => noTracking ? await dbSet.AsNoTracking().FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id) : await dbSet.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);

        public virtual IQueryable<TEntity> Get(Expression<Func<TEntity, bool>> expression) => GetAll().Where(expression);

        public virtual IQueryable<TEntity> Include<TProperty>(IQueryable<TEntity> query, Expression<Func<TEntity, TProperty>> path) => query.Include(path);

        public async Task DeleteAsync(TEntity entity)
        {
            dbSet.Remove(entity);
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync() => await GetAll().OrderBy(e => EF.Property<int>(e, "Id")).ToListAsync();

        public async Task<bool> ExistAsync(Expression<Func<TEntity, bool>> expression) => await dbSet.AsNoTracking().AnyAsync(expression);

        public async Task<TEntity> GetOneNoTracking(Expression<Func<TEntity, bool>> expression) => await dbSet.AsNoTracking().FirstOrDefaultAsync(expression);

        public async Task<TEntity> GetOneTracking(Expression<Func<TEntity, bool>> expression) => await dbSet.FirstOrDefaultAsync(expression);
        public async Task<IEnumerable<TEntity>> GetNoTrackingAsync(Expression<Func<TEntity, bool>> expression) => await GetAll().AsNoTracking().Where(expression).ToListAsync();

        public async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            await dbSet.AddRangeAsync(entities);
        }

        public async Task UpdateRangeAsync(IEnumerable<TEntity> entities)
        {
            dbSet.UpdateRange(entities);
        }

        public async Task UpdateRangeNoTrackingAsync(IEnumerable<TEntity> entities)
        {
            dbSet.AttachRange(entities);
            dbSet.UpdateRange(entities);
        }
    }
}
