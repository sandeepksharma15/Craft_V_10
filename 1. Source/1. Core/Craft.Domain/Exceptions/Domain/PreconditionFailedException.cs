using System.Net;

namespace Craft.Domain;

/// <summary>
/// Exception thrown when a precondition specified in the request headers fails.
/// Returns HTTP 412 Precondition Failed status code.
/// Commonly used for ETags and conditional requests (If-Match, If-None-Match, If-Modified-Since).
/// </summary>
public class PreconditionFailedException : CraftException
{
    public PreconditionFailedException()
        : base("Precondition failed for the request", [], (HttpStatusCode)412) { }

    public PreconditionFailedException(string message)
        : base(message, [], (HttpStatusCode)412) { }

    public PreconditionFailedException(string message, Exception innerException)
        : base(message, innerException, (HttpStatusCode)412) { }

    public PreconditionFailedException(string message, List<string>? errors = default)
        : base(message, errors, (HttpStatusCode)412) { }

    public PreconditionFailedException(string headerName, string expectedValue, string actualValue)
        : base($"Precondition header '{headerName}' failed. Expected: '{expectedValue}', Actual: '{actualValue}'", 
               [], (HttpStatusCode)412) { }
}
