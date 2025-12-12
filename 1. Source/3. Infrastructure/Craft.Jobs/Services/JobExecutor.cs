using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Craft.Jobs;

/// <summary>
/// Executes background jobs with dependency injection support.
/// </summary>
/// <typeparam name="TJob">The type of job to execute.</typeparam>
public class JobExecutor<TJob> where TJob : IBackgroundJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<JobExecutor<TJob>> _logger;

    public JobExecutor(
        IServiceProvider serviceProvider,
        ILogger<JobExecutor<TJob>> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Executes the job with the provided parameters.
    /// </summary>
    public async Task ExecuteAsync(object? parameters, JobExecutionContext context, CancellationToken cancellationToken)
    {
        context.StartedAt = DateTimeOffset.UtcNow;

        _logger.LogInformation(
            "Executing job {JobType} (ID: {JobId}, Attempt: {RetryAttempt})",
            context.JobType,
            context.JobId,
            context.RetryAttempt);

        try
        {
            // Create a scope for this job execution
            await using var scope = _serviceProvider.CreateAsyncScope();

            // Resolve the job from DI
            var job = scope.ServiceProvider.GetRequiredService<TJob>();

            // Execute based on whether job has parameters
            if (parameters != null && job is IBackgroundJob<object> parameterizedJob)
            {
                await parameterizedJob.ExecuteAsync(parameters, context, cancellationToken);
            }
            else
            {
                await job.ExecuteAsync(context, cancellationToken);
            }

            var duration = DateTimeOffset.UtcNow - context.StartedAt.Value;

            _logger.LogInformation(
                "Job {JobType} (ID: {JobId}) completed successfully in {Duration}ms",
                context.JobType,
                context.JobId,
                duration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Job {JobType} (ID: {JobId}, Attempt: {RetryAttempt}) failed: {Error}",
                context.JobType,
                context.JobId,
                context.RetryAttempt,
                ex.Message);

            throw;
        }
    }
}
