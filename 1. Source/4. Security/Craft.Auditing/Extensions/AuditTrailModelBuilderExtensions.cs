using Microsoft.EntityFrameworkCore;

namespace Craft.Auditing;

public static class AuditTrailModelBuilderExtensions
{
    public static ModelBuilder ConfigureAuditTrail(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.Entity<AuditTrail>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TableName)
                .HasMaxLength(AuditTrail.MaxTableNameLength)
                .IsRequired();

            entity.Property(e => e.KeyValues)
                .HasMaxLength(AuditTrail.MaxKeyValuesLength);

            entity.Property(e => e.ChangedColumns)
                .HasMaxLength(AuditTrail.MaxChangedColumnsLength);

            entity.Property(e => e.DateTimeUTC)
                .IsRequired();

            entity.Property(e => e.ChangeType)
                .IsRequired();

            entity.Property(e => e.UserId)
                .IsRequired();

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
