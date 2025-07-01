using Craft.Domain;

namespace Craft.HttpServices;

public interface IHttpChangeService<T, ViewT, DataTransferT, TKey> : IHttpReadService<T, TKey>
    where T : class, IEntity<TKey>, IModel<TKey>
    where ViewT : class, IModel<TKey>
    where DataTransferT : class, IModel<TKey>
{
    /// <summary>
    /// Adds a new entity.
    /// </summary>
    /// <param name="model">Entity to be added in the data store</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete</param>
    /// <returns>Added entity with updated keys</returns>
    Task<HttpResponseMessage> AddAsync(ViewT model, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple entities
    /// </summary>
    /// <param name="models">Entities to be added in the data store</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete</param>
    Task<HttpResponseMessage> AddRangeAsync(IEnumerable<ViewT> models, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity
    /// </summary>
    /// <param name="id">id of the entity to be deleted in the data store</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete</param>
    Task<HttpResponseMessage> DeleteAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes multiple entities
    /// </summary>
    /// <param name="models">Entities to be deleted in the data store</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete</param>
    Task<HttpResponseMessage> DeleteRangeAsync(IEnumerable<ViewT> models, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity
    /// </summary>
    /// <param name="model">Entity to be updated in the data store</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete</param>
    /// <returns>Updated entity</returns>
    Task<HttpResponseMessage> UpdateAsync(ViewT model, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates multiple entities
    /// </summary>
    /// <param name="models">Entities to be updated in the data store</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete</param>
    Task<HttpResponseMessage> UpdateRangeAsync(IEnumerable<ViewT> models, CancellationToken cancellationToken = default);
}

public interface IHttpChangeService<T, ViewT, DataTransferT> : IHttpChangeService<T, ViewT, DataTransferT, KeyType>
    where T : class, IEntity, IModel
    where ViewT : class, IModel
    where DataTransferT : class, IModel;
