using System.Net;

namespace Craft.Exceptions;

public class AlreadyExistsException : CraftException
{
    public AlreadyExistsException()
        : base("This resource already exists") { }

    public AlreadyExistsException(string message) : base(message) { }

    public AlreadyExistsException(string message, Exception innerException)
        : base(message, innerException) { }

    public AlreadyExistsException(string message, List<string> errors = default!,
        HttpStatusCode statusCode = HttpStatusCode.UnprocessableEntity) : base(message, errors, statusCode) { }

    public AlreadyExistsException(string name, object key)
        : base($"Entity \"{name}\" ({key}) already exists", (List<string>?)null, HttpStatusCode.UnprocessableEntity) { }
}
