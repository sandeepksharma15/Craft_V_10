using System.Net;

namespace Craft.Exceptions;

public class NotFoundException : CraftException
{
    public NotFoundException(string message)
        : base(message, (List<string>?)null, HttpStatusCode.NotFound) { }

    public NotFoundException() { }

    public NotFoundException(string message, Exception innerException) : base(message, innerException) { }

    public NotFoundException(string message, List<string> errors = default!,
        HttpStatusCode statusCode = HttpStatusCode.NotFound) : base(message, errors, statusCode) { }

    public NotFoundException(string name, object key)
        : base($"Entity \"{name}\" ({key}) was not found.") { }
}
