namespace Craft.Notifications;

/// <summary>
/// Represents the type/category of a notification.
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// General information notification.
    /// </summary>
    Info = 0,

    /// <summary>
    /// Success notification.
    /// </summary>
    Success = 1,

    /// <summary>
    /// Warning notification.
    /// </summary>
    Warning = 2,

    /// <summary>
    /// Error notification.
    /// </summary>
    Error = 3,

    /// <summary>
    /// System notification.
    /// </summary>
    System = 4,

    /// <summary>
    /// User action required.
    /// </summary>
    Action = 5,

    /// <summary>
    /// Reminder notification.
    /// </summary>
    Reminder = 6,

    /// <summary>
    /// Alert notification.
    /// </summary>
    Alert = 7
}
