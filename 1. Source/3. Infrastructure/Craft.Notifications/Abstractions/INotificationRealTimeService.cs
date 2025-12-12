namespace Craft.Notifications;

/// <summary>
/// Defines the contract for real-time notification delivery infrastructure.
/// </summary>
public interface INotificationRealTimeService
{
    /// <summary>
    /// Sends a real-time notification to a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="notification">The notification to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result> SendRealTimeAsync(
        string userId,
        Notification notification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a real-time notification to multiple users.
    /// </summary>
    /// <param name="userIds">The user IDs.</param>
    /// <param name="notification">The notification to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result> SendRealTimeToMultipleAsync(
        IEnumerable<string> userIds,
        Notification notification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user is currently connected for real-time delivery.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the user is connected.</returns>
    Task<bool> IsUserConnectedAsync(
        string userId,
        CancellationToken cancellationToken = default);
}
