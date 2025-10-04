namespace Fiap.Application.Games.Services
{
    public interface IGamesService
    {
        Task<GameResponse> CreateAsync(CreateGameRequest request);
        Task<IEnumerable<GameResponse>> GetAllAsync();
        Task<GameResponse> GetAsync(int id);
        Task<IEnumerable<GameResponse>> SearchGamesAsync(Dictionary<string, string> queryParams, int limit = 50);
        Task<IEnumerable<GameResponse>> GetMostPopularGamesFromElasticsearchAsync();
        Task<IEnumerable<GameResponse>> GetUserRecommendationsAsync(int userId);
    }
}
