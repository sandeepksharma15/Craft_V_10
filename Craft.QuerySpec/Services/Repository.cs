using Craft.Core;
using Craft.Data.Abstractions;
using Craft.Domain;
using Craft.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Craft.QuerySpec;

public class Repository<T, TKey>(IDbContext appDbContext, ILogger<Repository<T, TKey>> logger)
    : ChangeRepository<T, TKey>(appDbContext, logger), IRepository<T, TKey> where T : class, IEntity<TKey>, new()
{
    public virtual async Task DeleteAsync(IQuery<T> query, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[Repository] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"DeleteAsync\"]");

        await _dbSet
            .WithQuery(query)
            .ForEachAsync(entity =>
            {
                if (entity is ISoftDelete softDeleteEntity)
                {
                    softDeleteEntity.IsDeleted = true;
                    _dbSet.Update(entity);
                }
                else
                    _dbSet.Remove(entity);
            }, cancellationToken);

        if (autoSave)
            await _appDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public virtual async Task<T?> GetAsync(IQuery<T> query, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[Repository] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetAsync\"]");

        return await _dbSet
            .WithQuery(query)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<TResult?> GetAsync<TResult>(IQuery<T, TResult> query, CancellationToken cancellationToken = default)
        where TResult : class, new()
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[Repository] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetAsync\"]");

        return await _dbSet
            .WithQuery(query)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<long> GetCountAsync(IQuery<T> query, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[Repository] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetCountAsync\"]");


        return await _dbSet
            .WithQuery(query)
            .LongCountAsync(cancellationToken);
    }

    public virtual async Task<PageResponse<T>> GetPagedListAsync(IQuery<T> query, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[Repository] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetPagedListAsync\"]");

        var items = await _dbSet
            .WithQuery(query)
            .ToListAsync(cancellationToken);

        // We need to count the total records (matching the query) without the projection
        var totalCount = await _dbSet
            .WithQuery(query, WhereEvaluator.Instance)
            .CountAsync(cancellationToken);

        int pageSize = query.Take!.Value;
        int page = (query.Skip!.Value / pageSize) + 1;

        return new PageResponse<T>(items, totalCount, page, pageSize);
    }

    public virtual async Task<PageResponse<TResult>> GetPagedListAsync<TResult>(IQuery<T, TResult> query, CancellationToken cancellationToken = default)
            where TResult : class, new()
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[Repository] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetPagedListAsync\"]");

        var items = await _dbSet
            .WithQuery(query)
            .ToListAsync(cancellationToken);

        // We need to count the total records (matching the query) without the projection
        var totalCount = await _dbSet
            .WithQuery(query, WhereEvaluator.Instance)
            .CountAsync(cancellationToken);

        int pageSize = query.Take!.Value;
        int page = (query.Skip!.Value / pageSize) + 1;

        return new PageResponse<TResult>(items, totalCount, page, pageSize);
    }
}

public class Repository<T>(IDbContext appDbContext, ILogger<Repository<T, KeyType>> logger)
    : Repository<T, KeyType>(appDbContext, logger), IRepository<T>
        where T : class, IEntity, new();
