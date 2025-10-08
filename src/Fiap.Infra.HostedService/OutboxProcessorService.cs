namespace Fiap.Infra.HostedService
{
	public class OutboxProcessorService(
		ILogger<OutboxProcessorService> logger,
		IServiceProvider serviceProvider,
		IBus bus,
		IConfiguration configuration
		) : BackgroundService
	{
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					await ProcessNextOutboxMessage();
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "Error in outbox processor main loop.");
				}
				await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
			}
		}
		private async Task ProcessNextOutboxMessage()
		{
			using var scope = serviceProvider.CreateScope();
			var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();

			var outboxes = await outboxRepository.GetAsync(x => x.ProcessedOn == null);

			foreach (var msg in outboxes)
			{
				if (string.IsNullOrWhiteSpace(msg.Type))
					continue;

				var eventType = Type.GetType(msg.Type)
					?? AppDomain.CurrentDomain.GetAssemblies()
						.SelectMany(a => a.GetTypes())
						.FirstOrDefault(t => t.Name == msg.Type);

				if (eventType == null)
				{
					logger.LogWarning("Event type {EventType} not found for outbox message {OutboxId}.", msg.Type, msg.Id);
					continue;
				}

				var eventMessage = JsonSerializer.Deserialize(msg.Content, eventType);
				if (eventMessage == null)
				{
					logger.LogWarning("Failed to deserialize content for outbox message {OutboxId}.", msg.Id);
					continue;
				}

				await bus.Advanced.Routing.Send(configuration["ServiceBus:FCGQueueName"], eventMessage);
				msg.ProcessedOn = DateTime.UtcNow;
			}

			if (outboxes.Count() > 0)
				await outboxRepository.SaveChangesAsync();
		}
	}
}