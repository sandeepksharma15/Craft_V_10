using System.Net;

namespace Craft.Exceptions;

public class InvalidCredentialsException : CraftException
{
    public InvalidCredentialsException()
        : base("Invalid Credentials: Please check your credentials", (List<string>?)null, HttpStatusCode.Unauthorized) { }

    public InvalidCredentialsException(string message) 
        : base(message, (List<string>?)null, HttpStatusCode.Unauthorized) { }

    public InvalidCredentialsException(string message, Exception innerException) 
        : base(message, innerException, HttpStatusCode.Unauthorized) { }

    public InvalidCredentialsException(string message, List<string> errors = default!,
        HttpStatusCode statusCode = HttpStatusCode.Unauthorized) : base(message, errors, statusCode) { }
}
