namespace Craft.Notifications;

/// <summary>
/// Represents the priority level of a notification.
/// </summary>
public enum NotificationPriority
{
    /// <summary>
    /// Low priority notification, can be delivered with delay.
    /// </summary>
    Low = 0,

    /// <summary>
    /// Normal priority notification.
    /// </summary>
    Normal = 1,

    /// <summary>
    /// High priority notification, should be delivered quickly.
    /// </summary>
    High = 2,

    /// <summary>
    /// Urgent notification, requires immediate delivery.
    /// </summary>
    Urgent = 3
}
