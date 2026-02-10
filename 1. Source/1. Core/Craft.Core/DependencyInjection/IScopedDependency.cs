namespace Craft.Core;

/// <summary>
/// Marker interface for services that should be registered with Scoped lifetime.
/// <para>
/// Scoped services are created once per request (or scope). Use this for services that
/// need to maintain state within a single request, such as DbContext or request-specific caches.
/// </para>
/// </summary>
/// <example>
/// <code>
/// public class OrderService : IOrderService, IScopedDependency
/// {
///     // Service implementation
/// }
/// </code>
/// </example>
public interface IScopedDependency;
