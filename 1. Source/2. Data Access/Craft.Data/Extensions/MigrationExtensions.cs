using Craft.Data.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Craft.Data;

/// <summary>
/// Extension methods for database migrations and seeding.
/// </summary>
public static class MigrationExtensions
{
    /// <summary>
    /// Applies pending migrations and runs custom seeders for the specified DbContext.
    /// Includes automatic retry logic for containerized/Aspire scenarios where the database may not be immediately available.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type to migrate.</typeparam>
    /// <param name="app">The web application.</param>
    /// <param name="migrationTimeout">Optional timeout for migration operations (default: 10 minutes).</param>
    /// <param name="maxRetries">Maximum number of retry attempts if database is unavailable (default: 5).</param>
    /// <param name="retryDelay">Delay between retry attempts (default: 2 seconds).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The web application for chaining.</returns>
    public static async Task<WebApplication> MigrateDatabaseAsync<TContext>(this WebApplication app,
        TimeSpan? migrationTimeout = null, int maxRetries = 5, TimeSpan? retryDelay = null, CancellationToken cancellationToken = default) where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(app);

        migrationTimeout ??= TimeSpan.FromMinutes(10);
        retryDelay ??= TimeSpan.FromSeconds(2);

        var logger = app.Services.GetRequiredService<ILogger<TContext>>();
        logger.LogInformation("Starting database migration for {ContextType}", typeof(TContext).Name);

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            IServiceScope? scope = null;
            try
            {
                scope = app.Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<TContext>();

                logger.LogDebug("Migration attempt {Attempt}/{MaxRetries} for {ContextType}", attempt, maxRetries, typeof(TContext).Name);

                // Check if database is accessible before attempting migration
                if (!await context.Database.CanConnectAsync(cancellationToken))
                {
                    logger.LogWarning("Cannot connect to database for {ContextType}, retrying in {Delay}s (attempt {Attempt}/{MaxRetries})",
                        typeof(TContext).Name, retryDelay.Value.TotalSeconds, attempt, maxRetries);

                    if (attempt < maxRetries)
                        await Task.Delay(retryDelay.Value, cancellationToken);

                    continue;
                }

                // Set command timeout for migrations
                var previousTimeout = context.Database.GetCommandTimeout();
                context.Database.SetCommandTimeout(migrationTimeout.Value);

                try
                {
                    var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
                    var pendingCount = pendingMigrations.Count();

                    if (pendingCount > 0)
                    {
                        logger.LogInformation("Applying {MigrationCount} pending migrations for {ContextType} with timeout of {Timeout} minutes",
                            pendingCount, typeof(TContext).Name, migrationTimeout.Value.TotalMinutes);

                        await context.Database.MigrateAsync(cancellationToken);

                        logger.LogInformation("Successfully applied migrations for {ContextType}", typeof(TContext).Name);
                    }
                    else
                    {
                        logger.LogInformation("No pending migrations for {ContextType}", typeof(TContext).Name);
                    }
                }
                finally
                {
                    // Restore previous timeout
                    context.Database.SetCommandTimeout(previousTimeout);
                }

                // Run custom seeders
                var seederRunner = scope.ServiceProvider.GetService<CustomSeederRunner>();

                if (seederRunner != null)
                {
                    logger.LogInformation("Running custom seeders for {ContextType}", typeof(TContext).Name);
                    await seederRunner.RunSeedersAsync(cancellationToken);
                    logger.LogInformation("Custom seeders completed successfully for {ContextType}", typeof(TContext).Name);
                }
                else
                {
                    logger.LogDebug("No custom seeder runner registered for {ContextType}", typeof(TContext).Name);
                }

                return app; // Success - return immediately
            }
            catch (Npgsql.NpgsqlException ex)
            {
                LogPostgreSqlError(logger, ex, typeof(TContext).Name, attempt, maxRetries);

                if (attempt < maxRetries)
                    await Task.Delay(retryDelay.Value, cancellationToken);
            }
            catch (Exception ex)
            {
                LogGeneralMigrationError(logger, ex, typeof(TContext).Name, attempt, maxRetries);

                if (attempt < maxRetries)
                    await Task.Delay(retryDelay.Value, cancellationToken);
            }
            finally
            {
                scope?.Dispose();
            }
        }

        // All retries exhausted
        LogMigrationFailure(logger, typeof(TContext).Name, maxRetries);
        throw new InvalidOperationException(
            $"Database migration failed for {typeof(TContext).Name} after {maxRetries} attempts. See logs for details.");
    }

    private static void LogPostgreSqlError(ILogger logger, Npgsql.NpgsqlException ex, string contextName, int attempt, int maxRetries)
    {
        if (attempt < maxRetries)
        {
            logger.LogWarning("PostgreSQL error on migration attempt {Attempt}/{MaxRetries} for {ContextType}: {Code} - {Message}",
                attempt, maxRetries, contextName, ex.SqlState ?? "N/A", ex.Message);
        }
        else
        {
            logger.LogError(ex, "PostgreSQL migration failed for {ContextType} after {MaxRetries} attempts", contextName, maxRetries);
            logger.LogError("PostgreSQL Error Code: {Code}, Message: {Message}", ex.SqlState, ex.Message);
        }
    }

    private static void LogGeneralMigrationError(ILogger logger, Exception ex, string contextName, int attempt, int maxRetries)
    {
        if (attempt < maxRetries)
        {
            logger.LogWarning(ex, "Migration attempt {Attempt}/{MaxRetries} failed for {ContextType}", attempt, maxRetries, contextName);
        }
        else
        {
            logger.LogError(ex, "Migration failed for {ContextType} after {MaxRetries} attempts", contextName, maxRetries);
            logger.LogError("Error Type: {Type}, Message: {Message}", ex.GetType().Name, ex.Message);

            if (ex.InnerException != null)
                logger.LogError("Inner Error: {Message}", ex.InnerException.Message);
        }
    }

    private static void LogMigrationFailure(ILogger logger, string contextName, int maxRetries)
    {
        logger.LogCritical("=== DATABASE MIGRATION FAILED FOR {ContextType} ===", contextName);
        logger.LogCritical("The application cannot start because database migrations failed after {MaxRetries} attempts", maxRetries);
        logger.LogCritical("Troubleshooting steps:");
        logger.LogCritical("1. Verify database container is running (Docker: 'docker ps', Podman: 'podman ps')");
        logger.LogCritical("2. Check connection string configuration in appsettings.json or Aspire AppHost");
        logger.LogCritical("3. Review Aspire dashboard for service health and logs");
        logger.LogCritical("4. Verify database credentials and permissions");
        logger.LogCritical("5. Check database logs for connection/authentication errors");
    }

    /// <summary>
    /// Ensures the database for the specified DbContext is created.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <param name="app">The web application.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The web application for chaining.</returns>
    public static async Task<WebApplication> EnsureDatabaseCreatedAsync<TContext>(this WebApplication app, CancellationToken cancellationToken = default)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(app);

        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TContext>>();

        try
        {
            logger.LogInformation("Ensuring database exists for {ContextType}", typeof(TContext).Name);

            var context = scope.ServiceProvider.GetRequiredService<TContext>();
            var created = await context.Database.EnsureCreatedAsync(cancellationToken);

            if (created)
                logger.LogInformation("Database created for {ContextType}", typeof(TContext).Name);
            else
                logger.LogInformation("Database already exists for {ContextType}", typeof(TContext).Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while ensuring database exists for {ContextType}", typeof(TContext).Name);
            throw;
        }

        return app;
    }

    /// <summary>
    /// Deletes the database for the specified DbContext (use with caution!).
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <param name="app">The web application.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The web application for chaining.</returns>
    public static async Task<WebApplication> DeleteDatabaseAsync<TContext>(this WebApplication app, CancellationToken cancellationToken = default)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(app);

        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TContext>>();

        try
        {
            logger.LogWarning("Deleting database for {ContextType}", typeof(TContext).Name);

            var context = scope.ServiceProvider.GetRequiredService<TContext>();
            var deleted = await context.Database.EnsureDeletedAsync(cancellationToken);

            if (deleted)
                logger.LogWarning("Database deleted for {ContextType}", typeof(TContext).Name);
            else
                logger.LogInformation("Database did not exist for {ContextType}", typeof(TContext).Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while deleting database for {ContextType}", typeof(TContext).Name);
            throw;
        }

        return app;
    }

    /// <summary>
    /// Runs only custom seeders without applying migrations.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The web application for chaining.</returns>
    public static async Task<WebApplication> RunSeedersAsync(this WebApplication app, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(app);

        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<CustomSeederRunner>>();

        try
        {
            logger.LogInformation("Running custom seeders");

            var seederRunner = scope.ServiceProvider.GetService<CustomSeederRunner>();
            if (seederRunner != null)
            {
                await seederRunner.RunSeedersAsync(cancellationToken);
                logger.LogInformation("Custom seeders completed successfully");
            }
            else
            {
                logger.LogWarning("No custom seeder runner registered");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while running custom seeders");
            throw;
        }

        return app;
    }

    /// <summary>
    /// Gets database information for the specified DbContext.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <param name="app">The web application.</param>
    /// <returns>Database information including provider name and connection state.</returns>
    public static DatabaseInfo GetDatabaseInfo<TContext>(this WebApplication app)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(app);

        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();

        var providerName = context.Database.ProviderName ?? "Unknown";
        var isInMemory = providerName.Contains("InMemory", StringComparison.OrdinalIgnoreCase);

        return new DatabaseInfo
        {
            ProviderName = providerName,
            CanConnect = context.Database.CanConnect(),
            IsRelational = context.Database.IsRelational(),
            IsInMemory = isInMemory
        };
    }
}

/// <summary>
/// Information about a database.
/// </summary>
public record DatabaseInfo
{
    /// <summary>
    /// The name of the database provider.
    /// </summary>
    public required string ProviderName { get; init; }

    /// <summary>
    /// Indicates whether the database can be connected to.
    /// </summary>
    public required bool CanConnect { get; init; }

    /// <summary>
    /// Indicates whether the database is a relational database.
    /// </summary>
    public required bool IsRelational { get; init; }

    /// <summary>
    /// Indicates whether the database is an in-memory database.
    /// </summary>
    public required bool IsInMemory { get; init; }
}

