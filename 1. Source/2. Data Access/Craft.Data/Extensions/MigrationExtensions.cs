using Craft.Value.Helpers;
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
    /// </summary>
    /// <typeparam name="TContext">The DbContext type to migrate.</typeparam>
    /// <param name="app">The web application.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The web application for chaining.</returns>
    public static async Task<WebApplication> MigrateDatabaseAsync<TContext>(this WebApplication app, CancellationToken cancellationToken = default)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(app);

        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TContext>>();

        try
        {
            logger.LogInformation("Starting database migration for {ContextType}", typeof(TContext).Name);

            // Apply migrations
            var context = scope.ServiceProvider.GetRequiredService<TContext>();
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
            var pendingCount = pendingMigrations.Count();

            if (pendingCount > 0)
            {
                logger.LogInformation(
                    "Applying {MigrationCount} pending migrations for {ContextType}",
                    pendingCount,
                    typeof(TContext).Name);

                await context.Database.MigrateAsync(cancellationToken);

                logger.LogInformation(
                    "Successfully applied migrations for {ContextType}",
                    typeof(TContext).Name);
            }
            else
            {
                logger.LogInformation(
                    "No pending migrations for {ContextType}",
                    typeof(TContext).Name);
            }

            // Run custom seeders
            var seederRunner = scope.ServiceProvider.GetService<CustomSeederRunner>();

            if (seederRunner != null)
            {
                logger.LogInformation("Running custom seeders");
                await seederRunner.RunSeedersAsync(cancellationToken);
                logger.LogInformation("Custom seeders completed successfully");
            }
            else
            {
                logger.LogDebug("No custom seeder runner registered");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error occurred while migrating database for {ContextType}",
                typeof(TContext).Name);
            throw;
        }

        return app;
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
            logger.LogError(
                ex,
                "Error occurred while ensuring database exists for {ContextType}",
                typeof(TContext).Name);
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
            logger.LogError(
                ex,
                "Error occurred while deleting database for {ContextType}",
                typeof(TContext).Name);
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

