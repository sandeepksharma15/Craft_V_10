using Microsoft.Extensions.Caching.Memory;

namespace Craft.Cache;

/// <summary>
/// Legacy memory cache service implementation.
/// </summary>
[Obsolete("This class is deprecated. Use ICacheService with MemoryCacheProvider instead. This will be removed in a future version.")]
public class MemoryCacheService : ICacheService
{
    private readonly MemoryCacheEntryOptions _cacheOptions;
    private readonly IMemoryCache _memoryCache;

    public MemoryCacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;

        _cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = DateTime.Now.AddHours(12),
            Priority = CacheItemPriority.High,
            SlidingExpiration = TimeSpan.FromMinutes(30)
        };
    }

    public Task<T?> GetOrSetAsync<T>(string cacheKey, Func<Task<T>> valueFactory)
    {
        return _memoryCache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.SetOptions(_cacheOptions);
            return await valueFactory();
        });
    }

    public void Remove(string cacheKey)
    {
        _memoryCache.Remove(cacheKey);
    }

    public T? Set<T>(string cacheKey, T? value)
    {
        return _memoryCache.Set(cacheKey, value, _cacheOptions);
    }

    public (bool, T?) TryGet<T>(string cacheKey)
    {
        var _hasKey = _memoryCache.TryGetValue(cacheKey, out T? value);

        return (_hasKey, value);
    }

    // New interface methods with default implementations
    public T? Set<T>(string cacheKey, T? value, TimeSpan? expiration) => 
        Set(cacheKey, value);

    public T? Set<T>(string cacheKey, T? value, CacheEntryOptions options) => 
        Set(cacheKey, value);

    public Task<CacheResult<T>> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default)
    {
        var (hasValue, value) = TryGet<T>(cacheKey);
        return Task.FromResult(hasValue ? CacheResult<T>.Success(value) : CacheResult<T>.Success());
    }

    public Task<CacheResult> SetAsync<T>(string cacheKey, T? value, CacheEntryOptions? options = null, CancellationToken cancellationToken = default)
    {
        Set(cacheKey, value);
        return Task.FromResult(CacheResult.Success());
    }

    public Task<CacheResult> RemoveAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        Remove(cacheKey);
        return Task.FromResult(CacheResult.Success());
    }

    public Task<T?> GetOrSetAsync<T>(string cacheKey, Func<Task<T>> valueFactory, CacheEntryOptions? options, CancellationToken cancellationToken = default) => 
        GetOrSetAsync(cacheKey, valueFactory);

    public Task<IDictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys, CancellationToken cancellationToken = default) => 
        Task.FromResult<IDictionary<string, T?>>(new Dictionary<string, T?>());

    public Task<CacheResult> SetManyAsync<T>(IDictionary<string, T?> items, CacheEntryOptions? options = null, CancellationToken cancellationToken = default) => 
        Task.FromResult(CacheResult.Success());

    public Task<int> RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default) => 
        Task.FromResult(0);

    public Task<bool> ExistsAsync(string cacheKey, CancellationToken cancellationToken = default) => 
        Task.FromResult(_memoryCache.TryGetValue(cacheKey, out _));

    public Task<CacheStats> GetStatsAsync(CancellationToken cancellationToken = default) => 
        Task.FromResult(new CacheStats());

    public Task<CacheResult> ClearAsync(CancellationToken cancellationToken = default) => 
        Task.FromResult(CacheResult.Success());

    public Task<CacheResult> RefreshAsync(string cacheKey, CancellationToken cancellationToken = default) => 
        Task.FromResult(CacheResult.Success());
}
