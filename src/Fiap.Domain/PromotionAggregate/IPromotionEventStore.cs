namespace Fiap.Domain.PromotionAggregate
{
	public interface IPromotionEventStore
	{
		Task SaveAsync(Guid aggregateId, IEnumerable<object> events);
		Task<IEnumerable<object>> LoadAsync(Guid aggregateId);
	}
}
