using System.Net;

namespace Craft.Exceptions;

/// <summary>
/// Exception thrown when model validation fails.
/// Contains detailed validation error information for each property that failed validation.
/// Returns HTTP 400 Bad Request status code.
/// </summary>
public class ModelValidationException : CraftException
{
    /// <summary>
    /// Gets the dictionary of validation errors, keyed by property name.
    /// </summary>
    public IDictionary<string, string[]> ValidationErrors { get; } = new Dictionary<string, string[]>();

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelValidationException"/> class with a default message.
    /// </summary>
    public ModelValidationException()
        : base("One or more validation failures have occurred.", [], HttpStatusCode.BadRequest)
    {
        ValidationErrors = new Dictionary<string, string[]>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelValidationException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ModelValidationException(string message)
        : base(message, [], HttpStatusCode.BadRequest) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelValidationException"/> class with a specified error message
    /// and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ModelValidationException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelValidationException"/> class with a specified error message
    /// and validation errors.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="validationErrors">The dictionary of validation errors by property name.</param>
    public ModelValidationException(string message, IDictionary<string, string[]> validationErrors)
        : base(message, [.. validationErrors.SelectMany(kvp => kvp.Value)], HttpStatusCode.BadRequest)
    {
        ValidationErrors = new Dictionary<string, string[]>(validationErrors);
    }
}
