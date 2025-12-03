namespace Craft.Cache;

/// <summary>
/// Legacy Redis cache service stub.
/// </summary>
[Obsolete("This class is deprecated. Use ICacheService with RedisCacheProvider instead. This will be removed in a future version.")]
public class RedisCacheService : ICacheService
{
    public Task<T?> GetOrSetAsync<T>(string cacheKey, Func<Task<T>> valueFactory)
    {
        throw new NotImplementedException();
    }

    public void Remove(string cacheKey)
    {
        throw new NotImplementedException();
    }

    public T? Set<T>(string cacheKey, T? value)
    {
        throw new NotImplementedException();
    }

    public (bool, T?) TryGet<T>(string cacheKey)
    {
        throw new NotImplementedException();
    }

    // New interface methods
    public T? Set<T>(string cacheKey, T? value, TimeSpan? expiration) => throw new NotImplementedException();
    public T? Set<T>(string cacheKey, T? value, CacheEntryOptions options) => throw new NotImplementedException();
    public Task<CacheResult<T>> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<CacheResult> SetAsync<T>(string cacheKey, T? value, CacheEntryOptions? options = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<CacheResult> RemoveAsync(string cacheKey, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<T?> GetOrSetAsync<T>(string cacheKey, Func<Task<T>> valueFactory, CacheEntryOptions? options, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<IDictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<CacheResult> SetManyAsync<T>(IDictionary<string, T?> items, CacheEntryOptions? options = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<int> RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<bool> ExistsAsync(string cacheKey, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<CacheStats> GetStatsAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<CacheResult> ClearAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<CacheResult> RefreshAsync(string cacheKey, CancellationToken cancellationToken = default) => throw new NotImplementedException();
}
