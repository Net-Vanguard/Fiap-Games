[ExcludeFromCodeCoverage]
public class GenericErrorNotFoundExample : IExamplesProvider<BaseResponse<object>>
{
    public BaseResponse<object> GetExamples()
    {
        var notification = new NotificationModel
        {
            NotificationType = NotificationModel.ENotificationType.NotFound
        };

        notification.AddMessage("Field", "Resource not found");
        notification.AddMessage("NotFound", "The requested resource does not exist");

        return BaseResponse<object>.Fail(notification);
    }
}
