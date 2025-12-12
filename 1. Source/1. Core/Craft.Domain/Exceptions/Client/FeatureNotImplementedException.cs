using System.Net;

namespace Craft.Domain;

/// <summary>
/// Exception thrown when a feature or endpoint is not yet implemented.
/// </summary>
public class FeatureNotImplementedException : CraftException
{
    public FeatureNotImplementedException()
        : base("This feature is not implemented", [], HttpStatusCode.NotImplemented) { }

    public FeatureNotImplementedException(string message)
        : base(message, [], HttpStatusCode.NotImplemented) { }

    public FeatureNotImplementedException(string message, Exception innerException)
        : base(message, innerException, HttpStatusCode.NotImplemented) { }

    public FeatureNotImplementedException(string message, List<string>? errors = default)
        : base(message, errors, HttpStatusCode.NotImplemented) { }

    public FeatureNotImplementedException(string featureName, string details)
        : base($"Feature \"{featureName}\" is not implemented: {details}", [], HttpStatusCode.NotImplemented) { }
}
