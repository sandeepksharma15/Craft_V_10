namespace Craft.Permissions;

/// <summary>
/// Scoped service that stores the current user's permission codes for the lifetime of a
/// Blazor server circuit (one per user session). Implements both
/// <see cref="IPermissionSessionCache"/> for loading/clearing the set and
/// <see cref="IPermissionChecker"/> for fast O(1) membership checks.
/// </summary>
public sealed class PermissionSessionCache : IPermissionSessionCache, IPermissionChecker
{
    private HashSet<int> _permissions = [];

    /// <inheritdoc />
    public void SetPermissions(IEnumerable<int> permissionCodes)
    {
        ArgumentNullException.ThrowIfNull(permissionCodes);
        _permissions = [.. permissionCodes];
    }

    /// <inheritdoc />
    public void Clear() => _permissions = [];

    /// <inheritdoc />
    public IReadOnlyCollection<int> GetPermissions() => _permissions;

    /// <inheritdoc />
    public bool HasPermission(int permissionCode) => _permissions.Contains(permissionCode);

    /// <inheritdoc />
    public bool HasAnyPermission(params int[] permissionCodes)
    {
        ArgumentNullException.ThrowIfNull(permissionCodes);
        return permissionCodes.Any(_permissions.Contains);
    }

    /// <inheritdoc />
    public bool HasAllPermissions(params int[] permissionCodes)
    {
        ArgumentNullException.ThrowIfNull(permissionCodes);
        return permissionCodes.All(_permissions.Contains);
    }
}
