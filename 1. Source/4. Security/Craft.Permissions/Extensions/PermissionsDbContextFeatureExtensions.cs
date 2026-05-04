using Craft.Data;
using Craft.Permissions.DbContextFeatures;

namespace Craft.Permissions;

/// <summary>
/// Extension methods for fluent registration of the Permissions feature on a <see cref="DbContextFeatureCollection"/>.
/// </summary>
public static class PermissionsDbContextFeatureExtensions
{
    /// <summary>
    /// Adds role-permission persistence support to the DbContext.
    /// Registers the <c>ID_RolePermissions</c> table with unique composite index on
    /// <c>(RoleId, PermissionCode)</c> and a supporting index on <c>RoleId</c>.
    /// </summary>
    /// <example>
    /// <code>
    /// Features
    ///     .AddAuditTrail()
    ///     .AddPermissions();
    /// </code>
    /// </example>
    public static DbContextFeatureCollection AddPermissions(this DbContextFeatureCollection features)
        => features.AddFeature<PermissionsFeature>();
}
