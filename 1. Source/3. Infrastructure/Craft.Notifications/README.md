# Craft.Notifications - Production-Ready Notification Service

> **Version:** 1.0.0 | **Target Framework:** .NET 10

## ?? Table of Contents

1. [Overview](#overview)
2. [Features](#features)
3. [Quick Start](#quick-start)
4. [Configuration](#configuration)
5. [Notification Channels](#notification-channels)
6. [User Preferences](#user-preferences)
7. [Sending Notifications](#sending-notifications)
8. [Batch Notifications](#batch-notifications)
9. [Scheduled Notifications](#scheduled-notifications)
10. [Priority Levels](#priority-levels)
11. [Database Setup](#database-setup)
12. [Custom Providers](#custom-providers)
13. [Real-Time Delivery](#real-time-delivery)
14. [Best Practices](#best-practices)
15. [API Reference](#api-reference)
16. [Testing](#testing)
17. [Troubleshooting](#troubleshooting)

---

## Overview

`Craft.Notifications` provides a comprehensive, production-ready notification system with multi-channel delivery, user preferences, batch processing, and priority-based routing. Built with extensibility and scalability in mind.

### Why Craft.Notifications?

- ? **Multi-Channel Delivery** - In-app, email, push, SMS, webhooks
- ? **User Preferences** - Channel-specific preferences per user/category
- ? **Batch Processing** - Efficient bulk notification delivery
- ? **Priority Levels** - Urgent, high, normal, low with smart routing
- ? **Database Persistence** - Full Entity Framework Core integration
- ? **Real-Time Infrastructure** - Ready for SignalR integration
- ? **Web Push Support** - VAPID protocol for browser notifications
- ? **Teams Integration** - Microsoft Teams webhook support
- ? **Extensible Providers** - Easy to add custom delivery channels
- ? **Delivery Tracking** - Complete audit trail of delivery attempts
- ? **Auto Cleanup** - Configurable retention policies

---

## Features

### ?? Core Features

- ? **In-App Notifications**: Database-persisted notifications
- ? **Email Notifications**: Integration point for email providers
- ? **Push Notifications**: Web Push with VAPID
- ? **SMS Notifications**: Integration point for SMS providers
- ? **Webhooks**: Generic HTTP callbacks and Teams integration

### ?? User Management

- ? **User Preferences**: Per-user, per-category channel preferences
- ? **Priority Filtering**: Minimum priority threshold per user
- ? **Channel Selection**: Enable/disable specific channels
- ? **Push Subscriptions**: VAPID subscription management

### ?? Enterprise Features

- ? **Batch Processing**: Send to multiple users efficiently
- ? **Scheduled Delivery**: Schedule notifications for future delivery
- ? **Retry Logic**: Automatic retry with configurable attempts
- ? **Delivery Logs**: Complete audit trail
- ? **Auto Cleanup**: Retention policy management
- ? **Multi-Tenancy**: Tenant-aware notifications

---

## Quick Start

### 1. Installation

The project is already in your solution:
- **Project**: `Craft.Notifications`
- **Location**: `1. Source/3. Infrastructure/Craft.Notifications/`

### 2. Add to Your DbContext

```csharp
using Craft.Notifications;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<NotificationDeliveryLog> NotificationDeliveryLogs { get; set; }
    public DbSet<NotificationPreference> NotificationPreferences { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure notification entities
        modelBuilder.ConfigureNotificationEntities();
    }
}
```

### 3. Configuration

Add to your `appsettings.json`:

```json
{
  "NotificationOptions": {
    "EnablePersistence": true,
    "EnableRealTimeDelivery": false,
    "EnableBatchProcessing": true,
    "MaxBatchSize": 100,
    "DefaultExpirationDays": 30,
    "MaxRetryAttempts": 3,
    "RetryDelaySeconds": 60,
    "AutoMarkAsRead": false,
    "EnableMultiTenancy": false,
    "DefaultChannels": "InApp",
    "LogDeliveryAttempts": true,
    "EnableAutoCleanup": true,
    "CleanupAfterDays": 90,
    "VapidSubject": "mailto:admin@example.com",
    "VapidPublicKey": "your-vapid-public-key",
    "VapidPrivateKey": "your-vapid-private-key",
    "TeamsWebhookUrl": "https://outlook.office.com/webhook/...",
    "EnableDetailedLogging": false
  }
}
```

### 4. Register Services

In `Program.cs`:

```csharp
using Craft.Notifications;

var builder = WebApplication.CreateBuilder(args);

// Add notification services
builder.Services.AddNotificationServices(builder.Configuration);

// Add your DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();
app.Run();
```

### 5. Create Database Migration

```bash
dotnet ef migrations add AddNotifications
dotnet ef database update
```

### 6. Send Your First Notification

```csharp
public class WelcomeService
{
    private readonly INotificationService _notificationService;

    public WelcomeService(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task SendWelcomeNotificationAsync(string userId, string userName)
    {
        var notification = new NotificationRequest
        {
            Title = "Welcome!",
            Message = $"Welcome to our platform, {userName}!",
            Type = NotificationType.Info,
            Priority = NotificationPriority.Normal,
            Channels = NotificationChannel.InApp | NotificationChannel.Email,
            RecipientUserId = userId,
            Category = "Onboarding"
        };

        var result = await _notificationService.SendAsync(notification);

        if (result.IsSuccess)
        {
            Console.WriteLine($"Notification sent: {result.Value.Id}");
        }
        else
        {
            Console.WriteLine($"Failed: {result.ErrorMessage}");
        }
    }
}
```

---

## Configuration

### NotificationOptions Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `EnablePersistence` | bool | true | Persist notifications to database |
| `EnableRealTimeDelivery` | bool | false | Enable real-time delivery infrastructure |
| `EnableBatchProcessing` | bool | true | Allow batch notification sending |
| `MaxBatchSize` | int | 100 | Maximum notifications per batch (1-1000) |
| `DefaultExpirationDays` | int | 30 | Default notification expiration (1-365) |
| `MaxRetryAttempts` | int | 3 | Maximum delivery retry attempts (0-10) |
| `RetryDelaySeconds` | int | 60 | Delay between retries (1-3600) |
| `AutoMarkAsRead` | bool | false | Auto-mark as read after delivery |
| `EnableMultiTenancy` | bool | false | Enable multi-tenant support |
| `DefaultChannels` | NotificationChannel | InApp | Default channels for new notifications |
| `LogDeliveryAttempts` | bool | true | Log all delivery attempts |
| `EnableAutoCleanup` | bool | true | Enable automatic cleanup of old notifications |
| `CleanupAfterDays` | int | 90 | Days to keep delivered notifications (1-365) |
| `VapidSubject` | string? | null | VAPID subject for web push |
| `VapidPublicKey` | string? | null | VAPID public key |
| `VapidPrivateKey` | string? | null | VAPID private key |
| `TeamsWebhookUrl` | string? | null | Microsoft Teams webhook URL |
| `EnableDetailedLogging` | bool | false | Enable verbose logging |

---

## Notification Channels

### Available Channels

```csharp
[Flags]
public enum NotificationChannel
{
    None = 0,
    InApp = 1,      // Database-stored, in-app display
    Email = 2,      // Email delivery (requires provider)
    Push = 4,       // Web/mobile push notifications
    Sms = 8,        // SMS text messages (requires provider)
    Webhook = 16,   // HTTP webhooks
    All = InApp | Email | Push | Sms | Webhook
}
```

### Channel Combinations

```csharp
// Single channel
Channels = NotificationChannel.InApp

// Multiple channels
Channels = NotificationChannel.InApp | NotificationChannel.Email

// All channels
Channels = NotificationChannel.All

// All except SMS
Channels = NotificationChannel.All & ~NotificationChannel.Sms
```

### Built-In Providers

| Provider | Channel | Priority | Status |
|----------|---------|----------|--------|
| InAppNotificationProvider | InApp | 1 | ? Implemented |
| WebPushNotificationProvider | Push | 10 | ? Implemented (VAPID) |
| WebhookNotificationProvider | Webhook | 20 | ? Implemented |
| TeamsWebhookNotificationProvider | Webhook | 21 | ? Implemented |
| EmailNotificationProvider | Email | - | ?? Extensible |
| SmsNotificationProvider | Sms | - | ?? Extensible |

---

## User Preferences

### Setting User Preferences

```csharp
public class UserSettingsService
{
    private readonly INotificationPreferenceService _preferenceService;

    public async Task UpdatePreferencesAsync(string userId)
    {
        // Enable only in-app and email
        await _preferenceService.SetEnabledChannelsAsync(
            userId,
            NotificationChannel.InApp | NotificationChannel.Email);

        // Or update full preference
        var preference = new NotificationPreference
        {
            UserId = userId,
            EnabledChannels = NotificationChannel.InApp | NotificationChannel.Push,
            MinimumPriority = NotificationPriority.Normal,
            IsEnabled = true,
            Email = "user@example.com"
        };

        await _preferenceService.UpdatePreferenceAsync(preference);
    }
}
```

### Category-Specific Preferences

```csharp
// Global preferences (no category)
await _preferenceService.SetEnabledChannelsAsync(
    userId,
    NotificationChannel.InApp);

// Category-specific preferences
await _preferenceService.SetEnabledChannelsAsync(
    userId,
    NotificationChannel.All,
    category: "Marketing");

await _preferenceService.SetEnabledChannelsAsync(
    userId,
    NotificationChannel.InApp | NotificationChannel.Email,
    category: "Security");
```

### Registering Push Subscriptions

```csharp
// Register web push subscription
await _preferenceService.RegisterPushSubscriptionAsync(
    userId,
    endpoint: "https://fcm.googleapis.com/fcm/send/...",
    publicKey: "BEl62iUY...",
    auth: "tBHItJI5sv...");

// Remove push subscription
await _preferenceService.RemovePushSubscriptionAsync(userId);

// Check if channel is enabled
var isPushEnabled = await _preferenceService.IsChannelEnabledAsync(
    userId,
    NotificationChannel.Push,
    category: "Alerts");
```

---

## Sending Notifications

### Simple Notification

```csharp
var request = new NotificationRequest
{
    Title = "New Message",
    Message = "You have a new message from John",
    Type = NotificationType.Info,
    Priority = NotificationPriority.Normal,
    Channels = NotificationChannel.InApp,
    RecipientUserId = "user123"
};

var result = await _notificationService.SendAsync(request);
```

### With Action URL

```csharp
var request = new NotificationRequest
{
    Title = "Order Shipped",
    Message = "Your order #12345 has been shipped",
    Type = NotificationType.Success,
    Priority = NotificationPriority.High,
    Channels = NotificationChannel.InApp | NotificationChannel.Email,
    RecipientUserId = "user123",
    ActionUrl = "/orders/12345",
    ImageUrl = "/images/shipping-truck.png"
};

await _notificationService.SendAsync(request);
```

### With Metadata

```csharp
var request = new NotificationRequest
{
    Title = "Payment Required",
    Message = "Please update your payment method",
    Type = NotificationType.Action,
    Priority = NotificationPriority.Urgent,
    Channels = NotificationChannel.All,
    RecipientUserId = "user123",
    Metadata = new Dictionary<string, object>
    {
        ["Amount"] = 99.99,
        ["Currency"] = "USD",
        ["InvoiceId"] = "INV-2024-001"
    }
};

await _notificationService.SendAsync(request);
```

### Notification Types

```csharp
public enum NotificationType
{
    Info,       // General information (blue)
    Success,    // Success message (green)
    Warning,    // Warning message (orange)
    Error,      // Error message (red)
    System,     // System notification (gray)
    Action,     // Requires user action
    Reminder,   // Reminder notification
    Alert       // Alert notification (red)
}
```

---

## Batch Notifications

### Send to Multiple Users

```csharp
var request = new NotificationRequest
{
    Title = "System Maintenance",
    Message = "System will be down for maintenance on Saturday",
    Type = NotificationType.System,
    Priority = NotificationPriority.High,
    Channels = NotificationChannel.InApp | NotificationChannel.Email
};

var userIds = new[] { "user1", "user2", "user3", "user4" };

var result = await _notificationService.SendToMultipleAsync(request, userIds);

Console.WriteLine($"Sent {result.Value.Count} notifications");
```

### Custom Batch

```csharp
var notifications = new List<Notification>
{
    new Notification
    {
        Title = "Welcome!",
        Message = "Welcome User 1",
        RecipientUserId = "user1"
    },
    new Notification
    {
        Title = "Welcome!",
        Message = "Welcome User 2",
        RecipientUserId = "user2"
    }
};

var result = await _notificationService.SendBatchAsync(notifications);
```

### Batch Size Limits

```json
{
  "NotificationOptions": {
    "MaxBatchSize": 100  // Maximum notifications per batch
  }
}
```

---

## Scheduled Notifications

### Schedule for Specific Time

```csharp
var notification = new Notification
{
    Title = "Meeting Reminder",
    Message = "Meeting starts in 15 minutes",
    Type = NotificationType.Reminder,
    Priority = NotificationPriority.High,
    Channels = NotificationChannel.InApp | NotificationChannel.Push,
    RecipientUserId = "user123"
};

var scheduledFor = DateTimeOffset.UtcNow.AddMinutes(15);

await _notificationService.ScheduleAsync(notification, scheduledFor);
```

### Process Scheduled Notifications

Create a background job to process scheduled notifications:

```csharp
// Using Craft.Jobs or Hangfire
public class ProcessScheduledNotificationsJob : IBackgroundJob
{
    private readonly DbContext _dbContext;
    private readonly INotificationService _notificationService;

    public async Task ExecuteAsync(
        JobExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        
        var dueNotifications = await _dbContext.Set<Notification>()
            .Where(n => n.Status == NotificationStatus.Queued)
            .Where(n => n.ScheduledFor <= now)
            .ToListAsync(cancellationToken);

        foreach (var notification in dueNotifications)
        {
            await _notificationService.SendAsync(notification, cancellationToken);
        }
    }
}

// Register as recurring job
_jobScheduler.ScheduleRecurring<ProcessScheduledNotificationsJob>(
    "process-scheduled-notifications",
    CronExpressions.EveryMinute);
```

---

## Priority Levels

### Priority Enum

```csharp
public enum NotificationPriority
{
    Low = 0,        // Can be delayed, batched
    Normal = 1,     // Standard delivery
    High = 2,       // Deliver quickly
    Urgent = 3      // Immediate delivery
}
```

### Using Priority

```csharp
// Low priority - system updates, tips
var lowPriority = new NotificationRequest
{
    Title = "New Feature Available",
    Priority = NotificationPriority.Low,
    // ...
};

// Urgent priority - critical alerts
var urgentNotification = new NotificationRequest
{
    Title = "Security Alert",
    Message = "Unusual login detected",
    Priority = NotificationPriority.Urgent,
    Channels = NotificationChannel.All,
    // ...
};
```

### User Preference Filtering

```csharp
// User can set minimum priority threshold
var preference = new NotificationPreference
{
    UserId = "user123",
    MinimumPriority = NotificationPriority.High  // Only receive High and Urgent
};

await _preferenceService.UpdatePreferenceAsync(preference);
```

---

## Database Setup

### Entity Framework Core

The notification system uses Entity Framework Core for persistence.

### Entities

1. **Notification** - Main notification entity
2. **NotificationDeliveryLog** - Delivery attempt logs
3. **NotificationPreference** - User preferences

### Migration

```bash
# Add migration
dotnet ef migrations add AddNotificationSystem

# Update database
dotnet ef database update
```

### Manual DbContext Configuration

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    
    // Configure notification entities
    modelBuilder.ConfigureNotificationEntities();
    
    // Or manually configure
    modelBuilder.Entity<Notification>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => e.RecipientUserId);
        entity.HasIndex(e => e.Status);
        // ... more configuration
    });
}
```

### Database Tables

| Table | Description |
|-------|-------------|
| `Notifications` | Main notification records |
| `NotificationDeliveryLogs` | Delivery attempt audit trail |
| `NotificationPreferences` | User channel preferences |

---

## Custom Providers

### Creating a Custom Provider

```csharp
using Craft.Notifications;

public class CustomEmailProvider : NotificationProviderBase
{
    private readonly IEmailService _emailService;

    public CustomEmailProvider(
        ILogger<CustomEmailProvider> logger,
        NotificationOptions options,
        IEmailService emailService)
        : base(logger, options)
    {
        _emailService = emailService;
    }

    public override NotificationChannel Channel => NotificationChannel.Email;
    public override string Name => "CustomEmail";
    public override int Priority => 15;

    public override async Task<NotificationDeliveryResult> SendAsync(
        Notification notification,
        CancellationToken cancellationToken = default)
    {
        LogDeliveryStart(notification);

        var (success, durationMs) = await MeasureAsync(async () =>
        {
            try
            {
                await _emailService.SendEmailAsync(
                    to: notification.RecipientEmail!,
                    subject: notification.Title,
                    body: notification.Message,
                    cancellationToken: cancellationToken);
                
                return (true, (string?)null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        });

        if (success)
        {
            LogDeliverySuccess(notification, durationMs);
            return NotificationDeliveryResult.Success(
                notification.Id,
                Channel,
                "Email sent successfully",
                durationMs);
        }

        var error = success ? "Unknown error" : "Email delivery failed";
        LogDeliveryFailure(notification, error, durationMs);
        return NotificationDeliveryResult.Failure(
            notification.Id,
            Channel,
            error,
            durationMs);
    }

    public override bool CanDeliver(Notification notification)
    {
        return base.CanDeliver(notification) &&
               !string.IsNullOrEmpty(notification.RecipientEmail);
    }
}
```

### Register Custom Provider

```csharp
// Add to existing providers
builder.Services.AddNotificationServices(builder.Configuration)
    .AddNotificationProvider<CustomEmailProvider>();

// Replace default provider
builder.Services.AddNotificationServices(builder.Configuration)
    .ReplaceNotificationProvider<CustomEmailProvider>(NotificationChannel.Email);
```

---

## Real-Time Delivery

### Infrastructure Included

The notification service includes infrastructure for real-time delivery but requires you to implement the actual delivery mechanism (e.g., SignalR).

### Implementing Real-Time with SignalR

```csharp
using Microsoft.AspNetCore.SignalR;

public class NotificationHub : Hub
{
    public async Task Subscribe()
    {
        var userId = Context.User?.FindFirst("sub")?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }
    }
}

public class SignalRNotificationRealTimeService : INotificationRealTimeService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<SignalRNotificationRealTimeService> _logger;

    public SignalRNotificationRealTimeService(
        IHubContext<NotificationHub> hubContext,
        ILogger<SignalRNotificationRealTimeService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task<Result> SendRealTimeAsync(
        string userId,
        Notification notification,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _hubContext.Clients
                .Group(userId)
                .SendAsync("ReceiveNotification", notification, cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send real-time notification to {UserId}", userId);
            return Result.Failure($"Real-time delivery failed: {ex.Message}");
        }
    }

    public async Task<Result> SendRealTimeToMultipleAsync(
        IEnumerable<string> userIds,
        Notification notification,
        CancellationToken cancellationToken = default)
    {
        try
        {
            foreach (var userId in userIds)
            {
                await _hubContext.Clients
                    .Group(userId)
                    .SendAsync("ReceiveNotification", notification, cancellationToken);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send real-time notifications");
            return Result.Failure($"Real-time delivery failed: {ex.Message}");
        }
    }

    public Task<bool> IsUserConnectedAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        // Implement connection tracking logic
        return Task.FromResult(true);
    }
}
```

### Register SignalR Service

```csharp
builder.Services.AddSignalR();

builder.Services.AddNotificationServices(options =>
{
    options.EnableRealTimeDelivery = true;
})
.AddNotificationRealTimeService<SignalRNotificationRealTimeService>();

// In app configuration
app.MapHub<NotificationHub>("/notificationHub");
```

### Client-Side (JavaScript)

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .build();

connection.on("ReceiveNotification", (notification) => {
    console.log("Received notification:", notification);
    showNotification(notification.title, notification.message);
});

await connection.start();
```

---

## Best Practices

### 1. Choose Appropriate Channels

```csharp
// ? Good: Use appropriate channels for notification type
var securityAlert = new NotificationRequest
{
    Title = "Security Alert",
    Priority = NotificationPriority.Urgent,
    Channels = NotificationChannel.All  // Use all channels for urgent security alerts
};

var marketingUpdate = new NotificationRequest
{
    Title = "Weekly Newsletter",
    Priority = NotificationPriority.Low,
    Channels = NotificationChannel.Email  // Email only for marketing
};

// ? Avoid: Sending spam through all channels
var lowPrioritySpam = new NotificationRequest
{
    Channels = NotificationChannel.All  // Don't spam users
};
```

### 2. Respect User Preferences

```csharp
// ? Good: Check and honor preferences
var effectiveChannels = await _preferenceService.GetEffectiveChannelsAsync(
    userId,
    requestedChannels,
    priority,
    category);

// ? Avoid: Bypassing preferences
// Don't forcefully send through channels user has disabled
```

### 3. Use Appropriate Priority

```csharp
// ? Good: Match priority to importance
NotificationPriority.Urgent  // Security alerts, critical system issues
NotificationPriority.High    // Important updates, time-sensitive info
NotificationPriority.Normal  // Standard notifications
NotificationPriority.Low     // Tips, marketing, non-urgent updates

// ? Avoid: Everything as urgent
// Crying wolf leads to alert fatigue
```

### 4. Batch When Possible

```csharp
// ? Good: Use batch operations
var userIds = await GetActiveUserIdsAsync();
await _notificationService.SendToMultipleAsync(request, userIds);

// ? Avoid: Individual sends in a loop
foreach (var userId in userIds)
{
    await _notificationService.SendAsync(request);  // Inefficient
}
```

### 5. Add Action URLs

```csharp
// ? Good: Actionable notifications
var notification = new NotificationRequest
{
    Title = "New Comment",
    Message = "Someone commented on your post",
    ActionUrl = "/posts/123#comment-456",  // Direct link to the comment
    ImageUrl = "/avatars/commenter.jpg"
};

// ? Avoid: Dead-end notifications
// Users should be able to act on notifications
```

### 6. Clean Up Old Notifications

```csharp
// ? Good: Enable auto-cleanup
{
  "NotificationOptions": {
    "EnableAutoCleanup": true,
    "CleanupAfterDays": 90
  }
}

// Schedule cleanup job
_jobScheduler.ScheduleRecurring<CleanupNotificationsJob>(
    "cleanup-notifications",
    CronExpressions.Weekly);
```

### 7. Handle Failures Gracefully

```csharp
// ? Good: Log and retry
var result = await _notificationService.SendAsync(notification);

if (!result.IsSuccess)
{
    _logger.LogWarning(
        "Notification delivery failed, will retry: {Error}",
        result.ErrorMessage);
    
    // System will auto-retry based on configuration
}

// ? Avoid: Silent failures
// Always log failures for debugging
```

### 8. Use Categories

```csharp
// ? Good: Categorize notifications
var notification = new NotificationRequest
{
    Category = "Security",     // Users can set preferences per category
    // ...
};

// Common categories:
// - Security
// - Marketing  
// - Social
// - System
// - Billing
```

---

## API Reference

### INotificationService

```csharp
Task<Result<Notification>> SendAsync(Notification notification, CancellationToken);
Task<Result<Notification>> SendAsync(NotificationRequest request, CancellationToken);
Task<Result<List<Notification>>> SendBatchAsync(IEnumerable<Notification> notifications, CancellationToken);
Task<Result<List<Notification>>> SendToMultipleAsync(NotificationRequest request, IEnumerable<string> recipientUserIds, CancellationToken);
Task<Result<Notification>> ScheduleAsync(Notification notification, DateTimeOffset scheduledFor, CancellationToken);
Task<Result> MarkAsReadAsync(KeyType notificationId, CancellationToken);
Task<Result> MarkAllAsReadAsync(IEnumerable<KeyType> notificationIds, CancellationToken);
Task<Result> MarkAllAsReadForUserAsync(string userId, CancellationToken);
Task<Result> DeleteAsync(KeyType notificationId, CancellationToken);
Task<List<Notification>> GetUserNotificationsAsync(string userId, bool includeRead, CancellationToken);
Task<int> GetUnreadCountAsync(string userId, CancellationToken);
Task<Result> RetryDeliveryAsync(KeyType notificationId, CancellationToken);
Task<int> CleanupOldNotificationsAsync(CancellationToken);
```

### INotificationPreferenceService

```csharp
Task<NotificationPreference?> GetPreferenceAsync(string userId, string? category, CancellationToken);
Task<List<NotificationPreference>> GetAllPreferencesAsync(string userId, CancellationToken);
Task<Result<NotificationPreference>> UpdatePreferenceAsync(NotificationPreference preference, CancellationToken);
Task<Result> SetEnabledChannelsAsync(string userId, NotificationChannel channels, string? category, CancellationToken);
Task<bool> IsChannelEnabledAsync(string userId, NotificationChannel channel, string? category, CancellationToken);
Task<Result> RegisterPushSubscriptionAsync(string userId, string endpoint, string publicKey, string auth, CancellationToken);
Task<Result> RemovePushSubscriptionAsync(string userId, CancellationToken);
Task<NotificationChannel> GetEffectiveChannelsAsync(string userId, NotificationChannel requestedChannels, NotificationPriority priority, string? category, CancellationToken);
```

### INotificationProvider

```csharp
NotificationChannel Channel { get; }
string Name { get; }
int Priority { get; }
Task<NotificationDeliveryResult> SendAsync(Notification notification, CancellationToken);
Task<List<NotificationDeliveryResult>> SendBatchAsync(IEnumerable<Notification> notifications, CancellationToken);
bool CanDeliver(Notification notification);
```

### Extension Methods

```csharp
IServiceCollection AddNotificationServices(IConfiguration configuration);
IServiceCollection AddNotificationServices(IConfigurationSection configurationSection);
IServiceCollection AddNotificationServices(Action<NotificationOptions> configureOptions);
IServiceCollection AddNotificationProvider<TProvider>();
IServiceCollection ReplaceNotificationProvider<TProvider>(NotificationChannel channel);
IServiceCollection AddNotificationRealTimeService<TService>();
ModelBuilder ConfigureNotificationEntities(this ModelBuilder);
```

---

## Testing

Comprehensive unit tests will be provided in the next phase. Here's an example structure:

```csharp
public class NotificationServiceTests
{
    [Fact]
    public async Task SendAsync_CreatesNotificationSuccessfully()
    {
        // Arrange
        var services = CreateTestServices();
        var notificationService = services.GetRequiredService<INotificationService>();
        
        var request = new NotificationRequest
        {
            Title = "Test",
            Message = "Test message",
            RecipientUserId = "user123"
        };

        // Act
        var result = await notificationService.SendAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Test", result.Value.Title);
    }
}
```

---

## Troubleshooting

### Issue: Notifications not being saved to database

**Cause:** Persistence disabled or DbContext not configured

**Solution:**
1. Check configuration:
   ```json
   { "NotificationOptions": { "EnablePersistence": true } }
   ```
2. Ensure DbContext is registered with notification entities
3. Run migrations: `dotnet ef database update`

---

### Issue: User not receiving notifications

**Causes:**
- User preferences blocking delivery
- Channel not enabled
- Priority below user's minimum

**Solution:**
```csharp
// Check effective channels
var effectiveChannels = await _preferenceService.GetEffectiveChannelsAsync(
    userId,
    requestedChannels,
    priority,
    category);

// Check if result is None
if (effectiveChannels == NotificationChannel.None)
{
    // User has blocked this notification type
}
```

---

### Issue: Web Push not working

**Cause:** VAPID keys not configured

**Solution:**
1. Generate VAPID keys (use web-push library or online generator)
2. Configure in appsettings:
   ```json
   {
     "NotificationOptions": {
       "VapidSubject": "mailto:admin@example.com",
       "VapidPublicKey": "BEl62i...",
       "VapidPrivateKey": "5I2bu..."
     }
   }
   ```

---

### Issue: Teams webhook not sending

**Cause:** Webhook URL not configured or invalid

**Solution:**
1. Create incoming webhook in Teams
2. Configure URL in appsettings:
   ```json
   {
     "NotificationOptions": {
       "TeamsWebhookUrl": "https://outlook.office.com/webhook/..."
     }
   }
   ```

---

## Summary

`Craft.Notifications` provides a complete, production-ready notification system with:

? **Multi-Channel Delivery** - In-app, push, webhooks, email, SMS  
? **User Preferences** - Fine-grained control per user/category  
? **Batch Processing** - Efficient bulk operations  
? **Priority Routing** - Urgent to low priority support  
? **Database Persistence** - Full EF Core integration  
? **Extensible Providers** - Easy to add custom channels  
? **Delivery Tracking** - Complete audit trail  
? **Auto Cleanup** - Configurable retention policies  
? **Real-Time Ready** - Infrastructure for SignalR integration  

Perfect for:
- User notifications
- System alerts
- Marketing campaigns
- Security alerts
- Activity feeds
- Multi-tenant applications

---

**Version**: 1.0.0  
**Last Updated**: January 2025  
**Target Framework**: .NET 10  
**Status**: ? Production Ready

---

**Need Help?** Check the [Troubleshooting](#troubleshooting) section or enable detailed logging:

```json
{
  "NotificationOptions": {
    "EnableDetailedLogging": true
  }
}
```
