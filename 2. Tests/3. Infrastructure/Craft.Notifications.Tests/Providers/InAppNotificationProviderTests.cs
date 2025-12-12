using Microsoft.Extensions.Logging.Abstractions;

namespace Craft.Notifications.Tests;

public class InAppNotificationProviderTests
{
    private readonly NotificationOptions _options;
    private readonly InAppNotificationProvider _provider;

    public InAppNotificationProviderTests()
    {
        _options = new NotificationOptions();
        _provider = new InAppNotificationProvider(
            NullLogger<InAppNotificationProvider>.Instance,
            _options);
    }

    [Fact]
    public void Provider_Properties_AreSetCorrectly()
    {
        // Assert
        Assert.Equal(NotificationChannel.InApp, _provider.Channel);
        Assert.Equal("InApp", _provider.Name);
        Assert.Equal(1, _provider.Priority);
    }

    [Fact]
    public async Task SendAsync_ReturnsSuccess()
    {
        // Arrange
        var notification = new Notification
        {
            Id = 1,
            Title = "Test",
            Message = "Test message",
            RecipientUserId = "user123"
        };

        // Act
        var result = await _provider.SendAsync(notification);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(notification.Id, result.NotificationId);
        Assert.Equal(NotificationChannel.InApp, result.Channel);
        Assert.NotNull(result.ProviderResponse);
    }

    [Fact]
    public void CanDeliver_ReturnsTrueForInAppChannel()
    {
        // Arrange
        var notification = new Notification
        {
            Channels = NotificationChannel.InApp,
            RecipientUserId = "user123"
        };

        // Act
        var result = _provider.CanDeliver(notification);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanDeliver_ReturnsFalseWithoutRecipientUserId()
    {
        // Arrange
        var notification = new Notification
        {
            Channels = NotificationChannel.InApp,
            RecipientUserId = null
        };

        // Act
        var result = _provider.CanDeliver(notification);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanDeliver_ReturnsFalseForDifferentChannel()
    {
        // Arrange
        var notification = new Notification
        {
            Channels = NotificationChannel.Email,
            RecipientUserId = "user123"
        };

        // Act
        var result = _provider.CanDeliver(notification);

        // Assert
        Assert.False(result);
    }
}
