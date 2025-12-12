using System.Net;

namespace Craft.Domain;

/// <summary>
/// Exception thrown when the service is temporarily unavailable.
/// </summary>
public class ServiceUnavailableException : CraftException
{
    public ServiceUnavailableException()
        : base("The service is temporarily unavailable", [], HttpStatusCode.ServiceUnavailable) { }

    public ServiceUnavailableException(string message)
        : base(message, [], HttpStatusCode.ServiceUnavailable) { }

    public ServiceUnavailableException(string message, Exception innerException)
        : base(message, innerException, HttpStatusCode.ServiceUnavailable) { }

    public ServiceUnavailableException(string message, List<string>? errors = default)
        : base(message, errors, HttpStatusCode.ServiceUnavailable) { }
}
