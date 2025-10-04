namespace Fiap.Infra.Data.Repositories
{
    public class PromotionRepository(Context context) : BaseRepository<Promotion>(context), IPromotionRepository
    {
    }
}
