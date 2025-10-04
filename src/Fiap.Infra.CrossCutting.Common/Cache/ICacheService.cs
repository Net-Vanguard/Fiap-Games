namespace Fiap.Infra.CacheService.Services
{
    public interface ICacheService
    {
        Task SetAsync(string cacheTag, object cacheValue);

        Task RemoveAsync(string cacheTag);

        Task RemoveAsync(params string[] cacheTags);

        Task<TResponse?> GetAsync<TResponse>(string cacheTag);

        Task<TResponse?> GetOrSetAsync<TResponse>(string cacheTag, Func<TResponse?> fallbackFunc);

        Task<TResponse?> GetOrSetAsync<TResponse>(string cacheTag, Func<Task<TResponse>> fallbackFunc);
    }
}
