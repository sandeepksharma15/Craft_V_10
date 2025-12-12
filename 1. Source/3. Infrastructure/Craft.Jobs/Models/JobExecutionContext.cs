namespace Craft.Jobs;

/// <summary>
/// Represents the execution context for a background job.
/// </summary>
public class JobExecutionContext
{
    /// <summary>
    /// Gets or sets the unique job ID.
    /// </summary>
    public string JobId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the job type name.
    /// </summary>
    public string JobType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of retry attempts.
    /// </summary>
    public int RetryAttempt { get; set; }

    /// <summary>
    /// Gets or sets the tenant ID (if multi-tenancy is enabled).
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// Gets or sets the user ID who scheduled the job.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Gets or sets when the job was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets when the job started execution.
    /// </summary>
    public DateTimeOffset? StartedAt { get; set; }

    /// <summary>
    /// Gets or sets custom metadata for the job.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = [];
}
