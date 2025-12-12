using Microsoft.Extensions.Logging;

namespace Craft.Jobs.Samples;

/// <summary>
/// Sample job for database cleanup operations.
/// Demonstrates a recurring job without parameters.
/// </summary>
public class DatabaseCleanupJob : IBackgroundJob
{
    private readonly ILogger<DatabaseCleanupJob> _logger;

    public DatabaseCleanupJob(ILogger<DatabaseCleanupJob> logger)
    {
        _logger = logger;
    }

    public async Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting database cleanup for tenant {TenantId}", context.TenantId ?? "N/A");

        // Simulate cleanup operations
        await Task.Delay(2000, cancellationToken);

        _logger.LogInformation("Database cleanup completed");
    }
}
