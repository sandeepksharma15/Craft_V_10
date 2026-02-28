using Craft.Auditing;
using Craft.Core;
using Craft.QuerySpec;

namespace Craft.AppComponents.Auditing;

/// <summary>
/// Provides HTTP access to the audit trail API, extending the standard CRUD operations
/// with audit-specific queries for table names and user lookups.
/// </summary>
public interface IAuditTrailHttpService : IHttpService<AuditTrail>
{
    /// <summary>
    /// Returns the distinct table names that have audit trail entries.
    /// </summary>
    Task<ServiceResult<List<string>?>> GetTableNamesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns users who have audit trail entries, with their display names resolved.
    /// </summary>
    Task<ServiceResult<List<AuditUserDTO>?>> GetAuditUsersAsync(CancellationToken cancellationToken = default);
}
