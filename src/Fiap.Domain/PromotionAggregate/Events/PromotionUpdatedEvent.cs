namespace Fiap.Domain.PromotionAggregate.Events
{
    public record PromotionUpdatedEvent : StoredEvent
	{
		public int PromotionId { get; set; }

		public PromotionUpdatedEvent() { }

		public PromotionUpdatedEvent(Promotion entity)
		{
			Id = Guid.NewGuid();
			PromotionId = entity.Id;
			OccurredOn = DateTime.UtcNow;
			StreamName = $"{nameof(Promotion)}-{entity.Id}";
			Type = nameof(Promotion);
			Data = System.Text.Json.JsonSerializer.Serialize(new
			{
				entity.Id,
				entity.Discount,
				entity.StartDate,
				entity.EndDate,
				GameIds = entity.Games?.Select(g => g.Id).ToList()
			});
		}

		public PromotionUpdatedEvent(int promotionId)
		{
			PromotionId = promotionId;
		}
	}
}
