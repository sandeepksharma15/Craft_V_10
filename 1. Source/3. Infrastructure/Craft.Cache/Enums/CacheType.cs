namespace Craft.Cache;

/// <summary>
/// Represents the type of cache provider.
/// </summary>
public enum CacheType
{
    /// <summary>
    /// In-memory cache provider.
    /// </summary>
    Memory,

    /// <summary>
    /// Redis distributed cache provider.
    /// </summary>
    Redis,

    /// <summary>
    /// Hybrid cache combining L1 (memory) and L2 (distributed) caching.
    /// </summary>
    Hybrid,

    /// <summary>
    /// Null cache provider (no-op, for testing).
    /// </summary>
    Null
}

