using Craft.Auditing;
using Craft.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Craft.Data.DbContextFeatures;

/// <summary>
/// Feature that enables automatic audit trail generation for entities marked with [Audit] attribute.
/// Automatically registers the AuditTrail DbSet and captures entity changes before save.
/// </summary>
public class AuditTrailFeature : IDbContextFeature, IDbSetProvider
{
    /// <summary>
    /// Indicates this feature requires a DbSet for AuditTrail.
    /// </summary>
    public bool RequiresDbSet => true;

    /// <summary>
    /// Gets the entity type for the AuditTrail DbSet.
    /// </summary>
    public Type EntityType => typeof(AuditTrail);

    /// <summary>
    /// Configure the AuditTrail entity in the model.
    /// </summary>
    public void ConfigureModel(ModelBuilder modelBuilder)
    {
        // Entity configuration is handled by the AuditTrail class attributes
        // Additional configuration can be added here if needed
        modelBuilder.Entity<AuditTrail>(entity =>
        {
            entity.HasIndex(e => e.TableName);
            entity.HasIndex(e => e.DateTimeUTC);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ChangeType);
        });
    }

    /// <summary>
    /// Creates audit trail entries for all auditable entities before save.
    /// </summary>
    public void OnBeforeSaveChanges(DbContext context, KeyType userId)
    {
        // Get auditable entity types
        var auditableTypes = AuditingHelpers.GetAuditableBaseEntityTypes();

        // Get all changed entries
        var entries = context.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Where(e => auditableTypes.Contains(e.Entity.GetType().Name))
            .ToList();

        // Create audit trail entries
        foreach (var entry in entries)
        {
            var auditTrail = new AuditTrail(entry, userId);

            // Add audit trail to context
            context.Set<AuditTrail>().Add(auditTrail);
        }
    }
}
