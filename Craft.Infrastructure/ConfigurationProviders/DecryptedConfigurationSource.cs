using Craft.Utilities.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Craft.Infrastructure.ConfigurationProviders;

/// <summary>
/// Configuration source that provides decryption capabilities for encrypted configuration values.
/// </summary>
public class DecryptedConfigurationSource : IConfigurationSource
{
    private readonly IConfigurationSource _innerSource;
    private readonly IKeySafeService _keySafeService;
    private readonly string _encryptionPrefix;
    private readonly ILogger<DecryptedConfigurationProvider>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DecryptedConfigurationSource"/> class.
    /// </summary>
    /// <param name="innerSource">The underlying configuration source to wrap.</param>
    /// <param name="keySafeService">The encryption/decryption service.</param>
    /// <param name="encryptionPrefix">The prefix that identifies encrypted values (default: "ENC:").</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public DecryptedConfigurationSource(
        IConfigurationSource innerSource,
        IKeySafeService keySafeService,
        string encryptionPrefix = "ENC:",
        ILogger<DecryptedConfigurationProvider>? logger = null)
    {
        _innerSource = innerSource ?? throw new ArgumentNullException(nameof(innerSource));
        _keySafeService = keySafeService ?? throw new ArgumentNullException(nameof(keySafeService));
        _encryptionPrefix = encryptionPrefix;
        _logger = logger;
    }

    /// <summary>
    /// Builds the configuration provider.
    /// </summary>
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        var innerProvider = _innerSource.Build(builder);
        return new DecryptedConfigurationProvider(innerProvider, _keySafeService, _encryptionPrefix, _logger);
    }
}
