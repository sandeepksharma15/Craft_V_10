using Craft.Domain;
using Microsoft.EntityFrameworkCore;

namespace Craft.Data.DbContextFeatures;

/// <summary>
/// Feature that enables automatic concurrency stamp management for entities implementing IHasConcurrency.
/// Updates concurrency stamp on every modification to prevent concurrent update conflicts.
/// </summary>
public class ConcurrencyFeature : IDbContextFeature
{
    /// <summary>
    /// Automatically updates concurrency stamp on entity modifications.
    /// </summary>
    public void OnBeforeSaveChanges(DbContext context, KeyType userId)
    {
        var entries = context.ChangeTracker.Entries<IHasConcurrency>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified)
            .ToList();

        foreach (var entry in entries)
        {
            // Generate new concurrency stamp
            entry.Entity.SetConcurrencyStamp(Guid.NewGuid().ToString());
        }
    }

    /// <summary>
    /// Configures concurrency token for entities implementing IHasConcurrency.
    /// </summary>
    public void ConfigureModel(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IHasConcurrency).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(IHasConcurrency.ConcurrencyStamp))
                    .IsConcurrencyToken();
            }
        }
    }
}

