using System.Net;

namespace Craft.Exceptions;

/// <summary>
/// Exception thrown when a requested entity cannot be found.
/// </summary>
[Obsolete("Use NotFoundException instead. This class will be removed in v2.0.0", false)]
public class EntityNotFoundException : NotFoundException
{
    public EntityNotFoundException() : base() { }

    public EntityNotFoundException(string message) : base(message) { }

    public EntityNotFoundException(string message, Exception innerException) : base(message, innerException) { }

    public EntityNotFoundException(string message, List<string> errors = default!,
        HttpStatusCode statusCode = HttpStatusCode.NotFound) : base(message, errors) { }

    public EntityNotFoundException(string name, object key) : base(name, key) { }
}
