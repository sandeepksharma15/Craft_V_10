using Craft.Core;
using Craft.Domain;
using Craft.QuerySpec;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Craft.AppComponents.Security;


public interface IUsersRepository<T, TKey> : IRepository<T, TKey>
    where T : class, IEntity<TKey>, new()
{
}

public class UsersRepository<T, TKey> : Repository<T, TKey>, IUsersRepository<T, TKey>
    where T : class, IEntity<TKey>, new()
{
    public UsersRepository(IDbContext appDbContext, ILogger<Repository<T, TKey>> logger,
        IOptions<QueryOptions> queryOptions, IQueryMetrics? queryMetrics = null) 
            : base(appDbContext, logger, queryOptions, queryMetrics)
    {
    }
}
