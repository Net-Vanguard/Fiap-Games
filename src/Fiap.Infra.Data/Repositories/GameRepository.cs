namespace Fiap.Infra.Data.Repositories
{
    public class GameRepository(Context context) : BaseRepository<Game>(context), IGameRepository
    {
        public async Task<IEnumerable<Game>> GetAllWithPromotionsAsync()
        {
            return await dbSet
                .Include(g => g.Promotion)
                .OrderBy(g => g.Id)
                .ToListAsync();
        }

        public async Task<Game> GetByIdWithPromotionAsync(int id, bool noTracking = false)
        {
            var query = dbSet.Include(g => g.Promotion);
            return noTracking 
                ? await query.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id)
                : await query.FirstOrDefaultAsync(g => g.Id == id);
        }
    }
}
