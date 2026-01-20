using Craft.Domain;
using Craft.Repositories;
using Humanizer;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

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
        if (ex is DbUpdateException dbUpdateException)
        {
            // Try to get more specific error information from the inner exception
            if (dbUpdateException.InnerException != null)
            {
                var errorMessage = HandleDatabaseException(dbUpdateException.InnerException);
                return BadRequest(new[] { errorMessage });
            }
            
            // Generic DbUpdateException without specific database details
            logger.LogWarning(dbUpdateException, "[EntityChangeController] DbUpdateException without specific database details");
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
    /// Handles database-specific exceptions using reflection to support multiple database providers.
    /// Currently supports PostgreSQL, SQL Server, MySQL, and SQLite.
    /// </summary>
    /// <param name="innerException">The inner exception from DbUpdateException.</param>
    /// <returns>A user-friendly error message.</returns>
    private string HandleDatabaseException(Exception innerException)
    {
        string entityName = typeof(T).Name.Humanize();  // "Location", "Customer Order"
        string exceptionTypeName = innerException.GetType().Name;

        // Extract common properties using reflection (works for PostgreSQL, SQL Server, etc.)
        string? sqlState = GetPropertyValue<string>(innerException, "SqlState");
        string? constraintName = GetPropertyValue<string>(innerException, "ConstraintName");
        string? messageText = GetPropertyValue<string>(innerException, "MessageText") 
                           ?? GetPropertyValue<string>(innerException, "Message")
                           ?? innerException.Message;

        // For SQL Server
        int? errorNumber = GetPropertyValue<int?>(innerException, "Number");

        string? fieldName = ExtractFieldNameFromConstraint(constraintName);

        // Log technical details
        logger.LogWarning("[EntityChangeController] Database Error - Type: {ExceptionType}, SqlState: {SqlState}, ErrorNumber: {ErrorNumber}, Constraint: {Constraint}, Message: {Message}",
            exceptionTypeName, sqlState ?? "N/A", errorNumber?.ToString() ?? "N/A", constraintName ?? "N/A", messageText);

        // PostgreSQL error codes
        if (!string.IsNullOrEmpty(sqlState))
            return GetPostgreSqlErrorMessage(sqlState, entityName, fieldName, messageText);

        // SQL Server error numbers
        if (errorNumber.HasValue)
            return GetSqlServerErrorMessage(errorNumber.Value, entityName, fieldName, messageText);

        // Parse common error patterns from message text
        return ParseCommonErrorPatterns(messageText, entityName, fieldName);
    }

    /// <summary>
    /// Gets PostgreSQL-specific error messages based on SqlState.
    /// </summary>
    private static string GetPostgreSqlErrorMessage(string sqlState, string entityName, string? fieldName, string messageText)
    {
        return sqlState switch
        {
            // Unique constraint violation
            "23505" => fieldName != null
                ? $"A {entityName} with this {fieldName} already exists. Please use a different value."
                : $"A {entityName} with this value already exists. Please use a different value.",

            // Foreign key violation (on insert/update)
            "23503" => fieldName != null
                ? $"The selected {fieldName} is invalid or does not exist. Please choose a valid option."
                : "The selected value is invalid or does not exist. Please choose a valid option.",

            // Not null violation
            "23502" => fieldName != null
                ? $"The {fieldName} field is required and cannot be empty."
                : "A required field is missing. Please fill in all required fields.",

            // Check constraint violation
            "23514" => fieldName != null
                ? $"The value for {fieldName} does not meet the validation requirements."
                : "One or more values do not meet the validation requirements.",

            // String data right truncation (value too long)
            "22001" => fieldName != null
                ? $"The value for {fieldName} is too long. Please use a shorter value."
                : "One or more values are too long. Please use shorter values.",

            // Invalid text representation (data type error)
            "22P02" => fieldName != null
                ? $"The value for {fieldName} has an invalid format. Please check the data type."
                : "One or more values have an invalid format. Please check your input.",

            // Numeric value out of range
            "22003" => fieldName != null
                ? $"The value for {fieldName} is out of the acceptable range."
                : "One or more numeric values are out of the acceptable range.",

            // Division by zero
            "22012" => "A calculation resulted in division by zero. Please check your numeric values.",

            // Datetime field overflow
            "22008" => fieldName != null
                ? $"The datetime value for {fieldName} is invalid or out of range."
                : "One or more datetime values are invalid or out of range.",

            // Connection failure
            "08000" or "08003" or "08006" => "Unable to connect to the database. Please try again later.",

            // Insufficient resources
            "53000" or "53100" or "53200" or "53300" or "53400" =>
                "The database is currently busy. Please try again in a moment.",

            // Deadlock detected
            "40P01" => "A conflict occurred while processing your request. Please try again.",

            // Serialization failure
            "40001" => "A conflict occurred with another transaction. Please refresh and try again.",

            // Default case for other PostgreSQL errors
            _ => $"Database error: {messageText}"
        };
    }

    /// <summary>
    /// Gets SQL Server-specific error messages based on error number.
    /// </summary>
    private static string GetSqlServerErrorMessage(int errorNumber, string entityName, string? fieldName, string messageText)
    {
        return errorNumber switch
        {
            // Unique constraint/index violation
            2601 or 2627 => fieldName != null
                ? $"A {entityName} with this {fieldName} already exists. Please use a different value."
                : $"A {entityName} with this value already exists. Please use a different value.",

            // Foreign key violation / Check constraint violation (both use 547)
            547 => messageText.ToLower().Contains("foreign key")
                ? (fieldName != null
                    ? $"The selected {fieldName} is invalid or does not exist. Please choose a valid option."
                    : "The selected value is invalid or does not exist. Please choose a valid option.")
                : (fieldName != null
                    ? $"The value for {fieldName} does not meet the validation requirements."
                    : "One or more values do not meet the validation requirements."),

            // Cannot insert null
            515 => fieldName != null
                ? $"The {fieldName} field is required and cannot be empty."
                : "A required field is missing. Please fill in all required fields.",

            // String or binary data would be truncated
            2628 or 8152 => fieldName != null
                ? $"The value for {fieldName} is too long. Please use a shorter value."
                : "One or more values are too long. Please use shorter values.",

            // Arithmetic overflow or conversion error
            8115 or 8116 or 220 or 232 => fieldName != null
                ? $"The value for {fieldName} has an invalid format or is out of range."
                : "One or more values have an invalid format or are out of range.",

            // Deadlock
            1205 => "A conflict occurred while processing your request. Please try again.",

            // Timeout
            -2 => "The operation took too long to complete. Please try again.",

            // Default case
            _ => $"Database error: {messageText}"
        };
    }

    /// <summary>
    /// Parses common error patterns from exception messages when specific error codes aren't available.
    /// </summary>
    private static string ParseCommonErrorPatterns(string messageText, string entityName, string? fieldName)
    {
        string lowerMessage = messageText.ToLower();

        if (lowerMessage.Contains("unique") || lowerMessage.Contains("duplicate"))
        {
            return fieldName != null
                ? $"A {entityName} with this {fieldName} already exists. Please use a different value."
                : $"A {entityName} with this value already exists. Please use a different value.";
        }

        if (lowerMessage.Contains("foreign key") || lowerMessage.Contains("reference"))
        {
            return fieldName != null
                ? $"The selected {fieldName} is invalid or does not exist. Please choose a valid option."
                : "The selected value is invalid or does not exist. Please choose a valid option.";
        }

        if (lowerMessage.Contains("not null") || lowerMessage.Contains("required"))
        {
            return fieldName != null
                ? $"The {fieldName} field is required and cannot be empty."
                : "A required field is missing. Please fill in all required fields.";
        }

        if (lowerMessage.Contains("too long") || lowerMessage.Contains("truncat"))
        {
            return fieldName != null
                ? $"The value for {fieldName} is too long. Please use a shorter value."
                : "One or more values are too long. Please use shorter values.";
        }

        if (lowerMessage.Contains("deadlock"))
        {
            return "A conflict occurred while processing your request. Please try again.";
        }

        // Default generic message
        return "An error occurred while saving to the database. Please check your input and try again.";
    }

    /// <summary>
    /// Gets a property value from an object using reflection.
    /// Returns null if the property doesn't exist or can't be accessed.
    /// </summary>
    private static TValue? GetPropertyValue<TValue>(object obj, string propertyName)
    {
        try
        {
            var property = obj.GetType().GetProperty(propertyName);

            if (property != null)
            {
                var value = property.GetValue(obj);
                if (value is TValue typedValue)
                    return typedValue;
            }
        }
        catch
        {
            // Silently ignore reflection errors
        }

        return default;
    }

    /// <summary>
    /// Attempts to extract a field name from a database constraint name.
    /// Supports EF Core default naming: IX_TableName_FieldName, FK_TableName_FieldName, etc.
    /// Also handles composite keys and humanizes the field names.
    /// </summary>
    /// <param name="constraintName">The constraint name from the database.</param>
    /// <returns>The extracted and humanized field name(s), or null if extraction fails.</returns>
    private static string? ExtractFieldNameFromConstraint(string? constraintName)
    {
        if (string.IsNullOrWhiteSpace(constraintName))
            return null;

        try
        {
            // EF Core default pattern: PREFIX_TableName_FieldName(s)
            // Examples: IX_Locations_Name, FK_Orders_CustomerId, IX_Weeks_YearId_WeekNumber
            // Prefix: IX, FK, PK, CK, UQ, etc.
            
            // Split by underscore and skip the first two parts (prefix and table name)
            var parts = constraintName.Split('_', StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length >= 3)
            {
                // Take everything after PREFIX_TableName_ (starting from index 2)
                var fieldParts = parts.Skip(2).ToArray();
                
                if (fieldParts.Length == 1)
                {
                    // Single field: IX_Locations_Name → "Name"
                    return fieldParts[0].Humanize(LetterCasing.Title);
                }
                else if (fieldParts.Length > 1)
                {
                    // Composite key: IX_Weeks_YearId_WeekNumber → "Year Id and Week Number"
                    var humanizedFields = fieldParts.Select(f => f.Humanize(LetterCasing.Title));
                    return string.Join(" and ", humanizedFields);
                }
            }
        }
        catch
        {
            // Silently ignore parsing errors
        }

        return null;
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
            logger.LogCritical(ex.Message);
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
            logger.LogCritical(ex.Message);
            logger.LogError(ex, "[EntityChangeController] Error in UpdateRangeAsync for {EntityType}: {Message}", typeof(T).Name, ex.Message);
            return BadRequest(new[] { $"Failed to update multiple {typeof(T).Name.ToLower()}: {ex.Message}" });
        }
    }
}

public abstract class EntityChangeController<T, DataTransferT>(IChangeRepository<T> repository, ILogger<EntityChangeController<T, DataTransferT>> logger)
    : EntityChangeController<T, DataTransferT, KeyType>(repository, logger), IEntityChangeController<T, DataTransferT>
        where T : class, IEntity, new()
        where DataTransferT : class, IModel, new();

