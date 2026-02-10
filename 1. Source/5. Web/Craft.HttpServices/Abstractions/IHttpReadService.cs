using Craft.Core;
using Craft.Domain;

namespace Craft.HttpServices;

/// <summary>
/// Defines read operations for HTTP services.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
/// <typeparam name="TKey">Entity key type.</typeparam>
public interface IHttpReadService<T, TKey> : IHttpService where T : class, IEntity<TKey>, IModel<TKey>, new()
{
    /// <summary>
    /// Gets a list of all the entities.
    /// </summary>
    /// <param name="includeDetails">Set true to include all children of this entity.</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>Service result containing list of entities.</returns>
    Task<ServiceResult<IReadOnlyList<T>?>> GetAllAsync(bool includeDetails = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an entity with the given primary key.
    /// </summary>
    /// <param name="id">Primary key of the entity; if not found, returns null.</param>
    /// <param name="includeDetails">Set true to include all children of this entity.</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>Service result containing entity or null if not found.</returns>
    Task<ServiceResult<T?>> GetAsync(TKey id, bool includeDetails = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets total count of all entities.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>Service result containing total count of entities.</returns>
    Task<ServiceResult<long>> GetCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a paginated list of entities.
    /// </summary>
    /// <param name="page">The page for which the data is desired.</param>
    /// <param name="pageSize">The number of entities required per page.</param>
    /// <param name="includeDetails">Set true to include all children of this entity.</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>Service result containing paginated list of entities.</returns>
    Task<ServiceResult<PageResponse<T>?>> GetPagedListAsync(int page, int pageSize, bool includeDetails = false, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines read operations for HTTP services with default key type.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
public interface IHttpReadService<T> : IHttpReadService<T, KeyType> where T : class, IEntity, IModel, new();


