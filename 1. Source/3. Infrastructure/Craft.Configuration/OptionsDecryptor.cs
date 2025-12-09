using Craft.Utilities.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace Craft.Configuration;

/// <summary>
/// Post-configures options by decrypting encrypted string properties.
/// </summary>
/// <typeparam name="TOptions">The options type to post-configure.</typeparam>
internal class OptionsDecryptor<TOptions> : IPostConfigureOptions<TOptions> where TOptions : class
{
    private readonly IKeySafeService _keySafeService;
    private readonly string _encryptionPrefix;
    private readonly ILogger? _logger;

    public OptionsDecryptor(IKeySafeService keySafeService, string encryptionPrefix, ILogger? logger = null)
    {
        _keySafeService = keySafeService ?? throw new ArgumentNullException(nameof(keySafeService));
        _encryptionPrefix = encryptionPrefix;
        _logger = logger;
    }

    public void PostConfigure(string? name, TOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var properties = typeof(TOptions).GetProperties(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);

        foreach (var property in properties)
        {
            if (property.PropertyType != typeof(string))
                continue;

            if (!property.CanRead || !property.CanWrite)
                continue;

            var value = property.GetValue(options) as string;

            if (string.IsNullOrEmpty(value))
                continue;

            if (!value.StartsWith(_encryptionPrefix, StringComparison.OrdinalIgnoreCase))
                continue;

            try
            {
                var encryptedValue = value[_encryptionPrefix.Length..];
                var decryptedValue = _keySafeService.Decrypt(encryptedValue);

                property.SetValue(options, decryptedValue);

                _logger?.LogDebug("Successfully decrypted property {Property} on {OptionsType}",
                    property.Name, typeof(TOptions).Name);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to decrypt property {Property} on {OptionsType}",
                    property.Name, typeof(TOptions).Name);
            }
        }
    }
}
