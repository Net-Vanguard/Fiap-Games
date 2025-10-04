namespace Fiap.Domain.PromotionAggregate.Events
{
	public record PromotionCreatedEvent : StoredEvent
	{
		public int PromotionId { get; set; }
		public string Name { get; set; }

		public PromotionCreatedEvent() { }

		public PromotionCreatedEvent(Promotion entity)
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

		public PromotionCreatedEvent(int promotionId, string name)
		{
			PromotionId = promotionId;
			Name = name;
		}
	}
}
