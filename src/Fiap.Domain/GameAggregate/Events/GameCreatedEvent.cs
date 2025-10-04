using System.Text.Json;

namespace Fiap.Domain.GameAggregate.Events
{
	public record GameCreatedEvent : StoredEvent
	{		
		public int GameId { get; set; }
		public string Name { get; set; }

		public GameCreatedEvent(){}

		public GameCreatedEvent(Game entity)
		{
			Id = Guid.NewGuid();
			GameId = entity.Id;
			Name = entity.Name;
			OccurredOn = DateTime.UtcNow;
			StreamName = $"{nameof(Game)}-{entity.Id}";
			Type = nameof(Game);
			Data = JsonSerializer.Serialize(new
			{
				entity.Id,
				entity.Name,
                entity.Price,
				entity.Genre,
				entity.PromotionId,
			});
		}
	}
}
