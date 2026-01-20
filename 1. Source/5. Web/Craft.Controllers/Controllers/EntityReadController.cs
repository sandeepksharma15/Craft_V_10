using Craft.Core;
using Craft.Domain;
using Craft.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Craft.Controllers;

/// <summary>
/// Base controller providing read-only operations for entities.
/// Supports CRUD read operations, pagination, and filtering.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
/// <typeparam name="DataTransferT">The data transfer object type.</typeparam>
/// <typeparam name="TKey">The entity key type.</typeparam>
/// <remarks>
/// <para>
/// This controller provides the following read operations:
/// <list type="bullet">
/// <item>Get all entities (with optional details)</item>
/// <item>Get entity by ID (with optional details)</item>
/// <item>Get entity count</item>
/// <item>Get paginated list of entities</item>
/// </list>
/// </para>
/// <para>
/// All operations support cancellation tokens and are fully asynchronous.
/// </para>
/// </remarks>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[Consumes("application/json")]
public class EntityReadController<T, DataTransferT, TKey>(IReadRepository<T, TKey> repository, ILogger<EntityReadController<T, DataTransferT, TKey>> logger)
    : ControllerBase, IEntityReadController<T, TKey> where T : class, IEntity<TKey>, new()
        where DataTransferT : class, IModel<TKey>, new()

{
    /// <summary>
    /// Retrieves all entities from the repository.
    /// </summary>
    /// <param name="includeDetails">Whether to include related entity details.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A collection of all entities.</returns>
    /// <response code="200">Returns the collection of entities.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpGet("{includeDetails:bool}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult<IAsyncEnumerable<T>>> GetAllAsync(bool includeDetails, CancellationToken cancellationToken = default)
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug($"[EntityReadController] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetAllAsync\"]");

        try
        {
            return Ok(await repository.GetAllAsync(includeDetails, cancellationToken));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[EntityReadController] Error in GetAllAsync for {EntityType}", typeof(T).Name);
            return BadRequest(new[] { $"Failed to retrieve {typeof(T).Name.ToLower()} list: {ex.Message}" });
        }
    }

    /// <summary>
    /// Retrieves a specific entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <param name="includeDetails">Whether to include related entity details.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The entity if found, otherwise NotFound.</returns>
    /// <response code="200">Returns the requested entity.</response>
    /// <response code="404">If the entity is not found.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpGet("{id}/{includeDetails:bool}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult<T>> GetAsync(TKey id, bool includeDetails, CancellationToken cancellationToken = default)
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug($"[EntityReadController] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetAsync\"] Id: [\"{id}\"]");

        try
        {
            T? entity = await repository.GetAsync(id, includeDetails, cancellationToken);

            return entity == null ? NotFound() : Ok(entity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[EntityReadController] Error in GetAsync for {EntityType} with ID {Id}", typeof(T).Name, id);
            return BadRequest(new[] { $"Failed to retrieve {typeof(T).Name.ToLower()} with ID {id}: {ex.Message}" });
        }
    }

    /// <summary>
    /// Gets the total count of entities in the repository.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The total number of entities.</returns>
    /// <response code="200">Returns the total count of entities.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpGet]
    [Route("count")]
    [ProducesResponseType(typeof(long), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult<long>> GetCountAsync(CancellationToken cancellationToken = default)
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug($"[EntityReadController] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetCountAsync\"]");

        try
        {
            return Ok(await repository.GetCountAsync(cancellationToken));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[EntityReadController] Error in GetCountAsync for {EntityType}", typeof(T).Name);
            return BadRequest(new[] { $"Failed to get count of {typeof(T).Name.ToLower()}: {ex.Message}" });
        }
    }

    /// <summary>
    /// Retrieves a paginated list of entities.
    /// </summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="includeDetails">Whether to include related entity details.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A paginated response containing the requested page of entities and pagination metadata.</returns>
    /// <response code="200">Returns the paginated list of entities.</response>
    /// <response code="400">If page or pageSize parameters are invalid.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpGet]
    [Route("getpaged/{page:int}/{pageSize:int}/{includeDetails:bool}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult<PageResponse<T>>> GetPagedListAsync(int page, int pageSize, bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug($"[EntityReadController] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetPagedListAsync\"] Page: [{page}] PageSize: [{pageSize}] IncludeDetails: [{includeDetails}]");

        try
        {
            return Ok(await repository.GetPagedListAsync(page, pageSize, includeDetails, cancellationToken));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[EntityReadController] Error in GetPagedListAsync for {EntityType}", typeof(T).Name);
            return BadRequest(new[] { $"Failed to retrieve paged list of {typeof(T).Name.ToLower()}: {ex.Message}" });
        }
    }
}

public class EntityReadController<T, DataTransferT>(IReadRepository<T> repository, ILogger<EntityReadController<T, DataTransferT>> logger)
    : EntityReadController<T, DataTransferT, KeyType>(repository, logger), IEntityReadController<T>
        where T : class, IEntity, new()
        where DataTransferT : class, IModel, new();

