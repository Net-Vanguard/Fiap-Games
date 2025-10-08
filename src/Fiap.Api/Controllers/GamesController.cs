namespace Fiap.Api.Controllers
{
    /// <summary>
    /// Controller used to manage game operations, such as creation, retrieval and management
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class GamesController(IGamesService gamesService, INotification notification) : BaseController(notification)
    {
       
        /// <summary>
        /// Creates a new game.
        /// </summary>
        /// <param name="request">The game data required to create a new entry in the system.</param>
        /// <returns>A response containing the created game, or an error message if the input is invalid.</returns>
        [HttpPost]
        [SwaggerOperation(
            Summary = "Creates a new game.",
            Description = "Creates a new game entry using the provided data, including title, genre, price, and promotion (if applicable). " +
                          "Returns the created game on success. Returns a 400 if the input is invalid, " +
                          "409 if a duplicate or conflicting game exists, 401/403 if unauthorized, and 500 in case of server error."
        )]
        [ProducesResponseType(typeof(SuccessResponse<GameResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(GenericErrorBadRequestExample))]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status409Conflict)]
        [SwaggerResponseExample(StatusCodes.Status409Conflict, typeof(GenericErrorConflictExample))]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status401Unauthorized)]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(GenericErrorUnauthorizedExample))]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status403Forbidden)]
        [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(GenericErrorForbiddenExample))]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(GenericErrorInternalServerExample))]
        public async Task<IActionResult> Create([FromBody] CreateGameRequest request)
        {
            var result = await gamesService.CreateAsync(request);
            return Response<GameResponse>(result?.Id, result);
        }

        /// <summary>
        /// Retrieve all games using Elasticsearch for efficient search.
        /// </summary>
        /// <returns>A response containing a list of all games from Elasticsearch, with fallback to database if needed.</returns>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Retrieve all games using Elasticsearch",
            Description = "Returns a list of all available games using Elasticsearch for efficient search. " +
                          "Falls back to database if Elasticsearch is unavailable. Games are ordered by popularity and name."
        )]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<GameResponse>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status401Unauthorized)]
        [SwaggerResponseExample(StatusCodes.Status401Unauthorized, typeof(GenericErrorUnauthorizedExample))]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status403Forbidden)]
        [SwaggerResponseExample(StatusCodes.Status403Forbidden, typeof(GenericErrorForbiddenExample))]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(GenericErrorInternalServerExample))]
        public async Task<IActionResult> GetAll()
        {
            var result = await gamesService.GetAllAsync();
            return Response(BaseResponse<IEnumerable<GameResponse>>.Ok(result));
        }

        /// <summary>
        /// Retrieve a game by ID using Elasticsearch for efficient search.
        /// </summary>
        /// <param name="id">The id game data required to fetch.</param>
        /// <returns>A response containing a game from Elasticsearch, with fallback to database if needed.</returns>
        [HttpGet("{id:int:min(1)}")]
        [SwaggerOperation(
            Summary = "Retrieve a game by ID using Elasticsearch",
            Description = "Fetches a single game based on the provided ID using Elasticsearch for efficient search. " +
                          "Falls back to database if Elasticsearch is unavailable. " +
                          "If no game is found, a 404 response is returned."
        )]
        [ProducesResponseType(typeof(SuccessResponse<GameResponse>), (int)HttpStatusCode.OK)]
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
            var result = await gamesService.GetAsync(id);
            return Response(BaseResponse<GameResponse>.Ok(result));
        }

        /// <summary>
        /// Advanced search for games using Elasticsearch with dynamic filtering.
        /// </summary>
        /// <param name="q">General search term (searches in name, genre, tags)</param>
        /// <param name="name">Exact name search</param>
        /// <param name="genre">Exact genre search</param>
        /// <param name="priceMin">Minimum price filter</param>
        /// <param name="priceMax">Maximum price filter</param>
        /// <param name="hasPromotion">Filter by promotion status</param>
        /// <param name="sort">Sort field (name, price, popularity, genre)</param>
        /// <param name="order">Sort order (asc, desc)</param>
        /// <param name="limit">Maximum number of results (default: 50, max: 100)</param>
        /// <returns>List of games matching the search criteria using advanced Elasticsearch queries</returns>
        [HttpGet("search")]
        [SwaggerOperation(
            Summary = "Advanced search for games using Elasticsearch",
            Description = "Perform advanced search queries on games using Elasticsearch with dynamic filtering, sorting, and full-text search capabilities. " +
                          "Supports complex query combinations for efficient game discovery."
        )]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<GameResponse>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchGames(
            [FromQuery] string? q = null,
            [FromQuery] string? name = null,
            [FromQuery] string? genre = null,
            [FromQuery] decimal? priceMin = null,
            [FromQuery] decimal? priceMax = null,
            [FromQuery] bool? hasPromotion = null,
            [FromQuery] string? sort = "relevance",
            [FromQuery] string? order = "desc",
            [FromQuery] int limit = 50)
        {
            if (limit > 100) limit = 100;
            if (limit < 1) limit = 1;

            var queryParams = new Dictionary<string, string>();
                
            if (!string.IsNullOrWhiteSpace(q)) queryParams.Add("q", q);
            if (!string.IsNullOrWhiteSpace(name)) queryParams.Add("name", name);
            if (!string.IsNullOrWhiteSpace(genre)) queryParams.Add("genre", genre);
            if (priceMin.HasValue) queryParams.Add("pricemin", priceMin.Value.ToString());
            if (priceMax.HasValue) queryParams.Add("pricemax", priceMax.Value.ToString());
            if (hasPromotion.HasValue) queryParams.Add("promotion", hasPromotion.Value.ToString().ToLower());
            if (!string.IsNullOrWhiteSpace(sort)) queryParams.Add("sort", sort);
            if (!string.IsNullOrWhiteSpace(order)) queryParams.Add("order", order);

            var result = await gamesService.SearchGamesAsync(queryParams, limit);

            return Response(BaseResponse<IEnumerable<GameResponse>>.Ok(result));
        }

        /// <summary>
        /// Get most popular games with aggregated metrics from user libraries using Elasticsearch.
        /// </summary>
        /// <returns>List of most popular games based on CRM library frequency and Elasticsearch aggregations</returns>
        [HttpGet("popular")]
        [SwaggerOperation(
            Summary = "Get most popular games with aggregated metrics",
            Description = "Get the most popular games based on frequency in user libraries with aggregated metrics from CRM data using Elasticsearch. " +
                          "Uses advanced aggregations to calculate real-time popularity scores."
        )]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<GameResponse>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMostPopularGames()
        {
            var result = await gamesService.GetMostPopularGamesFromElasticsearchAsync();
            return Response<IEnumerable<GameResponse>>(result?.Count(), result);
        }

        /// <summary>
        /// Get personalized game recommendations using advanced Elasticsearch queries.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of personalized game recommendations using advanced Elasticsearch queries and CRM data</returns>
        [HttpGet("recommendations/user/{userId}")]
        [SwaggerOperation(
            Summary = "Get personalized user recommendations with advanced queries",
            Description = "Create advanced Elasticsearch queries to suggest games based on user's library history and preferred genres from CRM data. " +
                          "Uses complex aggregations and scoring algorithms for personalized recommendations."
        )]
        [ProducesResponseType(typeof(SuccessResponse<IEnumerable<GameResponse>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserRecommendations(int userId)
        {
            var result = await gamesService.GetUserRecommendationsAsync(userId);
            return Response<IEnumerable<GameResponse>>(result?.Count(), result);
        }

        
    }
}

