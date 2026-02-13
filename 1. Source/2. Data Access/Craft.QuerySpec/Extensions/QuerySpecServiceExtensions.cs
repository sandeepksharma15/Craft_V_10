using Craft.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.QuerySpec.Extensions;

/// <summary>
/// Extension methods for configuring QuerySpec services.
/// </summary>
public static class QuerySpecServiceExtensions
{
    /// <summary>
    /// Configures JSON serialization to support Query&lt;T&gt; and Query&lt;T, TResult&gt; types.
    /// This adds the necessary JSON converters for proper serialization/deserialization of queries over HTTP.
    /// </summary>
    /// <param name="builder">The IMvcBuilder instance.</param>
    /// <returns>The IMvcBuilder for chaining.</returns>
    /// <remarks>
    /// Call this method after AddControllers() to register QueryJsonConverter globally.
    /// This enables EntityController endpoints to properly deserialize query specifications.
    /// </remarks>
    public static IMvcBuilder AddQuerySpecJsonOptions(this IMvcBuilder builder)
    {
        builder.AddJsonOptions(options =>
        {
            // Add the converter factory that handles all Query<T> and Query<T, TResult> types
            options.JsonSerializerOptions.Converters.Add(new QueryJsonConverterFactory());

            // Set common JSON options that align with QuerySerializerOptions
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;

            // Handle circular references in navigation properties (e.g., PublicHoliday -> Location -> PublicHolidays)
            options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        });

        return builder;
    }

    /// <summary>
    /// Adds QuerySpec services with default configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Optional configuration to bind QueryOptions from.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// Registers:
    /// - QueryOptions (configured from appsettings or defaults)
    /// - IQueryValidator&lt;T&gt; as scoped service
    /// - IQueryMetrics as singleton service
    /// </remarks>
    public static IServiceCollection AddQuerySpec(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        // Register QueryOptions
        if (configuration != null)
        {
            services.Configure<QueryOptions>(configuration.GetSection("QuerySpec:Options"));
        }
        else
        {
            services.Configure<QueryOptions>(options => { }); // Use defaults
        }

        // Register query validator as scoped (one per request)
        services.AddScoped(typeof(IQueryValidator<>), typeof(QueryValidator<>));

        // Register metrics as singleton
        services.AddSingleton<IQueryMetrics, LoggingQueryMetrics>();

        return services;
    }

    /// <summary>
    /// Adds QuerySpec services with custom configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure QueryOptions.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddQuerySpec(
        this IServiceCollection services,
        Action<QueryOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.Configure(configureOptions);
        services.AddScoped(typeof(IQueryValidator<>), typeof(QueryValidator<>));
        services.AddSingleton<IQueryMetrics, LoggingQueryMetrics>();

        return services;
    }
}

