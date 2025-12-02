using System.Net;

namespace Craft.Exceptions;

public class EntityNotFoundException : CraftException
{
    public EntityNotFoundException(string message)
        : base(message, (List<string>?)null, HttpStatusCode.NotFound) { }

    public EntityNotFoundException() { }

    public EntityNotFoundException(string message, Exception innerException) : base(message, innerException) { }

    public EntityNotFoundException(string message, List<string> errors = default!,
        HttpStatusCode statusCode = HttpStatusCode.NotFound) : base(message, errors, statusCode) { }

    public EntityNotFoundException(string name, object key)
        : base($"Entity \"{name}\" ({key}) was not found.", (List<string>?)null, HttpStatusCode.NotFound) { }
}
