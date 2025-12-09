namespace Craft.Cache;

/// <summary>
/// Indicates that a method's result should be cached.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class CacheableAttribute : Attribute
{
    /// <summary>
    /// Gets the cache key prefix. If null, uses method signature.
    /// </summary>
    public string? KeyPrefix { get; init; }

    /// <summary>
    /// Gets the expiration time in seconds. Default is 3600 (1 hour).
    /// </summary>
    public int ExpirationSeconds { get; init; } = 3600;

    /// <summary>
    /// Gets the sliding expiration time in seconds. If null, no sliding expiration.
    /// </summary>
    public int? SlidingExpirationSeconds { get; init; }

    /// <summary>
    /// Gets whether to include method arguments in the cache key. Default is true.
    /// </summary>
    public bool IncludeArguments { get; init; } = true;

    /// <summary>
    /// Gets the specific argument names to include in cache key. If null, includes all.
    /// </summary>
    public string[]? KeyArguments { get; init; }

    /// <summary>
    /// Gets the condition that must be true for caching to occur (property name on return value).
    /// </summary>
    public string? Condition { get; init; }
}
