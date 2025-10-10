using Craft.Core;
using Craft.Domain;
using Craft.Extensions.Collections;
using Craft.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Craft.QuerySpec;

/// <summary>
/// Repository with QuerySpec support for advanced querying, paging, and batch operations.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
/// <typeparam name="TKey">Entity key type.</typeparam>
public class Repository<T, TKey>(IDbContext appDbContext, ILogger<Repository<T, TKey>> logger)
    : ChangeRepository<T, TKey>(appDbContext, logger), IRepository<T, TKey> where T : class, IEntity<TKey>, new()
{
    /// <inheritdoc />
    public virtual async Task DeleteAsync(IQuery<T> query, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));

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
            }, cancellationToken)
            .ConfigureAwait(false);

        if (autoSave)
            await _appDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<T?> GetAsync(IQuery<T> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[Repository] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetAsync\"]");

        // Defensive: avoid IndexOutOfRangeException if no match
        var queryable = _dbSet.WithQuery(query);

        if (queryable.Any())
            queryable = queryable.Take(2); // Limit to 2 to check for multiple matches  

        var list = await queryable
            .ToListSafeAsync(cancellationToken)
            .ConfigureAwait(false);

        return list.Count == 0
            ? null
            : list.Count > 1 ? throw new InvalidOperationException("Sequence contains more than one matching element.") : list[0];
    }

    /// <inheritdoc />
    public virtual async Task<TResult?> GetAsync<TResult>(IQuery<T, TResult> query, CancellationToken cancellationToken = default)
        where TResult : class, new()
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[Repository] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetAsync\"]");

        // Defensive: avoid IndexOutOfRangeException if no match
        var queryable = _dbSet.WithQuery(query);

        if (queryable.Any())
            queryable = queryable.Take(2); // Limit to 2 to check for multiple matches  

        var list = await queryable
            .ToListSafeAsync(cancellationToken)
            .ConfigureAwait(false);

        return list is null || list.Count == 0
            ? null
            : list.Count > 1 ? throw new InvalidOperationException("Sequence contains more than one matching element.") : list[0];
    }

    /// <inheritdoc />
    public virtual async Task<long> GetCountAsync(IQuery<T> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[Repository] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetCountAsync\"]");

        return await _dbSet
            .WithQuery(query)
            .LongCountSafeAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<PageResponse<T>> GetPagedListAsync(IQuery<T> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[Repository] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetPagedListAsync\"]");

        if (query.Take is null or <= 0)
            throw new ArgumentOutOfRangeException(nameof(query), "Page size (Take) must be set and greater than zero.");
        if (query.Skip is null or < 0)
            throw new ArgumentOutOfRangeException(nameof(query), "Skip must be set and non-negative.");

        var items = await _dbSet.WithQuery(query)
            .ToListSafeAsync(cancellationToken)
            .ConfigureAwait(false);

        // Count total records matching the query (without projection)
        var totalCount = await _dbSet
            .WithQuery(query, WhereEvaluator.Instance)
            .LongCountSafeAsync(cancellationToken)
            .ConfigureAwait(false);

        int pageSize = query.Take.Value;
        int page = (query.Skip.Value / pageSize) + 1;

        return new PageResponse<T>(items, totalCount, page, pageSize);
    }

    /// <inheritdoc />
    public virtual async Task<PageResponse<TResult>> GetPagedListAsync<TResult>(IQuery<T, TResult> query, CancellationToken cancellationToken = default)
        where TResult : class, new()
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[Repository] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetPagedListAsync\"]");

        if (query.Take is null or <= 0)
            throw new ArgumentOutOfRangeException(nameof(query), "Page size (Take) must be set and greater than zero.");
        if (query.Skip is null or < 0)
            throw new ArgumentOutOfRangeException(nameof(query), "Skip must be set and non-negative.");

        var items = await _dbSet
            .WithQuery(query)
            .ToListSafeAsync(cancellationToken)
            .ConfigureAwait(false);

        var totalCount = await _dbSet
            .WithQuery(query, WhereEvaluator.Instance)
            .LongCountSafeAsync(cancellationToken)
            .ConfigureAwait(false);

        int pageSize = query.Take.Value;
        int page = (query.Skip.Value / pageSize) + 1;

        return new PageResponse<TResult>(items, totalCount, page, pageSize);
    }

    /// <inheritdoc />
    public virtual async Task<List<T>> GetAllAsync(IQuery<T> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[Repository] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetAllAsync\"]");

        return await _dbSet
            .WithQuery(query)
            .ToListSafeAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<List<TResult>> GetAllAsync<TResult>(IQuery<T, TResult> query, CancellationToken cancellationToken = default)
        where TResult : class, new()
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[Repository] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetAllAsync\"]");

        return await _dbSet
            .WithQuery(query)
            .ToListSafeAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}

/// <summary>
/// Repository with QuerySpec support for default key type.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
public class Repository<T>(IDbContext appDbContext, ILogger<Repository<T, KeyType>> logger)
    : Repository<T, KeyType>(appDbContext, logger), IRepository<T> where T : class, IEntity, new();
