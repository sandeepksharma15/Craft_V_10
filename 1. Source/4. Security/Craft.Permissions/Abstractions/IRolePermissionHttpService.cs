using Craft.Core;

namespace Craft.Permissions;

/// <summary>
/// Client-side HTTP service for managing role-permission assignments via the API.
/// </summary>
public interface IRolePermissionHttpService
{
    /// <summary>Returns all permission codes assigned to the given role.</summary>
    Task<ServiceResult<int[]>> GetPermissionsForRoleAsync(KeyType roleId, CancellationToken cancellationToken = default);

    /// <summary>Assigns a single permission code to a role.</summary>
    Task<ServiceResult<bool>> AssignPermissionAsync(KeyType roleId, int permissionCode, CancellationToken cancellationToken = default);

    /// <summary>Revokes a single permission code from a role.</summary>
    Task<ServiceResult<bool>> RevokePermissionAsync(KeyType roleId, int permissionCode, CancellationToken cancellationToken = default);

    /// <summary>Replaces the full permission set for a role with the supplied codes.</summary>
    Task<ServiceResult<bool>> SetPermissionsForRoleAsync(KeyType roleId, IEnumerable<int> permissionCodes, CancellationToken cancellationToken = default);
}
