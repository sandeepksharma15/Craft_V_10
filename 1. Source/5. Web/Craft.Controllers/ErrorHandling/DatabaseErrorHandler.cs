using Craft.Controllers.ErrorHandling.Models;
using Craft.Controllers.ErrorHandling.Strategies;
using Humanizer;
using Microsoft.Extensions.Logging;

namespace Craft.Controllers.ErrorHandling;

/// <summary>
/// Service that orchestrates database error handling using strategies.
/// </summary>
public sealed class DatabaseErrorHandler : IDatabaseErrorHandler
{
    private readonly IEnumerable<IDatabaseErrorStrategy> _strategies;
    private readonly ILogger<DatabaseErrorHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseErrorHandler"/> class.
    /// </summary>
    /// <param name="strategies">The collection of error handling strategies.</param>
    /// <param name="logger">The logger instance.</param>
    public DatabaseErrorHandler(
        IEnumerable<IDatabaseErrorStrategy> strategies,
        ILogger<DatabaseErrorHandler> logger)
    {
        _strategies = strategies.OrderBy(s => s.Priority);
        _logger = logger;
    }

    /// <summary>
    /// Handles a database exception and returns a user-friendly error message.
    /// </summary>
    public string HandleException(Exception exception, Type entityType)
    {
        var entityName = entityType.Name.Humanize();

        // Extract error information using reflection
        var sqlState = GetPropertyValue<string>(exception, "SqlState");
        var constraintName = GetPropertyValue<string>(exception, "ConstraintName");
        var messageText = GetPropertyValue<string>(exception, "MessageText")
                       ?? GetPropertyValue<string>(exception, "Message")
                       ?? exception.Message;
        var errorNumber = GetPropertyValue<int?>(exception, "Number");
        var fieldName = ExtractFieldNameFromConstraint(constraintName);

        // Create error context
        var context = new ErrorContext
        {
            Exception = exception,
            EntityName = entityName,
            FieldName = fieldName,
            SqlState = sqlState,
            ErrorNumber = errorNumber,
            ConstraintName = constraintName,
            MessageText = messageText
        };

        // Log technical details
        _logger.LogWarning(
            "[DatabaseErrorHandler] Database Error - Type: {ExceptionType}, SqlState: {SqlState}, " +
            "ErrorNumber: {ErrorNumber}, Constraint: {Constraint}, Message: {Message}",
            context.ExceptionTypeName,
            sqlState ?? "N/A",
            errorNumber?.ToString() ?? "N/A",
            constraintName ?? "N/A",
            messageText);

        // Find and use the first strategy that can handle this error
        var strategy = _strategies.FirstOrDefault(s => s.CanHandle(context));

        if (strategy == null)
        {
            _logger.LogError("[DatabaseErrorHandler] No strategy found to handle exception of type {ExceptionType}",
                exception.GetType().Name);
            return "An error occurred while saving to the database. Please check your input and try again.";
        }

        return strategy.GetErrorMessage(context);
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
    private static string? ExtractFieldNameFromConstraint(string? constraintName)
    {
        if (string.IsNullOrWhiteSpace(constraintName))
            return null;

        try
        {
            // EF Core default pattern: PREFIX_TableName_FieldName(s)
            // Examples: IX_Locations_Name, FK_Orders_CustomerId, IX_Weeks_YearId_WeekNumber
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
}
