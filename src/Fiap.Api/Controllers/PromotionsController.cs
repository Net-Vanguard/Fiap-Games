namespace Fiap.Api.Controllers
{
    /// <summary>
    /// Controller used by admin to manage all promotions for the games
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class PromotionsController(IPromotionsService promotionsService, INotification notification) : BaseController(notification)
    {
        /// <summary>
        /// Create a new promotion for one or more games.
        /// </summary>
        /// <param name="request">The promotion data to be created.</param>
        /// <returns>A response containing the created promotion.</returns>
        [HttpPost]
        [SwaggerOperation(
            Summary = "Create a new promotion for one or more games.",
            Description = "Creates a new promotion with the specified discount, start date, and end date. " +
                          "Optionally, one or more games can be associated with the promotion. " +
                          "Returns the created promotion on success. Returns 400 for validation errors, 401/403 for unauthorized access, and 500 for server errors."
        )]
        [ProducesResponseType(typeof(SuccessResponse<PromotionResponse>), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(GenericErrorBadRequestExample))]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status401Unauthorized)]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(GenericErrorUnauthorizedExample))]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status403Forbidden)]
        [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(GenericErrorForbiddenExample))]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(GenericErrorInternalServerExample))]
        public async Task<IActionResult> CreateAsync([FromBody] CreatePromotionRequest request)
        {
            var result = await promotionsService.CreateAsync(request);
            return Response<PromotionResponse>(result?.PromotionId, result); 
        }

        /// <summary>
        /// Update a promotion and its associated games.
        /// </summary>
        /// <param name="id">The ID of the promotion to be updated (must be greater than 0).</param>
        /// <param name="request">The updated promotion data.</param>
        /// <returns>A response containing the updated promotion.</returns>
        [HttpPatch("{id:int:min(1)}")]
        [SwaggerOperation(
            Summary = "Update a promotion and its associated games.",
            Description = "Updates the values of an existing promotion, such as discount or date range. " +
                          "Can also update the list of associated games. " +
                          "Returns no content on success. Returns 400 for validation errors, 404 if the promotion doesn't exist, 401/403 for unauthorized access, and 500 for unexpected errors."
        )]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(GenericErrorBadRequestExample))]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(GenericErrorNotFoundExample))]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status401Unauthorized)]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(GenericErrorUnauthorizedExample))]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status403Forbidden)]
        [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(GenericErrorForbiddenExample))]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(GenericErrorInternalServerExample))]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdatePromotionRequest request)
        {
            var result = await promotionsService.UpdateAsync(id, request);
            return Response(BaseResponse<PromotionResponse>.Ok(null));
        }

        /// <summary>
        /// Retrieve a promotion by ID.
        /// </summary>
        /// <param name="id">The ID of the promotion to be fetched (must be greater than 0).</param>
        /// <returns>A response containing the promotion.</returns>
        [HttpGet("{id:int:min(1)}")]
        [SwaggerOperation(
            Summary = "Retrieve a promotion by ID.",
            Description = "Fetches the details of a promotion using its ID. " +
                          "Returns the promotion if found. Returns 404 if the promotion does not exist, " +
                          "400 for invalid input, 401/403 for unauthorized access, and 500 for server errors."
        )]
        [ProducesResponseType(typeof(SuccessResponse<PromotionResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(GenericErrorBadRequestExample))]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status404NotFound)]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(GenericErrorNotFoundExample))]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status401Unauthorized)]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(GenericErrorUnauthorizedExample))]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status403Forbidden)]
        [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(GenericErrorForbiddenExample))]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(GenericErrorInternalServerExample))]
        public async Task<IActionResult> GetAsync(int id)
        {
            var result = await promotionsService.GetPromotionAsync(id);
            return Response(BaseResponse<PromotionResponse>.Ok(result));
        }
    }
}
