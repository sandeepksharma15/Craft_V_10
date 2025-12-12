using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Craft.Middleware;

/// <summary>
/// Extension methods for configuring CORS (Cross-Origin Resource Sharing) policies.
/// </summary>
public static class CorsExtensions
{
    private const string CorsPolicy = nameof(CorsPolicy);

    /// <summary>
    /// Adds CORS policy to the service collection based on configuration settings.
    /// Reads origins from CorsSettings in appsettings.json and configures a permissive
    /// CORS policy that allows any header, any method, and credentials from specified origins.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="config">The configuration containing CorsSettings.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if services or config is null.</exception>
    /// <remarks>
    /// Expected configuration format in appsettings.json:
    /// <code>
    /// "CorsSettings": {
    ///   "Angular": "http://localhost:4200;https://angular.app.com",
    ///   "Blazor": "http://localhost:5000;https://blazor.app.com",
    ///   "React": "http://localhost:3000;https://react.app.com"
    /// }
    /// </code>
    /// Multiple origins can be separated by semicolons.
    /// This method automatically validates the configuration at startup.
    /// </remarks>
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(config);

        services.AddOptions<CorsSettings>()
            .Bind(config.GetSection(CorsSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddCors(options =>
        {
            var corsSettingsSection = config.GetSection(CorsSettings.SectionName);
            var corsSettings = corsSettingsSection.Get<CorsSettings>() ?? new CorsSettings();

            var origins = ParseOrigins(corsSettings);

            options.AddPolicy(CorsPolicy, policy =>
                policy.AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .WithOrigins([.. origins]));
        });

        return services;
    }

    /// <summary>
    /// Adds the configured CORS policy to the application pipeline.
    /// Must be called after AddCorsPolicy and before endpoints are mapped.
    /// </summary>
    /// <param name="app">The application builder to configure.</param>
    /// <returns>The application builder for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if app is null.</exception>
    public static IApplicationBuilder UseCorsPolicy(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.UseCors(CorsPolicy);
    }

    private static List<string> ParseOrigins(CorsSettings corsSettings)
    {
        var origins = new List<string>();

        AddOriginsFromSetting(origins, corsSettings.Angular);
        AddOriginsFromSetting(origins, corsSettings.Blazor);
        AddOriginsFromSetting(origins, corsSettings.React);

        return origins;
    }

    private static void AddOriginsFromSetting(List<string> origins, string? setting)
    {
        if (string.IsNullOrWhiteSpace(setting))
            return;

        var parsedOrigins = setting
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(origin => !string.IsNullOrWhiteSpace(origin));

        origins.AddRange(parsedOrigins);
    }
}
