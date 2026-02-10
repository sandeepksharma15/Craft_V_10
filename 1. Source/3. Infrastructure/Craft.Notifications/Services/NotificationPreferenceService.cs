using Craft.Core;
using Microsoft.EntityFrameworkCore;

namespace Craft.Notifications;

/// <summary>
/// Service for managing user notification preferences.
/// </summary>
public class NotificationPreferenceService : INotificationPreferenceService
{
    private readonly DbContext _dbContext;
    private readonly ILogger<NotificationPreferenceService> _logger;
    private readonly NotificationOptions _options;

    public NotificationPreferenceService(
        DbContext dbContext,
        ILogger<NotificationPreferenceService> logger,
        NotificationOptions options)
    {
        _dbContext = dbContext;
        _logger = logger;
        _options = options;
    }

    /// <inheritdoc/>
    public async Task<NotificationPreference?> GetPreferenceAsync(
        string userId,
        string? category = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<NotificationPreference>()
            .Where(p => p.UserId == userId);

        if (!string.IsNullOrEmpty(category))
            query = query.Where(p => p.Category == category);
        else
            query = query.Where(p => p.Category == null);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<NotificationPreference>> GetAllPreferencesAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<NotificationPreference>()
            .Where(p => p.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<NotificationPreference>> UpdatePreferenceAsync(
        NotificationPreference preference,
        CancellationToken cancellationToken = default)
    {
        try
        {
            preference.ModifiedAt = DateTimeOffset.UtcNow;

            var existing = await GetPreferenceAsync(
                preference.UserId,
                preference.Category,
                cancellationToken);

            if (existing != null)
            {
                preference.Id = existing.Id;
                _dbContext.Set<NotificationPreference>().Update(preference);
            }
            else
            {
                preference.CreatedAt = DateTimeOffset.UtcNow;
                await _dbContext.Set<NotificationPreference>().AddAsync(preference, cancellationToken);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Updated notification preferences for user {UserId}, category {Category}",
                preference.UserId,
                preference.Category ?? "default");

            return ServiceResult<NotificationPreference>.Success(preference);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update notification preferences for user {UserId}", preference.UserId);
            return ServiceResult<NotificationPreference>.Failure($"Failed to update preferences: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<ServiceResult> SetEnabledChannelsAsync(
        string userId,
        NotificationChannel channels,
        string? category = null,
        CancellationToken cancellationToken = default)
    {
        var preference = await GetPreferenceAsync(userId, category, cancellationToken)
            ?? new NotificationPreference
            {
                UserId = userId,
                Category = category
            };

        preference.EnabledChannels = channels;
        return await UpdatePreferenceAsync(preference, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> IsChannelEnabledAsync(
        string userId,
        NotificationChannel channel,
        string? category = null,
        CancellationToken cancellationToken = default)
    {
        var preference = await GetPreferenceAsync(userId, category, cancellationToken);
        
        if (preference == null)
            return _options.DefaultChannels.HasFlag(channel);

        return preference.IsChannelEnabled(channel);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult> RegisterPushSubscriptionAsync(
        string userId,
        string endpoint,
        string publicKey,
        string auth,
        CancellationToken cancellationToken = default)
    {
        var preference = await GetPreferenceAsync(userId, null, cancellationToken)
            ?? new NotificationPreference { UserId = userId };

        preference.PushEndpoint = endpoint;
        preference.PushPublicKey = publicKey;
        preference.PushAuth = auth;

        // Ensure Push channel is enabled
        if (!preference.EnabledChannels.HasFlag(NotificationChannel.Push))
            preference.EnabledChannels |= NotificationChannel.Push;

        return await UpdatePreferenceAsync(preference, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult> RemovePushSubscriptionAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var preference = await GetPreferenceAsync(userId, null, cancellationToken);
        
        if (preference == null)
            return ServiceResult.Success();

        preference.PushEndpoint = null;
        preference.PushPublicKey = null;
        preference.PushAuth = null;

        // Disable Push channel
        preference.EnabledChannels &= ~NotificationChannel.Push;

        return await UpdatePreferenceAsync(preference, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<NotificationChannel> GetEffectiveChannelsAsync(
        string userId,
        NotificationChannel requestedChannels,
        NotificationPriority priority,
        string? category = null,
        CancellationToken cancellationToken = default)
    {
        var preference = await GetPreferenceAsync(userId, category, cancellationToken);

        // If no preference, use defaults
        if (preference == null || !preference.IsEnabled)
            return _options.DefaultChannels & requestedChannels;

        // Check minimum priority
        if (priority < preference.MinimumPriority)
            return NotificationChannel.None;

        // Return intersection of requested and enabled channels
        return requestedChannels & preference.EnabledChannels;
    }
}


