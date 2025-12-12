using Microsoft.Extensions.DependencyInjection;

namespace Craft.Notifications.Tests;

public class NotificationServiceTests
{
    [Fact]
    public async Task SendAsync_CreatesNotification_Successfully()
    {
        // Arrange
        var dbContext = TestHelpers.CreateInMemoryDbContext();
        var serviceProvider = TestHelpers.CreateTestServiceProvider(dbContext: dbContext);
        var notificationService = serviceProvider.GetRequiredService<INotificationService>();
        
        var request = TestHelpers.CreateSampleRequest();

        // Act
        var result = await notificationService.SendAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(request.Title, result.Value.Title);
        Assert.Equal(request.Message, result.Value.Message);
        Assert.Equal(NotificationStatus.Delivered, result.Value.Status);
    }

    [Fact]
    public async Task SendAsync_WithNotification_PersistsToDatabase()
    {
        // Arrange
        var dbContext = TestHelpers.CreateInMemoryDbContext();
        var serviceProvider = TestHelpers.CreateTestServiceProvider(dbContext: dbContext);
        var notificationService = serviceProvider.GetRequiredService<INotificationService>();
        
        var notification = TestHelpers.CreateSampleNotification();

        // Act
        var result = await notificationService.SendAsync(notification);

        // Assert
        Assert.True(result.IsSuccess);
        
        var saved = await dbContext.Set<Notification>().FindAsync(result.Value!.Id);
        Assert.NotNull(saved);
        Assert.Equal(notification.Title, saved.Title);
    }

    [Fact]
    public async Task SendAsync_SetsDefaultExpiration()
    {
        // Arrange
        var options = new NotificationOptions { DefaultExpirationDays = 7 };
        var dbContext = TestHelpers.CreateInMemoryDbContext();
        var serviceProvider = TestHelpers.CreateTestServiceProvider(options, dbContext);
        var notificationService = serviceProvider.GetRequiredService<INotificationService>();
        
        var notification = TestHelpers.CreateSampleNotification();

        // Act
        var result = await notificationService.SendAsync(notification);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value!.ExpiresAt);
        Assert.True(result.Value.ExpiresAt > DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task SendBatchAsync_SendsMultipleNotifications()
    {
        // Arrange
        var dbContext = TestHelpers.CreateInMemoryDbContext();
        var serviceProvider = TestHelpers.CreateTestServiceProvider(dbContext: dbContext);
        var notificationService = serviceProvider.GetRequiredService<INotificationService>();
        
        var notifications = new[]
        {
            TestHelpers.CreateSampleNotification("user1"),
            TestHelpers.CreateSampleNotification("user2"),
            TestHelpers.CreateSampleNotification("user3")
        };

        // Act
        var result = await notificationService.SendBatchAsync(notifications);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.Count);
    }

    [Fact]
    public async Task SendBatchAsync_RespectsMaxBatchSize()
    {
        // Arrange
        var options = new NotificationOptions { MaxBatchSize = 2 };
        var dbContext = TestHelpers.CreateInMemoryDbContext();
        var serviceProvider = TestHelpers.CreateTestServiceProvider(options, dbContext);
        var notificationService = serviceProvider.GetRequiredService<INotificationService>();
        
        var notifications = new[]
        {
            TestHelpers.CreateSampleNotification("user1"),
            TestHelpers.CreateSampleNotification("user2"),
            TestHelpers.CreateSampleNotification("user3")
        };

        // Act
        var result = await notificationService.SendBatchAsync(notifications);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("exceeds maximum", result.ErrorMessage);
    }

    [Fact]
    public async Task SendToMultipleAsync_CreatesNotificationsForAllUsers()
    {
        // Arrange
        var dbContext = TestHelpers.CreateInMemoryDbContext();
        var serviceProvider = TestHelpers.CreateTestServiceProvider(dbContext: dbContext);
        var notificationService = serviceProvider.GetRequiredService<INotificationService>();
        
        var request = TestHelpers.CreateSampleRequest();
        var userIds = new[] { "user1", "user2", "user3" };

        // Act
        var result = await notificationService.SendToMultipleAsync(request, userIds);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.Count);
        Assert.All(result.Value, n => Assert.Contains(n.RecipientUserId!, userIds));
    }

    [Fact]
    public async Task ScheduleAsync_SetsScheduledTime()
    {
        // Arrange
        var dbContext = TestHelpers.CreateInMemoryDbContext();
        var serviceProvider = TestHelpers.CreateTestServiceProvider(dbContext: dbContext);
        var notificationService = serviceProvider.GetRequiredService<INotificationService>();
        
        var notification = TestHelpers.CreateSampleNotification();
        var scheduledFor = DateTimeOffset.UtcNow.AddHours(1);

        // Act
        var result = await notificationService.ScheduleAsync(notification, scheduledFor);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(scheduledFor, result.Value!.ScheduledFor);
        Assert.Equal(NotificationStatus.Queued, result.Value.Status);
    }

    [Fact]
    public async Task MarkAsReadAsync_UpdatesReadStatus()
    {
        // Arrange
        var dbContext = TestHelpers.CreateInMemoryDbContext();
        var serviceProvider = TestHelpers.CreateTestServiceProvider(dbContext: dbContext);
        var notificationService = serviceProvider.GetRequiredService<INotificationService>();
        
        var notification = TestHelpers.CreateSampleNotification();
        var sendResult = await notificationService.SendAsync(notification);
        var notificationId = sendResult.Value!.Id;

        // Act
        var result = await notificationService.MarkAsReadAsync(notificationId);

        // Assert
        Assert.True(result.IsSuccess);
        
        var updated = await dbContext.Set<Notification>().FindAsync(notificationId);
        Assert.NotNull(updated!.ReadAt);
        Assert.Equal(NotificationStatus.Read, updated.Status);
    }

    [Fact]
    public async Task MarkAllAsReadForUserAsync_UpdatesAllUnreadNotifications()
    {
        // Arrange
        var dbContext = TestHelpers.CreateInMemoryDbContext();
        var serviceProvider = TestHelpers.CreateTestServiceProvider(dbContext: dbContext);
        var notificationService = serviceProvider.GetRequiredService<INotificationService>();
        
        var userId = "user123";
        
        // Create multiple notifications
        await notificationService.SendAsync(TestHelpers.CreateSampleNotification(userId));
        await notificationService.SendAsync(TestHelpers.CreateSampleNotification(userId));
        await notificationService.SendAsync(TestHelpers.CreateSampleNotification(userId));

        // Act
        var result = await notificationService.MarkAllAsReadForUserAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);
        
        var unreadCount = await notificationService.GetUnreadCountAsync(userId);
        Assert.Equal(0, unreadCount);
    }

    [Fact]
    public async Task GetUserNotificationsAsync_ReturnsUserNotifications()
    {
        // Arrange
        var dbContext = TestHelpers.CreateInMemoryDbContext();
        var serviceProvider = TestHelpers.CreateTestServiceProvider(dbContext: dbContext);
        var notificationService = serviceProvider.GetRequiredService<INotificationService>();
        
        var userId = "user123";
        
        await notificationService.SendAsync(TestHelpers.CreateSampleNotification(userId));
        await notificationService.SendAsync(TestHelpers.CreateSampleNotification(userId));
        await notificationService.SendAsync(TestHelpers.CreateSampleNotification("otherUser"));

        // Act
        var notifications = await notificationService.GetUserNotificationsAsync(userId);

        // Assert
        Assert.Equal(2, notifications.Count);
        Assert.All(notifications, n => Assert.Equal(userId, n.RecipientUserId));
    }

    [Fact]
    public async Task GetUserNotificationsAsync_ExcludesReadByDefault()
    {
        // Arrange
        var dbContext = TestHelpers.CreateInMemoryDbContext();
        var serviceProvider = TestHelpers.CreateTestServiceProvider(dbContext: dbContext);
        var notificationService = serviceProvider.GetRequiredService<INotificationService>();
        
        var userId = "user123";
        
        var result1 = await notificationService.SendAsync(TestHelpers.CreateSampleNotification(userId));
        _ = await notificationService.SendAsync(TestHelpers.CreateSampleNotification(userId));

        await notificationService.MarkAsReadAsync(result1.Value!.Id);

        // Act
        var notifications = await notificationService.GetUserNotificationsAsync(userId, includeRead: false);

        // Assert
        Assert.Single(notifications);
        Assert.False(notifications[0].IsRead);
    }

    [Fact]
    public async Task GetUnreadCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        var dbContext = TestHelpers.CreateInMemoryDbContext();
        var serviceProvider = TestHelpers.CreateTestServiceProvider(dbContext: dbContext);
        var notificationService = serviceProvider.GetRequiredService<INotificationService>();
        
        var userId = "user123";
        
        var result1 = await notificationService.SendAsync(TestHelpers.CreateSampleNotification(userId));
        await notificationService.SendAsync(TestHelpers.CreateSampleNotification(userId));
        await notificationService.SendAsync(TestHelpers.CreateSampleNotification(userId));
        
        await notificationService.MarkAsReadAsync(result1.Value!.Id);

        // Act
        var count = await notificationService.GetUnreadCountAsync(userId);

        // Assert
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task DeleteAsync_RemovesNotification()
    {
        // Arrange
        var dbContext = TestHelpers.CreateInMemoryDbContext();
        var serviceProvider = TestHelpers.CreateTestServiceProvider(dbContext: dbContext);
        var notificationService = serviceProvider.GetRequiredService<INotificationService>();
        
        var notification = TestHelpers.CreateSampleNotification();
        var sendResult = await notificationService.SendAsync(notification);
        var notificationId = sendResult.Value!.Id;

        // Act
        var result = await notificationService.DeleteAsync(notificationId);

        // Assert
        Assert.True(result.IsSuccess);
        
        var deleted = await dbContext.Set<Notification>().FindAsync(notificationId);
        Assert.Null(deleted);
    }
}
