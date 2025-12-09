using System.Net;

namespace Craft.Exceptions;

public class InternalServerException : CraftException
{
    public InternalServerException()
        : base("An internal server error occurred", [], HttpStatusCode.InternalServerError) { }

    public InternalServerException(string message) : base(message, [], HttpStatusCode.InternalServerError) { }

    public InternalServerException(string message, Exception innerException)
        : base(message, innerException) { }

    public InternalServerException(string message, List<string>? errors = default)
        : base(message, errors, HttpStatusCode.InternalServerError) { }

    public InternalServerException(string message, List<string> errors = default!,
        HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(message, errors, statusCode) { }
}
