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
    public virtual async Task<List<T>> AddRangeAsync(IEnumerable<T> entities, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[ChangeRepository] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"AddRangeAsync\"]");

        var entityList = entities.ToList();

        await _dbSet.AddRangeAsync(entityList, cancellationToken).ConfigureAwait(false);

        if (autoSave)
            await _appDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        foreach (var entity in entityList)
            _appDbContext.Entry(entity).State = EntityState.Detached;

        return entityList;
    }

    /// <inheritdoc/>
    public virtual async Task<T> DeleteAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default)
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

        _appDbContext.Entry(entity).State = EntityState.Detached;

        return entity;
    }

    /// <inheritdoc/>
    public virtual async Task<List<T>> DeleteRangeAsync(IEnumerable<T> entities, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[ChangeRepository] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"DeleteRangeAsync\"]");

        var entityList = entities.ToList();

        if (entityList.Any(entity => entity is ISoftDelete))
        {
            foreach (var entity in entityList)
            {
                ISoftDelete softDeleteEntity = (ISoftDelete)entity;
                softDeleteEntity.IsDeleted = true;
            }

            _dbSet.UpdateRange(entityList);
        }
        else
            _dbSet.RemoveRange(entityList);

        if (autoSave)
            await _appDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        foreach (var entity in entityList)
            _appDbContext.Entry(entity).State = EntityState.Detached;

        return entityList;
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
    public virtual async Task<List<T>> UpdateRangeAsync(IEnumerable<T> entities, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[ChangeRepository] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"UpdateRangeAsync\"]");

        var entityList = entities.ToList();

        _dbSet.UpdateRange(entityList);

        if (autoSave)
            await _appDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        foreach (var entity in entityList)
            _appDbContext.Entry(entity).State = EntityState.Detached;

        return entityList;
    }
}

/// <summary>
/// Provides CRUD operations for entities with a default key type.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public class ChangeRepository<T>(IDbContext dbContext, ILogger<ChangeRepository<T, KeyType>> logger)
    : ChangeRepository<T, KeyType>(dbContext, logger), IChangeRepository<T>
        where T : class, IEntity, new();

