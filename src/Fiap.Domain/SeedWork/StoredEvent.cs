namespace Fiap.Domain.Common.Events
{
	public record StoredEvent : IEventSourcing
	{
		public Guid Id { get; set; }
		public string AggregateId { get; set; } = string.Empty;
		public string StreamName { get; set; } = string.Empty;
		public string Type { get; set; } = string.Empty;
		public string Data { get; set; } = string.Empty;
		public DateTime OccurredOn { get; set; }
	}
}
