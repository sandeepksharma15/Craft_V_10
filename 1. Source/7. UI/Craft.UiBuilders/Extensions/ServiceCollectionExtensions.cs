using Craft.UiBuilders.Services.Theme;
using Craft.UiBuilders.Services.UserPreference;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.UiBuilders.Extensions;

/// <summary>
/// Extension methods for registering UI Builder services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers user preferences services including data protection for browser storage.
    /// Configures ProtectedBrowserStorage with appropriate data protection policies.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="applicationName">Optional application name for data protection isolation. Defaults to "Craft.UiBuilders".</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddUserPreferences(this IServiceCollection services, string? applicationName = null)
    {
        // Configure Data Protection for ProtectedBrowserStorage
        // This ensures encrypted local storage persists across app restarts
        services.AddDataProtection()
            .SetApplicationName(applicationName ?? "Craft.UiBuilders")
            .PersistKeysToFileSystem(new DirectoryInfo("/var/keys"))
            .SetDefaultKeyLifetime(TimeSpan.FromDays(90));

        // In production, consider adding:
        // - .PersistKeysToDbContext<YourDbContext>()
        // - .ProtectKeysWithCertificate(certificate)

        // Register user preferences services
        services.AddScoped<IUserPreferencesManager, UserPreferencesManager>();
        services.AddScoped<IUserPreferences, UserPreferences>();

        return services;
    }

    /// <summary>
    /// Registers the theme manager service for managing MudBlazor themes.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddThemeManager(this IServiceCollection services)
    {
        services.AddScoped<IThemeManager, ThemeManager>();
        return services;
    }

    /// <summary>
    /// Registers all UI Builder services including user preferences and theme management.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="applicationName">Optional application name for data protection isolation.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddUiBuilders(this IServiceCollection services, string? applicationName = null)
    {
        services.AddUserPreferences(applicationName);
        services.AddThemeManager();

        return services;
    }
}
