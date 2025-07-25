using System;
using System.Collections.Generic;
using System.Text;
using Craft.Controllers.Controllers;
using Craft.Core;
using Craft.Domain;
using Craft.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Craft.QuerySpec.Services;

[Route("api/[controller]")]
[ApiController]
public abstract class EntityController<T, DataTransferT, TKey>(IRepository<T, TKey> repository, ILogger<EntityController<T, DataTransferT, TKey>> logger)
    : EntityChangeController<T, DataTransferT, TKey>(repository, logger), IEntityController<T, DataTransferT, TKey>
        where T : class, IEntity<TKey>, new()
        where DataTransferT : class, IModel<TKey>, new()
{
    [HttpPost("delete")]
    public virtual async Task<ActionResult> DeleteAsync(IQuery<T> query, CancellationToken cancellationToken = default)
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug($"[EntityController] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"DeleteAsync\"]");

        try
        {
            await repository.DeleteAsync(query, cancellationToken: cancellationToken);

            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex.Message);
            return Problem(ex.Message);
        }
    }

    [HttpPost("getall")]
    public virtual async Task<ActionResult<List<T>>> GetAllAsync(IQuery<T> query, CancellationToken cancellationToken = default)
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug($"[EntityController] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetAllAsync\"]");

        try
        {
            return Ok(await repository.GetAllAsync(query, cancellationToken));
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex.Message);
            return Problem(ex.Message);
        }
    }

    [HttpPost("getallselect")]
    public virtual async Task<ActionResult<List<TResult>>> GetAllAsync<TResult>(IQuery<T, TResult> query, CancellationToken cancellationToken = default) where TResult : class, new()
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug($"[EntityController] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetAllAsync\"]");
        try
        {
            return Ok(await repository.GetAllAsync<TResult>(query, cancellationToken));
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex.Message);
            return Problem(ex.Message);
        }
    }

    [HttpPost("find")]
    public virtual async Task<ActionResult<T>> GetAsync(IQuery<T> query, CancellationToken cancellationToken = default)
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug($"[EntityController] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetAsync\"]");

        try
        {
            T? entity = await repository.GetAsync(query, cancellationToken);

            return entity == null ? NotFound() : Ok(entity);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex.Message);
            return Problem(ex.Message);
        }
    }

    [HttpPost("selectone")]
    public virtual async Task<ActionResult<TResult>> GetAsync<TResult>(IQuery<T, TResult> query, CancellationToken cancellationToken = default)
            where TResult : class, new()
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug($"[EntityController] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetAsync\"]");

        try
        {
            TResult? entity = await repository.GetAsync<TResult>(query, cancellationToken);

            return entity == null ? NotFound() : Ok(entity);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex.Message);
            return Problem(ex.Message);
        }
    }

    [HttpPost("filtercount")]
    public virtual async Task<ActionResult<long>> GetCountAsync(IQuery<T> query, CancellationToken cancellationToken = default)
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug($"[EntityController] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetAsync\"]");

        try
        {
            return Ok(await repository.GetCountAsync(query, cancellationToken));
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex.Message);
            return Problem(ex.Message);
        }
    }

    [HttpPost("search")]
    public virtual async Task<ActionResult<PageResponse<T>>> GetPagedListAsync([FromBody] IQuery<T> query, CancellationToken cancellationToken = default)
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug($"[EntityController] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetPagedListAsync\"]");

        try
        {
            return Ok(await repository.GetPagedListAsync(query, cancellationToken));
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex.Message);
            return Problem(ex.Message);
        }
    }

    [HttpPost("select")]
    public virtual async Task<ActionResult<PageResponse<TResult>>> GetPagedListAsync<TResult>(IQuery<T, TResult> query, CancellationToken cancellationToken = default)
            where TResult : class, new()
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug($"[EntityController] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetPagedListAsync\"]");

        try
        {
            return Ok(await repository.GetPagedListAsync(query, cancellationToken));
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex.Message);
            return Problem(ex.Message);
        }
    }
}

public abstract class EntityController<T, DataTransferT>(IRepository<T> repository, ILogger<EntityController<T, DataTransferT>> logger)
    : EntityController<T, DataTransferT, KeyType>(repository, logger), IEntityController<T, DataTransferT>
        where T : class, IEntity, new()
        where DataTransferT : class, IModel, new();

