namespace Craft.Auditing;

public static class AuditTrailQueryExtensions
{
    public static IQueryable<AuditTrail> ForEntity<TEntity>(this IQueryable<AuditTrail> query, long entityId)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(query);

        var tableName = typeof(TEntity).Name;
        var keyPattern = $"\"Id\":{entityId}";

        return query.Where(a => a.TableName == tableName && 
                               a.KeyValues != null && 
                               a.KeyValues.Contains(keyPattern));
    }

    public static IQueryable<AuditTrail> ForTable(this IQueryable<AuditTrail> query, string tableName)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);

        return query.Where(a => a.TableName == tableName);
    }

    public static IQueryable<AuditTrail> InDateRange(this IQueryable<AuditTrail> query, DateTime from, DateTime to)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (from > to)
            throw new ArgumentException("From date must be less than or equal to To date");

        return query.Where(a => a.DateTimeUTC >= from && a.DateTimeUTC <= to);
    }

    public static IQueryable<AuditTrail> ByUser(this IQueryable<AuditTrail> query, KeyType userId)
    {
        ArgumentNullException.ThrowIfNull(query);

        return query.Where(a => a.UserId == userId);
    }

    public static IQueryable<AuditTrail> PendingArchival(this IQueryable<AuditTrail> query)
    {
        ArgumentNullException.ThrowIfNull(query);

        return query.Where(a => !a.IsArchived && 
                               a.ArchiveAfter.HasValue && 
                               a.ArchiveAfter.Value <= DateTime.UtcNow);
    }

    public static IQueryable<AuditTrail> OfChangeType(this IQueryable<AuditTrail> query, EntityChangeType changeType)
    {
        ArgumentNullException.ThrowIfNull(query);

        return query.Where(a => a.ChangeType == changeType);
    }

    public static IQueryable<AuditTrail> Archived(this IQueryable<AuditTrail> query)
    {
        ArgumentNullException.ThrowIfNull(query);

        return query.Where(a => a.IsArchived);
    }

    public static IQueryable<AuditTrail> NotArchived(this IQueryable<AuditTrail> query)
    {
        ArgumentNullException.ThrowIfNull(query);

        return query.Where(a => !a.IsArchived);
    }

    public static IQueryable<AuditTrail> Recent(this IQueryable<AuditTrail> query, int days = 7)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (days < 1)
            throw new ArgumentException("Days must be greater than 0", nameof(days));

        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        return query.Where(a => a.DateTimeUTC >= cutoffDate);
    }

    public static IQueryable<AuditTrail> OrderByMostRecent(this IQueryable<AuditTrail> query)
    {
        ArgumentNullException.ThrowIfNull(query);

        return query.OrderByDescending(a => a.DateTimeUTC);
    }

    public static IQueryable<AuditTrail> OrderByOldest(this IQueryable<AuditTrail> query)
    {
        ArgumentNullException.ThrowIfNull(query);

        return query.OrderBy(a => a.DateTimeUTC);
    }
}
