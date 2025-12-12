namespace Craft.Notifications;

/// <summary>
/// Represents the result of a notification delivery attempt.
/// </summary>
public class NotificationDeliveryResult
{
    /// <summary>
    /// Gets or sets the notification ID.
    /// </summary>
    public KeyType NotificationId { get; set; }

    /// <summary>
    /// Gets or sets the channel used for delivery.
    /// </summary>
    public NotificationChannel Channel { get; set; }

    /// <summary>
    /// Gets or sets whether the delivery was successful.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Gets or sets the error message if delivery failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the provider response.
    /// </summary>
    public string? ProviderResponse { get; set; }

    /// <summary>
    /// Gets or sets the delivery duration in milliseconds.
    /// </summary>
    public long? DurationMs { get; set; }

    /// <summary>
    /// Gets or sets the delivery timestamp.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Creates a successful delivery result.
    /// </summary>
    public static NotificationDeliveryResult Success(
        KeyType notificationId,
        NotificationChannel channel,
        string? providerResponse = null,
        long? durationMs = null)
    {
        return new NotificationDeliveryResult
        {
            NotificationId = notificationId,
            Channel = channel,
            IsSuccess = true,
            ProviderResponse = providerResponse,
            DurationMs = durationMs
        };
    }

    /// <summary>
    /// Creates a failed delivery result.
    /// </summary>
    public static NotificationDeliveryResult Failure(
        KeyType notificationId,
        NotificationChannel channel,
        string errorMessage,
        long? durationMs = null)
    {
        return new NotificationDeliveryResult
        {
            NotificationId = notificationId,
            Channel = channel,
            IsSuccess = false,
            ErrorMessage = errorMessage,
            DurationMs = durationMs
        };
    }
}
