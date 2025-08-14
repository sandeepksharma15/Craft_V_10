using System.Net;

namespace Craft.Exceptions;

public abstract class CraftException : Exception
{
    public List<string> Errors { get; } = [];
    public HttpStatusCode StatusCode { get; }

    protected CraftException() { }

    protected CraftException(string message) : base(message) { }

    protected CraftException(string message, Exception innerException, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) 
        : base(message, innerException) 
    { 
        StatusCode = statusCode;
    }

    protected CraftException(string message, List<string>? errors = default!,
        HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base(message)
    {
        Errors = errors ?? [];
        StatusCode = statusCode;
    }
}
