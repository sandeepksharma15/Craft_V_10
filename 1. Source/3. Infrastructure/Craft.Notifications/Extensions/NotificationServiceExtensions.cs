using Craft.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

#pragma warning disable IDE0130
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130

/// <summary>
/// Extension methods for configuring notification services.
/// </summary>
public static class NotificationServiceExtensions
{
    /// <summary>
    /// Adds notification services to the service collection with configuration from appsettings.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration containing NotificationOptions.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddNotificationServices(
        this IServiceCollection services,
        IConfiguration configuration)
        => services.AddNotificationServices(configuration.GetSection(NotificationOptions.SectionName));

    /// <summary>
    /// Adds notification services to the service collection with a configuration section.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configurationSection">The configuration section containing NotificationOptions.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddNotificationServices(
        this IServiceCollection services,
        IConfigurationSection configurationSection)
    {
        // Register options
        services.AddOptions<NotificationOptions>()
            .Bind(configurationSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Get options for conditional registration
        var options = configurationSection.Get<NotificationOptions>() ?? new NotificationOptions();

        // Register core services
        services.TryAddScoped<INotificationService, NotificationService>();
        services.TryAddScoped<INotificationPreferenceService, NotificationPreferenceService>();

        // Register real-time service
        if (options.EnableRealTimeDelivery)
        {
            services.TryAddScoped<INotificationRealTimeService, NullNotificationRealTimeService>();
        }

        // Register default providers
        services.TryAddScoped<INotificationProvider, InAppNotificationProvider>();
        services.TryAddScoped<INotificationProvider, WebPushNotificationProvider>();
        services.TryAddScoped<INotificationProvider, WebhookNotificationProvider>();

        // Register Teams webhook if URL is configured
        if (!string.IsNullOrEmpty(options.TeamsWebhookUrl))
        {
            services.TryAddScoped<INotificationProvider, TeamsWebhookNotificationProvider>();
        }

        // Register HttpClient for webhook providers
        services.AddHttpClient();

        // Add logging
        services.AddLogging();

        return services;
    }

    /// <summary>
    /// Adds notification services with a configuration action.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure notification options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddNotificationServices(
        this IServiceCollection services,
        Action<NotificationOptions> configureOptions)
    {
        services.AddOptions<NotificationOptions>()
            .Configure(configureOptions)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Get options for conditional registration
        var options = new NotificationOptions();
        configureOptions(options);

        // Register core services
        services.TryAddScoped<INotificationService, NotificationService>();
        services.TryAddScoped<INotificationPreferenceService, NotificationPreferenceService>();

        // Register real-time service
        if (options.EnableRealTimeDelivery)
        {
            services.TryAddScoped<INotificationRealTimeService, NullNotificationRealTimeService>();
        }

        // Register default providers
        services.TryAddScoped<INotificationProvider, InAppNotificationProvider>();
        services.TryAddScoped<INotificationProvider, WebPushNotificationProvider>();
        services.TryAddScoped<INotificationProvider, WebhookNotificationProvider>();

        // Register Teams webhook if URL is configured
        if (!string.IsNullOrEmpty(options.TeamsWebhookUrl))
        {
            services.TryAddScoped<INotificationProvider, TeamsWebhookNotificationProvider>();
        }

        // Register HttpClient for webhook providers
        services.AddHttpClient();

        // Add logging
        services.AddLogging();

        return services;
    }

    /// <summary>
    /// Registers a custom notification provider.
    /// </summary>
    /// <typeparam name="TProvider">The type of the notification provider.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddNotificationProvider<TProvider>(this IServiceCollection services)
        where TProvider : class, INotificationProvider
    {
        services.AddScoped<INotificationProvider, TProvider>();
        return services;
    }

    /// <summary>
    /// Replaces the default notification provider for a specific channel.
    /// </summary>
    /// <typeparam name="TProvider">The type of the notification provider.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="channel">The channel to replace the provider for.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection ReplaceNotificationProvider<TProvider>(
        this IServiceCollection services,
        NotificationChannel channel)
        where TProvider : class, INotificationProvider
    {
        // Remove existing providers for this channel
        var existingProviders = services
            .Where(sd => sd.ServiceType == typeof(INotificationProvider))
            .ToList();

        foreach (var descriptor in existingProviders)
        {
            if (descriptor.ImplementationType != null)
            {
                var instance = Activator.CreateInstance(descriptor.ImplementationType);
                if (instance is INotificationProvider provider && provider.Channel == channel)
                {
                    services.Remove(descriptor);
                }
            }
        }

        // Add new provider
        services.AddScoped<INotificationProvider, TProvider>();
        return services;
    }

    /// <summary>
    /// Registers a custom real-time notification service.
    /// </summary>
    /// <typeparam name="TService">The type of the real-time service.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddNotificationRealTimeService<TService>(this IServiceCollection services)
        where TService : class, INotificationRealTimeService
    {
        services.Replace(ServiceDescriptor.Scoped<INotificationRealTimeService, TService>());
        return services;
    }

    /// <summary>
    /// Configures Entity Framework Core for notification entities.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    public static void ConfigureNotificationEntities(this ModelBuilder modelBuilder)
    {
        // Configure Notification entity
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.RecipientUserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => new { e.RecipientUserId, e.ReadAt });
            entity.HasIndex(e => e.ScheduledFor);

            entity.HasMany(e => e.DeliveryLogs)
                .WithOne(e => e.Notification)
                .HasForeignKey(e => e.NotificationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure NotificationDeliveryLog entity
        modelBuilder.Entity<NotificationDeliveryLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.NotificationId);
            entity.HasIndex(e => new { e.NotificationId, e.Channel });
        });

        // Configure NotificationPreference entity
        modelBuilder.Entity<NotificationPreference>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.Category }).IsUnique();
            entity.HasIndex(e => e.UserId);
        });
    }
}
