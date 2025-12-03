namespace Craft.Cache;

/// <summary>
/// Represents the result of a cache operation.
/// </summary>
public class CacheResult
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Gets the error message if the operation failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Gets the exception that occurred if the operation failed.
    /// </summary>
    public Exception? Exception { get; init; }

    /// <summary>
    /// Gets the timestamp when the operation was performed.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static CacheResult Success() => new() { IsSuccess = true };

    /// <summary>
    /// Creates a failed result with an error message.
    /// </summary>
    public static CacheResult Failure(string errorMessage, Exception? exception = null) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage,
        Exception = exception
    };
}

/// <summary>
/// Represents the result of a cache operation with a value.
/// </summary>
public class CacheResult<T> : CacheResult
{
    /// <summary>
    /// Gets the value returned by the operation.
    /// </summary>
    public T? Value { get; init; }

    /// <summary>
    /// Gets a value indicating whether the value was found in cache.
    /// </summary>
    public bool HasValue { get; init; }

    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    public static CacheResult<T> Success(T? value) => new()
    {
        IsSuccess = true,
        HasValue = value != null,
        Value = value
    };

    /// <summary>
    /// Creates a successful result indicating value was not found.
    /// </summary>
    public static new CacheResult<T> Success() => new()
    {
        IsSuccess = true,
        HasValue = false,
        Value = default
    };

    /// <summary>
    /// Creates a failed result with an error message.
    /// </summary>
    public static new CacheResult<T> Failure(string errorMessage, Exception? exception = null) => new()
    {
        IsSuccess = false,
        HasValue = false,
        ErrorMessage = errorMessage,
        Exception = exception
    };
}
