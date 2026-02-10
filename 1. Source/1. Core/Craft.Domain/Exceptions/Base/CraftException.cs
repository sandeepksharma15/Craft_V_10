using System.Net;
using System.Text.Json.Serialization;

namespace Craft.Domain;

/// <summary>
/// Base class for all custom exceptions in the Craft framework.
/// Provides common properties for HTTP status codes and error collections.
/// </summary>
/// <remarks>
/// <para><b>Serialization Note:</b> This exception hierarchy does not implement <see cref="System.Runtime.Serialization.ISerializable"/>
/// because <c>BinaryFormatter</c> is obsolete and removed in .NET 10. For cross-process scenarios,
/// use the <see cref="ToErrorInfo"/> method to create a JSON-serializable representation.</para>
/// </remarks>
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
    /// Gets the integer representation of the HTTP status code.
    /// </summary>
    [JsonIgnore]
    public int StatusCodeValue => (int)StatusCode;

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

    /// <summary>
    /// Creates a JSON-serializable representation of this exception for cross-process communication.
    /// </summary>
    /// <param name="includeStackTrace">Whether to include the stack trace in the output.</param>
    /// <returns>An <see cref="ExceptionInfo"/> instance representing this exception.</returns>
    public virtual ExceptionInfo ToErrorInfo(bool includeStackTrace = false)
        => new()
        {
            ExceptionType = GetType().Name,
            Message = Message,
            StatusCode = (int)StatusCode,
            Errors = Errors.Count > 0 ? [.. Errors] : null,
            StackTrace = includeStackTrace ? StackTrace : null,
            InnerException = InnerException is CraftException craftEx
                ? craftEx.ToErrorInfo(includeStackTrace)
                : InnerException != null
                    ? new ExceptionInfo
                    {
                        ExceptionType = InnerException.GetType().Name,
                        Message = InnerException.Message,
                        StackTrace = includeStackTrace ? InnerException.StackTrace : null
                    }
                    : null
        };
}

/// <summary>
/// A JSON-serializable representation of an exception for cross-process communication.
/// </summary>
/// <remarks>
/// Use this class instead of <see cref="System.Runtime.Serialization.ISerializable"/> for modern .NET
/// applications where JSON serialization is preferred over binary serialization.
/// </remarks>
public sealed class ExceptionInfo
{
    /// <summary>
    /// Gets or sets the type name of the exception.
    /// </summary>
    public string? ExceptionType { get; init; }

    /// <summary>
    /// Gets or sets the exception message.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Gets or sets the HTTP status code, if applicable.
    /// </summary>
    public int? StatusCode { get; init; }

    /// <summary>
    /// Gets or sets the collection of error messages.
    /// </summary>
    public List<string>? Errors { get; init; }

    /// <summary>
    /// Gets or sets the stack trace, if included.
    /// </summary>
    public string? StackTrace { get; init; }

    /// <summary>
    /// Gets or sets the inner exception info, if present.
    /// </summary>
    public ExceptionInfo? InnerException { get; init; }
}
