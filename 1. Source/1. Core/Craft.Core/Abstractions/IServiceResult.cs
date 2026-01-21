namespace Craft.Core;

/// <summary>
/// Standardized interface for service result/response types.
/// </summary>
public interface IServiceResult
{
    bool IsSuccess { get; }
    List<string>? Errors { get; }
    int? StatusCode { get; }
    bool HasErrors { get; }
    bool IsFailure { get; }
    string? Message { get; }
}
