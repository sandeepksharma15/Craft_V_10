using Craft.Domain;

namespace Craft.Repositories;

public interface IChangeRepository<T, TKey> : IReadRepository<T, TKey> where T : class, IEntity<TKey>, new()
{
    /// <summary>
    /// Adds a new entity.
    /// </summary>
    /// <param name="entity">Entity to be added in the data store</param>
    /// <param name="autoSave">Set false to save changes later calling SaveChangesAsync Manually</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete</param>
    /// <returns>Added entity with updated keys</returns>
    Task<T> AddAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple entities
    /// </summary>
    /// <param name="entities">Entities to be added in the data store</param>
    /// <param name="autoSave">Set false to save changes later calling SaveChangesAsync Manually</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete</param>
    Task AddRangeAsync(IEnumerable<T> entities, bool autoSave = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity
    /// </summary>
    /// <param name="entity">Entity to be deleted in the data store</param>
    /// <param name="autoSave">Set false to save changes later calling SaveChangesAsync Manually</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete</param>
    Task DeleteAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes multiple entities
    /// </summary>
    /// <param name="entities">Entities to be deleted in the data store</param>
    /// <param name="autoSave">Set false to save changes later calling SaveChangesAsync Manually</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete</param>
    Task DeleteRangeAsync(IEnumerable<T> entities, bool autoSave = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity
    /// </summary>
    /// <param name="entity">Entity to be updated in the data store</param>
    /// <param name="autoSave">Set false to save changes later calling SaveChangesAsync Manually</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete</param>
    /// <returns>Updated entity</returns>
    Task<T> UpdateAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates multiple entities
    /// </summary>
    /// <param name="entities">Entities to be updated in the data store</param>
    /// <param name="autoSave">Set false to save changes later calling SaveChangesAsync Manually</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete</param>
    Task UpdateRangeAsync(IEnumerable<T> entities, bool autoSave = true, CancellationToken cancellationToken = default);
}

public interface IChangeRepository<T> : IChangeRepository<T, KeyType> where T : class, IEntity, new();
