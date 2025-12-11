using Craft.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Craft.Auditing.Examples;

/// <summary>
/// Examples demonstrating audit trail retention policy and archival management.
/// </summary>
public static class AuditTrailRetentionExample
{
    /// <summary>
    /// Example 1: Configure global default retention policy.
    /// All new audit trail entries will automatically have ArchiveAfter set.
    /// </summary>
    public static void ConfigureDefaultRetentionPolicy()
    {
        // Set default retention to 90 days
        AuditTrail.DefaultRetentionDays = 90;

        // All new audit trails will have ArchiveAfter = DateTimeUTC + 90 days
        // var audit = AuditTrail.Create(entityEntry, userId);
        // audit.ArchiveAfter will be automatically set
    }

    /// <summary>
    /// Example 2: Create audit trail with custom retention for specific entry.
    /// </summary>
    public static void CreateWithCustomRetention(EntityEntry entityEntry, KeyType userId)
    {
        // Create audit trail
        var audit = AuditTrail.Create(entityEntry, userId);

        // Set custom retention for this specific entry (30 days)
        audit.SetRetentionPolicy(30);

        // ArchiveAfter is now set to DateTimeUTC + 30 days
    }

    /// <summary>
    /// Example 3: Create audit trail without retention policy.
    /// </summary>
    public static void CreateWithoutRetention(EntityEntry entityEntry, KeyType userId)
    {
        // Disable default retention temporarily
        var previousRetention = AuditTrail.DefaultRetentionDays;
        AuditTrail.DefaultRetentionDays = null;

        try
        {
            // Create audit trail without retention
            var audit = AuditTrail.Create(entityEntry, userId);
            // audit.ArchiveAfter will be null
        }
        finally
        {
            // Restore previous retention setting
            AuditTrail.DefaultRetentionDays = previousRetention;
        }
    }

    /// <summary>
    /// Example 4: Different retention policies for different entity types.
    /// </summary>
    public static class EntitySpecificRetention
    {
        // Critical entities - keep for 7 years
        public static AuditTrail CreateCriticalEntityAudit(EntityEntry entityEntry, KeyType userId)
        {
            var audit = AuditTrail.Create(entityEntry, userId);
            audit.SetRetentionPolicy(365 * 7); // 7 years
            return audit;
        }

        // Regular entities - keep for 1 year
        public static AuditTrail CreateRegularEntityAudit(EntityEntry entityEntry, KeyType userId)
        {
            var audit = AuditTrail.Create(entityEntry, userId);
            audit.SetRetentionPolicy(365); // 1 year
            return audit;
        }

        // Temporary data - keep for 30 days
        public static AuditTrail CreateTemporaryEntityAudit(EntityEntry entityEntry, KeyType userId)
        {
            var audit = AuditTrail.Create(entityEntry, userId);
            audit.SetRetentionPolicy(30); // 30 days
            return audit;
        }
    }

    /// <summary>
    /// Example 5: Archive eligible audit trails (background job/scheduled task).
    /// </summary>
    public static async Task ArchiveEligibleAuditTrails(DbContext context)
    {
        // Find all audit trails eligible for archival
        var eligibleForArchival = await context.Set<AuditTrail>()
            .Where(a => a.ArchiveAfter != null 
                     && a.ArchiveAfter <= DateTime.UtcNow 
                     && !a.IsArchived)
            .ToListAsync();

        // Archive them
        foreach (var audit in eligibleForArchival)
        {
            audit.Archive();
        }

        await context.SaveChangesAsync();

        Console.WriteLine($"Archived {eligibleForArchival.Count} audit trail entries");
    }

    /// <summary>
    /// Example 6: Check if audit trail should be archived.
    /// </summary>
    public static void CheckArchivalStatus(AuditTrail audit)
    {
        if (audit.ShouldBeArchived())
        {
            Console.WriteLine($"Audit trail {audit.Id} is eligible for archival");
            audit.Archive();
        }
        else if (audit.IsArchived)
        {
            Console.WriteLine($"Audit trail {audit.Id} is already archived");
        }
        else if (audit.ArchiveAfter.HasValue)
        {
            var daysRemaining = (audit.ArchiveAfter.Value - DateTime.UtcNow).TotalDays;
            Console.WriteLine($"Audit trail {audit.Id} will be eligible for archival in {daysRemaining:F0} days");
        }
        else
        {
            Console.WriteLine($"Audit trail {audit.Id} has no retention policy");
        }
    }

    /// <summary>
    /// Example 7: Cleanup archived audit trails (delete or move to cold storage).
    /// </summary>
    public static async Task<int> CleanupArchivedAuditTrails(DbContext context, int olderThanDays = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-olderThanDays);

        // Find archived entries older than cutoff
        var toDelete = await context.Set<AuditTrail>()
            .Where(a => a.IsArchived && a.DateTimeUTC < cutoffDate)
            .ToListAsync();

        // Option 1: Delete permanently
        context.Set<AuditTrail>().RemoveRange(toDelete);
        await context.SaveChangesAsync();

        return toDelete.Count;

        // Option 2: Move to cold storage (not shown)
        // - Export to blob storage
        // - Export to archive database
        // - Then delete from main database
    }

    /// <summary>
    /// Example 8: Restore archived audit trail.
    /// </summary>
    public static void RestoreArchivedAudit(AuditTrail audit)
    {
        if (audit.IsArchived)
        {
            audit.Unarchive();
            Console.WriteLine($"Audit trail {audit.Id} has been restored from archive");
        }
    }

    /// <summary>
    /// Example 9: Query non-archived audit trails (common use case).
    /// </summary>
    public static async Task<List<AuditTrail>> GetActiveAuditTrails(DbContext context, string tableName)
    {
        return await context.Set<AuditTrail>()
            .Where(a => !a.IsArchived && a.TableName == tableName)
            .OrderByDescending(a => a.DateTimeUTC)
            .ToListAsync();
    }

    /// <summary>
    /// Example 10: Scheduled archival job (Hangfire, Quartz, etc.)
    /// </summary>
    public class ScheduledArchivalJob
    {
        private readonly DbContext _context;

        public ScheduledArchivalJob(DbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Run daily to archive eligible audit trails.
        /// </summary>
        public async Task ExecuteDaily()
        {
            var archived = 0;
            var deleted = 0;

            // Step 1: Archive eligible entries
            var eligibleForArchival = await _context.Set<AuditTrail>()
                .Where(a => a.ArchiveAfter != null 
                         && a.ArchiveAfter <= DateTime.UtcNow 
                         && !a.IsArchived)
                .ToListAsync();

            foreach (var audit in eligibleForArchival)
            {
                audit.Archive();
                archived++;
            }

            await _context.SaveChangesAsync();

            // Step 2: Delete archived entries older than 1 year
            var cutoffDate = DateTime.UtcNow.AddYears(-1);
            var toDelete = await _context.Set<AuditTrail>()
                .Where(a => a.IsArchived && a.DateTimeUTC < cutoffDate)
                .ToListAsync();

            _context.Set<AuditTrail>().RemoveRange(toDelete);
            deleted = toDelete.Count;

            await _context.SaveChangesAsync();

            Console.WriteLine($"Archival job completed: {archived} archived, {deleted} deleted");
        }
    }

    /// <summary>
    /// Example 11: Configure retention based on entity type using custom logic.
    /// </summary>
    public class CustomRetentionPolicy
    {
        public static int GetRetentionDays(string tableName)
        {
            return tableName switch
            {
                "Users" => 2555,        // 7 years (legal requirement)
                "Transactions" => 2555, // 7 years (financial records)
                "Orders" => 1825,       // 5 years (business records)
                "Products" => 730,      // 2 years
                "Sessions" => 90,       // 3 months
                "Logs" => 30,          // 1 month
                _ => 365               // Default: 1 year
            };
        }

        public static AuditTrail CreateWithPolicyForTable(EntityEntry entityEntry, KeyType userId)
        {
            var audit = AuditTrail.Create(entityEntry, userId);
            var tableName = entityEntry.Metadata.DisplayName();
            var retentionDays = GetRetentionDays(tableName);
            audit.SetRetentionPolicy(retentionDays);
            return audit;
        }
    }

    /// <summary>
    /// Example 12: Statistics and reporting on audit trail retention.
    /// </summary>
    public static async Task<RetentionStatistics> GetRetentionStatistics(DbContext context)
    {
        var now = DateTime.UtcNow;

        var stats = new RetentionStatistics
        {
            Total = await context.Set<AuditTrail>().CountAsync(),
            Archived = await context.Set<AuditTrail>().CountAsync(a => a.IsArchived),
            EligibleForArchival = await context.Set<AuditTrail>()
                .CountAsync(a => a.ArchiveAfter != null && a.ArchiveAfter <= now && !a.IsArchived),
            NoRetentionPolicy = await context.Set<AuditTrail>().CountAsync(a => a.ArchiveAfter == null),
            ActiveWithRetention = await context.Set<AuditTrail>()
                .CountAsync(a => !a.IsArchived && a.ArchiveAfter != null && a.ArchiveAfter > now)
        };

        return stats;
    }

    public class RetentionStatistics
    {
        public int Total { get; set; }
        public int Archived { get; set; }
        public int EligibleForArchival { get; set; }
        public int NoRetentionPolicy { get; set; }
        public int ActiveWithRetention { get; set; }
    }

    /// <summary>
    /// Example 13: Best practices for production use.
    /// </summary>
    public static class BestPractices
    {
        /*
         * 1. Set global default retention at application startup:
         *    AuditTrail.DefaultRetentionDays = 365; // 1 year default
         * 
         * 2. Override for specific entity types in AuditTrailFeature or custom logic
         * 
         * 3. Run scheduled job daily to:
         *    - Archive eligible entries
         *    - Delete or export very old archived entries
         * 
         * 4. Query active (non-archived) entries by default:
         *    context.AuditTrails.Where(a => !a.IsArchived)
         * 
         * 5. Create indexes for archival queries:
         *    - Index on ArchiveAfter
         *    - Index on IsArchived
         *    - Composite index on (IsArchived, DateTimeUTC)
         * 
         * 6. Consider partitioning by date for large audit tables
         * 
         * 7. Export archived data to cold storage before deletion:
         *    - Azure Blob Storage
         *    - AWS S3
         *    - Archive database
         * 
         * 8. Keep legal/compliance retention requirements in mind:
         *    - Financial records: 7 years
         *    - Health records: varies by jurisdiction
         *    - Personal data: GDPR requirements
         */
    }
}
