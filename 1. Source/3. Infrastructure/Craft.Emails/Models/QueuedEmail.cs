namespace Craft.Emails;

/// <summary>
/// Represents a queued email waiting to be sent.
/// </summary>
public class QueuedEmail
{
    /// <summary>
    /// Gets or sets the unique identifier for this queued email.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the email request details.
    /// </summary>
    public MailRequest Request { get; set; } = default!;

    /// <summary>
    /// Gets or sets the current status of the email.
    /// </summary>
    public EmailStatus Status { get; set; } = EmailStatus.Pending;

    /// <summary>
    /// Gets or sets the number of send attempts made.
    /// </summary>
    public int Attempts { get; set; } = 0;

    /// <summary>
    /// Gets or sets the timestamp when the email was queued.
    /// </summary>
    public DateTimeOffset QueuedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the timestamp when the email should be sent (for scheduled emails).
    /// </summary>
    public DateTimeOffset? ScheduledFor { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the email was last attempted.
    /// </summary>
    public DateTimeOffset? LastAttemptAt { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the email was successfully sent.
    /// </summary>
    public DateTimeOffset? SentAt { get; set; }

    /// <summary>
    /// Gets or sets the error message if the send failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the message ID returned by the email provider.
    /// </summary>
    public string? MessageId { get; set; }

    /// <summary>
    /// Gets or sets the priority of the email.
    /// </summary>
    public EmailPriority Priority { get; set; } = EmailPriority.Normal;
}

/// <summary>
/// Represents the status of a queued email.
/// </summary>
public enum EmailStatus
{
    /// <summary>
    /// Email is waiting to be sent.
    /// </summary>
    Pending,

    /// <summary>
    /// Email is currently being sent.
    /// </summary>
    Sending,

    /// <summary>
    /// Email was successfully sent.
    /// </summary>
    Sent,

    /// <summary>
    /// Email failed to send and will be retried.
    /// </summary>
    Failed,

    /// <summary>
    /// Email failed permanently after all retry attempts.
    /// </summary>
    FailedPermanently,

    /// <summary>
    /// Email is scheduled for future delivery.
    /// </summary>
    Scheduled
}

/// <summary>
/// Represents the priority of an email.
/// </summary>
public enum EmailPriority
{
    /// <summary>
    /// Low priority email.
    /// </summary>
    Low = 0,

    /// <summary>
    /// Normal priority email.
    /// </summary>
    Normal = 1,

    /// <summary>
    /// High priority email (sent first).
    /// </summary>
    High = 2
}
