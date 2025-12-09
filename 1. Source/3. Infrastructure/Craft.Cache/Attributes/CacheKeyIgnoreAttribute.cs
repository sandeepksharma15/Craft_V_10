namespace Craft.Cache;

/// <summary>
/// Marks a method parameter to be excluded from cache key generation.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public sealed class CacheKeyIgnoreAttribute : Attribute
{
}
