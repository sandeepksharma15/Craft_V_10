namespace Craft.Cache;

/// <summary>
/// Defines the contract for cache providers.
/// </summary>
public interface ICacheProvider
{
    /// <summary>
    /// Gets the name of the cache provider.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets a value indicating whether the provider is configured and ready to use.
    /// </summary>
    bool IsConfigured();

    /// <summary>
    /// Gets a value from the cache.
    /// </summary>
    Task<CacheResult<T>> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a value in the cache.
    /// </summary>
    Task<CacheResult> SetAsync<T>(string key, T? value, CacheEntryOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a value from the cache.
    /// </summary>
    Task<CacheResult> RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a key exists in the cache.
    /// </summary>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets multiple values from the cache.
    /// </summary>
    Task<IDictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets multiple values in the cache.
    /// </summary>
    Task<CacheResult> SetManyAsync<T>(IDictionary<string, T?> items, CacheEntryOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes entries matching a pattern.
    /// </summary>
    Task<int> RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets cache statistics.
    /// </summary>
    Task<CacheStats> GetStatsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all entries from the cache.
    /// </summary>
    Task<CacheResult> ClearAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes a cache entry.
    /// </summary>
    Task<CacheResult> RefreshAsync(string key, CancellationToken cancellationToken = default);
}
