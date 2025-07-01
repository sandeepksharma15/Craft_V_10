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
    /// <inheritdoc />
    public virtual async Task DeleteAsync(IQuery<T> query, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[Repository] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"DeleteAsync\"]");

        ArgumentNullException.ThrowIfNull(query, nameof(query));

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

    /// <inheritdoc />
    public virtual async Task<T?> GetAsync(IQuery<T> query, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[Repository] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetAsync\"]");

        ArgumentNullException.ThrowIfNull(query, nameof(query));

        return await _dbSet
            .WithQuery(query)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<TResult?> GetAsync<TResult>(IQuery<T, TResult> query, CancellationToken cancellationToken = default)
        where TResult : class, new()
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[Repository] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetAsync\"]");

        ArgumentNullException.ThrowIfNull(query, nameof(query));

        return await _dbSet
            .WithQuery(query)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<long> GetCountAsync(IQuery<T> query, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[Repository] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetCountAsync\"]");

        ArgumentNullException.ThrowIfNull(query, nameof(query));

        return await _dbSet
            .WithQuery(query)
            .LongCountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<PageResponse<T>> GetPagedListAsync(IQuery<T> query, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[Repository] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetPagedListAsync\"]");

        ArgumentNullException.ThrowIfNull(query, nameof(query));
        if (query.Take is null || query.Take <= 0)
            throw new ArgumentOutOfRangeException(nameof(query), "Page size (Take) must be set and greater than zero.");
        if (query.Skip is null || query.Skip < 0)
            throw new ArgumentOutOfRangeException(nameof(query), "Skip must be set and non-negative.");

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

    /// <inheritdoc />
    public virtual async Task<PageResponse<TResult>> GetPagedListAsync<TResult>(IQuery<T, TResult> query, CancellationToken cancellationToken = default)
            where TResult : class, new()
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[Repository] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetPagedListAsync\"]");

        ArgumentNullException.ThrowIfNull(query, nameof(query));
        if (query.Take is null || query.Take <= 0)
            throw new ArgumentOutOfRangeException(nameof(query), "Page size (Take) must be set and greater than zero.");
        if (query.Skip is null || query.Skip < 0)
            throw new ArgumentOutOfRangeException(nameof(query), "Skip must be set and non-negative.");

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
