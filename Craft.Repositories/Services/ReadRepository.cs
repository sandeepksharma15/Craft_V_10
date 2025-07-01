using Craft.Core;
using Craft.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Craft.Repositories.Services;

/// <summary>
/// Generic read-only repository for entities.
/// </summary>
public class ReadRepository<T, TKey>(DbContext appDbContext, ILogger<ReadRepository<T, TKey>> logger)
    : BaseRepository<T, TKey>(appDbContext, logger), IReadRepository<T, TKey> where T : class, IEntity<TKey>, new()
{
    /// <inheritdoc />
    public virtual async Task<List<T>> GetAllAsync(bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[ReadRepository] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetAllAsync\"]");

        return await _dbSet
            .IncludeDetails(includeDetails)
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<T?> GetAsync(TKey id, bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[ReadRepository] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetAsync\"] Id: [\"{id}\"]");

        return await _dbSet
            .IncludeDetails(includeDetails)
            .AsNoTracking()
            .FirstOrDefaultAsync(ti => ti.Id!.Equals(id), cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<long> GetCountAsync(CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[ReadRepository] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetCountAsync\"]");

        return await _dbSet
            .LongCountAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<PageResponse<T>> GetPagedListAsync(int currentPage, int pageSize, bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[ReadRepository] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetPagedListAsync\"] Page: [\"{currentPage}\"], PageSize: [\"{pageSize}\"]");

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(currentPage, nameof(currentPage));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize, nameof(pageSize));

        var totalCount = await _dbSet
            .LongCountAsync(cancellationToken)
            .ConfigureAwait(false);

        var pagedItems = await _dbSet
            .IncludeDetails(includeDetails)
            .AsNoTracking()
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PageResponse<T>(items: pagedItems, totalCount, currentPage, pageSize);
    }
}
