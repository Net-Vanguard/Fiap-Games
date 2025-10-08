namespace Fiap.Infra.Data.EventSourcing
{
	public class EventStoreRepository(EventStoreClient eventStore, ILogger<EventStoreRepository> logger) : IEventStoreRepository
	{
		private static readonly Dictionary<string, Type> _eventTypes = AppDomain.CurrentDomain
			  .GetAssemblies()
			  .SelectMany(a => a.GetTypes())
			  .Where(t => t.IsSubclassOf(typeof(StoredEvent)))
			  .ToDictionary(t => t.Name, t => t);
		public async Task SaveAsync<T>(T @event) where T : StoredEvent
		{
			var eventData = new EventData(
				Uuid.NewUuid(),
				@event.GetType().Name,
				System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(@event)
			);
					
			await eventStore.AppendToStreamAsync(
				@event.StreamName,
				StreamState.Any,
				new[] { eventData }
			);
		}

		public async Task<IEnumerable<StoredEvent>> GetEventsAsync(string aggregate, string streamName)
		{
			var events = new List<StoredEvent>();

			var result = eventStore.ReadStreamAsync(Direction.Forwards, streamName, StreamPosition.Start);

			await foreach (var resolvedEvent in result)
			{
				var eventTypeName = resolvedEvent.Event.EventType;
				if (!_eventTypes.TryGetValue(eventTypeName, out var eventType))
				{
					logger.LogWarning("Event type '{EventTypeName}' not found in asemblies.", eventTypeName);
					continue;
				}

				var storedEvent = (StoredEvent)System.Text.Json.JsonSerializer.Deserialize(resolvedEvent.Event.Data.Span, eventType);
				if (storedEvent is not null)
					events.Add(storedEvent);
			}

			return events;
		}

	}
}