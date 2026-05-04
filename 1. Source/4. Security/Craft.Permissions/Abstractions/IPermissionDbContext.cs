using Microsoft.EntityFrameworkCore;

namespace Craft.Permissions;

/// <summary>
/// Minimal DbContext contract that consuming apps must implement to enable permission persistence.
/// Add this interface alongside your application's <c>DbContext</c> and expose the
/// <see cref="RolePermissions"/> set.
/// </summary>
public interface IPermissionDbContext
{
    DbSet<RolePermission> RolePermissions { get; }
}
