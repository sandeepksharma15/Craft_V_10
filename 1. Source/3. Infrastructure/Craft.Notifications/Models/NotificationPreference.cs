namespace Craft.Notifications;

/// <summary>
/// Represents user-specific notification preferences.
/// </summary>
public class NotificationPreference : IEntity
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public KeyType Id { get; set; }

    /// <summary>
    /// Gets or sets the user ID.
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tenant ID (for multi-tenant scenarios).
    /// </summary>
    [MaxLength(100)]
    public string? TenantId { get; set; }

    /// <summary>
    /// Gets or sets the notification category.
    /// </summary>
    [MaxLength(100)]
    public string? Category { get; set; }

    /// <summary>
    /// Gets or sets the enabled channels for this preference.
    /// </summary>
    public NotificationChannel EnabledChannels { get; set; } = NotificationChannel.All;

    /// <summary>
    /// Gets or sets whether notifications are enabled for this user/category.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the minimum priority level for notifications.
    /// </summary>
    public NotificationPriority MinimumPriority { get; set; } = NotificationPriority.Low;

    /// <summary>
    /// Gets or sets the email address for email notifications.
    /// </summary>
    [MaxLength(256)]
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the phone number for SMS notifications.
    /// </summary>
    [MaxLength(50)]
    public string? Phone { get; set; }

    /// <summary>
    /// Gets or sets the push notification endpoint.
    /// </summary>
    [MaxLength(500)]
    public string? PushEndpoint { get; set; }

    /// <summary>
    /// Gets or sets the push notification public key.
    /// </summary>
    [MaxLength(200)]
    public string? PushPublicKey { get; set; }

    /// <summary>
    /// Gets or sets the push notification auth secret.
    /// </summary>
    [MaxLength(200)]
    public string? PushAuth { get; set; }

    /// <summary>
    /// Gets or sets the webhook URL.
    /// </summary>
    [MaxLength(500)]
    public string? WebhookUrl { get; set; }

    /// <summary>
    /// Gets or sets the creation time.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last modification time.
    /// </summary>
    public DateTimeOffset? ModifiedAt { get; set; }

    /// <summary>
    /// Determines if a specific channel is enabled.
    /// </summary>
    public bool IsChannelEnabled(NotificationChannel channel)
        => IsEnabled && EnabledChannels.HasFlag(channel);
}
