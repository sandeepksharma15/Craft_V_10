namespace Craft.Notifications;

/// <summary>
/// Base class for notification providers with common functionality.
/// </summary>
public abstract class NotificationProviderBase : INotificationProvider
{
    protected readonly ILogger Logger;
    protected readonly NotificationOptions Options;

    protected NotificationProviderBase(
        ILogger logger,
        NotificationOptions options)
    {
        Logger = logger;
        Options = options;
    }

    /// <inheritdoc/>
    public abstract NotificationChannel Channel { get; }

    /// <inheritdoc/>
    public abstract string Name { get; }

    /// <inheritdoc/>
    public virtual int Priority => 100;

    /// <inheritdoc/>
    public abstract Task<NotificationDeliveryResult> SendAsync(
        Notification notification,
        CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public virtual async Task<List<NotificationDeliveryResult>> SendBatchAsync(
        IEnumerable<Notification> notifications,
        CancellationToken cancellationToken = default)
    {
        var results = new List<NotificationDeliveryResult>();

        foreach (var notification in notifications)
        {
            var result = await SendAsync(notification, cancellationToken);
            results.Add(result);
        }

        return results;
    }

    /// <inheritdoc/>
    public virtual bool CanDeliver(Notification notification)
    {
        return notification.Channels.HasFlag(Channel);
    }

    /// <summary>
    /// Logs the start of a delivery attempt.
    /// </summary>
    protected void LogDeliveryStart(Notification notification)
    {
        if (Options.EnableDetailedLogging)
        {
            Logger.LogDebug(
                "Starting {Channel} delivery for notification {NotificationId} to {Recipient}",
                Channel,
                notification.Id,
                notification.RecipientUserId ?? notification.RecipientEmail ?? "unknown");
        }
    }

    /// <summary>
    /// Logs a successful delivery.
    /// </summary>
    protected void LogDeliverySuccess(Notification notification, long durationMs)
    {
        Logger.LogInformation(
            "Successfully delivered notification {NotificationId} via {Channel} in {Duration}ms",
            notification.Id,
            Channel,
            durationMs);
    }

    /// <summary>
    /// Logs a failed delivery.
    /// </summary>
    protected void LogDeliveryFailure(Notification notification, string error, long durationMs)
    {
        Logger.LogWarning(
            "Failed to deliver notification {NotificationId} via {Channel} in {Duration}ms: {Error}",
            notification.Id,
            Channel,
            durationMs,
            error);
    }

    /// <summary>
    /// Measures the execution time of a delivery operation.
    /// </summary>
    protected static async Task<(T Result, long DurationMs)> MeasureAsync<T>(Func<Task<T>> operation)
    {
        var startTime = DateTimeOffset.UtcNow;
        var result = await operation();
        var endTime = DateTimeOffset.UtcNow;
        var durationMs = (long)(endTime - startTime).TotalMilliseconds;

        return (result, durationMs);
    }
}
