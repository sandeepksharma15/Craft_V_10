using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Craft.Notifications.Tests;

/// <summary>
/// Helper class for creating test services and mock data.
/// </summary>
public static class TestHelpers
{
    /// <summary>
    /// Creates an in-memory DbContext for testing.
    /// </summary>
    public static DbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new TestDbContext(options);
    }

    /// <summary>
    /// Creates a service collection with all notification services registered.
    /// </summary>
    public static IServiceProvider CreateTestServiceProvider(
        NotificationOptions? options = null,
        DbContext? dbContext = null)
    {
        var services = new ServiceCollection();

        // Register DbContext
        if (dbContext != null)
        {
            services.AddSingleton(dbContext);
        }
        else
        {
            services.AddDbContext<DbContext>(opt =>
                opt.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        }

        // Register options - both as IOptions<T> and as singleton instance
        var notificationOptions = options ?? new NotificationOptions();
        services.AddSingleton(Options.Create(notificationOptions));
        services.AddSingleton(notificationOptions); // Register the instance directly

        // Register services
        services.AddLogging();
        services.AddHttpClient(); // Add HttpClient for webhook providers
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<INotificationPreferenceService, NotificationPreferenceService>();
        services.AddScoped<INotificationRealTimeService, NullNotificationRealTimeService>();

        // Register providers
        services.AddScoped<INotificationProvider, InAppNotificationProvider>();
        services.AddScoped<INotificationProvider, WebPushNotificationProvider>();
        services.AddScoped<INotificationProvider, WebhookNotificationProvider>();

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Creates a sample notification for testing.
    /// </summary>
    public static Notification CreateSampleNotification(string userId = "user123")
    {
        return new Notification
        {
            Title = "Test Notification",
            Message = "This is a test notification",
            Type = NotificationType.Info,
            Priority = NotificationPriority.Normal,
            Channels = NotificationChannel.InApp,
            RecipientUserId = userId,
            Status = NotificationStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Creates a sample notification request for testing.
    /// </summary>
    public static NotificationRequest CreateSampleRequest(string userId = "user123")
    {
        return new NotificationRequest
        {
            Title = "Test Notification",
            Message = "This is a test notification",
            Type = NotificationType.Info,
            Priority = NotificationPriority.Normal,
            Channels = NotificationChannel.InApp,
            RecipientUserId = userId
        };
    }

    /// <summary>
    /// Creates a sample notification preference for testing.
    /// </summary>
    public static NotificationPreference CreateSamplePreference(string userId = "user123")
    {
        return new NotificationPreference
        {
            UserId = userId,
            IsEnabled = true,
            EnabledChannels = NotificationChannel.All,
            MinimumPriority = NotificationPriority.Low,
            Email = "test@example.com",
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}

/// <summary>
/// Test DbContext for in-memory testing.
/// </summary>
public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions options) : base(options) { }

    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<NotificationDeliveryLog> NotificationDeliveryLogs { get; set; } = null!;
    public DbSet<NotificationPreference> NotificationPreferences { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ConfigureNotificationEntities();
    }
}
