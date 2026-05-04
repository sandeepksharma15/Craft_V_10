using Craft.Core;
using Craft.Repositories;

namespace Craft.Permissions;

/// <summary>
/// Server-side data-access contract for role-permission assignments.
/// </summary>
public interface IRolePermissionRepository : IRepository
{
    /// <summary>Returns all permission codes assigned to the given role.</summary>
    Task<IReadOnlyCollection<int>> GetPermissionCodesForRoleAsync(KeyType roleId, CancellationToken cancellationToken = default);

    /// <summary>Returns the union of permission codes for all supplied role IDs.</summary>
    Task<IReadOnlyCollection<int>> GetPermissionCodesForRolesAsync(IEnumerable<KeyType> roleIds, CancellationToken cancellationToken = default);

    /// <summary>Assigns a permission code to a role. No-op if already assigned.</summary>
    Task AssignPermissionAsync(KeyType roleId, int permissionCode, CancellationToken cancellationToken = default);

    /// <summary>Revokes a permission code from a role. No-op if not assigned.</summary>
    Task RevokePermissionAsync(KeyType roleId, int permissionCode, CancellationToken cancellationToken = default);

    /// <summary>Replaces the full set of permissions for a role with the provided codes.</summary>
    Task SetPermissionsForRoleAsync(KeyType roleId, IEnumerable<int> permissionCodes, CancellationToken cancellationToken = default);

    /// <summary>Saves pending changes to the underlying store.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
