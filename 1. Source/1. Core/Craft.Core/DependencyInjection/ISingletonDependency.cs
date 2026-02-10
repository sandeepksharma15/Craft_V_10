namespace Craft.Core;

/// <summary>
/// Marker interface for services that should be registered with Singleton lifetime.
/// <para>
/// Singleton services are created once and shared across all requests and scopes.
/// Use this for stateless services, configuration providers, or caches that need to persist
/// for the application lifetime.
/// </para>
/// <para>
/// ⚠️ Warning: Singleton services must be thread-safe and should not hold scoped dependencies.
/// </para>
/// </summary>
/// <example>
/// <code>
/// public class CacheService : ICacheService, ISingletonDependency
/// {
///     // Thread-safe service implementation
/// }
/// </code>
/// </example>
public interface ISingletonDependency;
