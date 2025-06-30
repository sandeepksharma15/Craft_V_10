using Craft.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Craft.Repositories;

public class BaseRepository<T, TKey>(DbContext appDbContext, ILogger<BaseRepository<T, TKey>> logger)
    : IBaseRepository<T, TKey> where T : class, IEntity<TKey>, new()
{
    protected readonly DbContext _appDbContext = appDbContext;
    protected readonly DbSet<T> _dbSet = appDbContext.Set<T>();
    protected readonly ILogger<BaseRepository<T, TKey>> _logger = logger;

    public virtual async Task<DbContext> GetDbContextAsync()
        => await Task.FromResult(_appDbContext);

    public virtual async Task<DbSet<T>> GetDbSetAsync()
        => await Task.FromResult(_dbSet);

    public virtual int SaveChanges()
        => _appDbContext.SaveChanges();

    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _appDbContext.SaveChangesAsync(cancellationToken);
}

public class BaseRepository<T>(DbContext appDbContext, ILogger<BaseRepository<T, KeyType>> logger)
    : BaseRepository<T, KeyType>(appDbContext, logger), IBaseRepository<T>
        where T : class, IEntity, new();
