using System.Net;

namespace Craft.Domain;

/// <summary>
/// Base class for all custom exceptions in the Craft framework.
/// Provides common properties for HTTP status codes and error collections.
/// </summary>
public abstract class CraftException : Exception
{
    /// <summary>
    /// Gets the collection of error messages associated with this exception.
    /// </summary>
    public List<string> Errors { get; } = [];

    /// <summary>
    /// Gets the HTTP status code associated with this exception.
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CraftException"/> class.
    /// </summary>
    protected CraftException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="CraftException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    protected CraftException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="CraftException"/> class with a specified error message,
    /// inner exception, and HTTP status code.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    /// <param name="statusCode">The HTTP status code for this exception.</param>
    protected CraftException(string message, Exception innerException, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CraftException"/> class with a specified error message,
    /// error collection, and HTTP status code.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="errors">The collection of error messages.</param>
    /// <param name="statusCode">The HTTP status code for this exception.</param>
    protected CraftException(string message, List<string>? errors = default!,
        HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(message)
    {
        Errors = errors ?? [];
        StatusCode = statusCode;
    }
}
