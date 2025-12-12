namespace Craft.Jobs;

/// <summary>
/// Defines the contract for scheduling and managing background jobs.
/// </summary>
public interface IJobScheduler
{
    /// <summary>
    /// Schedules a job for immediate execution (fire-and-forget).
    /// </summary>
    /// <typeparam name="TJob">The type of job to execute.</typeparam>
    /// <param name="parameters">Optional job parameters (will be JSON serialized).</param>
    /// <returns>The unique job ID.</returns>
    string Schedule<TJob>(object? parameters = null) where TJob : IBackgroundJob;

    /// <summary>
    /// Schedules a job for execution at a specific time (delayed job).
    /// </summary>
    /// <typeparam name="TJob">The type of job to execute.</typeparam>
    /// <param name="delay">When to execute the job (relative to now).</param>
    /// <param name="parameters">Optional job parameters (will be JSON serialized).</param>
    /// <returns>The unique job ID.</returns>
    string Schedule<TJob>(TimeSpan delay, object? parameters = null) where TJob : IBackgroundJob;

    /// <summary>
    /// Schedules a job for execution at a specific time (delayed job).
    /// </summary>
    /// <typeparam name="TJob">The type of job to execute.</typeparam>
    /// <param name="scheduledFor">When to execute the job (absolute time).</param>
    /// <param name="parameters">Optional job parameters (will be JSON serialized).</param>
    /// <returns>The unique job ID.</returns>
    string Schedule<TJob>(DateTimeOffset scheduledFor, object? parameters = null) where TJob : IBackgroundJob;

    /// <summary>
    /// Schedules a recurring job based on a cron expression.
    /// </summary>
    /// <typeparam name="TJob">The type of job to execute.</typeparam>
    /// <param name="recurringJobId">Unique identifier for the recurring job.</param>
    /// <param name="cronExpression">Cron expression defining the schedule.</param>
    /// <param name="parameters">Optional job parameters (will be JSON serialized).</param>
    void ScheduleRecurring<TJob>(string recurringJobId, string cronExpression, object? parameters = null) where TJob : IBackgroundJob;

    /// <summary>
    /// Schedules a recurring job with a minute interval.
    /// </summary>
    /// <typeparam name="TJob">The type of job to execute.</typeparam>
    /// <param name="recurringJobId">Unique identifier for the recurring job.</param>
    /// <param name="intervalMinutes">Interval in minutes between executions.</param>
    /// <param name="parameters">Optional job parameters (will be JSON serialized).</param>
    void ScheduleRecurring<TJob>(string recurringJobId, int intervalMinutes, object? parameters = null) where TJob : IBackgroundJob;

    /// <summary>
    /// Removes a recurring job.
    /// </summary>
    /// <param name="recurringJobId">The recurring job ID to remove.</param>
    void RemoveRecurring(string recurringJobId);

    /// <summary>
    /// Triggers a recurring job immediately (does not affect its schedule).
    /// </summary>
    /// <param name="recurringJobId">The recurring job ID to trigger.</param>
    void TriggerRecurring(string recurringJobId);

    /// <summary>
    /// Deletes a job from the queue.
    /// </summary>
    /// <param name="jobId">The job ID to delete.</param>
    /// <returns>True if the job was deleted, false otherwise.</returns>
    bool Delete(string jobId);

    /// <summary>
    /// Requeues a failed job.
    /// </summary>
    /// <param name="jobId">The job ID to requeue.</param>
    /// <returns>True if the job was requeued, false otherwise.</returns>
    bool Requeue(string jobId);
}
