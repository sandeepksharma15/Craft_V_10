using System.Net;

namespace Craft.Domain;

/// <summary>
/// Exception thrown when the server did not receive a timely response from an upstream server.
/// </summary>
public class GatewayTimeoutException : CraftException
{
    public GatewayTimeoutException()
        : base("Gateway timeout - no response from upstream server", [], HttpStatusCode.GatewayTimeout) { }

    public GatewayTimeoutException(string message)
        : base(message, [], HttpStatusCode.GatewayTimeout) { }

    public GatewayTimeoutException(string message, Exception innerException)
        : base(message, innerException, HttpStatusCode.GatewayTimeout) { }

    public GatewayTimeoutException(string message, List<string>? errors = default)
        : base(message, errors, HttpStatusCode.GatewayTimeout) { }

    public GatewayTimeoutException(string upstreamService, int timeoutSeconds)
        : base($"Gateway timeout from \"{upstreamService}\" after {timeoutSeconds} seconds", [], HttpStatusCode.GatewayTimeout) { }
}
