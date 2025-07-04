using Craft.Core;
using Craft.Domain;

namespace Craft.HttpServices;

/// <summary>
/// Defines read operations for HTTP services.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
/// <typeparam name="TKey">Entity key type.</typeparam>
public interface IHttpReadService<T, TKey> : IHttpService 
    where T : class, IEntity<TKey>, IModel<TKey>, new()
{
    /// <summary>
    /// Gets a list of all the entities.
    /// </summary>
    /// <param name="includeDetails">Set true to include all children of this entity.</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>List of entities.</returns>
    Task<IReadOnlyList<T>> GetAllAsync(bool includeDetails = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an entity with the given primary key.
    /// </summary>
    /// <param name="id">Primary key of the entity; if not found, returns null.</param>
    /// <param name="includeDetails">Set true to include all children of this entity.</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>Entity or null if not found.</returns>
    Task<T?> GetAsync(TKey id, bool includeDetails = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets total count of all entities.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>Total count of entities.</returns>
    Task<long> GetCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a paginated list of entities.
    /// </summary>
    /// <param name="page">The page for which the data is desired.</param>
    /// <param name="pageSize">The number of entities required per page.</param>
    /// <param name="includeDetails">Set true to include all children of this entity.</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>Paginated list of entities.</returns>
    Task<PageResponse<T>> GetPagedListAsync(int page, int pageSize, bool includeDetails = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a paginated list of entities with projection.
    /// </summary>
    /// <typeparam name="TResult">Projection result type.</typeparam>
    /// <param name="page">The page for which the data is desired.</param>
    /// <param name="pageSize">The number of entities required per page.</param>
    /// <param name="includeDetails">Set true to include all children of this entity.</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>Paginated list of projected entities.</returns>
    Task<PageResponse<TResult>> GetPagedListAsync<TResult>(int page, int pageSize, bool includeDetails = false, CancellationToken cancellationToken = default)
        where TResult : class, new();
}

/// <summary>
/// Defines read operations for HTTP services with default key type.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
public interface IHttpReadService<T> : IHttpReadService<T, KeyType> 
    where T : class, IEntity, IModel, new();
