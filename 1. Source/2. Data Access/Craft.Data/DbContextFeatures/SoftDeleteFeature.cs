using Craft.Domain;
using Microsoft.EntityFrameworkCore;

namespace Craft.Data.DbContextFeatures;

/// <summary>
/// Feature that enables soft delete behavior for entities implementing ISoftDelete.
/// Automatically applies global query filters to exclude soft-deleted entities and
/// configures filtered indexes to allow reusing unique values after soft deletion.
/// </summary>
public class SoftDeleteFeature : IDbContextFeature
{
    private readonly SoftDeleteConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the SoftDeleteFeature with default configuration.
    /// </summary>
    public SoftDeleteFeature() : this(new SoftDeleteConfiguration())
    {
    }

    /// <summary>
    /// Initializes a new instance of the SoftDeleteFeature with the specified configuration.
    /// </summary>
    /// <param name="configuration">The configuration options.</param>
    public SoftDeleteFeature(SoftDeleteConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Applies global query filter to exclude soft-deleted entities and configures indexes.
    /// </summary>
    public void ConfigureModel(ModelBuilder modelBuilder)
    {
        // Detect the database provider
        var provider = DetectDatabaseProvider(modelBuilder);

        // Apply global query filter for all entities implementing ISoftDelete
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
                continue;

            // Create the filter expression: entity => !entity.IsDeleted
            var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
            var property = System.Linq.Expressions.Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
            var filter = System.Linq.Expressions.Expression.Lambda(
                System.Linq.Expressions.Expression.Not(property),
                parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);

            // Add index on IsDeleted for efficient query filtering
            if (_configuration.CreateIsDeletedIndex)
            {
                var filterExpression = GetFilterExpression(provider);

                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex(nameof(ISoftDelete.IsDeleted))
                    .HasDatabaseName($"IX_{entityType.GetTableName()}_IsDeleted")
                    .HasFilter(filterExpression);
            }

            // Apply soft delete filters to all unique indexes
            if (_configuration.ApplyFiltersToUniqueIndexes)
            {
                ApplySoftDeleteFiltersToUniqueIndexes(modelBuilder, entityType, provider);
            }
        }
    }

    /// <summary>
    /// Applies soft delete filters to all unique indexes for the specified entity type.
    /// This allows reusing unique values (like names, emails, codes) after soft deletion.
    /// </summary>
    private void ApplySoftDeleteFiltersToUniqueIndexes(ModelBuilder modelBuilder,
        Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entityType, DatabaseProvider provider)
    {
        var filterExpression = GetFilterExpression(provider);
        var uniqueIndexes = entityType.GetIndexes()
            .Where(i => i.IsUnique)
            .ToList();

        foreach (var index in uniqueIndexes)
        {
            // Skip if a filter already exists (manually configured)
            if (!string.IsNullOrEmpty(index.GetFilter()))
                continue;

            // Apply filter to exclude soft-deleted records
            index.SetFilter(filterExpression);
        }
    }

    /// <summary>
    /// Detects the database provider from the ModelBuilder.
    /// </summary>
    private DatabaseProvider DetectDatabaseProvider(ModelBuilder modelBuilder)
    {
        // If explicitly configured, use that
        if (_configuration.DatabaseProvider != DatabaseProvider.AutoDetect)
            return _configuration.DatabaseProvider;

        // Try to detect from annotations
        var annotations = modelBuilder.Model.GetAnnotations().ToList();

        // Check for specific provider annotations
        if (annotations.Any(a => a.Name.Contains("Npgsql", StringComparison.OrdinalIgnoreCase)))
            return DatabaseProvider.PostgreSql;

        if (annotations.Any(a => a.Name.Contains("SqlServer", StringComparison.OrdinalIgnoreCase)))
            return DatabaseProvider.SqlServer;

        if (annotations.Any(a => a.Name.Contains("MySql", StringComparison.OrdinalIgnoreCase)))
            return DatabaseProvider.MySql;

        if (annotations.Any(a => a.Name.Contains("Sqlite", StringComparison.OrdinalIgnoreCase)))
            return DatabaseProvider.Sqlite;

        // Default to PostgreSQL
        return DatabaseProvider.PostgreSql;
    }

    /// <summary>
    /// Gets the SQL filter expression for excluding soft-deleted records based on the database provider.
    /// </summary>
    private static string GetFilterExpression(DatabaseProvider provider) => provider switch
    {
        DatabaseProvider.SqlServer => "[IsDeleted] = 0",
        DatabaseProvider.PostgreSql => "\"IsDeleted\" = false",
        DatabaseProvider.MySql => "`IsDeleted` = 0",
        DatabaseProvider.Sqlite => "\"IsDeleted\" = 0",
        _ => "\"IsDeleted\" = false" // Default to PostgreSQL syntax
    };

    /// <summary>
    /// Marks entities for soft deletion instead of hard deletion.
    /// </summary>
    public void OnBeforeSaveChanges(DbContext context, KeyType userId)
    {
        var entries = context.ChangeTracker.Entries<ISoftDelete>()
            .Where(e => e.State == EntityState.Deleted)
            .ToList();

        // Convert hard delete to soft delete
        foreach (var entry in entries)
        {
            entry.State = EntityState.Modified;
            entry.Entity.Delete();
        }
    }
}

