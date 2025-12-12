using Craft.MultiTenant;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Craft.Jobs;

/// <summary>
/// Extension methods for registering background job services.
/// </summary>
public static class JobServiceExtensions
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
        // Register and validate options
        services.AddOptions<JobOptions>()
            .Bind(configurationSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var options = configurationSection.Get<JobOptions>() 
            ?? throw new InvalidOperationException("JobOptions configuration is missing or invalid");

        // Add Hangfire services
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

            // Configure automatic retry
            if (options.EnableAutomaticRetry)
            {
                var retryFilter = new AutomaticRetryAttribute
                {
                    Attempts = options.MaxRetryAttempts,
                    OnAttemptsExceeded = AttemptsExceededAction.Delete
                };
                config.UseFilter(retryFilter);
            }

            // Configure multi-tenancy filter if enabled
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

        // Add Hangfire server
        services.AddHangfireServer(serverOptions =>
        {
            serverOptions.WorkerCount = options.WorkerCount;
            serverOptions.ServerName = $"{Environment.MachineName}:{Guid.NewGuid()}";
        });

        // Register job scheduler
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

        return AddJobServicesCore(services, options);
    }

    private static IServiceCollection AddJobServicesCore(IServiceCollection services, JobOptions options)
    {
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
        // TODO: Implement proper authorization
        // For now, allow all in development, restrict in production
        var httpContext = context.GetHttpContext();
        
        // Example: Only allow authenticated users
        return httpContext.User?.Identity?.IsAuthenticated ?? false;
        
        // Or for development only:
        // return true;
    }
}
