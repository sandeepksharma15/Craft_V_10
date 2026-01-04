using Craft.Domain;
using Craft.Repositories;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Craft.Controllers;

/// <summary>
/// Base controller providing full CRUD operations (Create, Read, Update, Delete) for entities.
/// Extends EntityReadController with write operations.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
/// <typeparam name="DataTransferT">The data transfer object type.</typeparam>
/// <typeparam name="TKey">The entity key type.</typeparam>
/// <remarks>
/// <para>
/// This controller provides the following operations:
/// <list type="bullet">
/// <item>All read operations from EntityReadController</item>
/// <item>Create single and multiple entities</item>
/// <item>Update single and multiple entities</item>
/// <item>Delete single and multiple entities</item>
/// </list>
/// </para>
/// <para>
/// All operations support:
/// <list type="bullet">
/// <item>Automatic DTO mapping using Mapster</item>
/// <item>Concurrency conflict detection</item>
/// <item>Comprehensive error handling</item>
/// <item>Structured logging</item>
/// </list>
/// </para>
/// </remarks>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[Consumes("application/json")]
public abstract class EntityChangeController<T, DataTransferT, TKey>(IChangeRepository<T, TKey> repository, ILogger<EntityChangeController<T, DataTransferT, TKey>> logger)
    : EntityReadController<T, DataTransferT, TKey>(repository, logger), IEntityChangeController<T, DataTransferT, TKey>
        where T : class, IEntity<TKey>, new()
        where DataTransferT : class, IModel<TKey>, new()
{
    protected virtual ActionResult<T> ReturnProperError(Exception ex)
    {
        return Problem(ex.Message);
    }

    /// <summary>
    /// Creates a new entity in the repository.
    /// </summary>
    /// <param name="model">The data transfer object containing entity data.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The created entity with generated ID.</returns>
    /// <remarks>
    /// <para>
    /// The entity is automatically mapped from the DTO using Mapster.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// <code>
    /// POST /api/entity
    /// {
    ///   "name": "New Entity",
    ///   "description": "Entity Description"
    /// }
    /// </code>
    /// </para>
    /// <para>
    /// <strong>Sample response (201 Created):</strong>
    /// <code>
    /// {
    ///   "id": 123,
    ///   "name": "New Entity",
    ///   "description": "Entity Description"
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    /// <response code="201">Returns the newly created entity.</response>
    /// <response code="400">If the model is invalid.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult<T>> AddAsync(DataTransferT model, CancellationToken cancellationToken = default)
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug($"[EntityChangeController] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"AddAsync\"]");

        try
        {
            T entity = await repository.AddAsync(model.Adapt<T>(), cancellationToken: cancellationToken);
            return Created(new Uri($"{entity.Id}/false", UriKind.Relative), entity);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex.Message);
            return ReturnProperError(ex);
        }
    }

    /// <summary>
    /// Creates multiple entities in a single batch operation.
    /// </summary>
    /// <param name="models">The collection of data transfer objects containing entity data.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Success response if all entities are created.</returns>
    /// <remarks>
    /// <para>
    /// All entities are created in a single transaction. If any entity fails, the entire operation is rolled back.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// <code>
    /// POST /api/entity/addrange
    /// [
    ///   {
    ///     "name": "Entity 1",
    ///     "description": "Description 1"
    ///   },
    ///   {
    ///     "name": "Entity 2",
    ///     "description": "Description 2"
    ///   }
    /// ]
    /// </code>
    /// </para>
    /// </remarks>
    /// <response code="200">If all entities are created successfully.</response>
    /// <response code="400">If any model is invalid.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpPost("addrange")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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

    /// <summary>
    /// Deletes an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to delete.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Success response if the entity is deleted.</returns>
    /// <remarks>
    /// <para>
    /// If the entity is not found, returns NotFound (404).
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// <code>
    /// DELETE /api/entity/123
    /// </code>
    /// </para>
    /// </remarks>
    /// <response code="200">If the entity is deleted successfully.</response>
    /// <response code="404">If the entity is not found.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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

    /// <summary>
    /// Deletes multiple entities in a single batch operation.
    /// </summary>
    /// <param name="models">The collection of data transfer objects identifying entities to delete.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Success response if all entities are deleted.</returns>
    /// <remarks>
    /// <para>
    /// All entities are deleted in a single transaction. If any entity fails, the entire operation is rolled back.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// <code>
    /// PUT /api/entity/deleterange
    /// [
    ///   { "id": 1 },
    ///   { "id": 2 },
    ///   { "id": 3 }
    /// ]
    /// </code>
    /// </para>
    /// </remarks>
    /// <response code="200">If all entities are deleted successfully.</response>
    /// <response code="400">If any model is invalid.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpPut("deleterange")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    /// <param name="model">The data transfer object containing updated entity data.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The updated entity.</returns>
    /// <remarks>
    /// <para>
    /// Automatically handles concurrency conflicts using optimistic concurrency control.
    /// If the entity has been modified by another user, returns a 409 Conflict with a descriptive message.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// <code>
    /// PUT /api/entity
    /// {
    ///   "id": 123,
    ///   "name": "Updated Name",
    ///   "description": "Updated Description"
    /// }
    /// </code>
    /// </para>
    /// <para>
    /// <strong>Sample response (200 OK):</strong>
    /// <code>
    /// {
    ///   "id": 123,
    ///   "name": "Updated Name",
    ///   "description": "Updated Description"
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    /// <response code="200">Returns the updated entity.</response>
    /// <response code="400">If the model is invalid.</response>
    /// <response code="404">If the entity is not found.</response>
    /// <response code="409">If a concurrency conflict occurs.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult<T>> UpdateAsync(DataTransferT model, CancellationToken cancellationToken = default)
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug($"[EntityChangeController] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"UpdateAsync\"] Id: [\"{model.Id}\"]");

        try
        {
            // Check if entity exists before attempting update
            T? existingEntity = await repository.GetAsync(model.Id, cancellationToken: cancellationToken);

            if (existingEntity == null)
                return NotFound();

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

    /// <summary>
    /// Updates multiple entities in a single batch operation.
    /// </summary>
    /// <param name="models">The collection of data transfer objects containing updated entity data.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Success response if all entities are updated.</returns>
    /// <remarks>
    /// <para>
    /// All entities are updated in a single transaction. If any entity fails, the entire operation is rolled back.
    /// </para>
    /// <para>
    /// <strong>Sample request:</strong>
    /// <code>
    /// POST /api/entity/updaterange
    /// [
    ///   {
    ///     "id": 1,
    ///     "name": "Updated Entity 1"
    ///   },
    ///   {
    ///     "id": 2,
    ///     "name": "Updated Entity 2"
    ///   }
    /// ]
    /// </code>
    /// </para>
    /// </remarks>
    /// <response code="200">If all entities are updated successfully.</response>
    /// <response code="400">If any model is invalid.</response>
    /// <response code="409">If a concurrency conflict occurs.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpPost("updaterange")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult> UpdateRangeAsync(IEnumerable<DataTransferT> models, CancellationToken cancellationToken = default)
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug($"[EntityChangeController] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"UpdateRangeAsync\"]");

        try
        {
            // Validate that all entities exist before attempting batch update
            var modelsList = models.ToList();
            var ids = modelsList.Select(m => m.Id).ToList();
            var existingEntities = new List<T>();

            foreach (var id in ids)
            {
                var entity = await repository.GetAsync(id, cancellationToken: cancellationToken);
                if (entity == null)
                    return NotFound($"Entity with ID {id} not found");
                existingEntities.Add(entity);
            }

            await repository.UpdateRangeAsync(modelsList.Adapt<IEnumerable<T>>(), cancellationToken: cancellationToken);
            return Ok();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex.Message);
            return Problem("The values in database have changed. Please update again with updated values.", statusCode: StatusCodes.Status409Conflict);
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

