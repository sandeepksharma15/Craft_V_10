namespace Craft.Utilities.CacheService;

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
}
