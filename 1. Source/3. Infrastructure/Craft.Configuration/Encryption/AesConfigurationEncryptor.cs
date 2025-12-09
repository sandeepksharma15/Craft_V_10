using Craft.Configuration.Abstractions;
using Craft.Utilities.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Craft.Configuration.Encryption;

/// <summary>
/// Provides AES-256 encryption and decryption for configuration values.
/// Uses the KeySafeService for cryptographic operations.
/// </summary>
public class AesConfigurationEncryptor : IConfigurationEncryption
{
    private readonly IKeySafeService _keySafeService;
    private readonly ILogger<AesConfigurationEncryptor> _logger;

    /// <inheritdoc/>
    public string EncryptionPrefix { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AesConfigurationEncryptor"/> class.
    /// </summary>
    /// <param name="keySafeService">The encryption service.</param>
    /// <param name="encryptionPrefix">The prefix to identify encrypted values.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public AesConfigurationEncryptor(
        IKeySafeService keySafeService,
        string encryptionPrefix = "ENC:",
        ILogger<AesConfigurationEncryptor>? logger = null)
    {
        _keySafeService = keySafeService ?? throw new ArgumentNullException(nameof(keySafeService));
        EncryptionPrefix = encryptionPrefix ?? throw new ArgumentNullException(nameof(encryptionPrefix));
        _logger = logger ?? NullLogger<AesConfigurationEncryptor>.Instance;
    }

    /// <inheritdoc/>
    public string Encrypt(string plainText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plainText);

        try
        {
            var encrypted = _keySafeService.Encrypt(plainText);
            _logger.LogDebug("Successfully encrypted configuration value");
            
            return encrypted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to encrypt configuration value");
            throw new InvalidOperationException("Encryption failed", ex);
        }
    }

    /// <inheritdoc/>
    public string Decrypt(string encryptedText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(encryptedText);

        try
        {
            var decrypted = _keySafeService.Decrypt(encryptedText);
            _logger.LogDebug("Successfully decrypted configuration value");
            
            return decrypted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt configuration value");
            throw new InvalidOperationException("Decryption failed", ex);
        }
    }

    /// <inheritdoc/>
    public bool IsEncrypted(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return value.StartsWith(EncryptionPrefix, StringComparison.OrdinalIgnoreCase);
    }
}
