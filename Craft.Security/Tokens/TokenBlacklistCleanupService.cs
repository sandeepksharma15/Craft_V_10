using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Craft.Security.Tokens;

/// <summary>
/// Background service that periodically removes expired tokens from the blacklist.
/// </summary>
public class TokenBlacklistCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TokenBlacklistCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval;

    public TokenBlacklistCleanupService(
        IServiceProvider serviceProvider,
        ILogger<TokenBlacklistCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _cleanupInterval = TimeSpan.FromHours(1);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Token blacklist cleanup service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_cleanupInterval, stoppingToken);

                using var scope = _serviceProvider.CreateScope();
                var blacklist = scope.ServiceProvider.GetRequiredService<ITokenBlacklist>();

                var removedCount = await blacklist.CleanupExpiredTokensAsync(stoppingToken);
                
                if (removedCount > 0)
                    _logger.LogInformation("Cleaned up {Count} expired tokens from blacklist", removedCount);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Token blacklist cleanup service is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during token blacklist cleanup");
            }
        }

        _logger.LogInformation("Token blacklist cleanup service stopped");
    }
}
