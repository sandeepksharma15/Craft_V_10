using Craft.Controllers;
using Craft.Core;
using Craft.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Craft.QuerySpec;

public interface IEntityController<T, DataTransferT, TKey> : IEntityChangeController<T, DataTransferT, TKey>
    where T : class, IEntity<TKey>, new()
    where DataTransferT : class, IModel<TKey>, new()
{
    Task<ActionResult> DeleteAsync(IQuery<T> query, CancellationToken cancellationToken = default);

    Task<ActionResult<T>> GetAsync(IQuery<T> query, CancellationToken cancellationToken = default);

    Task<ActionResult<TResult>> GetAsync<TResult>(IQuery<T, TResult> query, CancellationToken cancellationToken = default)
        where TResult : class, new();

    Task<ActionResult<long>> GetCountAsync(IQuery<T> query, CancellationToken cancellationToken = default);

    Task<ActionResult<PageResponse<T>>> GetPagedListAsync(IQuery<T> query, CancellationToken cancellationToken = default);

    Task<ActionResult<PageResponse<TResult>>> GetPagedListAsync<TResult>(IQuery<T, TResult> query, 
        CancellationToken cancellationToken = default) where TResult : class, new();
}

public interface IEntityController<T, DataTransferT> : IEntityController<T, DataTransferT, KeyType>
    where T : class, IEntity, new()
    where DataTransferT : class, IModel, new();
