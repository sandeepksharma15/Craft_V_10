using Microsoft.Extensions.DependencyInjection;

namespace Craft.Notifications.Tests;

public class NotificationPreferenceServiceTests
{
    [Fact]
    public async Task GetPreferenceAsync_ReturnsNullForNonExistentUser()
    {
        // Arrange
        var dbContext = TestHelpers.CreateInMemoryDbContext();
        var serviceProvider = TestHelpers.CreateTestServiceProvider(dbContext: dbContext);
        var preferenceService = serviceProvider.GetRequiredService<INotificationPreferenceService>();

        // Act
        var preference = await preferenceService.GetPreferenceAsync("nonexistent");

        // Assert
        Assert.Null(preference);
    }

    [Fact]
    public async Task UpdatePreferenceAsync_CreatesNewPreference()
    {
        // Arrange
        var dbContext = TestHelpers.CreateInMemoryDbContext();
        var serviceProvider = TestHelpers.CreateTestServiceProvider(dbContext: dbContext);
        var preferenceService = serviceProvider.GetRequiredService<INotificationPreferenceService>();
        
        var preference = TestHelpers.CreateSamplePreference();

        // Act
        var result = await preferenceService.UpdatePreferenceAsync(preference);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(preference.UserId, result.Value.UserId);
    }

    [Fact]
    public async Task UpdatePreferenceAsync_UpdatesExistingPreference()
    {
        // Arrange
        var dbContext = TestHelpers.CreateInMemoryDbContext();
        var serviceProvider = TestHelpers.CreateTestServiceProvider(dbContext: dbContext);
        var preferenceService = serviceProvider.GetRequiredService<INotificationPreferenceService>();
        
        var preference = TestHelpers.CreateSamplePreference();
        await preferenceService.UpdatePreferenceAsync(preference);

        // Modify preference
        preference.EnabledChannels = NotificationChannel.InApp;

        // Act
        var result = await preferenceService.UpdatePreferenceAsync(preference);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(NotificationChannel.InApp, result.Value!.EnabledChannels);
    }

    [Fact]
    public async Task SetEnabledChannelsAsync_UpdatesChannels()
    {
        // Arrange
        var dbContext = TestHelpers.CreateInMemoryDbContext();
        var serviceProvider = TestHelpers.CreateTestServiceProvider(dbContext: dbContext);
        var preferenceService = serviceProvider.GetRequiredService<INotificationPreferenceService>();
        
        var userId = "user123";
        var channels = NotificationChannel.InApp | NotificationChannel.Email;

        // Act
        var result = await preferenceService.SetEnabledChannelsAsync(userId, channels);

        // Assert
        Assert.True(result.IsSuccess);
        
        var preference = await preferenceService.GetPreferenceAsync(userId);
        Assert.NotNull(preference);
        Assert.Equal(channels, preference.EnabledChannels);
    }

    [Fact]
    public async Task IsChannelEnabledAsync_ReturnsTrueForEnabledChannel()
    {
        // Arrange
        var dbContext = TestHelpers.CreateInMemoryDbContext();
        var serviceProvider = TestHelpers.CreateTestServiceProvider(dbContext: dbContext);
        var preferenceService = serviceProvider.GetRequiredService<INotificationPreferenceService>();
        
        var userId = "user123";
        await preferenceService.SetEnabledChannelsAsync(
            userId,
            NotificationChannel.InApp | NotificationChannel.Email);

        // Act
        var isInAppEnabled = await preferenceService.IsChannelEnabledAsync(userId, NotificationChannel.InApp);
        var isEmailEnabled = await preferenceService.IsChannelEnabledAsync(userId, NotificationChannel.Email);
        var isPushEnabled = await preferenceService.IsChannelEnabledAsync(userId, NotificationChannel.Push);

        // Assert
        Assert.True(isInAppEnabled);
        Assert.True(isEmailEnabled);
        Assert.False(isPushEnabled);
    }

    [Fact]
    public async Task IsChannelEnabledAsync_ReturnsDefaultForNewUser()
    {
        // Arrange
        var options = new NotificationOptions
        {
            DefaultChannels = NotificationChannel.InApp
        };
        var dbContext = TestHelpers.CreateInMemoryDbContext();
        var serviceProvider = TestHelpers.CreateTestServiceProvider(options, dbContext);
        var preferenceService = serviceProvider.GetRequiredService<INotificationPreferenceService>();

        // Act
        var isInAppEnabled = await preferenceService.IsChannelEnabledAsync("newuser", NotificationChannel.InApp);
        var isEmailEnabled = await preferenceService.IsChannelEnabledAsync("newuser", NotificationChannel.Email);

        // Assert
        Assert.True(isInAppEnabled);
        Assert.False(isEmailEnabled);
    }

    [Fact]
    public async Task RegisterPushSubscriptionAsync_SavesSubscriptionInfo()
    {
        // Arrange
        var dbContext = TestHelpers.CreateInMemoryDbContext();
        var serviceProvider = TestHelpers.CreateTestServiceProvider(dbContext: dbContext);
        var preferenceService = serviceProvider.GetRequiredService<INotificationPreferenceService>();
        
        var userId = "user123";
        var endpoint = "https://fcm.googleapis.com/...";
        var publicKey = "BEl62i...";
        var auth = "tBHItJ...";

        // Act
        var result = await preferenceService.RegisterPushSubscriptionAsync(userId, endpoint, publicKey, auth);

        // Assert
        Assert.True(result.IsSuccess);
        
        var preference = await preferenceService.GetPreferenceAsync(userId);
        Assert.NotNull(preference);
        Assert.Equal(endpoint, preference.PushEndpoint);
        Assert.Equal(publicKey, preference.PushPublicKey);
        Assert.Equal(auth, preference.PushAuth);
        Assert.True(preference.EnabledChannels.HasFlag(NotificationChannel.Push));
    }

    [Fact]
    public async Task RemovePushSubscriptionAsync_ClearsSubscriptionInfo()
    {
        // Arrange
        var dbContext = TestHelpers.CreateInMemoryDbContext();
        var serviceProvider = TestHelpers.CreateTestServiceProvider(dbContext: dbContext);
        var preferenceService = serviceProvider.GetRequiredService<INotificationPreferenceService>();
        
        var userId = "user123";
        await preferenceService.RegisterPushSubscriptionAsync(userId, "endpoint", "key", "auth");

        // Act
        var result = await preferenceService.RemovePushSubscriptionAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);
        
        var preference = await preferenceService.GetPreferenceAsync(userId);
        Assert.NotNull(preference);
        Assert.Null(preference.PushEndpoint);
        Assert.Null(preference.PushPublicKey);
        Assert.Null(preference.PushAuth);
        Assert.False(preference.EnabledChannels.HasFlag(NotificationChannel.Push));
    }

    [Fact]
    public async Task GetEffectiveChannelsAsync_FiltersBasedOnPreferences()
    {
        // Arrange
        var dbContext = TestHelpers.CreateInMemoryDbContext();
        var serviceProvider = TestHelpers.CreateTestServiceProvider(dbContext: dbContext);
        var preferenceService = serviceProvider.GetRequiredService<INotificationPreferenceService>();
        
        var userId = "user123";
        var preference = new NotificationPreference
        {
            UserId = userId,
            IsEnabled = true,
            EnabledChannels = NotificationChannel.InApp | NotificationChannel.Email,
            MinimumPriority = NotificationPriority.Normal
        };
        await preferenceService.UpdatePreferenceAsync(preference);

        // Act
        var effectiveChannels = await preferenceService.GetEffectiveChannelsAsync(
            userId,
            NotificationChannel.All,
            NotificationPriority.High);

        // Assert
        Assert.Equal(NotificationChannel.InApp | NotificationChannel.Email, effectiveChannels);
    }

    [Fact]
    public async Task GetEffectiveChannelsAsync_ReturnsNoneForLowPriority()
    {
        // Arrange
        var dbContext = TestHelpers.CreateInMemoryDbContext();
        var serviceProvider = TestHelpers.CreateTestServiceProvider(dbContext: dbContext);
        var preferenceService = serviceProvider.GetRequiredService<INotificationPreferenceService>();
        
        var userId = "user123";
        var preference = new NotificationPreference
        {
            UserId = userId,
            IsEnabled = true,
            EnabledChannels = NotificationChannel.All,
            MinimumPriority = NotificationPriority.High
        };
        await preferenceService.UpdatePreferenceAsync(preference);

        // Act
        var effectiveChannels = await preferenceService.GetEffectiveChannelsAsync(
            userId,
            NotificationChannel.All,
            NotificationPriority.Normal);

        // Assert
        Assert.Equal(NotificationChannel.None, effectiveChannels);
    }

    [Fact]
    public async Task GetAllPreferencesAsync_ReturnsAllUserPreferences()
    {
        // Arrange
        var dbContext = TestHelpers.CreateInMemoryDbContext();
        var serviceProvider = TestHelpers.CreateTestServiceProvider(dbContext: dbContext);
        var preferenceService = serviceProvider.GetRequiredService<INotificationPreferenceService>();
        
        var userId = "user123";
        
        // Create preferences for different categories
        await preferenceService.UpdatePreferenceAsync(new NotificationPreference
        {
            UserId = userId,
            Category = null,
            EnabledChannels = NotificationChannel.All
        });
        
        await preferenceService.UpdatePreferenceAsync(new NotificationPreference
        {
            UserId = userId,
            Category = "Security",
            EnabledChannels = NotificationChannel.InApp
        });

        // Act
        var preferences = await preferenceService.GetAllPreferencesAsync(userId);

        // Assert
        Assert.Equal(2, preferences.Count);
        Assert.All(preferences, p => Assert.Equal(userId, p.UserId));
    }
}
