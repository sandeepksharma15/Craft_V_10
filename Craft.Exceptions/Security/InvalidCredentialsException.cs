using System.Net;

namespace Craft.Exceptions.Security;

public class InvalidCredentialsException : CraftException
{
    public InvalidCredentialsException()
        : base("Invalid Credentials: Please check your credentials") { }

    public InvalidCredentialsException(string message) : base(message) { }

    public InvalidCredentialsException(string message, Exception innerException) : base(message, innerException) { }

    public InvalidCredentialsException(string message, List<string> errors = default!,
        HttpStatusCode statusCode = HttpStatusCode.NotAcceptable) : base(message, errors, statusCode) { }
}
