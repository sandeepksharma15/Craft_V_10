namespace Craft.Cache;

/// <summary>
/// Indicates that a method should invalidate cached entries.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class CacheInvalidateAttribute : Attribute
{
    /// <summary>
    /// Gets the cache keys or patterns to invalidate.
    /// </summary>
    public string[] Keys { get; init; } = [];

    /// <summary>
    /// Gets whether the keys are patterns (support wildcards). Default is false.
    /// </summary>
    public bool IsPattern { get; init; }

    /// <summary>
    /// Gets the entity type to invalidate all entries for.
    /// </summary>
    public Type? EntityType { get; init; }

    /// <summary>
    /// Gets when to invalidate. Default is After.
    /// </summary>
    public CacheInvalidationTiming Timing { get; init; } = CacheInvalidationTiming.After;

    /// <summary>
    /// Gets whether to invalidate only on success. Default is true.
    /// </summary>
    public bool OnlyOnSuccess { get; init; } = true;
}

/// <summary>
/// Defines when cache invalidation should occur.
/// </summary>
public enum CacheInvalidationTiming
{
    /// <summary>
    /// Invalidate before method execution.
    /// </summary>
    Before,

    /// <summary>
    /// Invalidate after method execution.
    /// </summary>
    After,

    /// <summary>
    /// Invalidate both before and after.
    /// </summary>
    Both
}
