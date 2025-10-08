

namespace Fiap.Infra.Data.Repositories
{
    public class OutboxRepository(Context context) : BaseRepository<OutboxMessage>(context), IOutboxRepository
    {
    }
}
