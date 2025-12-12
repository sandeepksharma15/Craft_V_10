using System.Net;

namespace Craft.Domain;

/// <summary>
/// Exception thrown when the request content type is not supported.
/// </summary>
public class UnsupportedMediaTypeException : CraftException
{
    public UnsupportedMediaTypeException()
        : base("The media type is not supported", [], HttpStatusCode.UnsupportedMediaType) { }

    public UnsupportedMediaTypeException(string message)
        : base(message, [], HttpStatusCode.UnsupportedMediaType) { }

    public UnsupportedMediaTypeException(string message, Exception innerException)
        : base(message, innerException, HttpStatusCode.UnsupportedMediaType) { }

    public UnsupportedMediaTypeException(string message, List<string>? errors = default)
        : base(message, errors, HttpStatusCode.UnsupportedMediaType) { }

    public UnsupportedMediaTypeException(string mediaType, string[] supportedTypes)
        : base($"Media type \"{mediaType}\" is not supported. Supported types: {string.Join(", ", supportedTypes)}", 
               [], HttpStatusCode.UnsupportedMediaType) { }
}
