using System.Net;

namespace Craft.Domain;

/// <summary>
/// Exception thrown when a configuration error occurs.
/// This includes missing configuration values, invalid configuration formats,
/// or configuration validation failures.
/// </summary>
public class ConfigurationException : CraftException
{
    public ConfigurationException()
        : base("A configuration error occurred", [], HttpStatusCode.InternalServerError) { }

    public ConfigurationException(string message)
        : base(message, [], HttpStatusCode.InternalServerError) { }

    public ConfigurationException(string message, Exception innerException)
        : base(message, innerException, HttpStatusCode.InternalServerError) { }

    public ConfigurationException(string message, List<string>? errors = default)
        : base(message, errors, HttpStatusCode.InternalServerError) { }

    public ConfigurationException(string configurationKey, string reason)
        : base($"Configuration error for key \"{configurationKey}\": {reason}", [], HttpStatusCode.InternalServerError) { }
}
