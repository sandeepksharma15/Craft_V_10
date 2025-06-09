using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Craft.Logging.Extensions;

/// <summary>
/// Adds Serilog logging to the WebApplicationBuilder.
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Adds Serilog logging to the WebApplicationBuilder.
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder instance.</param>
    /// <returns>The WebApplicationBuilder instance.</returns>
    public static WebApplicationBuilder AddLogging(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();

        builder.Host
            .UseSerilog((context, config) =>
                config
                    .Enrich.FromLogContext()
                    .ReadFrom.Configuration(context.Configuration));

        return builder;
    }
}
