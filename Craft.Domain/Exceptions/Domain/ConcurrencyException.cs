using System.Net;

namespace Craft.Exceptions;

/// <summary>
/// Exception thrown when a concurrency conflict occurs during data modification.
/// Typically used when optimistic concurrency checks fail (e.g., row version mismatch).
/// </summary>
public class ConcurrencyException : CraftException
{
    public ConcurrencyException()
        : base("A concurrency conflict occurred. The record has been modified by another user.", [], HttpStatusCode.Conflict) { }

    public ConcurrencyException(string message)
        : base(message, [], HttpStatusCode.Conflict) { }

    public ConcurrencyException(string message, Exception innerException)
        : base(message, innerException, HttpStatusCode.Conflict) { }

    public ConcurrencyException(string message, List<string>? errors = default)
        : base(message, errors, HttpStatusCode.Conflict) { }

    public ConcurrencyException(string entityName, object key)
        : base($"Concurrency conflict for entity \"{entityName}\" ({key}). The record has been modified by another user.", [], HttpStatusCode.Conflict) { }

    public ConcurrencyException(string entityName, object key, string expectedVersion, string actualVersion)
        : base($"Concurrency conflict for entity \"{entityName}\" ({key}). Expected version: {expectedVersion}, Actual version: {actualVersion}.", [], HttpStatusCode.Conflict) { }
}
