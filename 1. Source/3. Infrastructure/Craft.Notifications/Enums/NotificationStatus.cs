namespace Craft.Notifications;

/// <summary>
/// Represents the delivery status of a notification.
/// </summary>
public enum NotificationStatus
{
    /// <summary>
    /// Notification has been created but not yet queued for delivery.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Notification is queued for delivery.
    /// </summary>
    Queued = 1,

    /// <summary>
    /// Notification is currently being sent.
    /// </summary>
    Sending = 2,

    /// <summary>
    /// Notification has been successfully delivered.
    /// </summary>
    Delivered = 3,

    /// <summary>
    /// Notification delivery failed.
    /// </summary>
    Failed = 4,

    /// <summary>
    /// Notification was read by the recipient.
    /// </summary>
    Read = 5,

    /// <summary>
    /// Notification delivery was cancelled.
    /// </summary>
    Cancelled = 6
}
