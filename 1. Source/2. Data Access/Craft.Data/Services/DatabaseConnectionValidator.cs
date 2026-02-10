using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Craft.Data;

/// <summary>
/// Background service that validates database connectivity on application startup.
/// Prevents application from starting if database is unreachable.
/// </summary>
public class DatabaseConnectionValidator : IHostedService
{
    private readonly DatabaseOptions _options;
    private readonly IEnumerable<IDatabaseProvider> _providers;
    private readonly ILogger<DatabaseConnectionValidator> _logger;

    public DatabaseConnectionValidator(
        IOptions<DatabaseOptions> options,
        IEnumerable<IDatabaseProvider> providers,
        ILogger<DatabaseConnectionValidator> logger)
    {
        _options = options.Value;
        _providers = providers;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Validating database connection on startup...");

        try
        {
            // Find the appropriate provider
            var provider = _providers.FirstOrDefault(p => p.CanHandle(_options.DbProvider));

            if (provider == null)
            {
                _logger.LogWarning(
                    "No database provider found for '{DbProvider}'. Skipping connection validation.",
                    _options.DbProvider);
                return Task.CompletedTask;
            }

            // Validate connection
            var isValid = provider.ValidateConnection(_options.ConnectionString);

            if (isValid)
            {
                _logger.LogInformation(
                    "Database connection validated successfully for provider '{DbProvider}'",
                    _options.DbProvider);
            }
            else
            {
                _logger.LogError(
                    "Database connection validation FAILED for provider '{DbProvider}'. Connection string may be invalid or database is unreachable.",
                    _options.DbProvider);

                // Optionally throw exception to prevent startup
                // throw new InvalidOperationException($"Failed to connect to database using provider '{_options.DbProvider}'");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Exception occurred while validating database connection for provider '{DbProvider}'",
                _options.DbProvider);

            // Optionally rethrow to prevent startup
            // throw;
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // No cleanup needed
        return Task.CompletedTask;
    }
}
