using Craft.Configuration.Abstractions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Craft.Configuration.Encryption;

/// <summary>
/// Provides encryption and decryption for configuration values using ASP.NET Core Data Protection.
/// This encryptor is useful when you want to use the built-in data protection system
/// for automatic key management and rotation.
/// </summary>
public class DataProtectionEncryptor : IConfigurationEncryption
{
    private readonly IDataProtector _protector;
    private readonly ILogger<DataProtectionEncryptor> _logger;

    /// <inheritdoc/>
    public string EncryptionPrefix { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataProtectionEncryptor"/> class.
    /// </summary>
    /// <param name="dataProtectionProvider">The data protection provider.</param>
    /// <param name="encryptionPrefix">The prefix to identify encrypted values.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public DataProtectionEncryptor(
        IDataProtectionProvider dataProtectionProvider,
        string encryptionPrefix = "ENC:",
        ILogger<DataProtectionEncryptor>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(dataProtectionProvider);
        
        _protector = dataProtectionProvider.CreateProtector("Craft.Configuration");
        EncryptionPrefix = encryptionPrefix ?? throw new ArgumentNullException(nameof(encryptionPrefix));
        _logger = logger ?? NullLogger<DataProtectionEncryptor>.Instance;
    }

    /// <inheritdoc/>
    public string Encrypt(string plainText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plainText);

        try
        {
            var encrypted = _protector.Protect(plainText);
            _logger.LogDebug("Successfully encrypted configuration value using Data Protection");
            
            return encrypted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to encrypt configuration value using Data Protection");
            throw new InvalidOperationException("Encryption failed", ex);
        }
    }

    /// <inheritdoc/>
    public string Decrypt(string encryptedText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(encryptedText);

        try
        {
            var decrypted = _protector.Unprotect(encryptedText);
            _logger.LogDebug("Successfully decrypted configuration value using Data Protection");
            
            return decrypted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt configuration value using Data Protection");
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
