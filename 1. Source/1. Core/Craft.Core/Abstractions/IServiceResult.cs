using Craft.Core.Common.Constants;

namespace Craft.Core;

/// <summary>
/// Represents the result of a service operation, including success, errors, status code, and message.
/// </summary>
/// <remarks>
/// <para>This interface provides a unified contract for operation results across all layers of the application.</para>
/// <para><b>Usage guidance:</b></para>
/// <list type="bullet">
///   <item><description><see cref="Result{T}"/> - Use in domain/application layer for business operations</description></item>
///   <item><description><c>HttpServiceResult&lt;T&gt;</c> - Use for HTTP client operations and external API calls</description></item>
///   <item><description><c>ServerResponse</c> - Use for API controller responses to clients</description></item>
/// </list>
/// </remarks>
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

    /// <summary>
    /// Gets the type of error that occurred.
    /// </summary>
    ErrorType ErrorType { get; }
}
