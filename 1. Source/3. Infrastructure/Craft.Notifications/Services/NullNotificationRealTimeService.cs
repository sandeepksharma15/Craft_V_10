namespace Craft.Notifications;

/// <summary>
/// No-op implementation of real-time notification service.
/// Replace with SignalR or other real-time implementation.
/// </summary>
public class NullNotificationRealTimeService : INotificationRealTimeService
{
    private readonly ILogger<NullNotificationRealTimeService> _logger;

    public NullNotificationRealTimeService(ILogger<NullNotificationRealTimeService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<Result> SendRealTimeAsync(
        string userId,
        Notification notification,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Real-time delivery not implemented. Notification {NotificationId} for user {UserId} skipped",
            notification.Id,
            userId);

        return Task.FromResult(Result.CreateSuccess());
    }

    /// <inheritdoc/>
    public Task<Result> SendRealTimeToMultipleAsync(
        IEnumerable<string> userIds,
        Notification notification,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Real-time delivery not implemented. Notification {NotificationId} for {UserCount} users skipped",
            notification.Id,
            userIds.Count());

        return Task.FromResult(Result.CreateSuccess());
    }

    /// <inheritdoc/>
    public Task<bool> IsUserConnectedAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }
}
