using Craft.Utilities.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Craft.Configuration;

/// <summary>
/// Configuration provider that automatically decrypts encrypted configuration values.
/// Values starting with the configured prefix (default: "ENC:") are decrypted using AES-256 encryption.
/// </summary>
/// <remarks>
/// This provider wraps another configuration provider and intercepts all Get operations to decrypt
/// values that start with the encryption prefix. Works with any configuration source (JSON, environment variables, etc.).
/// </remarks>
public class DecryptedConfigurationProvider : ConfigurationProvider
{
    private readonly IConfigurationProvider _innerProvider;
    private readonly IKeySafeService _keySafeService;
    private readonly string _encryptionPrefix;
    private readonly ILogger<DecryptedConfigurationProvider> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DecryptedConfigurationProvider"/> class.
    /// </summary>
    /// <param name="innerProvider">The underlying configuration provider to wrap.</param>
    /// <param name="keySafeService">The encryption/decryption service.</param>
    /// <param name="encryptionPrefix">The prefix that identifies encrypted values (default: "ENC:").</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public DecryptedConfigurationProvider(
        IConfigurationProvider innerProvider,
        IKeySafeService keySafeService,
        string encryptionPrefix = "ENC:",
        ILogger<DecryptedConfigurationProvider>? logger = null)
    {
        _innerProvider = innerProvider ?? throw new ArgumentNullException(nameof(innerProvider));
        _keySafeService = keySafeService ?? throw new ArgumentNullException(nameof(keySafeService));
        _encryptionPrefix = encryptionPrefix;
        _logger = logger ?? NullLogger<DecryptedConfigurationProvider>.Instance;
    }

    /// <summary>
    /// Loads configuration data from the inner provider.
    /// </summary>
    public override void Load()
    {
        _innerProvider.Load();
        LoadDecryptedData();
    }

    /// <summary>
    /// Attempts to get a configuration value by key, decrypting if necessary.
    /// </summary>
    public override bool TryGet(string key, out string? value)
        => Data.TryGetValue(key, out value);

    /// <summary>
    /// Sets a configuration value.
    /// </summary>
    public override void Set(string key, string? value)
        => Data[key] = value;

    /// <summary>
    /// Gets the child keys for a given parent key.
    /// </summary>
    public override IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string? parentPath)
        => _innerProvider.GetChildKeys(earlierKeys, parentPath);

    private void LoadDecryptedData()
    {
        var allKeys = GetAllKeys(_innerProvider);

        foreach (var key in allKeys)
        {
            if (_innerProvider.TryGet(key, out var value))
            {
                if (!string.IsNullOrEmpty(value) && value.StartsWith(_encryptionPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var encryptedValue = value[_encryptionPrefix.Length..];
                        var decryptedValue = _keySafeService.Decrypt(encryptedValue);

                        Data[key] = decryptedValue;

                        _logger.LogDebug("Successfully decrypted configuration key: {Key}", key);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to decrypt configuration key: {Key}", key);
                        Data[key] = value;
                    }
                }
                else
                {
                    Data[key] = value;
                }
            }
        }
    }

    private static IEnumerable<string> GetAllKeys(IConfigurationProvider provider)
    {
        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        CollectKeys(provider, null, keys);
        
        return keys;
    }

    private static void CollectKeys(IConfigurationProvider provider, string? parentPath, HashSet<string> keys)
    {
        var childKeys = provider.GetChildKeys([], parentPath);

        foreach (var key in childKeys)
        {
            var fullKey = string.IsNullOrEmpty(parentPath)
                ? key
                : $"{parentPath}{ConfigurationPath.KeyDelimiter}{key}";

            keys.Add(fullKey);

            if (provider.TryGet(fullKey, out _))
                continue;

            CollectKeys(provider, fullKey, keys);
        }
    }
}
