namespace Craft.Core;

/// <summary>
/// Marker interface for all DI-registered services in Craft.Core.
/// <para>
/// Implement this interface on any service intended for dependency injection registration.
/// This standardizes service discovery and registration across the solution.
/// </para>
/// <example>
/// public class MyService : IService { }
/// </example>
/// </summary>
public interface IService
{
    // Marker interface, no members.
}
