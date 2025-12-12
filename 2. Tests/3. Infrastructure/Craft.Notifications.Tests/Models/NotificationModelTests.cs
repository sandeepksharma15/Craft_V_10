namespace Craft.Notifications.Tests;

public class NotificationModelTests
{
    [Fact]
    public void Notification_DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var notification = new Notification();

        // Assert
        Assert.Equal(NotificationType.Info, notification.Type);
        Assert.Equal(NotificationPriority.Normal, notification.Priority);
        Assert.Equal(NotificationChannel.InApp, notification.Channels);
        Assert.Equal(NotificationStatus.Pending, notification.Status);
        Assert.False(notification.IsRead);
        Assert.Equal(0, notification.DeliveryAttempts);
        Assert.False(notification.IsDeleted);
    }

    [Fact]
    public void Notification_IsRead_ReturnsTrueWhenReadAtIsSet()
    {
        // Arrange
        var notification = new Notification
        {
            ReadAt = DateTimeOffset.UtcNow
        };

        // Act & Assert
        Assert.True(notification.IsRead);
    }

    [Fact]
    public void Notification_IsRead_ReturnsFalseWhenReadAtIsNull()
    {
        // Arrange
        var notification = new Notification
        {
            ReadAt = null
        };

        // Act & Assert
        Assert.False(notification.IsRead);
    }

    [Fact]
    public void Notification_SetMetadata_StoresDataCorrectly()
    {
        // Arrange
        var notification = new Notification();
        var metadata = new Dictionary<string, object>
        {
            ["Key1"] = "Value1",
            ["Key2"] = 123,
            ["Key3"] = true
        };

        // Act
        notification.SetMetadata(metadata);
        var result = notification.GetMetadata();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Equal("Value1", result["Key1"].ToString());
    }

    [Fact]
    public void Notification_GetMetadata_ReturnsNullForEmptyMetadata()
    {
        // Arrange
        var notification = new Notification
        {
            Metadata = null
        };

        // Act
        var result = notification.GetMetadata();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Notification_GetMetadata_ReturnsNullForInvalidJson()
    {
        // Arrange
        var notification = new Notification
        {
            Metadata = "invalid json {"
        };

        // Act
        var result = notification.GetMetadata();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void NotificationRequest_Validation_RequiresTitleAndMessage()
    {
        // Arrange
        var request = new NotificationRequest
        {
            Title = "",
            Message = ""
        };

        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(request, context, results, validateAllProperties: true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(NotificationRequest.Title)));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(NotificationRequest.Message)));
    }

    [Fact]
    public void NotificationRequest_Validation_ValidatesEmailAddress()
    {
        // Arrange
        var request = new NotificationRequest
        {
            Title = "Test",
            Message = "Test message",
            RecipientEmail = "invalid-email"
        };

        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(request, context, results, validateAllProperties: true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(NotificationRequest.RecipientEmail)));
    }

    [Fact]
    public void NotificationPreference_IsChannelEnabled_ReturnsTrueForEnabledChannel()
    {
        // Arrange
        var preference = new NotificationPreference
        {
            IsEnabled = true,
            EnabledChannels = NotificationChannel.InApp | NotificationChannel.Email
        };

        // Act & Assert
        Assert.True(preference.IsChannelEnabled(NotificationChannel.InApp));
        Assert.True(preference.IsChannelEnabled(NotificationChannel.Email));
        Assert.False(preference.IsChannelEnabled(NotificationChannel.Push));
    }

    [Fact]
    public void NotificationPreference_IsChannelEnabled_ReturnsFalseWhenDisabled()
    {
        // Arrange
        var preference = new NotificationPreference
        {
            IsEnabled = false,
            EnabledChannels = NotificationChannel.All
        };

        // Act & Assert
        Assert.False(preference.IsChannelEnabled(NotificationChannel.InApp));
    }

    [Theory]
    [InlineData(NotificationChannel.None, false)]
    [InlineData(NotificationChannel.InApp, true)]
    [InlineData(NotificationChannel.Email, false)]
    [InlineData(NotificationChannel.Push, false)]
    [InlineData(NotificationChannel.InApp | NotificationChannel.Email, true)]
    public void NotificationChannel_FlagsWork_Correctly(NotificationChannel channel, bool hasInApp)
    {
        // Act
        var result = channel.HasFlag(NotificationChannel.InApp);

        // Assert
        Assert.Equal(hasInApp, result);
    }

    [Fact]
    public void NotificationDeliveryResult_Success_CreatesSuccessResult()
    {
        // Arrange
        KeyType notificationId = 123L;
        var channel = NotificationChannel.Email;

        // Act
        var result = NotificationDeliveryResult.Success(notificationId, channel, "Sent", 100);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(notificationId, result.NotificationId);
        Assert.Equal(channel, result.Channel);
        Assert.Equal("Sent", result.ProviderResponse);
        Assert.Equal(100, result.DurationMs);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void NotificationDeliveryResult_Failure_CreatesFailureResult()
    {
        // Arrange
        KeyType notificationId = 123L;
        var channel = NotificationChannel.Email;
        var error = "SMTP error";

        // Act
        var result = NotificationDeliveryResult.Failure(notificationId, channel, error, 50);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(notificationId, result.NotificationId);
        Assert.Equal(channel, result.Channel);
        Assert.Equal(error, result.ErrorMessage);
        Assert.Equal(50, result.DurationMs);
        Assert.Null(result.ProviderResponse);
    }
}
