namespace Fiap.Domain.OutboxAggregate
{
    public interface IOutboxRepository : IBaseRepository<OutboxMessage>, IUnitOfWork
    {
    }
}
