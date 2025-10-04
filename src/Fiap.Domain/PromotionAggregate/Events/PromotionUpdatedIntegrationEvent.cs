namespace Fiap.Domain.PromotionAggregate.Events
{
	public record PromotionUpdatedIntegrationEvent(int PromotionId, Money Discount, DateTime StartDate, DateTime EndDate, List<int>? GameIds);
}