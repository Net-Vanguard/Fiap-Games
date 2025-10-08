namespace Fiap.Domain.PromotionAggregate.Events
{
	public record PromotionCreatedIntegrationEvent(int PromotionId, Money Discount, DateTime StartDate, DateTime EndDate, List<int>? GameIds);
}
