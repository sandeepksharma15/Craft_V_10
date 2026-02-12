using Craft.Core;
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
        {
            await _appDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            _appDbContext.Entry(entity).State = EntityState.Detached;
        }

        return result.Entity;
    }

    /// <inheritdoc/>
    public virtual async Task<List<T>> AddRangeAsync(IEnumerable<T> entities, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[ChangeRepository] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"AddRangeAsync\"]");

        var entityList = entities.ToList();

        if (entityList.Count == 0)
            return entityList;

        await _dbSet.AddRangeAsync(entityList, cancellationToken).ConfigureAwait(false);

        if (autoSave)
        {
            // Use transaction for large batch operations to ensure atomicity
            if (entityList.Count > 100)
            {
                await using var transaction = await _appDbContext.Database
                    .BeginTransactionAsync(cancellationToken)
                    .ConfigureAwait(false);

                try
                {
                    await _appDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

                    foreach (var entity in entityList)
                        _appDbContext.Entry(entity).State = EntityState.Detached;
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    throw;
                }
            }
            else
            {
                await _appDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                foreach (var entity in entityList)
                    _appDbContext.Entry(entity).State = EntityState.Detached;
            }
        }

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
        {
            await _appDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            _appDbContext.Entry(entity).State = EntityState.Detached;
        }

        return entity;
    }

    /// <inheritdoc/>
    public virtual async Task<List<T>> DeleteRangeAsync(IEnumerable<T> entities, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[ChangeRepository] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"DeleteRangeAsync\"]");

        var entityList = entities.ToList();

        if (entityList.Count == 0)
            return entityList;

        var softDeleteEntities = new List<T>();
        var hardDeleteEntities = new List<T>();

        foreach (var entity in entityList)
        {
            if (entity is ISoftDelete)
                softDeleteEntities.Add(entity);
            else
                hardDeleteEntities.Add(entity);
        }

        if (softDeleteEntities.Count > 0)
        {
            foreach (var entity in softDeleteEntities)
            {
                var softDeleteEntity = (ISoftDelete)entity;
                softDeleteEntity.IsDeleted = true;
            }

            _dbSet.UpdateRange(softDeleteEntities);
        }

        if (hardDeleteEntities.Count > 0)
            _dbSet.RemoveRange(hardDeleteEntities);

        if (autoSave)
        {
            // Use transaction for large batch operations to ensure atomicity
            if (entityList.Count > 100)
            {
                await using var transaction = await _appDbContext.Database
                    .BeginTransactionAsync(cancellationToken)
                    .ConfigureAwait(false);

                try
                {
                    await _appDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

                    foreach (var entity in entityList)
                        _appDbContext.Entry(entity).State = EntityState.Detached;
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    throw;
                }
            }
            else
            {
                await _appDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                foreach (var entity in entityList)
                    _appDbContext.Entry(entity).State = EntityState.Detached;
            }
        }

        return entityList;
    }

    /// <inheritdoc/>
    public virtual async Task<T> UpdateAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[ChangeRepository] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"UpdateAsync\"] Id: [\"{entity.Id}\"]");

        _dbSet.Update(entity);

        if (autoSave)
        {
            try
            {
                await _appDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                _appDbContext.Entry(entity).State = EntityState.Detached;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency conflict updating {EntityType} with Id {EntityId}", 
                    typeof(T).Name, entity.Id);
                throw;
            }
        }

        return entity;
    }

    /// <inheritdoc/>
    public virtual async Task<List<T>> UpdateRangeAsync(IEnumerable<T> entities, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[ChangeRepository] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"UpdateRangeAsync\"]");

        var entityList = entities.ToList();

        if (entityList.Count == 0)
            return entityList;

        _dbSet.UpdateRange(entityList);

        if (autoSave)
        {
            try
            {
                // Use transaction for large batch operations to ensure atomicity
                if (entityList.Count > 100)
                {
                    await using var transaction = await _appDbContext.Database
                        .BeginTransactionAsync(cancellationToken)
                        .ConfigureAwait(false);

                    try
                    {
                        await _appDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

                        foreach (var entity in entityList)
                            _appDbContext.Entry(entity).State = EntityState.Detached;
                    }
                    catch
                    {
                        await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                        throw;
                    }
                }
                else
                {
                    await _appDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                    foreach (var entity in entityList)
                        _appDbContext.Entry(entity).State = EntityState.Detached;
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency conflict updating range of {EntityType}", typeof(T).Name);
                throw;
            }
        }

        return entityList;
    }

    /// <inheritdoc/>
    public virtual async Task<T> RestoreAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        if (entity is not ISoftDelete softDeleteEntity)
            throw new InvalidOperationException($"Entity type {typeof(T).Name} does not implement ISoftDelete and cannot be restored.");

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[ChangeRepository] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"RestoreAsync\"] Id: [\"{entity.Id}\"]");

        softDeleteEntity.IsDeleted = false;
        _dbSet.Update(entity);

        if (autoSave)
        {
            await _appDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            _appDbContext.Entry(entity).State = EntityState.Detached;
        }

        return entity;
    }

    /// <inheritdoc/>
    public virtual async Task<List<T>> RestoreRangeAsync(IEnumerable<T> entities, bool autoSave = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[ChangeRepository] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"RestoreRangeAsync\"]");

        var entityList = entities.ToList();

        if (entityList.Count == 0)
            return entityList;

        // Validate all entities implement ISoftDelete
        var nonSoftDeleteEntities = entityList.Where(e => e is not ISoftDelete).ToList();
        if (nonSoftDeleteEntities.Count != 0)
            throw new InvalidOperationException($"Entity type {typeof(T).Name} does not implement ISoftDelete and cannot be restored. {nonSoftDeleteEntities.Count} entities in the collection do not support soft delete.");

        foreach (var entity in entityList)
        {
            var softDeleteEntity = (ISoftDelete)entity;
            softDeleteEntity.IsDeleted = false;
        }

        _dbSet.UpdateRange(entityList);

        if (autoSave)
        {
            // Use transaction for large batch operations to ensure atomicity
            if (entityList.Count > 100)
            {
                await using var transaction = await _appDbContext.Database
                    .BeginTransactionAsync(cancellationToken)
                    .ConfigureAwait(false);

                try
                {
                    await _appDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

                    foreach (var entity in entityList)
                        _appDbContext.Entry(entity).State = EntityState.Detached;
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
                    throw;
                }
            }
            else
            {
                await _appDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                foreach (var entity in entityList)
                    _appDbContext.Entry(entity).State = EntityState.Detached;
            }
        }

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

