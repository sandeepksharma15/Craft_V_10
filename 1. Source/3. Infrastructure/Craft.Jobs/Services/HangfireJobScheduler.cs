using Craft.Core;
using Craft.MultiTenant;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Craft.Jobs;

/// <summary>
/// Hangfire-based job scheduler implementation.
/// </summary>
public class HangfireJobScheduler : IJobScheduler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HangfireJobScheduler> _logger;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IRecurringJobManager _recurringJobManager;

    public HangfireJobScheduler(
        IServiceProvider serviceProvider,
        ILogger<HangfireJobScheduler> logger,
        IBackgroundJobClient backgroundJobClient,
        IRecurringJobManager recurringJobManager)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _backgroundJobClient = backgroundJobClient;
        _recurringJobManager = recurringJobManager;
    }

    public string Schedule<TJob>(object? parameters = null) where TJob : IBackgroundJob
    {
        var context = CreateContext<TJob>(parameters);

        var jobId = _backgroundJobClient.Enqueue<JobExecutor<TJob>>(
            executor => executor.ExecuteAsync(parameters, context, CancellationToken.None));

        _logger.LogInformation(
            "Scheduled job {JobType} with ID {JobId}",
            typeof(TJob).Name,
            jobId);

        return jobId;
    }

    public string Schedule<TJob>(TimeSpan delay, object? parameters = null) where TJob : IBackgroundJob
    {
        var context = CreateContext<TJob>(parameters);

        var jobId = _backgroundJobClient.Schedule<JobExecutor<TJob>>(
            executor => executor.ExecuteAsync(parameters, context, CancellationToken.None),
            delay);

        _logger.LogInformation(
            "Scheduled delayed job {JobType} with ID {JobId} to run in {Delay}",
            typeof(TJob).Name,
            jobId,
            delay);

        return jobId;
    }

    public string Schedule<TJob>(DateTimeOffset scheduledFor, object? parameters = null) where TJob : IBackgroundJob
    {
        var context = CreateContext<TJob>(parameters);

        var jobId = _backgroundJobClient.Schedule<JobExecutor<TJob>>(
            executor => executor.ExecuteAsync(parameters, context, CancellationToken.None),
            scheduledFor);

        _logger.LogInformation(
            "Scheduled job {JobType} with ID {JobId} to run at {ScheduledFor}",
            typeof(TJob).Name,
            jobId,
            scheduledFor);

        return jobId;
    }

    public void ScheduleRecurring<TJob>(string recurringJobId, string cronExpression, object? parameters = null) where TJob : IBackgroundJob
    {
        var context = CreateContext<TJob>(parameters, recurringJobId);

        _recurringJobManager.AddOrUpdate<JobExecutor<TJob>>(
            recurringJobId,
            executor => executor.ExecuteAsync(parameters, context, CancellationToken.None),
            cronExpression,
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc
            });

        _logger.LogInformation(
            "Scheduled recurring job {JobType} with ID {RecurringJobId} using cron {CronExpression}",
            typeof(TJob).Name,
            recurringJobId,
            cronExpression);
    }

    public void ScheduleRecurring<TJob>(string recurringJobId, int intervalMinutes, object? parameters = null) where TJob : IBackgroundJob
    {
        if (intervalMinutes < 1)
            throw new ArgumentException("Interval must be at least 1 minute", nameof(intervalMinutes));

        var cronExpression = $"*/{intervalMinutes} * * * *";
        ScheduleRecurring<TJob>(recurringJobId, cronExpression, parameters);
    }

    public void RemoveRecurring(string recurringJobId)
    {
        _recurringJobManager.RemoveIfExists(recurringJobId);

        _logger.LogInformation(
            "Removed recurring job {RecurringJobId}",
            recurringJobId);
    }

    public void TriggerRecurring(string recurringJobId)
    {
        _recurringJobManager.Trigger(recurringJobId);

        _logger.LogInformation(
            "Triggered recurring job {RecurringJobId}",
            recurringJobId);
    }

    public bool Delete(string jobId)
    {
        var result = _backgroundJobClient.Delete(jobId);

        if (result)
        {
            _logger.LogInformation("Deleted job {JobId}", jobId);
        }
        else
        {
            _logger.LogWarning("Failed to delete job {JobId}", jobId);
        }

        return result;
    }

    public bool Requeue(string jobId)
    {
        var result = _backgroundJobClient.Requeue(jobId);

        if (result)
        {
            _logger.LogInformation("Requeued job {JobId}", jobId);
        }
        else
        {
            _logger.LogWarning("Failed to requeue job {JobId}", jobId);
        }

        return result;
    }

    private JobExecutionContext CreateContext<TJob>(object? _, string? recurringJobId = null) where TJob : IBackgroundJob
    {
        var context = new JobExecutionContext
        {
            JobId = recurringJobId ?? Guid.NewGuid().ToString(),
            JobType = typeof(TJob).FullName ?? typeof(TJob).Name,
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Add tenant information if multi-tenancy is enabled
        using var scope = _serviceProvider.CreateScope();
        var tenant = scope.ServiceProvider.GetService<ITenant>();
        if (tenant?.Id != null)
        {
            context.TenantId = tenant.Id.ToString();
            context.Metadata["TenantName"] = tenant.Name ?? string.Empty;
        }

        // Add user information if available
        var currentUser = scope.ServiceProvider.GetService<ICurrentUser>();
        if (currentUser?.IsAuthenticated() == true)
        {
            context.UserId = currentUser.Id.ToString();
            context.Metadata["UserName"] = currentUser.GetUserName() ?? string.Empty;
        }

        return context;
    }
}
