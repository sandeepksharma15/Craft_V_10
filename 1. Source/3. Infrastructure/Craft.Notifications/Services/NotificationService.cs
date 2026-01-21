using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Craft.Notifications;

/// <summary>
/// Main notification service implementation.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly DbContext _dbContext;
    private readonly ILogger<NotificationService> _logger;
    private readonly NotificationOptions _options;
    private readonly IEnumerable<INotificationProvider> _providers;
    private readonly INotificationPreferenceService _preferenceService;
    private readonly INotificationRealTimeService? _realTimeService;

    public NotificationService(
        DbContext dbContext,
        ILogger<NotificationService> logger,
        IOptions<NotificationOptions> options,
        IEnumerable<INotificationProvider> providers,
        INotificationPreferenceService preferenceService,
        INotificationRealTimeService? realTimeService = null)
    {
        _dbContext = dbContext;
        _logger = logger;
        _options = options.Value;
        _providers = [.. providers.OrderBy(p => p.Priority)];
        _preferenceService = preferenceService;
        _realTimeService = realTimeService;
    }

    /// <inheritdoc/>
    public async Task<Result<Notification>> SendAsync(
        Notification notification,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Set defaults
            notification.CreatedAt = DateTimeOffset.UtcNow;
            notification.Status = NotificationStatus.Pending;

            if (notification.ExpiresAt == null)
            {
                notification.ExpiresAt = DateTimeOffset.UtcNow
                    .AddDays(_options.DefaultExpirationDays);
            }

            // Apply user preferences
            if (!string.IsNullOrEmpty(notification.RecipientUserId))
            {
                var effectiveChannels = await _preferenceService.GetEffectiveChannelsAsync(
                    notification.RecipientUserId,
                    notification.Channels,
                    notification.Priority,
                    notification.Category,
                    cancellationToken);

                notification.Channels = effectiveChannels;

                if (effectiveChannels == NotificationChannel.None)
                {
                    _logger.LogInformation(
                        "Notification {NotificationId} skipped due to user preferences for {UserId}",
                        notification.Id,
                        notification.RecipientUserId);

                    notification.Status = NotificationStatus.Cancelled;
                    return Result<Notification>.CreateFailure("Notification blocked by user preferences");
                }
            }

            // Persist to database if enabled
            if (_options.EnablePersistence)
            {
                await _dbContext.Set<Notification>().AddAsync(notification, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            // Deliver through providers
            await DeliverNotificationAsync(notification, cancellationToken);

            // Send real-time if enabled
            if (_options.EnableRealTimeDelivery &&
                _realTimeService != null &&
                !string.IsNullOrEmpty(notification.RecipientUserId))
            {
                await _realTimeService.SendRealTimeAsync(
                    notification.RecipientUserId,
                    notification,
                    cancellationToken);
            }

            _logger.LogInformation(
                "Notification {NotificationId} sent successfully via channels: {Channels}",
                notification.Id,
                notification.Channels);

            return Result<Notification>.CreateSuccess(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification");
            return Result<Notification>.CreateFailure($"Failed to send notification: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<Notification>> SendAsync(
        NotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        var notification = MapRequestToNotification(request);
        return await SendAsync(notification, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Result<List<Notification>>> SendBatchAsync(
        IEnumerable<Notification> notifications,
        CancellationToken cancellationToken = default)
    {
        if (!_options.EnableBatchProcessing)
        {
            return Result<List<Notification>>.CreateFailure("Batch processing is not enabled");
        }

        var notificationList = notifications.ToList();
        
        if (notificationList.Count > _options.MaxBatchSize)
        {
            return Result<List<Notification>>.CreateFailure(
                $"Batch size {notificationList.Count} exceeds maximum {_options.MaxBatchSize}");
        }

        try
        {
            var results = new List<Notification>();

            foreach (var notification in notificationList)
            {
                var result = await SendAsync(notification, cancellationToken);
                if (result.IsSuccess && result.Value != null)
                {
                    results.Add(result.Value);
                }
            }

            _logger.LogInformation(
                "Batch sent {SuccessCount} of {TotalCount} notifications",
                results.Count,
                notificationList.Count);

            return Result<List<Notification>>.CreateSuccess(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send batch notifications");
            return Result<List<Notification>>.CreateFailure($"Failed to send batch: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<List<Notification>>> SendToMultipleAsync(
        NotificationRequest request,
        IEnumerable<string> recipientUserIds,
        CancellationToken cancellationToken = default)
    {
        var notifications = recipientUserIds.Select(userId =>
        {
            var notification = MapRequestToNotification(request);
            notification.RecipientUserId = userId;
            return notification;
        });

        return await SendBatchAsync(notifications, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Result<Notification>> ScheduleAsync(
        Notification notification,
        DateTimeOffset scheduledFor,
        CancellationToken cancellationToken = default)
    {
        notification.ScheduledFor = scheduledFor;
        notification.Status = NotificationStatus.Queued;

        if (_options.EnablePersistence)
        {
            await _dbContext.Set<Notification>().AddAsync(notification, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation(
            "Notification {NotificationId} scheduled for {ScheduledTime}",
            notification.Id,
            scheduledFor);

        return Result<Notification>.CreateSuccess(notification);
    }

    /// <inheritdoc/>
    public async Task<Result> MarkAsReadAsync(
        KeyType notificationId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var notification = await _dbContext.Set<Notification>()
                .FindAsync([notificationId], cancellationToken);

            if (notification == null)
                return Result.CreateFailure("Notification not found");

            if (notification.ReadAt.HasValue)
                return Result.CreateSuccess(); // Already read

            notification.ReadAt = DateTimeOffset.UtcNow;
            notification.Status = NotificationStatus.Read;
            notification.ModifiedAt = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogDebug("Notification {NotificationId} marked as read", notificationId);
            return Result.CreateSuccess();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark notification {NotificationId} as read", notificationId);
            return Result.CreateFailure($"Failed to mark as read: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<Result> MarkAllAsReadAsync(
        IEnumerable<KeyType> notificationIds,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var ids = notificationIds.ToList();
            var notifications = await _dbContext.Set<Notification>()
                .Where(n => ids.Contains(n.Id))
                .ToListAsync(cancellationToken);

            var now = DateTimeOffset.UtcNow;
            foreach (var notification in notifications.Where(n => !n.ReadAt.HasValue))
            {
                notification.ReadAt = now;
                notification.Status = NotificationStatus.Read;
                notification.ModifiedAt = now;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Marked {Count} notifications as read", notifications.Count);
            return Result.CreateSuccess();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark notifications as read");
            return Result.CreateFailure($"Failed to mark as read: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<Result> MarkAllAsReadForUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var notifications = await _dbContext.Set<Notification>()
                .Where(n => n.RecipientUserId == userId && !n.ReadAt.HasValue)
                .ToListAsync(cancellationToken);

            var now = DateTimeOffset.UtcNow;
            foreach (var notification in notifications)
            {
                notification.ReadAt = now;
                notification.Status = NotificationStatus.Read;
                notification.ModifiedAt = now;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Marked all {Count} notifications as read for user {UserId}",
                notifications.Count,
                userId);

            return Result.CreateSuccess();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark all notifications as read for user {UserId}", userId);
            return Result.CreateFailure($"Failed to mark as read: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<Result> DeleteAsync(
        KeyType notificationId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var notification = await _dbContext.Set<Notification>()
                .FindAsync([notificationId], cancellationToken);

            if (notification == null)
                return Result.CreateFailure("Notification not found");

            _dbContext.Set<Notification>().Remove(notification);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogDebug("Notification {NotificationId} deleted", notificationId);
            return Result.CreateSuccess();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete notification {NotificationId}", notificationId);
            return Result.CreateFailure($"Failed to delete notification: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<List<Notification>> GetUserNotificationsAsync(
        string userId,
        bool includeRead = false,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<Notification>()
            .Where(n => n.RecipientUserId == userId);

        if (!includeRead)
            query = query.Where(n => !n.ReadAt.HasValue);

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> GetUnreadCountAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Notification>()
            .CountAsync(n => n.RecipientUserId == userId && !n.ReadAt.HasValue, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Result> RetryDeliveryAsync(
        KeyType notificationId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var notification = await _dbContext.Set<Notification>()
                .FindAsync([notificationId], cancellationToken);

            if (notification == null)
                return Result.CreateFailure("Notification not found");

            if (notification.DeliveryAttempts >= _options.MaxRetryAttempts)
            {
                return Result.CreateFailure($"Maximum retry attempts ({_options.MaxRetryAttempts}) exceeded");
            }

            await DeliverNotificationAsync(notification, cancellationToken);

            _logger.LogInformation("Retried delivery for notification {NotificationId}", notificationId);
            return Result.CreateSuccess();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retry notification {NotificationId}", notificationId);
            return Result.CreateFailure($"Failed to retry: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<int> CleanupOldNotificationsAsync(
        CancellationToken cancellationToken = default)
    {
        if (!_options.EnableAutoCleanup)
            return 0;

        try
        {
            var cutoffDate = DateTimeOffset.UtcNow.AddDays(-_options.CleanupAfterDays);

            var oldNotifications = await _dbContext.Set<Notification>()
                .Where(n => n.Status == NotificationStatus.Delivered || n.Status == NotificationStatus.Read)
                .Where(n => n.CreatedAt < cutoffDate)
                .ToListAsync(cancellationToken);

            _dbContext.Set<Notification>().RemoveRange(oldNotifications);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Cleaned up {Count} notifications older than {Days} days",
                oldNotifications.Count,
                _options.CleanupAfterDays);

            return oldNotifications.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup old notifications");
            return 0;
        }
    }

    private async Task DeliverNotificationAsync(
        Notification notification,
        CancellationToken cancellationToken)
    {
        notification.Status = NotificationStatus.Sending;
        notification.DeliveryAttempts++;

        var deliveryResults = new List<NotificationDeliveryResult>();
        var hasAnySuccess = false;

        foreach (var provider in _providers.Where(p => p.CanDeliver(notification)))
        {
            try
            {
                var result = await provider.SendAsync(notification, cancellationToken);
                deliveryResults.Add(result);

                if (result.IsSuccess)
                    hasAnySuccess = true;

                if (_options.LogDeliveryAttempts)
                {
                    await LogDeliveryAttemptAsync(notification, result, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Provider {Provider} failed to deliver notification {NotificationId}",
                    provider.Name,
                    notification.Id);

                var failureResult = NotificationDeliveryResult.Failure(
                    notification.Id,
                    provider.Channel,
                    ex.Message);

                deliveryResults.Add(failureResult);

                if (_options.LogDeliveryAttempts)
                {
                    await LogDeliveryAttemptAsync(notification, failureResult, cancellationToken);
                }
            }
        }

        // Update notification status
        if (hasAnySuccess)
        {
            notification.Status = NotificationStatus.Delivered;
            notification.DeliveredAt = DateTimeOffset.UtcNow;
            notification.ErrorMessage = null;
        }
        else
        {
            notification.Status = NotificationStatus.Failed;
            notification.ErrorMessage = string.Join("; ",
                deliveryResults.Where(r => !r.IsSuccess).Select(r => r.ErrorMessage));
        }

        notification.ModifiedAt = DateTimeOffset.UtcNow;

        if (_options.EnablePersistence)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task LogDeliveryAttemptAsync(
        Notification notification,
        NotificationDeliveryResult result,
        CancellationToken cancellationToken)
    {
        var log = new NotificationDeliveryLog
        {
            NotificationId = notification.Id,
            Channel = result.Channel,
            IsSuccess = result.IsSuccess,
            ErrorMessage = result.ErrorMessage,
            ProviderResponse = result.ProviderResponse,
            AttemptNumber = notification.DeliveryAttempts,
            DurationMs = result.DurationMs,
            CreatedAt = result.Timestamp
        };

        await _dbContext.Set<NotificationDeliveryLog>().AddAsync(log, cancellationToken);
    }

    private static Notification MapRequestToNotification(NotificationRequest request)
    {
        var notification = new Notification
        {
            Title = request.Title,
            Message = request.Message,
            Type = request.Type,
            Priority = request.Priority,
            Channels = request.Channels,
            RecipientUserId = request.RecipientUserId,
            RecipientEmail = request.RecipientEmail,
            RecipientPhone = request.RecipientPhone,
            ActionUrl = request.ActionUrl,
            ImageUrl = request.ImageUrl,
            Category = request.Category,
            ExpiresAt = request.ExpiresAt
        };

        if (request.Metadata != null)
        {
            notification.SetMetadata(request.Metadata);
        }

        return notification;
    }
}
