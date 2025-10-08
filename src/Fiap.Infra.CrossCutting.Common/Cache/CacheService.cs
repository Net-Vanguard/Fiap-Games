namespace Fiap.Infra.CacheService.Services;

[ExcludeFromCodeCoverage]
public sealed class CacheService(HybridCache cache, IConnectionMultiplexer redis, ILogger<CacheService> logger) : ICacheService
{
    private static readonly HybridCacheEntryOptions CACHE_OPTIONS = new()
    {
        Expiration = TimeSpan.FromMinutes(30),
        LocalCacheExpiration = TimeSpan.FromMinutes(5)
    };

    public async Task SetAsync(string cacheTag, object cacheValue)
    {
        logger.LogInformation("Creating cache\n\tTag: {0}", cacheTag);

        if (cacheTag is null || cacheValue is null)
        {
            logger.LogWarning("Cache not created {0} {1} cannot be null", nameof(cacheTag), nameof(cacheValue));
            return;
        }

        await cache.SetAsync(cacheTag, cacheValue, CACHE_OPTIONS);
    }

    public async Task RemoveAsync(string cacheTag)
    {
        if (cacheTag is null)
            return;

        await cache.RemoveAsync(cacheTag);

        var database = redis.GetDatabase();

        foreach (var endpoint in redis.GetEndPoints())
        {
            var server = redis.GetServer(endpoint);

            var keys = server.KeysAsync(pattern: $"{cacheTag}*");

            await foreach (var key in keys)
            {
                logger.LogInformation("Deleting key: {0}\n", key);

                var deletedStatus = await database.KeyDeleteAsync(key);

                if (deletedStatus)
                    logger.LogInformation("Deleted Key: {0}", key);
                else
                    logger.LogWarning("NOT Deleted Key: {0}", key);
            }
        }
    }

    public Task RemoveAsync(params string[] cacheTags)
    {
        var taskArray = cacheTags.Select(tag => RemoveAsync(tag));

        return Task.WhenAll(taskArray);
    }

    public async Task<TResponse?> GetAsync<TResponse>(string cacheTag)
    {
        if (cacheTag is null)
            return default;

        logger.LogInformation("Retrieving Cache\n\tTag: {0}\n", cacheTag);


        var response = await cache.GetOrCreateAsync(
            cacheTag,
            (token) => new ValueTask<TResponse>(default(TResponse)));

        var success = response is not null;
        logger.LogInformation("Retrieving Cache\n\tTag: {0}\n\tSuccess: {1}\n", cacheTag, success);

        return response;
    }

    public async Task<TResponse?> GetOrSetAsync<TResponse>(string cacheTag, Func<TResponse?> fallbackFunc)
    {
        return await cache.GetOrCreateAsync(
            cacheTag,
            (token) =>
            {
                var result = fallbackFunc();
                logger.LogInformation("Cache miss for tag: {0}, executing fallback", cacheTag);
                return new ValueTask<TResponse>(result);
            },
            CACHE_OPTIONS);
    }

    public async Task<TResponse?> GetOrSetAsync<TResponse>(string cacheTag, Func<Task<TResponse>> fallbackFuncAsync)
    {
        return await cache.GetOrCreateAsync(
            cacheTag,
            async (token) =>
            {
                var result = await fallbackFuncAsync();
                logger.LogInformation("Cache miss for tag: {0}, executing async fallback", cacheTag);
                return result;
            },
            CACHE_OPTIONS);
    }
}