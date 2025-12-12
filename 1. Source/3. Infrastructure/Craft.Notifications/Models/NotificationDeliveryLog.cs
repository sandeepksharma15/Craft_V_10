namespace Craft.Notifications;

/// <summary>
/// Represents a log entry for notification delivery attempts.
/// </summary>
public class NotificationDeliveryLog : IEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the delivery log.
    /// </summary>
    public KeyType Id { get; set; }

    /// <summary>
    /// Gets or sets the notification ID.
    /// </summary>
    public KeyType NotificationId { get; set; }

    /// <summary>
    /// Gets or sets the channel through which delivery was attempted.
    /// </summary>
    public NotificationChannel Channel { get; set; }

    /// <summary>
    /// Gets or sets whether the delivery was successful.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Gets or sets the error message if delivery failed.
    /// </summary>
    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the response from the delivery provider.
    /// </summary>
    [MaxLength(2000)]
    public string? ProviderResponse { get; set; }

    /// <summary>
    /// Gets or sets the delivery attempt number.
    /// </summary>
    public int AttemptNumber { get; set; }

    /// <summary>
    /// Gets or sets the delivery duration in milliseconds.
    /// </summary>
    public long? DurationMs { get; set; }

    /// <summary>
    /// Gets or sets the creation time (time of delivery attempt).
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Navigation property to the parent notification.
    /// </summary>
    public virtual Notification? Notification { get; set; }
}
