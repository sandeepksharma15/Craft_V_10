using Craft.Jobs;
using Craft.MultiTenant;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Craft.Hosting.Extensions;

/// <summary>
/// Extension methods for registering background job services.
/// </summary>
public static class JobExtensions
{
    /// <summary>
    /// Adds background job services with Hangfire and PostgreSQL.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration containing JobOptions.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddJobServices(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddJobServices(configuration.GetSection(JobOptions.SectionName));
    }

    /// <summary>
    /// Adds background job services with Hangfire and PostgreSQL.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configurationSection">The configuration section containing JobOptions.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddJobServices(this IServiceCollection services, IConfigurationSection configurationSection)
    {
        services.AddOptions<JobOptions>()
            .Bind(configurationSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var options = configurationSection.Get<JobOptions>() 
            ?? throw new InvalidOperationException("JobOptions configuration is missing or invalid");

        services.AddHangfire((serviceProvider, config) =>
        {
            var logger = serviceProvider.GetRequiredService<ILogger<JobOptions>>();

            logger.LogInformation("Configuring Hangfire with PostgreSQL storage");

            config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(options.ConnectionString), new PostgreSqlStorageOptions
                {
                    SchemaName = options.SchemaName,
                    QueuePollInterval = TimeSpan.FromSeconds(options.PollingIntervalSeconds),
                    JobExpirationCheckInterval = TimeSpan.FromHours(1),
                    CountersAggregateInterval = TimeSpan.FromMinutes(5),
                    PrepareSchemaIfNecessary = true,
                    InvisibilityTimeout = TimeSpan.FromMinutes(30)
                });

            if (options.EnableAutomaticRetry)
            {
                var retryFilter = new AutomaticRetryAttribute
                {
                    Attempts = options.MaxRetryAttempts,
                    OnAttemptsExceeded = AttemptsExceededAction.Delete
                };
                config.UseFilter(retryFilter);
            }

            if (options.EnableMultiTenancy)
            {
                config.UseFilter(new TenantJobFilter(serviceProvider));
                logger.LogInformation("Multi-tenancy support enabled for background jobs");
            }

            logger.LogInformation(
                "Hangfire configured with {WorkerCount} workers, {MaxRetryAttempts} max retries, schema: {SchemaName}",
                options.WorkerCount,
                options.MaxRetryAttempts,
                options.SchemaName);
        });

        services.AddHangfireServer(serverOptions =>
        {
            serverOptions.WorkerCount = options.WorkerCount;
            serverOptions.ServerName = $"{Environment.MachineName}:{Guid.NewGuid()}";
        });

        services.AddScoped<IJobScheduler, HangfireJobScheduler>();

        return services;
    }

    /// <summary>
    /// Adds background job services with inline configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure job options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddJobServices(this IServiceCollection services, Action<JobOptions> configureOptions)
    {
        services.AddOptions<JobOptions>()
            .Configure(configureOptions)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var options = new JobOptions();
        configureOptions(options);

        services.AddHangfire((serviceProvider, config) =>
        {
            config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(options.ConnectionString), new PostgreSqlStorageOptions
                {
                    SchemaName = options.SchemaName,
                    PrepareSchemaIfNecessary = true
                });

            if (options.EnableAutomaticRetry)
            {
                config.UseFilter(new AutomaticRetryAttribute { Attempts = options.MaxRetryAttempts });
            }

            if (options.EnableMultiTenancy)
            {
                config.UseFilter(new TenantJobFilter(serviceProvider));
            }
        });

        services.AddHangfireServer(serverOptions =>
        {
            serverOptions.WorkerCount = options.WorkerCount;
        });

        services.AddScoped<IJobScheduler, HangfireJobScheduler>();

        return services;
    }

    /// <summary>
    /// Uses the Hangfire dashboard (call after UseRouting and before UseEndpoints).
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseJobDashboard(this IApplicationBuilder app)
    {
        var options = app.ApplicationServices.GetRequiredService<IOptions<JobOptions>>().Value;

        if (options.EnableDashboard)
        {
            app.UseHangfireDashboard(options.DashboardPath, new DashboardOptions
            {
                Authorization = [new HangfireDashboardAuthorizationFilter()],
                StatsPollingInterval = 2000,
                DisplayStorageConnectionString = false
            });

            var logger = app.ApplicationServices.GetRequiredService<ILogger<JobOptions>>();
            logger.LogInformation(
                "Hangfire dashboard enabled at {DashboardPath}",
                options.DashboardPath);
        }

        return app;
    }
}

/// <summary>
/// Dashboard authorization filter (customize based on your auth requirements).
/// </summary>
internal class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User?.Identity?.IsAuthenticated ?? false;
    }
}
