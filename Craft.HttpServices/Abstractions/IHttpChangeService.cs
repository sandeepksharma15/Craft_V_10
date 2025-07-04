﻿using Craft.Domain;

namespace Craft.HttpServices;

/// <summary>
/// Defines change (CRUD) operations for HTTP services.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
/// <typeparam name="ViewT">View model type for the entity.</typeparam>
/// <typeparam name="DataTransferT">Data transfer object type for the entity.</typeparam>
/// <typeparam name="TKey">Entity key type.</typeparam>
public interface IHttpChangeService<T, ViewT, DataTransferT, TKey> : IHttpReadService<T, TKey>
    where T : class, IEntity<TKey>, IModel<TKey>, new()
    where ViewT : class, IModel<TKey>
    where DataTransferT : class, IModel<TKey>
{
    /// <summary>
    /// Adds a new entity to the data store.
    /// </summary>
    /// <param name="model">The entity to add.</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>The HTTP response message containing the result.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="model"/> is null.</exception>
    Task<HttpResponseMessage> AddAsync(ViewT model, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple entities to the data store.
    /// </summary>
    /// <param name="models">The entities to add.</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>The HTTP response message containing the result.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="models"/> is null.</exception>
    Task<HttpResponseMessage> AddRangeAsync(IReadOnlyCollection<ViewT> models, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity from the data store by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity to delete.</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>The HTTP response message containing the result.</returns>
    Task<HttpResponseMessage> DeleteAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes multiple entities from the data store.
    /// </summary>
    /// <param name="models">The entities to delete.</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>The HTTP response message containing the result.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="models"/> is null.</exception>
    Task<HttpResponseMessage> DeleteRangeAsync(IReadOnlyCollection<ViewT> models, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity in the data store.
    /// </summary>
    /// <param name="model">The entity to update.</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>The HTTP response message containing the result.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="model"/> is null.</exception>
    Task<HttpResponseMessage> UpdateAsync(ViewT model, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates multiple entities in the data store.
    /// </summary>
    /// <param name="models">The entities to update.</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>The HTTP response message containing the result.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="models"/> is null.</exception>
    Task<HttpResponseMessage> UpdateRangeAsync(IReadOnlyCollection<ViewT> models, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines change (CRUD) operations for HTTP services with default key type.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
/// <typeparam name="ViewT">View model type for the entity.</typeparam>
/// <typeparam name="DataTransferT">Data transfer object type for the entity.</typeparam>
public interface IHttpChangeService<T, ViewT, DataTransferT> : IHttpChangeService<T, ViewT, DataTransferT, KeyType>
    where T : class, IEntity, IModel, new()
    where ViewT : class, IModel
    where DataTransferT : class, IModel;
