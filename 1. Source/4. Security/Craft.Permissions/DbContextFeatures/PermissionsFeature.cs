using Craft.Data;
using Microsoft.EntityFrameworkCore;

namespace Craft.Permissions.DbContextFeatures;

/// <summary>
/// Feature that registers and configures the <see cref="RolePermission"/> entity for EF Core.
/// Applies the <c>ID_RolePermissions</c> table name, a unique composite index on
/// <c>(RoleId, PermissionCode)</c>, and a supporting index on <c>RoleId</c> for efficient
/// per-role permission lookups.
/// </summary>
public class PermissionsFeature : IDbContextFeature, IDbSetProvider
{
    /// <summary>
    /// Indicates this feature contributes a <see cref="RolePermission"/> DbSet.
    /// </summary>
    public bool RequiresDbSet => true;

    /// <summary>
    /// The entity type contributed by this feature.
    /// </summary>
    public Type EntityType => typeof(RolePermission);

    /// <summary>
    /// Configures the <see cref="RolePermission"/> table name and indexes.
    /// </summary>
    public void ConfigureModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("ID_RolePermissions");

            // Enforce uniqueness at the DB level; prevents duplicate assignments surviving concurrent writes
            entity.HasIndex(e => new { e.RoleId, e.PermissionCode })
                  .IsUnique()
                  .HasDatabaseName("IX_ID_RolePermissions_RoleId_PermissionCode");

            // Supporting index for fast per-role lookups
            entity.HasIndex(e => e.RoleId)
                  .HasDatabaseName("IX_ID_RolePermissions_RoleId");
        });
    }
}
