using System.Net;

namespace Craft.Domain;

/// <summary>
/// Exception thrown when an external service call fails.
/// This includes HTTP service failures, API errors, third-party service errors,
/// and external integration failures.
/// </summary>
public class ExternalServiceException : CraftException
{
    public ExternalServiceException()
        : base("An external service error occurred", [], HttpStatusCode.BadGateway) { }

    public ExternalServiceException(string message)
        : base(message, [], HttpStatusCode.BadGateway) { }

    public ExternalServiceException(string message, Exception innerException)
        : base(message, innerException, HttpStatusCode.BadGateway) { }

    public ExternalServiceException(string message, List<string>? errors = default)
        : base(message, errors, HttpStatusCode.BadGateway) { }

    public ExternalServiceException(string serviceName, string errorDetails)
        : base($"External service \"{serviceName}\" error: {errorDetails}", [], HttpStatusCode.BadGateway) { }

    public ExternalServiceException(string serviceName, int statusCode, string errorDetails)
        : base($"External service \"{serviceName}\" returned status {statusCode}: {errorDetails}", [], HttpStatusCode.BadGateway) { }
}
