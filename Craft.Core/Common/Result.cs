namespace Craft.Core;

/// <summary>
/// Represents the result of an operation that can succeed or fail.
/// </summary>
public class Result
{
    public bool IsSuccess { get; protected init; }
    public bool IsFailure => !IsSuccess;
    public string? ErrorMessage { get; protected init; }
    public List<string>? Errors { get; protected init; }

    protected Result(bool isSuccess, string? errorMessage = null, List<string>? errors = null)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Errors = errors;
    }

    public static Result Success() => new(true);
    
    public static Result Failure(string errorMessage) => new(false, errorMessage);
    
    public static Result Failure(List<string> errors) => new(false, errors: errors);
}

/// <summary>
/// Represents the result of an operation that returns a value.
/// </summary>
public class Result<T> : Result
{
    public T? Value { get; private init; }

    private Result(bool isSuccess, T? value = default, string? errorMessage = null, List<string>? errors = null)
        : base(isSuccess, errorMessage, errors)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(true, value);
    
    public static new Result<T> Failure(string errorMessage) => new(false, errorMessage: errorMessage);
    
    public static new Result<T> Failure(List<string> errors) => new(false, errors: errors);

    // Functional programming helpers
    public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
    {
        return IsSuccess && Value != null
            ? Result<TNew>.Success(mapper(Value))
            : Result<TNew>.Failure(ErrorMessage ?? "Operation failed");
    }

    public Result<TNew> Bind<TNew>(Func<T, Result<TNew>> binder)
    {
        return IsSuccess && Value != null
            ? binder(Value)
            : Result<TNew>.Failure(ErrorMessage ?? "Operation failed");
    }
}
