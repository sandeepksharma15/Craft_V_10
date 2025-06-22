using System.Net;

namespace Craft.Exceptions;

public class ModelValidationException : CraftException
{
    public IDictionary<string, string[]> ValidationErrors { get; } = new Dictionary<string, string[]>();

    public ModelValidationException()
        : base("One or more validation failures have occurred.", [], HttpStatusCode.BadRequest)
    {
        ValidationErrors = new Dictionary<string, string[]>();
    }

    public ModelValidationException(string message)
        : base(message, [], HttpStatusCode.BadRequest) { }

    public ModelValidationException(string message, Exception innerException)
        : base(message, innerException) { }

    public ModelValidationException(string message, IDictionary<string, string[]> validationErrors)
        : base(message, [.. validationErrors.SelectMany(kvp => kvp.Value)], HttpStatusCode.BadRequest)
    {
        ValidationErrors = new Dictionary<string, string[]>(validationErrors);
    }
}
