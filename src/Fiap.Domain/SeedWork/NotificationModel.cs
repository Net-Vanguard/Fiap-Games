namespace Fiap.Domain.SeedWork
{
    [ExcludeFromCodeCoverage]
    public class NotificationModel
    {
        public Guid NotificationId { get; private set; } = Guid.NewGuid();
        public ENotificationType NotificationType { get; set; }

        public List<NotificationMessage> FieldMessages { get; private set; } = new();
        public List<NotificationMessage> GeneralMessages { get; private set; } = new();

        public void AddMessage(string key, string message)
        {
            var msg = new NotificationMessage
            {
                Key = key,
                Message = message
            };

            if (!string.IsNullOrWhiteSpace(key) && char.IsUpper(key[0]))
                FieldMessages.Add(msg);
            else
                GeneralMessages.Add(msg);
        }

        public class NotificationMessage
        {
            public string Key { get; set; } = null!;
            public string Message { get; set; } = null!;
        }

        public enum ENotificationType : byte
        {
            Default = 0,
            InternalServerError = 1,
            BusinessRules = 2,
            NotFound = 3,
            BadRequestError = 4,
            Unauthorized = 5

        }
    }
}
