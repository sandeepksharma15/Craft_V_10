namespace Craft.AppComponents.Auditing;

public interface IAuditTrailRepository
{
    /// <summary>
    /// Retrieves the distinct table names that have audit trail entries.
    /// </summary>
    Task<List<string>> GetTableNamesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the users who have audit trail entries with their display names.
    /// </summary>
    Task<List<AuditUserDTO>> GetAuditUsersAsync(CancellationToken cancellationToken = default);
}
