using Craft.Data.Abstractions;
using Craft.Domain;
using Craft.Repositories.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Craft.Repositories;

/// <summary>
/// Provides CRUD operations for entities, including support for soft delete and batch operations.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
public class ChangeRepository<T, TKey>(IDbContext dbContext, ILogger<ChangeRepository<T, TKey>> logger)
    : ReadRepository<T, TKey>(dbContext, logger), IChangeRepository<T, TKey> where T : class, IEntity<TKey>, new()
{
    /// <inheritdoc/>
    public virtual async Task<T> AddAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[ChangeRepository] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"AddAsync\"] Id: [\"{entity.Id}\"]");

        var result = await _dbSet
            .AddAsync(entity, cancellationToken)
            .ConfigureAwait(false);

        if (autoSave)
            await _appDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        result.State = EntityState.Detached;

        return result.Entity;
    }

    /// <inheritdoc/>
    public virtual async Task AddRangeAsync(IEnumerable<T> entities, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[ChangeRepository] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"AddRangeAsync\"]");

        await _dbSet
            .AddRangeAsync(entities, cancellationToken)
            .ConfigureAwait(false);

        if (autoSave)
            await _appDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async Task DeleteAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[ChangeRepository] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"DeleteAsync\"] Id: [\"{entity.Id}\"]");

        if (entity is ISoftDelete softDeleteEntity)
        {
            softDeleteEntity.IsDeleted = true;
            _dbSet.Update(entity);
        }
        else
            _dbSet.Remove(entity);

        if (autoSave)
            await _appDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async Task DeleteRangeAsync(IEnumerable<T> entities, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[ChangeRepository] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"DeleteRangeAsync\"]");

        if (entities.Any(entity => entity is ISoftDelete))
        {
            foreach (var entity in entities)
            {
                ISoftDelete softDeleteEntity = (ISoftDelete)entity;
                softDeleteEntity.IsDeleted = true;
            }

            _dbSet.UpdateRange(entities);
        }
        else
            _dbSet.RemoveRange(entities);

        if (autoSave)
            await _appDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public virtual async Task<T> UpdateAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[ChangeRepository] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"UpdateAsync\"] Id: [\"{entity.Id}\"]");

        var result = _dbSet.Update(entity);

        if (autoSave)
            await _appDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        result.State = EntityState.Detached;

        return result.Entity;
    }

    /// <inheritdoc/>
    public virtual async Task UpdateRangeAsync(IEnumerable<T> entities, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[ChangeRepository] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"UpdateRangeAsync\"]");

        _dbSet.UpdateRange(entities);

        if (autoSave)
            await _appDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}

/// <summary>
/// Provides CRUD operations for entities with a default key type.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public class ChangeRepository<T>(IDbContext dbContext, ILogger<ChangeRepository<T, KeyType>> logger)
    : ChangeRepository<T, KeyType>(dbContext, logger), IChangeRepository<T>
        where T : class, IEntity, new();

