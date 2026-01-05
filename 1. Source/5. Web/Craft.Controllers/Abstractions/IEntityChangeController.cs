using Craft.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Craft.Controllers;

public interface IEntityChangeController<T, DataTransferT, TKey> : IEntityReadController<T, TKey>
    where T : class, IEntity<TKey>, new()
    where DataTransferT : class, IModel<TKey>, new()
{
    Task<ActionResult<T>> AddAsync(DataTransferT model, CancellationToken cancellationToken = default);

    Task<ActionResult<List<T>>> AddRangeAsync(IEnumerable<DataTransferT> models, CancellationToken cancellationToken = default);

    Task<ActionResult> DeleteAsync(TKey id, CancellationToken cancellationToken = default);

    Task<ActionResult> DeleteRangeAsync(IEnumerable<DataTransferT> models, CancellationToken cancellationToken = default);

    Task<ActionResult<T>> UpdateAsync(DataTransferT model, CancellationToken cancellationToken = default);

    Task<ActionResult<List<T>>> UpdateRangeAsync(IEnumerable<DataTransferT> models, CancellationToken cancellationToken = default);
}

public interface IEntityChangeController<T, DataTransferT> : IEntityChangeController<T, DataTransferT, KeyType>
    where T : class, IEntity, new()
    where DataTransferT : class, IModel, new();
