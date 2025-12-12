using Craft.Controllers;
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
/// <typeparam name="T">The entity type.</typeparam>
/// <typeparam name="DataTransferT">The data transfer object type.</typeparam>
/// <typeparam name="TKey">The entity key type.</typeparam>
/// <remarks>
/// <para>
/// This controller provides all CRUD operations from EntityChangeController plus:
/// <list type="bullet">
/// <item>Query-based deletion</item>
/// <item>Advanced search with filtering and sorting</item>
/// <item>Projection queries (select specific fields)</item>
/// <item>Count queries</item>
/// <item>Paginated queries with sorting and filtering</item>
/// </list>
/// </para>
/// <para>
/// All query operations use POST method with IQuery&lt;T&gt; in the request body to support
/// complex query specifications that exceed URL length limits.
/// </para>
/// </remarks>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[Consumes("application/json")]
public abstract class EntityController<T, DataTransferT, TKey>(IRepository<T, TKey> repository, ILogger<EntityController<T, DataTransferT, TKey>> logger)
    : EntityChangeController<T, DataTransferT, TKey>(repository, logger), IEntityController<T, DataTransferT, TKey>
        where T : class, IEntity<TKey>, new()
        where DataTransferT : class, IModel<TKey>, new()
{
    /// <summary>
    /// Deletes entities matching the specified query criteria.
    /// </summary>
    /// <param name="query">The query specification containing filter criteria.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Success response if entities are deleted.</returns>
    /// <remarks>
    /// <para>
    /// This endpoint allows bulk deletion based on complex query criteria.
    /// All entities matching the query will be deleted in a single operation.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// <code>
    /// POST /api/entity/deletebyquery
    /// {
    ///   "filters": [
    ///     {
    ///       "field": "status",
    ///       "operator": "equals",
    ///       "value": "inactive"
    ///     }
    ///   ]
    /// }
    /// </code>
    /// </para>
    /// <para>
    /// <strong>Warning:</strong> This is a bulk operation. Ensure your query criteria are correct
    /// to avoid unintended deletions.
    /// </para>
    /// </remarks>
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
            logger.LogCritical(ex.Message);
            return Problem(ex.Message);
        }
    }

    /// <summary>
    /// Searches for entities using advanced query criteria.
    /// </summary>
    /// <param name="query">The query specification containing filter, sort, and other criteria.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A collection of entities matching the query criteria.</returns>
    /// <remarks>
    /// <para>
    /// This endpoint supports advanced querying with:
    /// <list type="bullet">
    /// <item>Complex filtering (multiple conditions, operators)</item>
    /// <item>Sorting (multiple fields, ascending/descending)</item>
    /// <item>Includes (eager loading of related entities)</item>
    /// <item>Tracking control (AsNoTracking for read-only queries)</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// <code>
    /// POST /api/entity/searchall
    /// {
    ///   "filters": [
    ///     {
    ///       "field": "category",
    ///       "operator": "equals",
    ///       "value": "Electronics"
    ///     },
    ///     {
    ///       "field": "price",
    ///       "operator": "lessThan",
    ///       "value": 1000
    ///     }
    ///   ],
    ///   "sortOrder": [
    ///     {
    ///       "field": "price",
    ///       "direction": "ascending"
    ///     }
    ///   ],
    ///   "asNoTracking": true
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    /// <response code="200">Returns the collection of matching entities.</response>
    /// <response code="400">If the query specification is invalid.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpPost("searchall")]
    [ProducesResponseType(typeof(List<>), StatusCodes.Status200OK)]
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
            logger.LogCritical(ex.Message);
            return Problem(ex.Message);
        }
    }

    /// <summary>
    /// Searches for entities and projects results to a specific type.
    /// </summary>
    /// <typeparam name="TResult">The result type to project to.</typeparam>
    /// <param name="query">The query specification with projection selector.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A collection of projected results.</returns>
    /// <remarks>
    /// <para>
    /// This endpoint allows selecting specific fields from entities and projecting them to a different type.
    /// Useful for optimizing queries by retrieving only needed fields.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// <code>
    /// POST /api/entity/searchallselect
    /// {
    ///   "filters": [
    ///     {
    ///       "field": "isActive",
    ///       "operator": "equals",
    ///       "value": true
    ///     }
    ///   ],
    ///   "selector": {
    ///     "fields": ["id", "name", "email"]
    ///   }
    /// }
    /// </code>
    /// </para>
    /// <para>
    /// <strong>Benefits:</strong>
    /// <list type="bullet">
    /// <item>Reduced data transfer (only selected fields)</item>
    /// <item>Better performance (fewer columns retrieved)</item>
    /// <item>Type-safe projections</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <response code="200">Returns the collection of projected results.</response>
    /// <response code="400">If the query specification or projection is invalid.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpPost("searchallselect")]
    [ProducesResponseType(typeof(List<>), StatusCodes.Status200OK)]
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
            logger.LogCritical(ex.Message);
            return Problem(ex.Message);
        }
    }

    /// <summary>
    /// Retrieves a single entity matching the query criteria.
    /// </summary>
    /// <param name="query">The query specification to find the entity.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The matching entity if found, otherwise NotFound.</returns>
    /// <remarks>
    /// <para>
    /// Returns the first entity matching the query criteria. If multiple entities match,
    /// only the first one is returned. Use sorting in the query to control which entity is returned.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// <code>
    /// POST /api/entity/queryone
    /// {
    ///   "filters": [
    ///     {
    ///       "field": "email",
    ///       "operator": "equals",
    ///       "value": "user@example.com"
    ///     }
    ///   ],
    ///   "includes": ["profile", "orders"]
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    /// <response code="200">Returns the matching entity.</response>
    /// <response code="404">If no entity matches the query.</response>
    /// <response code="400">If the query specification is invalid.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpPost("queryone")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
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
            logger.LogCritical(ex.Message);
            return Problem(ex.Message);
        }
    }

    /// <summary>
    /// Retrieves a single entity with projection to a specific result type.
    /// </summary>
    /// <typeparam name="TResult">The result type to project to.</typeparam>
    /// <param name="query">The query specification with projection selector.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The projected result if found, otherwise NotFound.</returns>
    /// <remarks>
    /// <para>
    /// Combines single entity retrieval with projection. Useful for returning only specific fields
    /// of an entity instead of the entire object.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// <code>
    /// POST /api/entity/queryoneselect
    /// {
    ///   "filters": [
    ///     {
    ///       "field": "id",
    ///       "operator": "equals",
    ///       "value": 123
    ///     }
    ///   ],
    ///   "selector": {
    ///     "fields": ["name", "email", "phoneNumber"]
    ///   }
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    /// <response code="200">Returns the projected result.</response>
    /// <response code="404">If no entity matches the query.</response>
    /// <response code="400">If the query specification or projection is invalid.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpPost("queryoneselect")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
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
            logger.LogCritical(ex.Message);
            return Problem(ex.Message);
        }
    }

    /// <summary>
    /// Gets the count of entities matching the query criteria.
    /// </summary>
    /// <param name="query">The query specification containing filter criteria.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The number of entities matching the query.</returns>
    /// <remarks>
    /// <para>
    /// Returns only the count without retrieving the actual entities. More efficient than
    /// retrieving all entities and counting them in memory.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// <code>
    /// POST /api/entity/countbyquery
    /// {
    ///   "filters": [
    ///     {
    ///       "field": "status",
    ///       "operator": "equals",
    ///       "value": "active"
    ///     },
    ///     {
    ///       "field": "createdDate",
    ///       "operator": "greaterThan",
    ///       "value": "2024-01-01"
    ///     }
    ///   ]
    /// }
    /// </code>
    /// </para>
    /// <para>
    /// <strong>Sample response:</strong>
    /// <code>
    /// 42
    /// </code>
    /// </para>
    /// </remarks>
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
            logger.LogCritical(ex.Message);
            return Problem(ex.Message);
        }
    }

    /// <summary>
    /// Retrieves a paginated list of entities with advanced query support.
    /// </summary>
    /// <param name="query">The query specification containing pagination, filter, and sort criteria.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A paginated response with entities and pagination metadata.</returns>
    /// <remarks>
    /// <para>
    /// Combines pagination with advanced filtering and sorting. Returns both the requested page
    /// of data and metadata about the pagination (total count, pages, etc.).
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// <code>
    /// POST /api/entity/querypaged
    /// {
    ///   "pageNumber": 1,
    ///   "pageSize": 20,
    ///   "filters": [
    ///     {
    ///       "field": "category",
    ///       "operator": "equals",
    ///       "value": "Electronics"
    ///     }
    ///   ],
    ///   "sortOrder": [
    ///     {
    ///       "field": "name",
    ///       "direction": "ascending"
    ///     }
    ///   ]
    /// }
    /// </code>
    /// </para>
    /// <para>
    /// <strong>Sample response:</strong>
    /// <code>
    /// {
    ///   "data": [
    ///     { "id": 1, "name": "Product 1" },
    ///     { "id": 2, "name": "Product 2" }
    ///   ],
    ///   "pageInfo": {
    ///     "currentPage": 1,
    ///     "pageSize": 20,
    ///     "totalCount": 45,
    ///     "totalPages": 3,
    ///     "hasNextPage": true,
    ///     "hasPreviousPage": false
    ///   }
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    /// <response code="200">Returns the paginated list with metadata.</response>
    /// <response code="400">If the query specification is invalid.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpPost("querypaged")]
    [ProducesResponseType(typeof(PageResponse<>), StatusCodes.Status200OK)]
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
            logger.LogCritical(ex.Message);
            return Problem(ex.Message);
        }
    }

    /// <summary>
    /// Retrieves a paginated list with projection to a specific result type.
    /// </summary>
    /// <typeparam name="TResult">The result type to project to.</typeparam>
    /// <param name="query">The query specification with pagination, filter, sort, and projection.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A paginated response with projected results and pagination metadata.</returns>
    /// <remarks>
    /// <para>
    /// The most powerful query endpoint, combining:
    /// <list type="bullet">
    /// <item>Pagination (page number, page size)</item>
    /// <item>Filtering (complex filter criteria)</item>
    /// <item>Sorting (multiple fields)</item>
    /// <item>Projection (select specific fields)</item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// <code>
    /// POST /api/entity/querypagedselect
    /// {
    ///   "pageNumber": 1,
    ///   "pageSize": 10,
    ///   "filters": [
    ///     {
    ///       "field": "isActive",
    ///       "operator": "equals",
    ///       "value": true
    ///     }
    ///   ],
    ///   "sortOrder": [
    ///     {
    ///       "field": "name",
    ///       "direction": "ascending"
    ///     }
    ///   ],
    ///   "selector": {
    ///     "fields": ["id", "name", "email"]
    ///   }
    /// }
    /// </code>
    /// </para>
    /// <para>
    /// <strong>Performance Benefits:</strong>
    /// <list type="bullet">
    /// <item>Only retrieves requested page of data</item>
    /// <item>Only selects specified fields</item>
    /// <item>Efficient database queries with filtering and sorting at DB level</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <response code="200">Returns the paginated list of projected results with metadata.</response>
    /// <response code="400">If the query specification is invalid.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpPost("querypagedselect")]
    [ProducesResponseType(typeof(PageResponse<>), StatusCodes.Status200OK)]
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
            logger.LogCritical(ex.Message);
            return Problem(ex.Message);
        }
    }
}

public abstract class EntityController<T, DataTransferT>(IRepository<T> repository, ILogger<EntityController<T, DataTransferT>> logger)
    : EntityController<T, DataTransferT, KeyType>(repository, logger), IEntityController<T, DataTransferT>
        where T : class, IEntity, new()
        where DataTransferT : class, IModel, new();

