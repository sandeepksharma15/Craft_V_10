using Craft.Controllers.ErrorHandling.Models;

namespace Craft.Controllers.ErrorHandling.Strategies;

/// <summary>
/// Fallback strategy that parses common error patterns from exception messages.
/// Used when specific database provider strategies cannot handle the error.
/// </summary>
public sealed class GenericErrorStrategy : IDatabaseErrorStrategy
{
    /// <summary>
    /// This strategy can always handle errors as a fallback.
    /// </summary>
    public bool CanHandle(ErrorContext context) => true;

    /// <summary>
    /// Priority 999 - This is the fallback strategy, checked last.
    /// </summary>
    public int Priority => 999;

    /// <summary>
    /// Parses common error patterns from exception messages.
    /// </summary>
    public string GetErrorMessage(ErrorContext context)
    {
        var messageText = context.MessageText;
        var entityName = context.EntityName;
        var fieldName = context.FieldName;
        var lowerMessage = messageText.ToLower();

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
}
