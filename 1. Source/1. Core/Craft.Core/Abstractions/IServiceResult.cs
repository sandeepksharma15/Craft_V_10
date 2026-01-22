namespace Craft.Core;

/// <summary>
/// Represents the result of a service operation, including success, errors, status code, and message.
/// </summary>
public interface IServiceResult
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    bool IsSuccess { get; }

    /// <summary>
    /// Gets the list of error messages, if any.
    /// </summary>
    List<string>? Errors { get; }

    /// <summary>
    /// Gets the status code associated with the result, if any.
    /// </summary>
    int? StatusCode { get; }

    /// <summary>
    /// Gets a value indicating whether there are any errors.
    /// </summary>
    bool HasErrors { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    bool IsFailure { get; }

    /// <summary>
    /// Gets an optional message describing the result.
    /// </summary>
    string? Message { get; }
}
