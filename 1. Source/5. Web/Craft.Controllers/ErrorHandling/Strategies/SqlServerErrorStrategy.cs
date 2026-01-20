using Craft.Controllers.ErrorHandling.Models;

namespace Craft.Controllers.ErrorHandling.Strategies;

/// <summary>
/// Handles SQL Server-specific database errors using error numbers.
/// </summary>
public sealed class SqlServerErrorStrategy : IDatabaseErrorStrategy
{
    /// <summary>
    /// SQL Server errors have error numbers.
    /// </summary>
    public bool CanHandle(ErrorContext context) => context.ErrorNumber.HasValue;

    /// <summary>
    /// Priority 2 - Check SQL Server after PostgreSQL.
    /// </summary>
    public int Priority => 2;

    /// <summary>
    /// Gets SQL Server-specific error messages based on error number.
    /// </summary>
    public string GetErrorMessage(ErrorContext context)
    {
        var errorNumber = context.ErrorNumber!.Value;
        var entityName = context.EntityName;
        var fieldName = context.FieldName;
        var messageText = context.MessageText;

        return errorNumber switch
        {
            // Unique constraint/index violation
            2601 or 2627 => fieldName != null
                ? $"A {entityName} with this {fieldName} already exists. Please use a different value."
                : $"A {entityName} with this value already exists. Please use a different value.",

            // Foreign key violation / Check constraint violation (both use 547)
            547 => messageText.Contains("foreign key", StringComparison.CurrentCultureIgnoreCase)
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
}
