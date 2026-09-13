using Craft.Controllers;
using Craft.Controllers.ErrorHandling;
using Craft.Core;
using Craft.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Craft.QuerySpec.Services;

/// <summary>
/// Extended controller providing CRUD operations with advanced query support using Craft.QuerySpec.
/// Extends EntityChangeController with query-based operations for filtering, sorting, and pagination.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[Consumes("application/json")]
public abstract class EntityController<T, DataTransferT, TKey>(IRepository<T, TKey> repository,
    ILogger<EntityController<T, DataTransferT, TKey>> logger, IDatabaseErrorHandler databaseErrorHandler)
    : EntityChangeController<T, DataTransferT, TKey>(repository, logger, databaseErrorHandler), IEntityController<T, DataTransferT, TKey>
        where T : class, IEntity<TKey>, new()
        where DataTransferT : class, IModel<TKey>, new()
{
    /// <summary>
    /// Deletes entities matching the specified query criteria.
    /// </summary>
    /// <response code="200">If entities are deleted successfully.</response>
    /// <response code="400">If the query specification is invalid.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpPost("deletebyquery")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult> DeleteAsync([FromBody] IQuery<T> query, CancellationToken cancellationToken = default)
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
            logger.LogError(ex, "[EntityController] Error in DeleteAsync for {EntityType}", typeof(T).Name);
            return BadRequest(new[] { $"Failed to delete {typeof(T).Name.ToLower()}: {ex.Message}" });
        }
    }

    /// <summary>
    /// Searches for entities using advanced query criteria.
    /// </summary>
    /// <response code="200">Returns the collection of matching entities.</response>
    /// <response code="400">If the query specification is invalid.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpPost("searchall")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult<List<T>>> GetAllAsync([FromBody] IQuery<T> query, CancellationToken cancellationToken = default)
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug($"[EntityController] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetAllAsync\"]");

        try
        {
            return Ok(await repository.GetAllAsync(query, cancellationToken));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[EntityController] Error in GetAllAsync for {EntityType}", typeof(T).Name);
            return BadRequest(new[] { $"Failed to search {typeof(T).Name.ToLower()}: {ex.Message}" });
        }
    }

    /// <summary>
    /// Searches for entities and projects results to a specific type.
    /// </summary>
    /// <response code="200">Returns the collection of projected results.</response>
    /// <response code="400">If the query specification or projection is invalid.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpPost("searchallselect")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult<List<TResult>>> GetAllAsync<TResult>([FromBody] IQuery<T, TResult> query,
        CancellationToken cancellationToken = default) where TResult : class, new()
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug($"[EntityController] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetAllAsync\"]");
        try
        {
            return Ok(await repository.GetAllAsync<TResult>(query, cancellationToken));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[EntityController] Error in GetAllAsync<{ResultType}> for {EntityType}", typeof(TResult).Name, typeof(T).Name);
            return BadRequest(new[] { $"Failed to search {typeof(T).Name.ToLower()}: {ex.Message}" });
        }
    }

    /// <summary>
    /// Retrieves a single entity matching the query criteria.
    /// </summary>
    /// <response code="200">Returns the matching entity.</response>
    /// <response code="404">If no entity matches the query.</response>
    /// <response code="400">If the query specification is invalid.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpPost("queryone")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult<T>> GetAsync([FromBody] IQuery<T> query, CancellationToken cancellationToken = default)
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
            logger.LogError(ex, "[EntityController] Error in GetAsync for {EntityType}", typeof(T).Name);
            return BadRequest(new[] { $"Failed to retrieve {typeof(T).Name.ToLower()}: {ex.Message}" });
        }
    }

    /// <summary>
    /// Retrieves a single entity with projection to a specific result type.
    /// </summary>
    /// <response code="200">Returns the projected result.</response>
    /// <response code="404">If no entity matches the query.</response>
    /// <response code="400">If the query specification or projection is invalid.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpPost("queryoneselect")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult<TResult>> GetAsync<TResult>([FromBody] IQuery<T, TResult> query,
        CancellationToken cancellationToken = default) where TResult : class, new()
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
            logger.LogError(ex, "[EntityController] Error in GetAsync<{ResultType}> for {EntityType}", typeof(TResult).Name, typeof(T).Name);
            return BadRequest(new[] { $"Failed to retrieve {typeof(T).Name.ToLower()}: {ex.Message}" });
        }
    }

    /// <summary>
    /// Gets the count of entities matching the query criteria.
    /// </summary>
    /// <response code="200">Returns the count of matching entities.</response>
    /// <response code="400">If the query specification is invalid.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpPost("countbyquery")]
    [ProducesResponseType(typeof(long), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult<long>> GetCountAsync([FromBody] IQuery<T> query, CancellationToken cancellationToken = default)
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug($"[EntityController] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetAsync\"]");

        try
        {
            return Ok(await repository.GetCountAsync(query, cancellationToken));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[EntityController] Error in GetCountAsync for {EntityType}", typeof(T).Name);
            return BadRequest(new[] { $"Failed to count {typeof(T).Name.ToLower()}: {ex.Message}" });
        }
    }

    /// <summary>
    /// Retrieves a paginated list of entities with advanced query support.
    /// </summary>
    /// <response code="200">Returns the paginated list with metadata.</response>
    /// <response code="400">If the query specification is invalid.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpPost("querypaged")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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
            logger.LogError(ex, "[EntityController] Error in GetPagedListAsync for {EntityType}", typeof(T).Name);
            return BadRequest(new[] { $"Failed to retrieve paged list of {typeof(T).Name.ToLower()}: {ex.Message}" });
        }
    }

    /// <summary>
    /// Retrieves a paginated list with projection to a specific result type.
    /// </summary>
    /// <response code="200">Returns the paginated list of projected results with metadata.</response>
    /// <response code="400">If the query specification is invalid.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpPost("querypagedselect")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult<PageResponse<TResult>>> GetPagedListAsync<TResult>([FromBody] IQuery<T, TResult> query,
        CancellationToken cancellationToken = default) where TResult : class, new()
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug($"[EntityController] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetPagedListAsync\"]");

        try
        {
            return Ok(await repository.GetPagedListAsync(query, cancellationToken));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[EntityController] Error in GetPagedListAsync<{ResultType}> for {EntityType}", typeof(TResult).Name, typeof(T).Name);
            return BadRequest(new[] { $"Failed to retrieve paged list of {typeof(T).Name.ToLower()}: {ex.Message}" });
        }
    }
}

public abstract class EntityController<T, DataTransferT>(
IRepository<T> repository,
ILogger<EntityController<T, DataTransferT>> logger,
IDatabaseErrorHandler databaseErrorHandler)
: EntityController<T, DataTransferT, KeyType>(repository, logger, databaseErrorHandler), IEntityController<T, DataTransferT>
    where T : class, IEntity, new()
    where DataTransferT : class, IModel, new();

