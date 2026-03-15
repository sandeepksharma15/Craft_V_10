namespace Craft.Core;

/// <summary>
/// Marker interface for services that should be registered with Transient lifetime.
/// <para>
/// Transient services are created each time they are requested. Use this for lightweight,
/// stateless services where a new instance is preferred for each usage.
/// </para>
/// </summary>
/// <example>
/// <code>
/// public class EmailBuilder : IEmailBuilder, ITransientDependency
/// {
///     // Service implementation
/// }
/// </code>
/// </example>
public interface ITransientDependency;
