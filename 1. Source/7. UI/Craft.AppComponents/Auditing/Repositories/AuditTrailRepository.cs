using Craft.Auditing;
using Craft.Core;
using Craft.QuerySpec;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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

public class AuditTrailRepository : Repository<AuditTrail, KeyType>, IAuditTrailRepository
{
    public AuditTrailRepository(IDbContext appDbContext, ILogger<Repository<AuditTrail, KeyType>> logger, IOptions<QueryOptions> queryOptions,
        IQueryMetrics? queryMetrics = null) : base(appDbContext, logger, queryOptions, queryMetrics) { }

    public Task<List<AuditUserDTO>> GetAuditUsersAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<List<string>> GetTableNamesAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
