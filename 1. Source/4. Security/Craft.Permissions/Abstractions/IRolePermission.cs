using Craft.Domain;

namespace Craft.Permissions;

/// <summary>
/// Represents the DB entity that links a role to a permission code.
/// </summary>
public interface IRolePermission : IEntity
{
    /// <summary>The role this permission is assigned to.</summary>
    KeyType RoleId { get; set; }

    /// <summary>The integer permission code assigned to the role.</summary>
    int PermissionCode { get; set; }
}
