using Craft.Core;

namespace Craft.Notifications;

/// <summary>
/// Defines the contract for the notification service.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends a notification to a single recipient.
    /// </summary>
    /// <param name="notification">The notification to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<ServiceResult<Notification>> SendAsync(
        Notification notification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a notification with specified parameters.
    /// </summary>
    /// <param name="request">The notification request parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<ServiceResult<Notification>> SendAsync(
        NotificationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends notifications to multiple recipients in batch.
    /// </summary>
    /// <param name="notifications">The notifications to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result with list of sent notifications.</returns>
    Task<ServiceResult<List<Notification>>> SendBatchAsync(
        IEnumerable<Notification> notifications,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends the same notification to multiple recipients.
    /// </summary>
    /// <param name="request">The notification request.</param>
    /// <param name="recipientUserIds">List of recipient user IDs.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result with list of sent notifications.</returns>
    Task<ServiceResult<List<Notification>>> SendToMultipleAsync(
        NotificationRequest request,
        IEnumerable<string> recipientUserIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Schedules a notification for future delivery.
    /// </summary>
    /// <param name="notification">The notification to schedule.</param>
    /// <param name="scheduledFor">The delivery time.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result with the scheduled notification.</returns>
    Task<ServiceResult<Notification>> ScheduleAsync(
        Notification notification,
        DateTimeOffset scheduledFor,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a notification as read.
    /// </summary>
    /// <param name="notificationId">The notification ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<ServiceResult> MarkAsReadAsync(
        KeyType notificationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks multiple notifications as read.
    /// </summary>
    /// <param name="notificationIds">The notification IDs.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<ServiceResult> MarkAllAsReadAsync(
        IEnumerable<KeyType> notificationIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks all notifications for a user as read.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<ServiceResult> MarkAllAsReadForUserAsync(
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a notification.
    /// </summary>
    /// <param name="notificationId">The notification ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<ServiceResult> DeleteAsync(
        KeyType notificationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets notifications for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="includeRead">Whether to include read notifications.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of notifications.</returns>
    Task<List<Notification>> GetUserNotificationsAsync(
        string userId,
        bool includeRead = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets unread notification count for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Count of unread notifications.</returns>
    Task<int> GetUnreadCountAsync(
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retries failed notification delivery.
    /// </summary>
    /// <param name="notificationId">The notification ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<ServiceResult> RetryDeliveryAsync(
        KeyType notificationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cleans up old notifications based on configuration.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of notifications cleaned up.</returns>
    Task<int> CleanupOldNotificationsAsync(
        CancellationToken cancellationToken = default);
}
