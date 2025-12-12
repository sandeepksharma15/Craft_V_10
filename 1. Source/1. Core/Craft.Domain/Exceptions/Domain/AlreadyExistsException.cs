using System.Net;

namespace Craft.Domain;

/// <summary>
/// Exception thrown when attempting to create a resource that already exists.
/// Returns HTTP 422 Unprocessable Entity status code.
/// </summary>
public class AlreadyExistsException : CraftException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AlreadyExistsException"/> class with a default message.
    /// </summary>
    public AlreadyExistsException()
        : base("This resource already exists") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AlreadyExistsException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public AlreadyExistsException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AlreadyExistsException"/> class with a specified error message
    /// and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public AlreadyExistsException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AlreadyExistsException"/> class with a specified error message,
    /// error collection, and HTTP status code.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="errors">The collection of error messages.</param>
    /// <param name="statusCode">The HTTP status code for this exception.</param>
    public AlreadyExistsException(string message, List<string> errors = default!,
        HttpStatusCode statusCode = HttpStatusCode.UnprocessableEntity) : base(message, errors, statusCode) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AlreadyExistsException"/> class for a specific entity.
    /// </summary>
    /// <param name="name">The entity type name.</param>
    /// <param name="key">The entity identifier.</param>
    public AlreadyExistsException(string name, object key)
        : base($"Entity \"{name}\" ({key}) already exists", (List<string>?)null, HttpStatusCode.UnprocessableEntity) { }
}
