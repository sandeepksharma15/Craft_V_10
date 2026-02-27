using Craft.Auditing;
using Craft.Core;
using Craft.QuerySpec;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Craft.AppComponents.Auditing;

public class AuditTrailRepository : Repository<AuditTrail, KeyType>, IAuditTrailRepository
{
    private readonly IAuditUserResolver _userResolver;

    public AuditTrailRepository(IDbContext appDbContext, ILogger<Repository<AuditTrail, KeyType>> logger,
        IOptions<QueryOptions> queryOptions, IAuditUserResolver userResolver, IQueryMetrics? queryMetrics = null)
        : base(appDbContext, logger, queryOptions, queryMetrics)
    {
        _userResolver = userResolver;
    }

    /// <inheritdoc />
    public async Task<List<string>> GetTableNamesAsync(CancellationToken cancellationToken = default)
    {
        return await _appDbContext.Set<AuditTrail>()
            .Where(x => !x.IsDeleted)
            .Select(x => x.TableName!)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<AuditUserDTO>> GetAuditUsersAsync(CancellationToken cancellationToken = default)
    {
        var userIds = await _appDbContext.Set<AuditTrail>()
            .Where(x => !x.IsDeleted)
            .Select(x => x.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return await _userResolver.GetAuditUsersAsync(userIds, cancellationToken);
    }
}
