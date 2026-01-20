using Craft.CryptKey;
using Microsoft.Extensions.Options;

namespace Craft.Hosting.Extensions;

/// <summary>
/// Extension methods for configuring encryption and hashing services.
/// </summary>
public static class CryptKeyExtensions
{
    /// <summary>
    /// Adds HashKeys service for encoding/decoding entity IDs.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure HashKey options.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// services.AddHashKeys(options =>
    /// {
    ///     options.Salt = "YourSecretSalt";
    ///     options.Alphabet = "abcdefghijklmnopqrstuvwxyz1234567890";
    ///     options.MinHashLength = 8;
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddHashKeys(this IServiceCollection services,
        Action<HashKeyOptions>? configureOptions = null)
    {
        services.Configure(configureOptions ?? (_ => { }));
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<HashKeyOptions>>().Value);
        services.AddSingleton<IHashKeys>(sp =>
        {
            var options = sp.GetRequiredService<HashKeyOptions>();
            return new HashKeys(options);
        });

        return services;
    }
}
