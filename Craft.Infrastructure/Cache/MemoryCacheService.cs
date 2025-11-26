using Microsoft.Extensions.Caching.Memory;

namespace Craft.Infrastructure.Cache;

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
}
