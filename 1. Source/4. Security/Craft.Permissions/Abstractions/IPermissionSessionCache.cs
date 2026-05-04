namespace Craft.Permissions;

/// <summary>
/// Manages the in-memory permission set for the current user's session.
/// Implemented by <see cref="Services.PermissionSessionCache"/> which is registered as Scoped.
/// </summary>
public interface IPermissionSessionCache
{
    /// <summary>Replaces the cached permission set with the supplied codes.</summary>
    void SetPermissions(IEnumerable<int> permissionCodes);

    /// <summary>Clears all cached permissions (e.g. on logout).</summary>
    void Clear();

    /// <summary>Returns a read-only snapshot of the currently cached permission codes.</summary>
    IReadOnlyCollection<int> GetPermissions();
}
