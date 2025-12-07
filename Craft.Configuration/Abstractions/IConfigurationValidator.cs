namespace Craft.Configuration.Abstractions;

/// <summary>
/// Provides methods for validating configuration on startup.
/// </summary>
public interface IConfigurationValidator
{
    /// <summary>
    /// Validates all registered configuration options.
    /// </summary>
    /// <returns>A validation result containing any errors found.</returns>
    ConfigurationValidationResult Validate();

    /// <summary>
    /// Validates a specific configuration section.
    /// </summary>
    /// <typeparam name="T">The type of the configuration section.</typeparam>
    /// <returns>A validation result containing any errors found.</returns>
    ConfigurationValidationResult Validate<T>() where T : class;
}

/// <summary>
/// Represents the result of configuration validation.
/// </summary>
public class ConfigurationValidationResult
{
    /// <summary>
    /// Gets or sets a value indicating whether validation succeeded.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets the list of validation errors.
    /// </summary>
    public List<string> Errors { get; set; } = [];

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    public static ConfigurationValidationResult Success() => new() { IsValid = true };

    /// <summary>
    /// Creates a failed validation result with errors.
    /// </summary>
    public static ConfigurationValidationResult Failure(params string[] errors) =>
        new() { IsValid = false, Errors = [.. errors] };

    /// <summary>
    /// Creates a failed validation result with a list of errors.
    /// </summary>
    public static ConfigurationValidationResult Failure(List<string> errors) =>
        new() { IsValid = false, Errors = errors };
}
