namespace Craft.Cache;

public interface ICacheService
{
    void Remove(string cacheKey);

    T? Set<T>(string cacheKey, T? value);

    (bool, T?) TryGet<T>(string cacheKey);

    Task<T?> GetOrSetAsync<T>(string cacheKey, Func<Task<T>> valueFactory);
}
