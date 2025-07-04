using Craft.Core;
using Craft.Domain;

namespace Craft.HttpServices;

public interface IHttpReadService<T, TKey> : IHttpService where T : class, IEntity<TKey>, IModel<TKey>, new()
{
    /// <summary>
    /// Gets a list of all the entities.
    /// </summary>
    /// <param name="includeDetails">Set true to include all children of this entity</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>List of entities</returns>
    Task<List<T>> GetAllAsync(bool includeDetails = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an entity with given primary key.
    /// </summary>
    /// <param name="id">Primary key of the entity; if not found, returns null</param>
    /// <param name="includeDetails">Set true to include all children of this entity</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>Entity</returns>
    Task<T?> GetAsync(TKey id, bool includeDetails = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets total count of all entities.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>total count of entities</returns>
    Task<long> GetCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a paginated list of entities.
    /// </summary>
    /// <param name="page">The page for which the data is desired</param>
    /// <param name="pageSize">The number of entities required per page</param>
    /// <param name="includeDetails">Set true to include all children of this entity</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>Paginated list of entities</returns>
    Task<PageResponse<T>> GetPagedListAsync(int page, int pageSize, bool includeDetails = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a paginated list of entities.
    /// </summary>
    /// <param name="query">A Query containing filtering and sorting parameters</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>Paginated list of entities</returns>
    Task<PageResponse<TResult>> GetPagedListAsync<TResult>(int page, int pageSize, bool includeDetails = false, CancellationToken cancellationToken = default)
       where TResult : class, new();
}

public interface IHttpReadService<T> : IHttpReadService<T, KeyType> where T : class, IEntity, IModel, new();
