using Craft.Core;
using Craft.Domain;
using Craft.QuerySpec;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Craft.AppComponents.Security;


public interface IRolesRepository<T, TKey> : IRepository<T, TKey>
    where T : class, IEntity<TKey>, new()
{
}

public class RolesRepository<T, TKey> : Repository<T, TKey>, IRolesRepository<T, TKey>
    where T : class, IEntity<TKey>, new()
{
    public RolesRepository(IDbContext appDbContext, ILogger<Repository<T, TKey>> logger,
        IOptions<QueryOptions> queryOptions, IQueryMetrics? queryMetrics = null) 
            : base(appDbContext, logger, queryOptions, queryMetrics)
    {
    }
}
