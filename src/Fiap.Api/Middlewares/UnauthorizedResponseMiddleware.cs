[ExcludeFromCodeCoverage]
public class UnauthorizedResponseMiddleware
{
    private readonly RequestDelegate _next;

    public UnauthorizedResponseMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        await _next(context);

        if (!context.Response.HasStarted)
        {
            if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
            {
                await WriteErrorResponseAsync(context, NotificationModel.ENotificationType.Unauthorized,
                    ("Authorization", "You are not authorized to perform this action."),
                    ("Token", "Missing or invalid authentication token."));
            }

            else if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
            {
                await WriteErrorResponseAsync(context, NotificationModel.ENotificationType.Unauthorized,
                    ("Permission", "You do not have permission to access this resource."),
                    ("Access", "Access is denied for this role or user."));
            }
        }
    }

    private static async Task WriteErrorResponseAsync(HttpContext context,
        NotificationModel.ENotificationType type,
        params (string Key, string Message)[] messages)
    {
        context.Response.ContentType = "application/json";

        var notification = new NotificationModel
        {
            NotificationType = type
        };

        foreach (var (key, msg) in messages)
        {
            notification.AddMessage(key, msg);
        }

        var response = BaseResponse<object>.Fail(notification);
        var json = JsonSerializer.Serialize(response);

        await context.Response.WriteAsync(json);
    }
}
