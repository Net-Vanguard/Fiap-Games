namespace Fiap.Infra.CrossCutting.Common.Elastic.Services
{
    public interface IElasticSearchService
    {
        Task<bool> IndexGamesAsync(IEnumerable<GameDocument> games);
        Task<IReadOnlyList<GameDocument>> GetAllGamesAsync();
        Task<GameDocument?> GetGameByIdAsync(int id);
        Task<IReadOnlyList<GameDocument>> GetByIdsAsync(IEnumerable<int> ids);
        Task<IReadOnlyList<GameDocument>> GetRecommendationsByGenreAsync(string genre, int userId, int limit = 10);
        Task<IReadOnlyList<GameDocument>> GetMostPopularGamesAsync(int limit = 10);
        Task<bool> UpdateGamePopularityAsync(int gameId, int popularityScore);
        Task<bool> BulkUpdatePopularityAsync(Dictionary<int, int> gamePopularityScores);        
        Task<IReadOnlyList<GameDocument>> SearchGamesAsync(string? searchTerm = null, string? genre = null, int limit = 50);
        Task<IReadOnlyList<GameDocument>> SearchWithDSLAsync(Dictionary<string, string> queryParams, int limit = 50);
        Task<IReadOnlyList<GameDocument>> MatchQueryAsync(string field, string query, int limit = 50);
        Task<IReadOnlyList<GameDocument>> FuzzyQueryAsync(string field, string query, int limit = 50);
        Task<IReadOnlyList<GameDocument>> WildcardQueryAsync(string field, string query, int limit = 50);
        Task<IReadOnlyList<GameDocument>> MultiMatchQueryAsync(string[] fields, string query, int limit = 50);
        Task<IReadOnlyList<GameDocument>> BoolQueryAsync(Query mustQuery, Query shouldQuery = null, Query mustNotQuery = null, int limit = 50);
        Task<IReadOnlyList<GameDocument>> GoogleStyleSearchAsync(string query, int limit = 50);
    }
}