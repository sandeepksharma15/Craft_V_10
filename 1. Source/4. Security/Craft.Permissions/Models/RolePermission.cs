using Craft.Domain;

namespace Craft.Permissions;

/// <summary>
/// Persisted entity that links a role to a single integer permission code.
/// One row per role-permission pair; add <see cref="IPermissionDbContext"/> to
/// your application's DbContext to include this table.
/// </summary>
public class RolePermission : BaseEntity, IRolePermission
{
    /// <inheritdoc />
    public KeyType RoleId { get; set; }

    /// <inheritdoc />
    public int PermissionCode { get; set; }
}
