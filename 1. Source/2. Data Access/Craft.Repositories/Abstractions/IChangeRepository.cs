using Craft.Domain;

namespace Craft.Repositories;

/// <summary>
/// Defines methods for adding, updating, and deleting entities in the data store, including batch and soft delete operations.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
public interface IChangeRepository<T, TKey> : IReadRepository<T, TKey> where T : class, IEntity<TKey>, new()
{
    /// <summary>
    /// Adds a new entity to the data store.
    /// </summary>
    /// <param name="entity">Entity to be added in the data store.</param>
    /// <param name="autoSave">Set false to save changes later by calling SaveChangesAsync manually. Default is true.</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>Added entity with updated keys.</returns>
    Task<T> AddAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple entities to the data store.
    /// </summary>
    /// <param name="entities">Entities to be added in the data store.</param>
    /// <param name="autoSave">Set false to save changes later by calling SaveChangesAsync manually. Default is true.</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>The list of added entities with updated keys.</returns>
    Task<List<T>> AddRangeAsync(IEnumerable<T> entities, bool autoSave = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity from the data store. If the entity implements <see cref="ISoftDelete"/>, it will be soft deleted.
    /// </summary>
    /// <param name="entity">Entity to be deleted in the data store.</param>
    /// <param name="autoSave">Set false to save changes later by calling SaveChangesAsync manually. Default is true.</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>The deleted entity.</returns>
    Task<T> DeleteAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes multiple entities from the data store. If any entity implements <see cref="ISoftDelete"/>, it will be soft deleted.
    /// </summary>
    /// <param name="entities">Entities to be deleted in the data store.</param>
    /// <param name="autoSave">Set false to save changes later by calling SaveChangesAsync manually. Default is true.</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>The list of deleted entities.</returns>
    Task<List<T>> DeleteRangeAsync(IEnumerable<T> entities, bool autoSave = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity in the data store.
    /// </summary>
    /// <param name="entity">Entity to be updated in the data store.</param>
    /// <param name="autoSave">Set false to save changes later by calling SaveChangesAsync manually. Default is true.</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>Updated entity.</returns>
    Task<T> UpdateAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates multiple entities in the data store.
    /// </summary>
    /// <param name="entities">Entities to be updated in the data store.</param>
    /// <param name="autoSave">Set false to save changes later by calling SaveChangesAsync manually. Default is true.</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>The list of updated entities.</returns>
    Task<List<T>> UpdateRangeAsync(IEnumerable<T> entities, bool autoSave = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Restores a soft-deleted entity. Only works if the entity implements <see cref="ISoftDelete"/>.
    /// </summary>
    /// <param name="entity">Entity to be restored.</param>
    /// <param name="autoSave">Set false to save changes later by calling SaveChangesAsync manually. Default is true.</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>The restored entity.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the entity does not implement ISoftDelete.</exception>
    Task<T> RestoreAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Restores multiple soft-deleted entities. Only works if the entities implement <see cref="ISoftDelete"/>.
    /// </summary>
    /// <param name="entities">Entities to be restored.</param>
    /// <param name="autoSave">Set false to save changes later by calling SaveChangesAsync manually. Default is true.</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>The list of restored entities.</returns>
    /// <exception cref="InvalidOperationException">Thrown if any entity does not implement ISoftDelete.</exception>
    Task<List<T>> RestoreRangeAsync(IEnumerable<T> entities, bool autoSave = true, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines methods for adding, updating, and deleting entities in the data store, using the default key type.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public interface IChangeRepository<T> : IChangeRepository<T, KeyType> where T : class, IEntity, new();
