using Craft.Core;

namespace Craft.Notifications;

/// <summary>
/// Defines the contract for managing user notification preferences.
/// </summary>
public interface INotificationPreferenceService
{
    /// <summary>
    /// Gets notification preferences for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="category">Optional category filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User's notification preferences.</returns>
    Task<NotificationPreference?> GetPreferenceAsync(
        string userId,
        string? category = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all notification preferences for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of user's notification preferences.</returns>
    Task<List<NotificationPreference>> GetAllPreferencesAsync(
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates notification preferences for a user.
    /// </summary>
    /// <param name="preference">The preference to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result with updated preference.</returns>
    Task<ServiceResult<NotificationPreference>> UpdatePreferenceAsync(
        NotificationPreference preference,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the enabled channels for a user and category.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="channels">The channels to enable.</param>
    /// <param name="category">Optional category.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<ServiceResult> SetEnabledChannelsAsync(
        string userId,
        NotificationChannel channels,
        string? category = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a specific channel is enabled for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="channel">The channel to check.</param>
    /// <param name="category">Optional category.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the channel is enabled.</returns>
    Task<bool> IsChannelEnabledAsync(
        string userId,
        NotificationChannel channel,
        string? category = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a push subscription for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="endpoint">The push endpoint URL.</param>
    /// <param name="publicKey">The push public key.</param>
    /// <param name="auth">The push auth secret.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<ServiceResult> RegisterPushSubscriptionAsync(
        string userId,
        string endpoint,
        string publicKey,
        string auth,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a push subscription for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<ServiceResult> RemovePushSubscriptionAsync(
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the effective channels for delivering a notification to a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="requestedChannels">The requested channels.</param>
    /// <param name="priority">The notification priority.</param>
    /// <param name="category">Optional category.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The channels that should be used for delivery.</returns>
    Task<NotificationChannel> GetEffectiveChannelsAsync(
        string userId,
        NotificationChannel requestedChannels,
        NotificationPriority priority,
        string? category = null,
        CancellationToken cancellationToken = default);
}
