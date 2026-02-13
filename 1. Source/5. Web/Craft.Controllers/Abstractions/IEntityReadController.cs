using Craft.Core;
using Craft.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Craft.Controllers;

public interface IEntityReadController<T, TKey> : IEntityController where T : class, IEntity<TKey>, new()
{
    /// <summary>
    /// Retrieves an entity asynchronously by its unique identifier.
    /// </summary>
    Task<ActionResult<T>> GetAsync(TKey id, bool includeDetails, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all entities asynchronously, optionally including detailed information.
    /// </summary>
    Task<ActionResult<IAsyncEnumerable<T>>> GetAllAsync(bool includeDetails, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves the total count of items.
    /// </summary>
    Task<ActionResult<long>> GetCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of items asynchronously.
    /// </summary>
    Task<ActionResult<PageResponse<T>>> GetPagedListAsync(int page, int pageSize, bool includeDetails = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an entity with the given identifier exists.
    /// </summary>
    Task<ActionResult<bool>> ExistsAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any entities exist in the repository.
    /// </summary>
    Task<ActionResult<bool>> AnyAsync(CancellationToken cancellationToken = default);

}

public interface IEntityReadController<T> : IEntityReadController<T, KeyType> where T : class, IEntity, new();
