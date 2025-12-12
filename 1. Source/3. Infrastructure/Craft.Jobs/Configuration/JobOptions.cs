using System.ComponentModel.DataAnnotations;

namespace Craft.Jobs;

/// <summary>
/// Configuration options for background job processing.
/// </summary>
public class JobOptions : IValidatableObject
{
    public const string SectionName = "JobOptions";

    /// <summary>
    /// Gets or sets the PostgreSQL connection string for Hangfire.
    /// </summary>
    [Required(ErrorMessage = "PostgreSQL connection string is required for job processing")]
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether to enable the Hangfire dashboard.
    /// </summary>
    public bool EnableDashboard { get; set; } = true;

    /// <summary>
    /// Gets or sets the dashboard URL path.
    /// </summary>
    public string DashboardPath { get; set; } = "/hangfire";

    /// <summary>
    /// Gets or sets the maximum number of concurrent jobs.
    /// </summary>
    [Range(1, 100, ErrorMessage = "Worker count must be between 1 and 100")]
    public int WorkerCount { get; set; } = 20;

    /// <summary>
    /// Gets or sets the job retry attempts for failed jobs.
    /// </summary>
    [Range(0, 10, ErrorMessage = "Max retry attempts must be between 0 and 10")]
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets whether to enable automatic retry.
    /// </summary>
    public bool EnableAutomaticRetry { get; set; } = true;

    /// <summary>
    /// Gets or sets the job expiration time in days.
    /// </summary>
    [Range(1, 365, ErrorMessage = "Job expiration must be between 1 and 365 days")]
    public int JobExpirationDays { get; set; } = 7;

    /// <summary>
    /// Gets or sets whether to use multi-tenancy support.
    /// </summary>
    public bool EnableMultiTenancy { get; set; } = false;

    /// <summary>
    /// Gets or sets the schema name for Hangfire tables.
    /// </summary>
    public string SchemaName { get; set; } = "hangfire";

    /// <summary>
    /// Gets or sets the polling interval in seconds for checking new jobs.
    /// </summary>
    [Range(1, 60, ErrorMessage = "Polling interval must be between 1 and 60 seconds")]
    public int PollingIntervalSeconds { get; set; } = 15;

    /// <summary>
    /// Gets or sets whether to enable detailed logging.
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = false;

    /// <summary>
    /// Validates the job options.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
        {
            yield return new ValidationResult(
                "ConnectionString is required for job processing",
                [nameof(ConnectionString)]);
        }

        if (!ConnectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase))
        {
            yield return new ValidationResult(
                "ConnectionString must be a valid PostgreSQL connection string",
                [nameof(ConnectionString)]);
        }

        if (string.IsNullOrWhiteSpace(DashboardPath))
        {
            yield return new ValidationResult(
                "DashboardPath cannot be empty",
                [nameof(DashboardPath)]);
        }

        if (!DashboardPath.StartsWith('/'))
        {
            yield return new ValidationResult(
                "DashboardPath must start with '/'",
                [nameof(DashboardPath)]);
        }
    }
}
