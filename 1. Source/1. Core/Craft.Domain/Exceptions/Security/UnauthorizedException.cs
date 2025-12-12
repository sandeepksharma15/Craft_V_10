using System.Net;

namespace Craft.Domain;

public class UnauthorizedException : CraftException
{
    public UnauthorizedException()
        : base("Unauthorized access", [], HttpStatusCode.Unauthorized) { }

    public UnauthorizedException(string message)
        : base(message, [], HttpStatusCode.Unauthorized) { }

    public UnauthorizedException(string message, Exception innerException)
        : base(message, innerException, HttpStatusCode.Unauthorized) { }

    public UnauthorizedException(string message, List<string>? errors = default)
        : base(message, errors, HttpStatusCode.Unauthorized) { }

    public UnauthorizedException(string message, List<string>? errors, HttpStatusCode statusCode)
        : base(message, errors, statusCode) { }
}
