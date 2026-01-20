using Craft.Controllers.ErrorHandling.Models;

namespace Craft.Controllers.ErrorHandling.Strategies;

/// <summary>
/// Handles PostgreSQL-specific database errors using SqlState codes.
/// </summary>
public sealed class PostgreSqlErrorStrategy : IDatabaseErrorStrategy
{
    /// <summary>
    /// PostgreSQL errors have SqlState codes.
    /// </summary>
    public bool CanHandle(ErrorContext context) => !string.IsNullOrEmpty(context.SqlState);

    /// <summary>
    /// Priority 1 - PostgreSQL has specific error codes, so check it first.
    /// </summary>
    public int Priority => 1;

    /// <summary>
    /// Gets PostgreSQL-specific error messages based on SqlState.
    /// </summary>
    public string GetErrorMessage(ErrorContext context)
    {
        var sqlState = context.SqlState!;
        var entityName = context.EntityName;
        var fieldName = context.FieldName;
        var messageText = context.MessageText;

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
}
