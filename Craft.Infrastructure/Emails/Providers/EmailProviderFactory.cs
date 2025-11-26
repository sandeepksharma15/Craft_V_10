using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Craft.Infrastructure.Emails;

/// <summary>
/// Factory implementation for creating email provider instances.
/// </summary>
public class EmailProviderFactory : IEmailProviderFactory
{
    private readonly IEnumerable<IEmailProvider> _providers;
    private readonly EmailOptions _options;
    private readonly ILogger<EmailProviderFactory> _logger;

    public EmailProviderFactory(
        IEnumerable<IEmailProvider> providers,
        IOptions<EmailOptions> options,
        ILogger<EmailProviderFactory> logger)
    {
        _providers = providers;
        _options = options.Value;
        _logger = logger;
    }

    public IEmailProvider GetProvider(string providerName)
    {
        var provider = _providers.FirstOrDefault(p =>
            p.Name.Equals(providerName, StringComparison.OrdinalIgnoreCase));

        if (provider == null)
        {
            _logger.LogError("Email provider '{ProviderName}' not found", providerName);
            throw new InvalidOperationException($"Email provider '{providerName}' is not registered");
        }

        if (!provider.IsConfigured())
        {
            _logger.LogWarning("Email provider '{ProviderName}' is not properly configured", providerName);
        }

        return provider;
    }

    public IEmailProvider GetDefaultProvider()
    {
        return GetProvider(_options.Provider);
    }

    public IEnumerable<IEmailProvider> GetAllProviders()
    {
        return _providers;
    }
}
