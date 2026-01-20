using Craft.Controllers.ErrorHandling;
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
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[Consumes("application/json")]
public abstract class EntityChangeController<T, DataTransferT, TKey>(
    IChangeRepository<T, TKey> repository,
    ILogger<EntityChangeController<T, DataTransferT, TKey>> logger,
    IDatabaseErrorHandler databaseErrorHandler)
    : EntityReadController<T, DataTransferT, TKey>(repository, logger), IEntityChangeController<T, DataTransferT, TKey>
        where T : class, IEntity<TKey>, new()
        where DataTransferT : class, IModel<TKey>, new()
{
    /// <summary>
    /// Handles exceptions and returns appropriate error responses with user-friendly messages.
    /// Logs technical details for debugging while returning sanitized messages to clients.
    /// </summary>
    /// <param name="ex">The exception that occurred.</param>
    /// <returns>A BadRequest response with error messages that can be parsed by the client.</returns>
    protected virtual ActionResult<T> ReturnProperError(Exception ex)
    {
        // Log the full exception details for debugging
        logger.LogError(ex, "[EntityChangeController] Error in {EntityType}: {Message}", typeof(T).Name, ex.Message);

        // Handle DbUpdateException (includes all database constraint violations)
        if (ex is DbUpdateException dbUpdateException && dbUpdateException.InnerException != null)
        {
            var errorMessage = databaseErrorHandler.HandleException(dbUpdateException.InnerException, typeof(T));
            return BadRequest(new[] { errorMessage });
        }

        // Handle DbUpdateException without inner exception
        if (ex is DbUpdateException)
        {
            logger.LogWarning(ex, "[EntityChangeController] DbUpdateException without specific database details");
            return BadRequest(new[] { "An error occurred while saving to the database. Please check your input and try again." });
        }

        // Handle other known exceptions
        if (ex is InvalidOperationException)
            return BadRequest(new[] { "The operation is invalid in the current state. Please verify your data and try again." });

        if (ex is ArgumentException argumentException)
            return BadRequest(new[] { $"Invalid argument: {argumentException.Message}" });

        // For unknown exceptions, log and return a generic message
        logger.LogError(ex, "[EntityChangeController] Unexpected exception in {EntityType}", typeof(T).Name);
        return BadRequest(new[] { "An unexpected error occurred. Please try again or contact support if the issue persists." });
    }

    /// <summary>
    /// Creates a new entity in the repository.
    /// </summary>
    /// <param name="model">The data transfer object containing entity data.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The created entity with generated ID.</returns>
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
    /// <response code="200">If all entities are created successfully.</response>
    /// <response code="400">If any model is invalid.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpPost("addrange")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult<List<T>>> AddRangeAsync(IEnumerable<DataTransferT> models, CancellationToken cancellationToken = default)
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug($"[EntityChangeController] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"AddRangeAsync\"]");

        try
        {
            var addedEntities = await repository.AddRangeAsync(models.Adapt<IEnumerable<T>>(), cancellationToken: cancellationToken);
            return Ok(addedEntities);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[EntityChangeController] Error in AddRangeAsync for {EntityType}: {Message}", typeof(T).Name, ex.Message);
            return BadRequest(new[] { $"Failed to add multiple {typeof(T).Name.ToLower()}: {ex.Message}" });
        }
    }

    /// <summary>
    /// Deletes an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to delete.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Success response if the entity is deleted.</returns>
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
            logger.LogError(ex, "[EntityChangeController] Error in DeleteAsync for {EntityType}: {Message}", typeof(T).Name, ex.Message);
            return BadRequest(new[] { $"Failed to delete {typeof(T).Name.ToLower()}: {ex.Message}" });
        }
    }

    /// <summary>
    /// Deletes multiple entities in a single batch operation.
    /// </summary>
    /// <param name="models">The collection of data transfer objects identifying entities to delete.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Success response if all entities are deleted.</returns>
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
            logger.LogError(ex, "[EntityChangeController] Error in DeleteRangeAsync for {EntityType}: {Message}", typeof(T).Name, ex.Message);
            return BadRequest(new[] { $"Failed to delete multiple {typeof(T).Name.ToLower()}: {ex.Message}" });
        }
    }

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    /// <param name="model">The data transfer object containing updated entity data.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The updated entity.</returns>
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
            return BadRequest(new[] { "The values in database have changed. Please refresh and update again with updated values." });
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
    public virtual async Task<ActionResult<List<T>>> UpdateRangeAsync(IEnumerable<DataTransferT> models, CancellationToken cancellationToken = default)
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

            var updatedEntities = await repository.UpdateRangeAsync(modelsList.Adapt<IEnumerable<T>>(), cancellationToken: cancellationToken);
            return Ok(updatedEntities);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex.Message);
            return BadRequest(new[] { "The values in database have changed. Please refresh and update again with updated values." });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[EntityChangeController] Error in UpdateRangeAsync for {EntityType}: {Message}", typeof(T).Name, ex.Message);
            return BadRequest(new[] { $"Failed to update multiple {typeof(T).Name.ToLower()}: {ex.Message}" });
        }
    }
}

public abstract class EntityChangeController<T, DataTransferT>(
IChangeRepository<T> repository,
ILogger<EntityChangeController<T, DataTransferT>> logger,
IDatabaseErrorHandler databaseErrorHandler)
: EntityChangeController<T, DataTransferT, KeyType>(repository, logger, databaseErrorHandler), IEntityChangeController<T, DataTransferT>
    where T : class, IEntity, new()
    where DataTransferT : class, IModel, new();

