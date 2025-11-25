using System.ComponentModel.DataAnnotations;

namespace Craft.Infrastructure.Cors;

/// <summary>
/// Configuration settings for CORS (Cross-Origin Resource Sharing) policies.
/// </summary>
/// <remarks>
/// Configure in appsettings.json:
/// <code>
/// "CorsSettings": {
///   "Angular": "http://localhost:4200;https://angular.app.com",
///   "Blazor": "http://localhost:5000;https://blazor.app.com",
///   "React": "http://localhost:3000;https://react.app.com"
/// }
/// </code>
/// Multiple origins for each setting should be separated by semicolons.
/// </remarks>
public class CorsSettings : IValidatableObject
{
    public const string SectionName = "CorsSettings";

    /// <summary>
    /// Gets or sets the allowed origins for Angular applications.
    /// Multiple origins can be separated by semicolons.
    /// </summary>
    public string? Angular { get; set; }

    /// <summary>
    /// Gets or sets the allowed origins for Blazor applications.
    /// Multiple origins can be separated by semicolons.
    /// </summary>
    public string? Blazor { get; set; }

    /// <summary>
    /// Gets or sets the allowed origins for React applications.
    /// Multiple origins can be separated by semicolons.
    /// </summary>
    public string? React { get; set; }

    /// <summary>
    /// Validates that at least one origin is configured.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Angular) &&
            string.IsNullOrWhiteSpace(Blazor) &&
            string.IsNullOrWhiteSpace(React))
        {
            yield return new ValidationResult(
                "At least one CORS origin must be configured (Angular, Blazor, or React).",
                [nameof(Angular), nameof(Blazor), nameof(React)]);
        }
    }
}
