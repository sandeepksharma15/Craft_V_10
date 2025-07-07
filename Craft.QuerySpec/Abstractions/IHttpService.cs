using Craft.Core;
using Craft.Core.Common;
using Craft.Domain;
using Craft.HttpServices;
using System.Threading;
using System.Threading.Tasks;

namespace Craft.QuerySpec;

public interface IHttpService<T, ViewT, DataTransferT, TKey> : IHttpChangeService<T, ViewT, DataTransferT, TKey>
    where T : class, IEntity<TKey>, IModel<TKey>, new()
    where ViewT : class, IModel<TKey>, new()
    where DataTransferT : class, IModel<TKey>, new()
{
    /// <summary>
    /// Get a single entity by the given <paramref name="query"/>; returns null if no entry meets criteria
    /// </summary>
    /// <param name="query">A Query containing filtering parameters</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>Service result containing the entity or errors.</returns>
    Task<HttpServiceResult<T?>> GetAsync(IQuery<T> query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a single entity by the given <paramref name="query"/>; returns null if no entry meets criteria
    /// </summary>
    /// <param name="query">A Query containing filtering parameters</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>Service result containing the entity or errors.</returns>
    Task<HttpServiceResult<TResult?>> GetAsync<TResult>(IQuery<T, TResult> query, CancellationToken cancellationToken = default)
        where TResult : class, new();

    /// <summary>
    /// Deletes all the entities that meet the criteria by the given <paramref name="query"/>
    /// </summary>
    /// <param name="query">A Query containing filtering parameters</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>Service result indicating success or errors.</returns>
    Task<HttpServiceResult<bool>> DeleteAsync(IQuery<T> query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a list of all the entities that meet the criteria by the given <paramref name="query"/>
    /// </summary>
    /// <param name="query">A Query containing filtering and sorting parameters</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>Service result containing list of entities or errors.</returns>
    Task<HttpServiceResult<List<T>>> GetAllAsync(IQuery<T> query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a list of all the entities that meet the criteria by the given <paramref name="query"/>
    /// </summary>
    /// <param name="query">A Query containing filtering and sorting parameters</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>Service result containing list of projected entities or errors.</returns>
    Task<HttpServiceResult<List<TResult>>> GetAllAsync<TResult>(IQuery<T, TResult> query, CancellationToken cancellationToken = default)
        where TResult : class, new();

    /// <summary>
    /// Gets total count of all entities that meet the criteria by the given <paramref name="query"/>
    /// </summary>
    /// <param name="query">A Query containing filtering parameters</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>Service result containing total count or errors.</returns>
    Task<HttpServiceResult<long>> GetCountAsync(IQuery<T> query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a paginated list of entities.
    /// </summary>
    /// <param name="query">A Query containing filtering and sorting parameters</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>Service result containing paginated list of entities or errors.</returns>
    Task<HttpServiceResult<PageResponse<T>?>> GetPagedListAsync(IQuery<T> query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a paginated list of projected entities.
    /// </summary>
    /// <typeparam name="TResult">Projection result type.</typeparam>
    /// <param name="query">A Query containing filtering and sorting parameters</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>Service result containing paginated list of projected entities or errors.</returns>
    Task<HttpServiceResult<PageResponse<TResult>?>> GetPagedListAsync<TResult>(IQuery<T, TResult> query, CancellationToken cancellationToken = default)
        where TResult : class, new();
}

public interface IHttpService<T, ViewT, DataTransferT> : IHttpService<T, ViewT, DataTransferT, KeyType>
    where T : class, IEntity, IModel, new()
    where ViewT : class, IModel, new()
    where DataTransferT : class, IModel, new();

public interface IHttpService<T> : IHttpService<T, T, T>
    where T : class, IEntity, IModel, new();

