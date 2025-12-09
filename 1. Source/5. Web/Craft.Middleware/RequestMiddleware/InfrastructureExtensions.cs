using Craft.Middleware.RequestMiddleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Craft.Middleware.RequestMiddleware;

/// <summary>
/// Extension methods for configuring request/response middleware and exception handling.
/// </summary>
public static class InfrastructureExtensions
{
    private static RequestMiddlewareSettings GetSystemSettings(IConfiguration config) =>
        config.GetSection(nameof(RequestMiddlewareSettings)).Get<RequestMiddlewareSettings>() ?? new RequestMiddlewareSettings();

    /// <summary>
    /// Registers exception handling services with custom ProblemDetails factory.
    /// </summary>
    public static IServiceCollection AddExceptionHandling(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<RequestMiddlewareSettings>(config.GetSection(nameof(RequestMiddlewareSettings)));

        var settings = GetSystemSettings(config);

        if (settings.EnableExceptionHandler)
            services.AddExceptionHandler<GlobalExceptionHandler>();

        // Register custom ProblemDetails factory
        services.AddTransient<Microsoft.AspNetCore.Mvc.Infrastructure.ProblemDetailsFactory, CraftProblemDetailsFactory>();
        
        services.AddProblemDetails();

        return services;
    }

    /// <summary>
    /// Registers detailed request/response logging services.
    /// </summary>
    public static IServiceCollection AddDetailedLogging(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<RequestMiddlewareSettings>(config.GetSection(nameof(RequestMiddlewareSettings)));

        var settings = GetSystemSettings(config);

        if (settings.EnableDetailedLogging)
        {
            services.AddScoped<RequestLoggingMiddleware>();
            services.AddScoped<ResponseLoggingMiddleware>();
        }

        return services;
    }

    /// <summary>
    /// Configures the exception handling middleware in the request pipeline.
    /// </summary>
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app, IConfiguration config)
    {
        var settings = GetSystemSettings(config);

        if (settings.EnableExceptionHandler)
            app.UseExceptionHandler();

        return app;
    }

    /// <summary>
    /// Configures detailed request/response logging middleware in the request pipeline.
    /// </summary>
    public static IApplicationBuilder UseDetailedLogging(this IApplicationBuilder app, IConfiguration config)
    {
        var settings = GetSystemSettings(config);

        if (settings.EnableDetailedLogging)
        {
            app.UseMiddleware<RequestLoggingMiddleware>();
            app.UseMiddleware<ResponseLoggingMiddleware>();
        }

        return app;
    }

    /// <summary>
    /// Configures Serilog's built-in request logging with enrichment for correlation tracking.
    /// </summary>
    public static IApplicationBuilder UseSerilogRequestLogging(this IApplicationBuilder app, IConfiguration config)
    {
        var settings = GetSystemSettings(config);

        if (settings.EnableSerilogRequestLogging)
            app.UseSerilogRequestLogging(options =>
            {
                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                    diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                    diagnosticContext.Set("RequestProtocol", httpContext.Request.Protocol);
                    diagnosticContext.Set("RequestPath", httpContext.Request.Path.Value);
                    diagnosticContext.Set("RequestQueryString", httpContext.Request.QueryString.Value);
                    diagnosticContext.Set("RequestContentType", httpContext.Request.ContentType);
                    diagnosticContext.Set("RequestContentLength", httpContext.Request.ContentLength);
                    diagnosticContext.Set("ResponseStatusCode", httpContext.Response.StatusCode);
                    diagnosticContext.Set("ResponseContentType", httpContext.Response.ContentType);

                    if (httpContext.Items.TryGetValue("CorrelationId", out var correlationId))
                        diagnosticContext.Set("CorrelationId", correlationId);

                    if (httpContext.User.Identity?.IsAuthenticated == true)
                    {
                        diagnosticContext.Set("UserId", httpContext.User.FindFirst("sub")?.Value
                            ?? httpContext.User.FindFirst("userId")?.Value);
                        diagnosticContext.Set("UserEmail", httpContext.User.Identity.Name);
                    }
                };

                options.GetLevel = (httpContext, elapsed, ex) =>
                {
                    if (ex != null)
                        return Serilog.Events.LogEventLevel.Error;

                    if (httpContext.Response.StatusCode >= 500)
                        return Serilog.Events.LogEventLevel.Error;

                    if (httpContext.Response.StatusCode >= 400)
                        return Serilog.Events.LogEventLevel.Warning;

                    if (elapsed > 5000)
                        return Serilog.Events.LogEventLevel.Warning;

                    return Serilog.Events.LogEventLevel.Information;
                };

                options.MessageTemplate =
                    "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
            });

        return app;
    }
}
