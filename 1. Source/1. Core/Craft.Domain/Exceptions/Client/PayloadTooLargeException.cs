using System.Net;

namespace Craft.Domain;

/// <summary>
/// Exception thrown when the request payload exceeds the maximum allowed size.
/// Returns HTTP 413 Payload Too Large status code.
/// </summary>
public class PayloadTooLargeException : CraftException
{
    public PayloadTooLargeException()
        : base("The request payload is too large", [], (HttpStatusCode)413) { }

    public PayloadTooLargeException(string message)
        : base(message, [], (HttpStatusCode)413) { }

    public PayloadTooLargeException(string message, Exception innerException)
        : base(message, innerException, (HttpStatusCode)413) { }

    public PayloadTooLargeException(string message, List<string>? errors = default)
        : base(message, errors, (HttpStatusCode)413) { }

    public PayloadTooLargeException(long actualSize, long maxSize)
        : base($"Payload size {actualSize:N0} bytes exceeds maximum allowed size of {maxSize:N0} bytes", 
               [], (HttpStatusCode)413) { }

    public PayloadTooLargeException(string resourceType, long actualSize, long maxSize)
        : base($"{resourceType} size {actualSize:N0} bytes exceeds maximum allowed size of {maxSize:N0} bytes", 
               [], (HttpStatusCode)413) { }
}
