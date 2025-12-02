using System.Net;

namespace Craft.Exceptions;

/// <summary>
/// Exception thrown when a request is malformed or contains invalid data that doesn't fit validation scenarios.
/// </summary>
public class BadRequestException : CraftException
{
    public BadRequestException()
        : base("The request is invalid", [], HttpStatusCode.BadRequest) { }

    public BadRequestException(string message)
        : base(message, [], HttpStatusCode.BadRequest) { }

    public BadRequestException(string message, Exception innerException)
        : base(message, innerException, HttpStatusCode.BadRequest) { }

    public BadRequestException(string message, List<string>? errors = default)
        : base(message, errors, HttpStatusCode.BadRequest) { }
}
