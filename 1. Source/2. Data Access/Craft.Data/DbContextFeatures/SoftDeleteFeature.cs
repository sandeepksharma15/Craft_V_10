using Craft.Domain;
using Microsoft.EntityFrameworkCore;

namespace Craft.Data.DbContextFeatures;

/// <summary>
/// Feature that enables soft delete behavior for entities implementing ISoftDelete.
/// Automatically applies global query filters to exclude soft-deleted entities.
/// </summary>
public class SoftDeleteFeature : IDbContextFeature
{
    /// <summary>
    /// Applies global query filter to exclude soft-deleted entities.
    /// </summary>
    public void ConfigureModel(ModelBuilder modelBuilder)
    {
        // Apply global query filter for all entities implementing ISoftDelete
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                // Create the filter expression: entity => !entity.IsDeleted
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
                var filter = System.Linq.Expressions.Expression.Lambda(
                    System.Linq.Expressions.Expression.Not(property),
                    parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }
    }

    /// <summary>
    /// Marks entities for soft deletion instead of hard deletion.
    /// </summary>
    public void OnBeforeSaveChanges(DbContext context, KeyType userId)
    {
        var entries = context.ChangeTracker.Entries<ISoftDelete>()
            .Where(e => e.State == EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            // Convert hard delete to soft delete
            entry.State = EntityState.Modified;
            entry.Entity.Delete();
        }
    }
}

