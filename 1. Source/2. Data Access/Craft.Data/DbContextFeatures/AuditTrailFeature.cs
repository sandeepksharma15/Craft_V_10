using Craft.Auditing;
using Microsoft.EntityFrameworkCore;

namespace Craft.Data.DbContextFeatures;

/// <summary>
/// Feature that enables automatic audit trail generation for entities marked with [Audit] attribute.
/// Automatically registers the AuditTrail DbSet and captures entity changes before save.
/// </summary>
public class AuditTrailFeature : IDbContextFeature, IDbSetProvider
{
    // Cache auditable types for performance - initialized once and reused
    private static readonly Lazy<HashSet<Type>> _auditableTypes = new(() =>
    {
        var typeNames = AuditingHelpers.GetAuditableBaseEntityTypes();
        return new HashSet<Type>(
            AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeNames.Contains(t.Name))
        );
    });

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
    /// <remarks>
    /// This feature focuses solely on audit trail creation.
    /// For concurrency stamp management, use <see cref="ConcurrencyFeature"/>.
    /// For version tracking, use <see cref="VersionTrackingFeature"/>.
    /// Performance optimized: Uses cached type HashSet for O(1) lookups instead of string comparisons.
    /// </remarks>
    public void OnBeforeSaveChanges(DbContext context, KeyType userId)
    {
        // Get all changed entries for auditable entities using cached type set
        var entries = context.ChangeTracker.Entries()
            .Where(e => e.Entity is not AuditTrail) // Prevent self-auditing
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Where(e => _auditableTypes.Value.Contains(e.Entity.GetType()))
            .ToList();

        // Create audit trail entries
        foreach (var entry in entries)
        {
            var auditTrail = new AuditTrail(entry, userId);
            context.Set<AuditTrail>().Add(auditTrail);
        }
    }
}

