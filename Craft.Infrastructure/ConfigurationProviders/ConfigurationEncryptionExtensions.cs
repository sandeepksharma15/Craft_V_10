using Craft.Utilities.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Craft.Infrastructure.ConfigurationProviders;

/// <summary>
/// Extension methods for configuration encryption/decryption support.
/// </summary>
public static class ConfigurationEncryptionExtensions
{
    private const string DefaultEncryptionPrefix = "ENC:";

    /// <summary>
    /// Adds decryption support to the configuration builder for all configuration sources.
    /// Values starting with the encryption prefix (default: "ENC:") will be automatically decrypted.
    /// </summary>
    /// <param name="builder">The configuration builder.</param>
    /// <param name="encryptionPrefix">The prefix that identifies encrypted values (default: "ENC:").</param>
    /// <returns>The configuration builder for chaining.</returns>
    /// <remarks>
    /// Requires AES_ENCRYPTION_KEY and AES_ENCRYPTION_IV environment variables to be set.
    /// Call this method AFTER adding all your configuration sources.
    /// </remarks>
    public static IConfigurationBuilder AddDecryption(this IConfigurationBuilder builder, string encryptionPrefix = DefaultEncryptionPrefix)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var keySafeService = new KeySafeService();

        var sources = builder.Sources.ToList();
        builder.Sources.Clear();

        foreach (var source in sources)
        {
            var decryptedSource = new DecryptedConfigurationSource(source, keySafeService, encryptionPrefix);
            builder.Add(decryptedSource);
        }

        return builder;
    }

    /// <summary>
    /// Adds decryption support to the configuration builder for all configuration sources with custom logger.
    /// Values starting with the encryption prefix (default: "ENC:") will be automatically decrypted.
    /// </summary>
    /// <param name="builder">The configuration builder.</param>
    /// <param name="loggerFactory">Factory to create logger for diagnostics.</param>
    /// <param name="encryptionPrefix">The prefix that identifies encrypted values (default: "ENC:").</param>
    /// <returns>The configuration builder for chaining.</returns>
    public static IConfigurationBuilder AddDecryption(this IConfigurationBuilder builder, ILoggerFactory loggerFactory,
        string encryptionPrefix = DefaultEncryptionPrefix)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        var keySafeService = new KeySafeService();
        var logger = loggerFactory.CreateLogger<DecryptedConfigurationProvider>();

        var sources = builder.Sources.ToList();
        builder.Sources.Clear();

        foreach (var source in sources)
        {
            var decryptedSource = new DecryptedConfigurationSource(source, keySafeService, encryptionPrefix, logger);
            builder.Add(decryptedSource);
        }

        return builder;
    }

    /// <summary>
    /// Adds decryption support for IOptions pattern with post-configuration.
    /// Automatically decrypts any string properties that start with the encryption prefix.
    /// </summary>
    /// <typeparam name="TOptions">The options type to configure.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="encryptionPrefix">The prefix that identifies encrypted values (default: "ENC:").</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// This decrypts string properties at the options class level, working even if
    /// the configuration provider doesn't support decryption.
    /// </remarks>
    public static IServiceCollection AddOptionsDecryption<TOptions>(this IServiceCollection services, string encryptionPrefix = DefaultEncryptionPrefix)
        where TOptions : class
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IPostConfigureOptions<TOptions>>(sp =>
        {
            var keySafeService = sp.GetService<IKeySafeService>() ?? new KeySafeService();
            var logger = sp.GetService<ILogger<OptionsDecryptor<TOptions>>>();
            
            return new OptionsDecryptor<TOptions>(keySafeService, encryptionPrefix, logger);
        });

        return services;
    }

    /// <summary>
    /// Decrypts all encrypted values in the configuration root (post-processing approach).
    /// This modifies the configuration in place.
    /// </summary>
    /// <param name="configuration">The configuration root to decrypt.</param>
    /// <param name="encryptionPrefix">The prefix that identifies encrypted values (default: "ENC:").</param>
    /// <returns>The same configuration root for chaining.</returns>
    /// <remarks>
    /// This is a post-processing approach that modifies configuration after it's loaded.
    /// Use this if you can't use the ConfigurationProvider approach.
    /// </remarks>
    public static IConfigurationRoot DecryptConfiguration(this IConfigurationRoot configuration, string encryptionPrefix = DefaultEncryptionPrefix)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var keySafeService = new KeySafeService();
        DecryptConfigurationRecursive(configuration, keySafeService, encryptionPrefix);
        
        return configuration;
    }

    /// <summary>
    /// Decrypts all encrypted values in the configuration root with logging (post-processing approach).
    /// This modifies the configuration in place.
    /// </summary>
    /// <param name="configuration">The configuration root to decrypt.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    /// <param name="encryptionPrefix">The prefix that identifies encrypted values (default: "ENC:").</param>
    /// <returns>The same configuration root for chaining.</returns>
    public static IConfigurationRoot DecryptConfiguration(this IConfigurationRoot configuration, ILogger logger,
        string encryptionPrefix = DefaultEncryptionPrefix)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(logger);

        var keySafeService = new KeySafeService();
        DecryptConfigurationRecursive(configuration, keySafeService, encryptionPrefix, logger);
        
        return configuration;
    }

    private static void DecryptConfigurationRecursive(IConfiguration configuration, IKeySafeService keySafeService,
        string encryptionPrefix, ILogger? logger = null)
    {
        foreach (var child in configuration.GetChildren())
        {
            if (child.Value?.StartsWith(encryptionPrefix, StringComparison.OrdinalIgnoreCase) == true)
            {
                try
                {
                    var encryptedValue = child.Value[encryptionPrefix.Length..];
                    var decryptedValue = keySafeService.Decrypt(encryptedValue);
                    configuration[child.Key] = decryptedValue;
                    
                    logger?.LogDebug("Successfully decrypted configuration key: {Key}", child.Path);
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Failed to decrypt configuration key: {Key}", child.Path);
                }
            }

            DecryptConfigurationRecursive(child, keySafeService, encryptionPrefix, logger);
        }
    }
}
