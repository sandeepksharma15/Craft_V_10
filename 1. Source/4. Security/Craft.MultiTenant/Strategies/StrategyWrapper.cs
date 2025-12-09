using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Craft.MultiTenant;

public class StrategyWrapper(ITenantStrategy strategy, ILogger logger) : ITenantStrategy
{
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ITenantStrategy _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));

    public async Task<string?> GetIdentifierAsync(HttpContext context)
    {
        string? identifier;

        try
        {
            identifier = await _strategy.GetIdentifierAsync(context);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception in GetIdentifierAsync");
            throw new MultiTenantException($"Exception in {_strategy.GetType()}.GetIdentifierAsync.", e);
        }

        if (_logger.IsEnabled(LogLevel.Debug))
            if (identifier != null)
                _logger.LogDebug("GetIdentifierAsync: Found identifier: \"{Identifier}\"", identifier);
            else
                _logger.LogDebug("GetIdentifierAsync: No identifier found");

        return identifier!;
    }
}
