using System.Text.Json;

namespace Fiap.Domain.OutboxAggregate
{
    public class OutboxMessage : Entity<Guid>
    {
        public string Type { get; set; }
        public string Content { get; set; }
        public DateTime OccuredOn { get; set; }
        public DateTime? ProcessedOn { get; set; }

        public OutboxMessage()
        {
            
        }
        
        public OutboxMessage(string type, object content, DateTime occuredOn, DateTime? processedOn = null)
        {
            Type = type;
            Content = JsonSerializer.Serialize(content);
            OccuredOn = occuredOn;
            ProcessedOn = processedOn;
        }
    }
}
