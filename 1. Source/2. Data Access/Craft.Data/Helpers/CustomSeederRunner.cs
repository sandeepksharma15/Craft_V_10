using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Craft.Data.Helpers;

/// <summary>
/// Orchestrates the execution of custom database seeders with transactional support.
/// </summary>
public class CustomSeederRunner(IServiceProvider serviceProvider, ILogger<CustomSeederRunner> logger)
{
    private readonly ICustomSeeder[] _seeders = [.. serviceProvider.GetServices<ICustomSeeder>()];

    /// <summary>
    /// Runs all registered seeders within a single database transaction.
    /// If any seeder fails, all changes are rolled back to maintain database consistency.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="InvalidOperationException">Thrown when no DbContext is available for transaction management.</exception>
    public async Task RunSeedersAsync(CancellationToken cancellationToken)
    {
        if (_seeders.Length == 0)
        {
            logger.LogDebug("No custom seeders registered");
            return;
        }

        logger.LogInformation("Starting execution of {SeederCount} custom seeders", _seeders.Length);

        // Try to get a DbContext for transaction management
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetService<DbContext>();

        if (dbContext == null)
        {
            logger.LogWarning(
                "No DbContext found in service provider. Running seeders without transaction support. " +
                "This may leave the database in an inconsistent state if a seeder fails.");

            await RunSeedersWithoutTransactionAsync(cancellationToken);
            return;
        }

        // Run seeders within a transaction
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var seeder in _seeders)
            {
                var seederName = seeder.GetType().Name;
                logger.LogInformation("Executing seeder: {SeederName}", seederName);

                await seeder.InitializeAsync(cancellationToken);

                logger.LogInformation("Seeder {SeederName} completed successfully", seederName);
            }

            await transaction.CommitAsync(cancellationToken);
            logger.LogInformation("All seeders executed successfully and committed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Seeder execution failed. Rolling back all changes.");
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Runs seeders without transaction support (fallback when no DbContext is available).
    /// </summary>
    private async Task RunSeedersWithoutTransactionAsync(CancellationToken cancellationToken)
    {
        foreach (var seeder in _seeders)
        {
            var seederName = seeder.GetType().Name;
            logger.LogInformation("Executing seeder: {SeederName}", seederName);

            await seeder.InitializeAsync(cancellationToken);

            logger.LogInformation("Seeder {SeederName} completed successfully", seederName);
        }

        logger.LogInformation("All seeders executed successfully (without transaction)");
    }
}

