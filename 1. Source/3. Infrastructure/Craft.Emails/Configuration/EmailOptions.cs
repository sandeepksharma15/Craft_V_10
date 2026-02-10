using System.ComponentModel.DataAnnotations;

namespace Craft.Emails;

/// <summary>
/// Configuration settings for email services.
/// </summary>
public class EmailOptions : IValidatableObject
{
    public const string SectionName = "EmailOptions";

    /// <summary>
    /// Gets or sets the default email provider to use (e.g., "smtp", "sendgrid", "mock").
    /// </summary>
    [Required(ErrorMessage = "Email provider is required")]
    public string Provider { get; set; } = "smtp";

    /// <summary>
    /// Gets or sets the default "From" email address.
    /// </summary>
    [Required(ErrorMessage = "From email address is required")]
    [EmailAddress(ErrorMessage = "Invalid From email address")]
    public string? From { get; set; }

    /// <summary>
    /// Gets or sets the default "From" display name.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to enable email queue for background processing.
    /// </summary>
    public bool EnableQueue { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of retry attempts for failed emails.
    /// </summary>
    [Range(0, 10, ErrorMessage = "MaxRetryAttempts must be between 0 and 10")]
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the delay in seconds between retry attempts.
    /// </summary>
    [Range(1, 3600, ErrorMessage = "RetryDelaySeconds must be between 1 and 3600")]
    public int RetryDelaySeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets a value indicating whether to enable email tracking.
    /// </summary>
    public bool EnableTracking { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to cache compiled templates.
    /// </summary>
    public bool EnableTemplateCache { get; set; } = true;

    /// <summary>
    /// Gets or sets the template cache duration in minutes.
    /// </summary>
    [Range(1, 1440, ErrorMessage = "TemplateCacheDurationMinutes must be between 1 and 1440")]
    public int TemplateCacheDurationMinutes { get; set; } = 60;

    /// <summary>
    /// Gets or sets the path to the email templates folder.
    /// </summary>
    public string TemplatesPath { get; set; } = "Email Templates";

    /// <summary>
    /// Gets or sets SMTP-specific settings.
    /// </summary>
    public SmtpSettings? Smtp { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Provider.Equals("smtp", StringComparison.OrdinalIgnoreCase))
        {
            if (Smtp == null)
                yield return new ValidationResult(
                    "SMTP settings are required when using SMTP provider",
                    [nameof(Smtp)]);
            else
            {
                var smtpValidationContext = new ValidationContext(Smtp);
                var smtpResults = new List<ValidationResult>();
                if (!Validator.TryValidateObject(Smtp, smtpValidationContext, smtpResults, true))
                    foreach (var result in smtpResults)
                        yield return result;
            }
        }
    }
}

/// <summary>
/// SMTP-specific configuration settings.
/// </summary>
public class SmtpSettings : IValidatableObject
{
    /// <summary>
    /// Gets or sets the SMTP host server address.
    /// </summary>
    [Required(ErrorMessage = "SMTP host is required")]
    public string? Host { get; set; }

    /// <summary>
    /// Gets or sets the SMTP port number.
    /// </summary>
    [Range(1, 65535, ErrorMessage = "SMTP port must be between 1 and 65535")]
    public int Port { get; set; } = 587;

    /// <summary>
    /// Gets or sets the SMTP username.
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// Gets or sets the SMTP password.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use SSL/TLS.
    /// </summary>
    public bool EnableSsl { get; set; } = true;

    /// <summary>
    /// Gets or sets the timeout in seconds for SMTP operations.
    /// </summary>
    [Range(1, 300, ErrorMessage = "Timeout must be between 1 and 300 seconds")]
    public int TimeoutSeconds { get; set; } = 30;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Host))
            yield return new ValidationResult(
                "SMTP host is required",
                [nameof(Host)]);
    }
}

