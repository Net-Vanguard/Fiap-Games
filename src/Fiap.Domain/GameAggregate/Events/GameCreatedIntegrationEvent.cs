namespace Fiap.Domain.GameAggregate.Events
{
	public record GameCreatedIntegrationEvent(int GameId, string Name, string Genre, decimal Price, int? PromotionId);
}
