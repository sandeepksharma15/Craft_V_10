using System.Net;

namespace Craft.Exceptions.Server;

/// <summary>
/// Exception thrown when the server received an invalid response from an upstream server.
/// </summary>
public class BadGatewayException : CraftException
{
    public BadGatewayException()
        : base("Bad gateway - invalid response from upstream server", [], HttpStatusCode.BadGateway) { }

    public BadGatewayException(string message)
        : base(message, [], HttpStatusCode.BadGateway) { }

    public BadGatewayException(string message, Exception innerException)
        : base(message, innerException, HttpStatusCode.BadGateway) { }

    public BadGatewayException(string message, List<string>? errors = default)
        : base(message, errors, HttpStatusCode.BadGateway) { }

    public BadGatewayException(string upstreamService, string reason)
        : base($"Bad gateway from \"{upstreamService}\": {reason}", [], HttpStatusCode.BadGateway) { }
}
