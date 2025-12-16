using Craft.UiConponents.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.UiConponents;

/// <summary>
/// Extension methods for configuring Craft UI Components services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Craft UI Components services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCraftUiComponents(this IServiceCollection services)
    {
        services.AddScoped<IThemeService, ThemeService>();

        return services;
    }

    /// <summary>
    /// Adds Craft UI Components services with custom configuration.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configure">The configuration action.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCraftUiComponents(
        this IServiceCollection services,
        Action<CraftUiOptions> configure)
    {
        var options = new CraftUiOptions();
        configure(options);

        services.AddSingleton(options);
        services.AddScoped<IThemeService, ThemeService>();

        return services;
    }
}

/// <summary>
/// Configuration options for Craft UI Components.
/// </summary>
public class CraftUiOptions
{
    /// <summary>
    /// Gets or sets the default theme.
    /// </summary>
    public Enums.Theme DefaultTheme { get; set; } = Enums.Theme.System;

    /// <summary>
    /// Gets or sets whether to enable component logging.
    /// </summary>
    public bool EnableLogging { get; set; }

    /// <summary>
    /// Gets or sets the default animation duration.
    /// </summary>
    public Enums.AnimationDuration DefaultAnimationDuration { get; set; } = Enums.AnimationDuration.Normal;

    /// <summary>
    /// Gets or sets the CSS class prefix for all components.
    /// </summary>
    public string CssPrefix { get; set; } = "craft";
}
