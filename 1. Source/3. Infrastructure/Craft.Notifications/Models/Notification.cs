using System.Text.Json;

namespace Craft.Notifications;

/// <summary>
/// Represents a notification entity that can be persisted to the database.
/// </summary>
public class Notification : IEntity, ISoftDelete
{
    /// <summary>
    /// Gets or sets the unique identifier for the notification.
    /// </summary>
    public KeyType Id { get; set; }

    /// <summary>
    /// Gets or sets the notification title.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the notification message content.
    /// </summary>
    [Required]
    [MaxLength(2000)]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of notification.
    /// </summary>
    public NotificationType Type { get; set; } = NotificationType.Info;

    /// <summary>
    /// Gets or sets the priority level of the notification.
    /// </summary>
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

    /// <summary>
    /// Gets or sets the delivery channels for this notification.
    /// </summary>
    public NotificationChannel Channels { get; set; } = NotificationChannel.InApp;

    /// <summary>
    /// Gets or sets the current delivery status.
    /// </summary>
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;

    /// <summary>
    /// Gets or sets the user ID of the recipient.
    /// </summary>
    [MaxLength(450)]
    public string? RecipientUserId { get; set; }

    /// <summary>
    /// Gets or sets the email address of the recipient (if applicable).
    /// </summary>
    [MaxLength(256)]
    public string? RecipientEmail { get; set; }

    /// <summary>
    /// Gets or sets the phone number of the recipient (if applicable).
    /// </summary>
    [MaxLength(50)]
    public string? RecipientPhone { get; set; }

    /// <summary>
    /// Gets or sets the user ID of the sender.
    /// </summary>
    [MaxLength(450)]
    public string? SenderUserId { get; set; }

    /// <summary>
    /// Gets or sets the tenant ID (for multi-tenant scenarios).
    /// </summary>
    [MaxLength(100)]
    public string? TenantId { get; set; }

    /// <summary>
    /// Gets or sets additional metadata as JSON.
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Gets or sets the URL or action associated with the notification.
    /// </summary>
    [MaxLength(500)]
    public string? ActionUrl { get; set; }

    /// <summary>
    /// Gets or sets the image URL for the notification.
    /// </summary>
    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the notification was read.
    /// </summary>
    public DateTimeOffset? ReadAt { get; set; }

    /// <summary>
    /// Gets or sets the scheduled delivery date and time.
    /// </summary>
    public DateTimeOffset? ScheduledFor { get; set; }

    /// <summary>
    /// Gets or sets the actual delivery date and time.
    /// </summary>
    public DateTimeOffset? DeliveredAt { get; set; }

    /// <summary>
    /// Gets or sets the expiration date and time for the notification.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the number of delivery attempts.
    /// </summary>
    public int DeliveryAttempts { get; set; }

    /// <summary>
    /// Gets or sets the last error message if delivery failed.
    /// </summary>
    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the category/group of the notification.
    /// </summary>
    [MaxLength(100)]
    public string? Category { get; set; }

    /// <summary>
    /// Gets or sets whether the notification is read.
    /// </summary>
    public bool IsRead => ReadAt.HasValue;

    /// <summary>
    /// Gets or sets the creation time.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last modification time.
    /// </summary>
    public DateTimeOffset? ModifiedAt { get; set; }

    /// <summary>
    /// Gets or sets whether the notification is soft deleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the deletion time.
    /// </summary>
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>
    /// Navigation property for delivery logs.
    /// </summary>
    public virtual ICollection<NotificationDeliveryLog> DeliveryLogs { get; set; } = [];

    /// <summary>
    /// Gets the metadata as a dictionary.
    /// </summary>
    public Dictionary<string, object>? GetMetadata()
    {
        if (string.IsNullOrWhiteSpace(Metadata))
            return null;

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(Metadata);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Sets the metadata from a dictionary.
    /// </summary>
    public void SetMetadata(Dictionary<string, object>? metadata)
    {
        Metadata = metadata != null ? JsonSerializer.Serialize(metadata) : null;
    }
}
