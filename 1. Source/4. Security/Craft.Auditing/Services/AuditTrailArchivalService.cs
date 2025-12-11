using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Craft.Auditing;

public class AuditTrailArchivalService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<AuditTrailArchivalService> _logger;
    private readonly TimeSpan _archivalInterval;

    public AuditTrailArchivalService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<AuditTrailArchivalService> logger,
        TimeSpan? archivalInterval = null)
    {
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _archivalInterval = archivalInterval ?? TimeSpan.FromHours(24);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Audit Trail Archival Service started. Running every {Interval}", _archivalInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ArchiveOldEntriesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while archiving audit trail entries");
            }

            await Task.Delay(_archivalInterval, stoppingToken);
        }

        _logger.LogInformation("Audit Trail Archival Service stopped");
    }

    private async Task ArchiveOldEntriesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetService<IAuditTrailDbContext>();

        if (dbContext == null)
        {
            _logger.LogWarning("IAuditTrailDbContext not registered. Archival skipped.");
            return;
        }

        var now = DateTime.UtcNow;
        var entriesToArchive = dbContext.AuditTrails
            .Where(a => !a.IsArchived && 
                       a.ArchiveAfter.HasValue && 
                       a.ArchiveAfter.Value <= now)
            .ToList();

        if (entriesToArchive.Count == 0)
        {
            _logger.LogDebug("No audit trail entries to archive");
            return;
        }

        _logger.LogInformation("Archiving {Count} audit trail entries", entriesToArchive.Count);

        foreach (var entry in entriesToArchive)
        {
            entry.Archive();
        }

        if (dbContext is Microsoft.EntityFrameworkCore.DbContext context)
        {
            var archived = await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Successfully archived {Count} audit trail entries", archived);
        }
    }
}

public static class AuditTrailArchivalServiceExtensions
{
    public static IServiceCollection AddAuditTrailArchivalService(
        this IServiceCollection services,
        TimeSpan? archivalInterval = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHostedService(provider =>
        {
            var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
            var logger = provider.GetRequiredService<ILogger<AuditTrailArchivalService>>();
            return new AuditTrailArchivalService(scopeFactory, logger, archivalInterval);
        });

        return services;
    }
}
