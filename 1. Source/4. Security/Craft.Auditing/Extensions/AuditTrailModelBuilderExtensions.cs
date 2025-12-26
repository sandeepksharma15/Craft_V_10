using Microsoft.EntityFrameworkCore;

namespace Craft.Auditing;

public static class AuditTrailModelBuilderExtensions
{
    /// <summary>
    /// Configures the AuditTrail entity with indexes for optimal query performance.
    /// Data annotations on the AuditTrail entity handle column configurations in a database-agnostic way.
    /// </summary>
    public static ModelBuilder ConfigureAuditTrail(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.Entity<AuditTrail>(entity =>
        {
            // Only configure indexes here - data annotations handle the rest
            entity.HasIndex(e => e.TableName);
            entity.HasIndex(e => e.DateTimeUTC);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ChangeType);
            entity.HasIndex(e => e.IsArchived);
            entity.HasIndex(e => e.ArchiveAfter);
            entity.HasIndex(e => new { e.TableName, e.DateTimeUTC });
            entity.HasIndex(e => new { e.UserId, e.DateTimeUTC });
        });

        return modelBuilder;
    }
}
