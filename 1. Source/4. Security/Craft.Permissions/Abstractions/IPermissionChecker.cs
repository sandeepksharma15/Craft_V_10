namespace Craft.Permissions;

/// <summary>
/// Provides methods for checking whether the current user holds a given permission.
/// Implemented by <see cref="Services.PermissionSessionCache"/> which is registered as Scoped.
/// </summary>
public interface IPermissionChecker
{
    /// <summary>Returns <see langword="true"/> when the current session holds <paramref name="permissionCode"/>.</summary>
    bool HasPermission(int permissionCode);

    /// <summary>Returns <see langword="true"/> when the current session holds at least one of the supplied codes.</summary>
    bool HasAnyPermission(params int[] permissionCodes);

    /// <summary>Returns <see langword="true"/> when the current session holds all of the supplied codes.</summary>
    bool HasAllPermissions(params int[] permissionCodes);
}
