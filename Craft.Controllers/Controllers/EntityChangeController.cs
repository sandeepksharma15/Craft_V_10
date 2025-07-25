using Craft.Domain;
using Craft.Repositories;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Craft.Controllers.Controllers;
[Route("api/[controller]")]
[ApiController]
public abstract class EntityChangeController<T, DataTransferT, TKey>(IChangeRepository<T, TKey> repository, ILogger<EntityChangeController<T, DataTransferT, TKey>> logger)
    : EntityReadController<T, DataTransferT, TKey>(repository, logger), IEntityChangeController<T, DataTransferT, TKey>
        where T : class, IEntity<TKey>, new()
        where DataTransferT : class, IModel<TKey>, new()
{
    protected virtual ActionResult<T> ReturnProperError(Exception ex)
    {
        return Problem(ex.Message);
    }

    [HttpPost]
    public virtual async Task<ActionResult<T>> AddAsync(DataTransferT model, CancellationToken cancellationToken = default)
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug($"[EntityChangeController] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"AddAsync\"]");

        try
        {
            T entity = await repository.AddAsync(model.Adapt<T>(), cancellationToken: cancellationToken);
            return CreatedAtAction(nameof(GetAsync), new { id = entity.Id }, entity);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex.Message);
            return ReturnProperError(ex);
        }
    }

    [HttpPost("addrange")]
    public virtual async Task<ActionResult> AddRangeAsync(IEnumerable<DataTransferT> models, CancellationToken cancellationToken = default)
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug($"[EntityChangeController] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"AddRangeAsync\"]");

        try
        {
            await repository.AddRangeAsync(models.Adapt<IEnumerable<T>>(), cancellationToken: cancellationToken);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex.Message);
            return Problem(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public virtual async Task<ActionResult> DeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug($"[EntityChangeController] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"DeleteAsync\"] Id: [\"{id}\"]");

        try
        {
            T? entity = await repository.GetAsync(id, cancellationToken: cancellationToken);

            if (entity != null)
                await repository.DeleteAsync(entity, cancellationToken: cancellationToken);

            return entity == null ? NotFound() : Ok();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex.Message);
            return Problem(ex.Message);
        }
    }

    [HttpPut("deleterange")]
    public virtual async Task<ActionResult> DeleteRangeAsync(IEnumerable<DataTransferT> models, CancellationToken cancellationToken = default)
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug($"[EntityChangeController] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"DeleteRangeAsync\"]");

        try
        {
            await repository.DeleteRangeAsync(models.Adapt<IEnumerable<T>>(), cancellationToken: cancellationToken);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex.Message);
            return Problem(ex.Message);
        }
    }

    [HttpPut]
    public virtual async Task<ActionResult<T>> UpdateAsync(DataTransferT model, CancellationToken cancellationToken = default)
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug($"[EntityChangeController] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"UpdateAsync\"] Id: [\"{model.Id}\"]");

        try
        {
            return Ok(await repository.UpdateAsync(model.Adapt<T>(), cancellationToken: cancellationToken));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex.Message);
            return Problem("The values in database have changed. Please update again with updated values.");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex.Message);
            return ReturnProperError(ex);
        }
    }

    [HttpPost("updaterange")]
    public virtual async Task<ActionResult> UpdateRangeAsync(IEnumerable<DataTransferT> models, CancellationToken cancellationToken = default)
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug($"[EntityChangeController] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"UpdateRangeAsync\"]");

        try
        {
            await repository.UpdateRangeAsync(models.Adapt<IEnumerable<T>>(), cancellationToken: cancellationToken);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex.Message);
            return Problem(ex.Message);
        }
    }
}

public abstract class EntityChangeController<T, DataTransferT>(IChangeRepository<T> repository, ILogger<EntityChangeController<T, DataTransferT>> logger)
    : EntityChangeController<T, DataTransferT, KeyType>(repository, logger), IEntityChangeController<T, DataTransferT>
        where T : class, IEntity, new()
        where DataTransferT : class, IModel, new();

