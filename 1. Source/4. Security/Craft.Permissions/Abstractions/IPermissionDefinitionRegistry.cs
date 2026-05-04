namespace Craft.Permissions;

/// <summary>
/// Singleton registry that holds all <see cref="PermissionDefinition"/> entries registered
/// by the consuming application at startup. Used by the management UI to display
/// friendly names and groups when assigning permissions to roles.
/// </summary>
public interface IPermissionDefinitionRegistry
{
    /// <summary>
    /// Registers a permission definition. Throws <see cref="InvalidOperationException"/>
    /// if a definition with the same <see cref="PermissionDefinition.Code"/> is already registered.
    /// </summary>
    void Register(PermissionDefinition definition);

    /// <summary>Returns all registered permission definitions.</summary>
    IReadOnlyCollection<PermissionDefinition> GetAll();

    /// <summary>Returns the definition for the given code, or <see langword="null"/> if not found.</summary>
    PermissionDefinition? GetByCode(int code);
}
