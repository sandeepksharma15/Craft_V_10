using System.Net;

namespace Craft.Exceptions;

/// <summary>
/// Exception thrown when a requested resource cannot be found.
/// Returns HTTP 404 Not Found status code.
/// </summary>
public class NotFoundException : CraftException
{
    public NotFoundException()
        : base("The requested resource was not found", [], HttpStatusCode.NotFound) { }

    public NotFoundException(string message)
        : base(message, [], HttpStatusCode.NotFound) { }

    public NotFoundException(string message, Exception innerException)
        : base(message, innerException, HttpStatusCode.NotFound) { }

    public NotFoundException(string message, List<string>? errors = default)
        : base(message, errors, HttpStatusCode.NotFound) { }

    public NotFoundException(string entityName, object key)
        : base($"Entity \"{entityName}\" ({key}) was not found.", [], HttpStatusCode.NotFound) { }
}
