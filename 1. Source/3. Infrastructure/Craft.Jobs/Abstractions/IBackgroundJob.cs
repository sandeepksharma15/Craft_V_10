namespace Craft.Jobs;

/// <summary>
/// Defines the contract for a background job that can be executed.
/// </summary>
public interface IBackgroundJob
{
    /// <summary>
    /// Executes the job asynchronously.
    /// </summary>
    /// <param name="context">The execution context containing runtime information.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines the contract for a parameterized background job.
/// </summary>
/// <typeparam name="TParameters">The type of parameters required by the job.</typeparam>
public interface IBackgroundJob<in TParameters> : IBackgroundJob where TParameters : class
{
    /// <summary>
    /// Executes the job with the provided parameters.
    /// </summary>
    /// <param name="parameters">The job parameters.</param>
    /// <param name="context">The execution context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ExecuteAsync(TParameters parameters, JobExecutionContext context, CancellationToken cancellationToken = default);
}
