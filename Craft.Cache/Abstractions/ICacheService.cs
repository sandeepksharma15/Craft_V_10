namespace Craft.Cache;

/// <summary>
/// Defines the contract for cache service operations.
/// </summary>
public interface ICacheService
{
    // ========================================
    // Legacy Methods (Backward Compatibility)
    // ========================================

    /// <summary>
    /// Removes an entry from the cache by key.
    /// </summary>
    /// <param name="cacheKey">The cache key to remove.</param>
    [Obsolete("Use RemoveAsync instead. This synchronous method will be removed in a future version.")]
    void Remove(string cacheKey);

    /// <summary>
    /// Sets a value in the cache with default options.
    /// </summary>
    /// <typeparam name="T">The type of value to cache.</typeparam>
    /// <param name="cacheKey">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <returns>The cached value.</returns>
    [Obsolete("Use SetAsync instead. This synchronous method will be removed in a future version.")]
    T? Set<T>(string cacheKey, T? value);

    /// <summary>
    /// Tries to get a value from the cache.
    /// </summary>
    /// <typeparam name="T">The type of value to retrieve.</typeparam>
    /// <param name="cacheKey">The cache key.</param>
    /// <returns>A tuple indicating success and the cached value.</returns>
    [Obsolete("Use GetAsync instead. This synchronous method will be removed in a future version.")]
    (bool, T?) TryGet<T>(string cacheKey);

    /// <summary>
    /// Gets a value from cache or sets it using the value factory if not found.
    /// </summary>
    /// <typeparam name="T">The type of value.</typeparam>
    /// <param name="cacheKey">The cache key.</param>
    /// <param name="valueFactory">The factory to create the value if not cached.</param>
    /// <returns>The cached or created value.</returns>
    Task<T?> GetOrSetAsync<T>(string cacheKey, Func<Task<T>> valueFactory);

    // ========================================
    // Enhanced Methods (New Functionality)
    // ========================================

    /// <summary>
    /// Sets a value in the cache with custom expiration.
    /// </summary>
    /// <typeparam name="T">The type of value to cache.</typeparam>
    /// <param name="cacheKey">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="expiration">The expiration time (null for default).</param>
    /// <returns>The cached value.</returns>
    T? Set<T>(string cacheKey, T? value, TimeSpan? expiration);

    /// <summary>
    /// Sets a value in the cache with custom options.
    /// </summary>
    /// <typeparam name="T">The type of value to cache.</typeparam>
    /// <param name="cacheKey">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="options">The cache entry options.</param>
    /// <returns>The cached value.</returns>
    T? Set<T>(string cacheKey, T? value, CacheEntryOptions options);

    // ========================================
    // Async Operations
    // ========================================

    /// <summary>
    /// Gets a value from the cache asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of value to retrieve.</typeparam>
    /// <param name="cacheKey">The cache key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing the cache result with the value.</returns>
    Task<CacheResult<T>> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a value in the cache asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of value to cache.</typeparam>
    /// <param name="cacheKey">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="options">The cache entry options (null for default).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing the cache operation result.</returns>
    Task<CacheResult> SetAsync<T>(string cacheKey, T? value, CacheEntryOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an entry from the cache asynchronously.
    /// </summary>
    /// <param name="cacheKey">The cache key to remove.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing the cache operation result.</returns>
    Task<CacheResult> RemoveAsync(string cacheKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a value from cache or sets it using the value factory if not found, with custom options.
    /// </summary>
    /// <typeparam name="T">The type of value.</typeparam>
    /// <param name="cacheKey">The cache key.</param>
    /// <param name="valueFactory">The factory to create the value if not cached.</param>
    /// <param name="options">The cache entry options (null for default).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The cached or created value.</returns>
    Task<T?> GetOrSetAsync<T>(string cacheKey, Func<Task<T>> valueFactory, CacheEntryOptions? options, CancellationToken cancellationToken = default);

    // ========================================
    // Bulk Operations
    // ========================================

    /// <summary>
    /// Gets multiple values from the cache asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of values to retrieve.</typeparam>
    /// <param name="keys">The cache keys to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing a dictionary of found values.</returns>
    Task<IDictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets multiple values in the cache asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of values to cache.</typeparam>
    /// <param name="items">The dictionary of key-value pairs to cache.</param>
    /// <param name="options">The cache entry options (null for default).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing the cache operation result.</returns>
    Task<CacheResult> SetManyAsync<T>(IDictionary<string, T?> items, CacheEntryOptions? options = null, CancellationToken cancellationToken = default);

    // ========================================
    // Pattern Operations
    // ========================================

    /// <summary>
    /// Removes all entries matching the specified pattern asynchronously.
    /// </summary>
    /// <param name="pattern">The pattern to match (supports wildcards like "user:*").</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing the number of entries removed.</returns>
    Task<int> RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);

    // ========================================
    // Cache Information
    // ========================================

    /// <summary>
    /// Checks if a key exists in the cache asynchronously.
    /// </summary>
    /// <param name="cacheKey">The cache key to check.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task indicating whether the key exists.</returns>
    Task<bool> ExistsAsync(string cacheKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets cache statistics asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing the cache statistics.</returns>
    Task<CacheStats> GetStatsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all entries from the cache asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing the cache operation result.</returns>
    Task<CacheResult> ClearAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes a cache entry to reset its expiration time.
    /// </summary>
    /// <param name="cacheKey">The cache key to refresh.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing the cache operation result.</returns>
    Task<CacheResult> RefreshAsync(string cacheKey, CancellationToken cancellationToken = default);
}
