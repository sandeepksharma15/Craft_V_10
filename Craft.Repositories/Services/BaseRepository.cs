using Craft.Data.Abstractions;
using Craft.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Craft.Repositories;

public class BaseRepository<T, TKey>(IDbContext dbContext, ILogger<BaseRepository<T, TKey>> logger)
    : IBaseRepository<T, TKey> where T : class, IEntity<TKey>, new()
{
    protected readonly IDbContext _appDbContext = dbContext;
    protected readonly DbSet<T> _dbSet = dbContext.Set<T>();
    protected readonly ILogger<BaseRepository<T, TKey>> _logger = logger;

    public virtual async Task<IDbContext> GetDbContextAsync()
        => await Task.FromResult(_appDbContext);

    public virtual async Task<DbSet<T>> GetDbSetAsync()
        => await Task.FromResult(_dbSet);

    public virtual int SaveChanges()
        => _appDbContext.SaveChanges();

    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _appDbContext.SaveChangesAsync(cancellationToken);
}

public class BaseRepository<T>(IDbContext dbContext, ILogger<BaseRepository<T, KeyType>> logger)
    : BaseRepository<T, KeyType>(dbContext, logger), IBaseRepository<T>
        where T : class, IEntity, new();
