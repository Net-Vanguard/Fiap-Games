namespace Fiap.Api
{
    [ExcludeFromCodeCoverage]
    public class BaseController(INotification notification) : ControllerBase
    {
        private bool IsValidOperation() => !notification.HasNotification;

        protected IActionResult Response<T>(BaseResponse<T> response)
        {
            if (IsValidOperation())
            {
                if (response.Data == null)
                    return NoContent();

                return Ok(response);
            }

            response.Success = false;
            response.Data = default; 
            response.Error = notification.NotificationModel;

            return response.Error.NotificationType switch
            {
                ENotificationType.BusinessRules => Conflict(response),
                ENotificationType.NotFound => NotFound(response),
                ENotificationType.BadRequestError => BadRequest(response),
                _ => StatusCode((int)HttpStatusCode.InternalServerError, response)
            };
        }

        protected new IActionResult Response<T>(int? id, object response)
        {
            if (!IsValidOperation())
            {
                var statusCode = MapNotificationToStatusCode(notification.NotificationModel.NotificationType);

                return StatusCode(statusCode, new
                {
                    success = false,
                    error = notification.NotificationModel
                });
            }

            if (id == null)
                return Ok(new { success = true, data = response });

            var controller = ControllerContext.RouteData.Values["controller"]?.ToString();
            var version = RouteData.Values["version"]?.ToString();
            var location = $"/api/v{version}/{controller}/{id}";

            return Created(location, new { success = true, data = response ?? new object() });
        }

        private int MapNotificationToStatusCode(NotificationModel.ENotificationType notificationType)
        {
            return notificationType switch
            {
                NotificationModel.ENotificationType.InternalServerError => StatusCodes.Status500InternalServerError,
                NotificationModel.ENotificationType.BusinessRules => StatusCodes.Status409Conflict,
                NotificationModel.ENotificationType.NotFound => StatusCodes.Status404NotFound,
                NotificationModel.ENotificationType.Unauthorized => StatusCodes.Status401Unauthorized,
                NotificationModel.ENotificationType.BadRequestError => StatusCodes.Status400BadRequest,
                NotificationModel.ENotificationType.Default => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status400BadRequest
            };
        }

        protected int GetLoggedUser()
        {
            var userIdentity = HttpContext.User.Identity as ClaimsIdentity;
            var user = userIdentity?.Claims.Where(c => c.Type == "id").FirstOrDefault();
            return user == null ? 0 : int.Parse(user.Value);
        }
    }
}
