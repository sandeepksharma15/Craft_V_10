using System.Net;

namespace Craft.Domain;

/// <summary>
/// Exception thrown when a request conflicts with the current state of a resource.
/// </summary>
public class ConflictException : CraftException
{
    public ConflictException()
        : base("A conflict occurred with the current state of the resource", [], HttpStatusCode.Conflict) { }

    public ConflictException(string message)
        : base(message, [], HttpStatusCode.Conflict) { }

    public ConflictException(string message, Exception innerException)
        : base(message, innerException, HttpStatusCode.Conflict) { }

    public ConflictException(string message, List<string>? errors = default)
        : base(message, errors, HttpStatusCode.Conflict) { }

    public ConflictException(string resourceName, string reason)
        : base($"Conflict with resource \"{resourceName}\": {reason}", [], HttpStatusCode.Conflict) { }
}
