namespace Fiap.Infra.Utils
{
    public class ServiceBus
    {
        public string QueueName { get; set; } = string.Empty;
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
    }
}
