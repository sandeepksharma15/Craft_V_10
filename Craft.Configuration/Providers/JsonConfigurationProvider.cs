using Craft.Configuration.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Craft.Configuration.Providers;

/// <summary>
/// Provides JSON-based configuration with automatic encryption/decryption support and reload on change.
/// </summary>
public class JsonConfigurationProvider : IConfigurationService
{
    private readonly IConfiguration _configuration;
    private readonly IConfigurationEncryption? _encryption;
    private readonly ILogger<JsonConfigurationProvider> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonConfigurationProvider"/> class.
    /// </summary>
    /// <param name="configuration">The configuration root.</param>
    /// <param name="encryption">Optional encryption service for handling encrypted values.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public JsonConfigurationProvider(
        IConfiguration configuration,
        IConfigurationEncryption? encryption = null,
        ILogger<JsonConfigurationProvider>? logger = null)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _encryption = encryption;
        _logger = logger ?? NullLogger<JsonConfigurationProvider>.Instance;
    }

    /// <inheritdoc/>
    public string? Get(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var value = _configuration[key];

        if (value == null)
        {
            _logger.LogDebug("Configuration key '{Key}' not found", key);
            return null;
        }

        if (_encryption?.IsEncrypted(value) == true)
        {
            try
            {
                var encryptedValue = value[_encryption.EncryptionPrefix.Length..];
                var decrypted = _encryption.Decrypt(encryptedValue);
                _logger.LogDebug("Successfully decrypted configuration key: {Key}", key);
                
                return decrypted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to decrypt configuration key: {Key}", key);
                return value;
            }
        }

        return value;
    }

    /// <inheritdoc/>
    public T? Get<T>(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var value = Get(key);
        
        if (value == null)
            return default;

        try
        {
            if (typeof(T) == typeof(string))
                return (T)(object)value;

            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to convert configuration key '{Key}' to type {Type}", key, typeof(T).Name);
            return default;
        }
    }

    /// <inheritdoc/>
    public T? GetSection<T>(string sectionKey) where T : class, new()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionKey);

        try
        {
            var section = _configuration.GetSection(sectionKey);
            
            if (!section.Exists())
            {
                _logger.LogDebug("Configuration section '{SectionKey}' not found", sectionKey);
                return null;
            }

            var result = new T();
            section.Bind(result);

            DecryptSectionProperties(result);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to bind configuration section '{SectionKey}' to type {Type}", 
                sectionKey, typeof(T).Name);
            return null;
        }
    }

    /// <inheritdoc/>
    public void Set(string key, string? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        try
        {
            _configuration[key] = value;
            _logger.LogDebug("Set configuration key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set configuration key: {Key}", key);
            throw;
        }
    }

    /// <inheritdoc/>
    public void Reload()
    {
        try
        {
            if (_configuration is IConfigurationRoot configRoot)
            {
                configRoot.Reload();
                _logger.LogInformation("Configuration reloaded successfully");
            }
            else
            {
                _logger.LogWarning("Configuration does not support reload");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reload configuration");
            throw;
        }
    }

    /// <inheritdoc/>
    public IEnumerable<string> GetAllKeys()
    {
        var keys = new HashSet<string>();
        CollectKeys(_configuration, null, keys);
        
        return keys;
    }

    /// <inheritdoc/>
    public bool Exists(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        return _configuration[key] != null;
    }

    private void DecryptSectionProperties<T>(T instance) where T : class
    {
        if (_encryption == null || instance == null)
            return;

        var properties = typeof(T).GetProperties(
            System.Reflection.BindingFlags.Public | 
            System.Reflection.BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (property.PropertyType != typeof(string) || !property.CanRead || !property.CanWrite)
                continue;

            var value = property.GetValue(instance) as string;
            
            if (_encryption.IsEncrypted(value))
            {
                try
                {
                    var encryptedValue = value![_encryption.EncryptionPrefix.Length..];
                    var decrypted = _encryption.Decrypt(encryptedValue);
                    property.SetValue(instance, decrypted);
                    
                    _logger.LogDebug("Successfully decrypted property {Property} on {Type}", 
                        property.Name, typeof(T).Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to decrypt property {Property} on {Type}", 
                        property.Name, typeof(T).Name);
                }
            }
        }
    }

    private static void CollectKeys(IConfiguration configuration, string? parentPath, HashSet<string> keys)
    {
        foreach (var child in configuration.GetChildren())
        {
            var fullKey = string.IsNullOrEmpty(parentPath)
                ? child.Key
                : $"{parentPath}:{child.Key}";

            keys.Add(fullKey);
            CollectKeys(child, fullKey, keys);
        }
    }
}
