namespace Craft.QuerySpec;

/// <summary>
/// Represents the result of a query validation operation.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Gets whether the validation was successful.
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// Gets the collection of validation error messages.
    /// </summary>
    public IReadOnlyList<string> Errors { get; }

    private ValidationResult(bool isValid, IReadOnlyList<string> errors)
    {
        IsValid = isValid;
        Errors = errors;
    }

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    public static ValidationResult Success() => new(true, []);

    /// <summary>
    /// Creates a failed validation result with the specified errors.
    /// </summary>
    /// <param name="errors">The validation error messages.</param>
    public static ValidationResult Failure(IEnumerable<string> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        return new ValidationResult(false, [.. errors]);
    }

    /// <summary>
    /// Creates a failed validation result with a single error.
    /// </summary>
    /// <param name="error">The validation error message.</param>
    public static ValidationResult Failure(string error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);
        return new ValidationResult(false, [error]);
    }
}
