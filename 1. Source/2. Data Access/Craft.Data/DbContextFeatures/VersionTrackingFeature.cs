using Craft.Domain;
using Microsoft.EntityFrameworkCore;

namespace Craft.Data.DbContextFeatures;

/// <summary>
/// Feature that enables automatic version tracking for entities implementing IHasVersion.
/// Increments version numbers on entity modifications.
/// </summary>
public class VersionTrackingFeature : IDbContextFeature
{
    /// <summary>
    /// Automatically sets initial version and increments version on modifications.
    /// </summary>
    public void OnBeforeSaveChanges(DbContext context, KeyType userId)
    {
        var entries = context.ChangeTracker.Entries<IHasVersion>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified)
            .ToList();

        foreach (var entry in entries)
        {
            // Set initial version for new entities
            if (entry.State == EntityState.Added)
                entry.Entity.SetVersion(1);
            else if (entry.State == EntityState.Modified)
            {
                // Increment version for modified entities
                entry.Entity.IncrementVersion();
            }
        }
    }
}

