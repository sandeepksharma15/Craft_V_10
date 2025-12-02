using System.Net;

namespace Craft.Exceptions;

public class ForbiddenException : CraftException
{
    public ForbiddenException(string message)
        : base(message, (List<string>?)null, HttpStatusCode.Forbidden) { }

    public ForbiddenException() { }

    public ForbiddenException(string message, Exception innerException) : base(message, innerException) { }

    public ForbiddenException(string message, List<string> errors = default!,
        HttpStatusCode statusCode = HttpStatusCode.Forbidden) : base(message, errors, statusCode) { }
}
