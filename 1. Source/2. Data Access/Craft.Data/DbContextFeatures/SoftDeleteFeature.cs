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
    /// Applies global query filter to exclude soft-deleted entities and configures indexes.
    /// </summary>
    public void ConfigureModel(ModelBuilder modelBuilder)
    {
        // Detect database provider from the model
        var databaseProvider = modelBuilder.Model.GetDefaultSchema() != null ? "PostgreSQL" : "Unknown";
        var providerName = modelBuilder.Model.GetPropertyAccessMode().ToString();

        // Try to determine from connection string or provider name
        // Default to PostgreSQL for safety
        bool isPostgreSQL = true;
        bool isSqlServer = false;

        // Check the provider name from annotations
        var annotation = modelBuilder.Model.GetAnnotations()
            .FirstOrDefault(a => a.Name.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) || 
                               a.Name.Contains("SqlServer", StringComparison.OrdinalIgnoreCase));

        if (annotation != null)
        {
            isPostgreSQL = annotation.Name.Contains("Npgsql", StringComparison.OrdinalIgnoreCase);
            isSqlServer = annotation.Name.Contains("SqlServer", StringComparison.OrdinalIgnoreCase);
        }

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

                // Add index on IsDeleted for efficient query filtering
                // Using filtered index with database-specific syntax
                var filterExpression = isPostgreSQL 
                    ? $"\"{nameof(ISoftDelete.IsDeleted)}\" = false"
                    : $"[{nameof(ISoftDelete.IsDeleted)}] = 0";

                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex(nameof(ISoftDelete.IsDeleted))
                    .HasDatabaseName($"IX_{entityType.GetTableName()}_IsDeleted")
                    .HasFilter(filterExpression);
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

