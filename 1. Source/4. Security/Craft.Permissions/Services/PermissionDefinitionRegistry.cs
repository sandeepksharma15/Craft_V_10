using System.Collections.Concurrent;

namespace Craft.Permissions;

/// <summary>
/// Singleton implementation of <see cref="IPermissionDefinitionRegistry"/>.
/// Enforces uniqueness of permission codes at registration time — duplicate codes
/// throw <see cref="InvalidOperationException"/> so misconfiguration is caught on startup.
/// </summary>
public sealed class PermissionDefinitionRegistry : IPermissionDefinitionRegistry
{
    private readonly ConcurrentDictionary<int, PermissionDefinition> _definitions = new();

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    /// Thrown when a <see cref="PermissionDefinition"/> with the same
    /// <see cref="PermissionDefinition.Code"/> has already been registered.
    /// </exception>
    public void Register(PermissionDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (!_definitions.TryAdd(definition.Code, definition))
        {
            var existing = _definitions[definition.Code];
            throw new InvalidOperationException(
                $"Permission code {definition.Code} is already registered as '{existing.Name}'. " +
                $"Each permission code must be unique across the application.");
        }
    }

    /// <inheritdoc />
    public IReadOnlyCollection<PermissionDefinition> GetAll()
        => [.. _definitions.Values.OrderBy(d => d.Group).ThenBy(d => d.Name)];

    /// <inheritdoc />
    public PermissionDefinition? GetByCode(int code)
        => _definitions.GetValueOrDefault(code);
}
