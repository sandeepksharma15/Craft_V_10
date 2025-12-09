using Microsoft.Extensions.Caching.Memory;

namespace Craft.Cache;

/// <summary>
/// Options for configuring cache entry behavior.
/// </summary>
public class CacheEntryOptions
{
    /// <summary>
    /// Gets or sets the absolute expiration time for the cache entry.
    /// </summary>
    public DateTimeOffset? AbsoluteExpiration { get; set; }

    /// <summary>
    /// Gets or sets the absolute expiration time relative to now for the cache entry.
    /// </summary>
    public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }

    /// <summary>
    /// Gets or sets the sliding expiration time for the cache entry.
    /// </summary>
    public TimeSpan? SlidingExpiration { get; set; }

    /// <summary>
    /// Gets or sets the priority of the cache entry.
    /// </summary>
    public CacheItemPriority Priority { get; set; } = CacheItemPriority.Normal;

    /// <summary>
    /// Gets or sets the size of the cache entry for size-limited caches.
    /// </summary>
    public long? Size { get; set; }

    /// <summary>
    /// Creates a default CacheEntryOptions with common settings.
    /// </summary>
    public static CacheEntryOptions DefaultOptions => new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
        SlidingExpiration = TimeSpan.FromMinutes(30),
        Priority = CacheItemPriority.Normal
    };

    /// <summary>
    /// Creates CacheEntryOptions with a specific expiration time.
    /// </summary>
    public static CacheEntryOptions WithExpiration(TimeSpan expiration) => new()
    {
        AbsoluteExpirationRelativeToNow = expiration,
        Priority = CacheItemPriority.Normal
    };

    /// <summary>
    /// Creates CacheEntryOptions with sliding expiration.
    /// </summary>
    public static CacheEntryOptions WithSlidingExpiration(TimeSpan slidingExpiration) => new()
    {
        SlidingExpiration = slidingExpiration,
        Priority = CacheItemPriority.Normal
    };

    /// <summary>
    /// Converts to MemoryCacheEntryOptions for IMemoryCache.
    /// </summary>
    public MemoryCacheEntryOptions ToMemoryCacheEntryOptions()
    {
        var options = new MemoryCacheEntryOptions
        {
            Priority = Priority
        };

        if (AbsoluteExpiration.HasValue)
            options.AbsoluteExpiration = AbsoluteExpiration;

        if (AbsoluteExpirationRelativeToNow.HasValue)
            options.AbsoluteExpirationRelativeToNow = AbsoluteExpirationRelativeToNow;

        if (SlidingExpiration.HasValue)
            options.SlidingExpiration = SlidingExpiration;

        if (Size.HasValue)
            options.Size = Size;

        return options;
    }
}
