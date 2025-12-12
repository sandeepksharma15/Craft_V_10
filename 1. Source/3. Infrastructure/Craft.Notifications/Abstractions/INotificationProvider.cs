namespace Craft.Notifications;

/// <summary>
/// Defines the contract for notification delivery providers.
/// </summary>
public interface INotificationProvider
{
    /// <summary>
    /// Gets the channel this provider handles.
    /// </summary>
    NotificationChannel Channel { get; }

    /// <summary>
    /// Gets the name of the provider.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Sends a notification through this provider.
    /// </summary>
    /// <param name="notification">The notification to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure with provider response.</returns>
    Task<NotificationDeliveryResult> SendAsync(
        Notification notification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends notifications in batch through this provider.
    /// </summary>
    /// <param name="notifications">The notifications to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Results for each notification.</returns>
    Task<List<NotificationDeliveryResult>> SendBatchAsync(
        IEnumerable<Notification> notifications,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if the provider can deliver to the given notification.
    /// </summary>
    /// <param name="notification">The notification to validate.</param>
    /// <returns>True if the provider can deliver this notification.</returns>
    bool CanDeliver(Notification notification);

    /// <summary>
    /// Gets the priority order for this provider (lower is higher priority).
    /// </summary>
    int Priority { get; }
}
