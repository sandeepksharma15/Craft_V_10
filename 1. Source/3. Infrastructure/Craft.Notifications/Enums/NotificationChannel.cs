namespace Craft.Notifications;

/// <summary>
/// Represents different channels through which notifications can be delivered.
/// </summary>
[Flags]
public enum NotificationChannel
{
    /// <summary>
    /// No channel specified.
    /// </summary>
    None = 0,

    /// <summary>
    /// In-app notification displayed within the application.
    /// </summary>
    InApp = 1 << 0,

    /// <summary>
    /// Email notification.
    /// </summary>
    Email = 1 << 1,

    /// <summary>
    /// Push notification (web, mobile, desktop).
    /// </summary>
    Push = 1 << 2,

    /// <summary>
    /// SMS text message notification.
    /// </summary>
    Sms = 1 << 3,

    /// <summary>
    /// Webhook HTTP callback.
    /// </summary>
    Webhook = 1 << 4,

    /// <summary>
    /// All available channels.
    /// </summary>
    All = InApp | Email | Push | Sms | Webhook
}
