namespace Craft.Notifications;

/// <summary>
/// Request model for creating and sending a notification.
/// </summary>
public class NotificationRequest
{
    /// <summary>
    /// Gets or sets the notification title.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the notification message.
    /// </summary>
    [Required]
    [MaxLength(2000)]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the notification type.
    /// </summary>
    public NotificationType Type { get; set; } = NotificationType.Info;

    /// <summary>
    /// Gets or sets the priority level.
    /// </summary>
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

    /// <summary>
    /// Gets or sets the delivery channels.
    /// </summary>
    public NotificationChannel Channels { get; set; } = NotificationChannel.InApp;

    /// <summary>
    /// Gets or sets the recipient user ID.
    /// </summary>
    public string? RecipientUserId { get; set; }

    /// <summary>
    /// Gets or sets the recipient email address.
    /// </summary>
    [EmailAddress]
    public string? RecipientEmail { get; set; }

    /// <summary>
    /// Gets or sets the recipient phone number.
    /// </summary>
    [Phone]
    public string? RecipientPhone { get; set; }

    /// <summary>
    /// Gets or sets additional metadata.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Gets or sets the action URL.
    /// </summary>
    [Url]
    [MaxLength(500)]
    public string? ActionUrl { get; set; }

    /// <summary>
    /// Gets or sets the image URL.
    /// </summary>
    [Url]
    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the notification category.
    /// </summary>
    [MaxLength(100)]
    public string? Category { get; set; }

    /// <summary>
    /// Gets or sets the expiration date.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }
}
