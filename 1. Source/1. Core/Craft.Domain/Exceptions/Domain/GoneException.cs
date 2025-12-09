using System.Net;

namespace Craft.Exceptions;

/// <summary>
/// Exception thrown when a resource is no longer available and has been permanently removed.
/// Returns HTTP 410 Gone status code.
/// Useful for soft-deleted resources or deprecated endpoints.
/// </summary>
public class GoneException : CraftException
{
    public GoneException()
        : base("The requested resource is no longer available", [], (HttpStatusCode)410) { }

    public GoneException(string message)
        : base(message, [], (HttpStatusCode)410) { }

    public GoneException(string message, Exception innerException)
        : base(message, innerException, (HttpStatusCode)410) { }

    public GoneException(string message, List<string>? errors = default)
        : base(message, errors, (HttpStatusCode)410) { }

    public GoneException(string entityName, object key)
        : base($"Entity \"{entityName}\" ({key}) has been permanently deleted and is no longer available.", 
               [], (HttpStatusCode)410) { }

    public GoneException(string entityName, object key, DateTime deletedAt)
        : base($"Entity \"{entityName}\" ({key}) was permanently deleted on {deletedAt:yyyy-MM-dd HH:mm:ss} UTC and is no longer available.", 
               [], (HttpStatusCode)410) { }
}
