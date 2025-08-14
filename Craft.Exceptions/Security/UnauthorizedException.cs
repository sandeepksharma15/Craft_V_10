using System.Net;

namespace Craft.Exceptions.Security;

public class UnauthorizedException : CraftException
{
    public UnauthorizedException(string message)
        : base(message, (List<string>?)null, HttpStatusCode.Unauthorized) { }

    public UnauthorizedException() { }

    public UnauthorizedException(string message, Exception innerException) : base(message, innerException) { }

    public UnauthorizedException(string message, List<string> errors = default!,
        HttpStatusCode statusCode = HttpStatusCode.Unauthorized) : base(message, errors, statusCode) { }
}
