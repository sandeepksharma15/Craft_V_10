namespace Craft.Notifications;

/// <summary>
/// In-app notification provider that stores notifications in the database.
/// </summary>
public class InAppNotificationProvider : NotificationProviderBase
{
    public InAppNotificationProvider(
        ILogger<InAppNotificationProvider> logger,
        NotificationOptions options)
        : base(logger, options)
    {
    }

    /// <inheritdoc/>
    public override NotificationChannel Channel => NotificationChannel.InApp;

    /// <inheritdoc/>
    public override string Name => "InApp";

    /// <inheritdoc/>
    public override int Priority => 1; // Highest priority - always execute first

    /// <inheritdoc/>
    public override async Task<NotificationDeliveryResult> SendAsync(
        Notification notification,
        CancellationToken cancellationToken = default)
    {
        LogDeliveryStart(notification);

        var (success, durationMs) = await MeasureAsync(async () =>
        {
            // In-app notifications are already persisted to database
            // This provider just marks them as delivered
            await Task.CompletedTask;
            return true;
        });

        if (success)
        {
            LogDeliverySuccess(notification, durationMs);
            return NotificationDeliveryResult.Success(
                notification.Id,
                Channel,
                "Notification stored in database",
                durationMs);
        }

        var errorMessage = "Failed to store in-app notification";
        LogDeliveryFailure(notification, errorMessage, durationMs);
        return NotificationDeliveryResult.Failure(
            notification.Id,
            Channel,
            errorMessage,
            durationMs);
    }

    /// <inheritdoc/>
    public override bool CanDeliver(Notification notification)
    {
        // In-app notifications can always be delivered if the channel is enabled
        return base.CanDeliver(notification) &&
               !string.IsNullOrEmpty(notification.RecipientUserId);
    }
}
