namespace Craft.Notifications;

/// <summary>
/// Configuration options for the notification service.
/// </summary>
public class NotificationOptions
{
    /// <summary>
    /// The configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "NotificationOptions";

    /// <summary>
    /// Gets or sets whether to enable notification persistence to database.
    /// Default: true
    /// </summary>
    public bool EnablePersistence { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable real-time delivery infrastructure.
    /// Default: false
    /// </summary>
    public bool EnableRealTimeDelivery { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to enable batch notification processing.
    /// Default: true
    /// </summary>
    public bool EnableBatchProcessing { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum batch size for batch notifications.
    /// Default: 100
    /// </summary>
    [Range(1, 1000)]
    public int MaxBatchSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets the default notification expiration in days.
    /// Default: 30
    /// </summary>
    [Range(1, 365)]
    public int DefaultExpirationDays { get; set; } = 30;

    /// <summary>
    /// Gets or sets the maximum delivery retry attempts.
    /// Default: 3
    /// </summary>
    [Range(0, 10)]
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the retry delay in seconds.
    /// Default: 60
    /// </summary>
    [Range(1, 3600)]
    public int RetryDelaySeconds { get; set; } = 60;

    /// <summary>
    /// Gets or sets whether to auto-mark notifications as read after delivery.
    /// Default: false
    /// </summary>
    public bool AutoMarkAsRead { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to enable multi-tenancy support.
    /// Default: false
    /// </summary>
    public bool EnableMultiTenancy { get; set; } = false;

    /// <summary>
    /// Gets or sets the default channels for new notifications.
    /// Default: InApp
    /// </summary>
    public NotificationChannel DefaultChannels { get; set; } = NotificationChannel.InApp;

    /// <summary>
    /// Gets or sets whether to log delivery attempts.
    /// Default: true
    /// </summary>
    public bool LogDeliveryAttempts { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to clean up old notifications automatically.
    /// Default: true
    /// </summary>
    public bool EnableAutoCleanup { get; set; } = true;

    /// <summary>
    /// Gets or sets the number of days to keep delivered notifications before cleanup.
    /// Default: 90
    /// </summary>
    [Range(1, 365)]
    public int CleanupAfterDays { get; set; } = 90;

    /// <summary>
    /// Gets or sets the web push VAPID subject (email or URL).
    /// </summary>
    [MaxLength(500)]
    public string? VapidSubject { get; set; }

    /// <summary>
    /// Gets or sets the web push VAPID public key.
    /// </summary>
    [MaxLength(200)]
    public string? VapidPublicKey { get; set; }

    /// <summary>
    /// Gets or sets the web push VAPID private key.
    /// </summary>
    [MaxLength(200)]
    public string? VapidPrivateKey { get; set; }

    /// <summary>
    /// Gets or sets the Microsoft Teams webhook URL.
    /// </summary>
    [MaxLength(500)]
    public string? TeamsWebhookUrl { get; set; }

    /// <summary>
    /// Gets or sets whether to enable detailed logging.
    /// Default: false
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = false;
}
