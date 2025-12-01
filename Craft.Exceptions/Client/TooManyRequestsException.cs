using System.Net;

namespace Craft.Exceptions.Client;

/// <summary>
/// Exception thrown when the client has sent too many requests in a given time period.
/// </summary>
public class TooManyRequestsException : CraftException
{
    public TooManyRequestsException()
        : base("Too many requests - rate limit exceeded", [], (HttpStatusCode)429) { }

    public TooManyRequestsException(string message)
        : base(message, [], (HttpStatusCode)429) { }

    public TooManyRequestsException(string message, Exception innerException)
        : base(message, innerException, (HttpStatusCode)429) { }

    public TooManyRequestsException(string message, List<string>? errors = default)
        : base(message, errors, (HttpStatusCode)429) { }

    public TooManyRequestsException(int limit, string period)
        : base($"Rate limit exceeded: {limit} requests per {period}", [], (HttpStatusCode)429) { }

    public TooManyRequestsException(int retryAfterSeconds)
        : base($"Too many requests. Retry after {retryAfterSeconds} seconds", [], (HttpStatusCode)429) { }
}
