namespace Craft.Configuration.Abstractions;

/// <summary>
/// Provides methods for accessing and managing configuration values.
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Gets a configuration value by key.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <returns>The configuration value, or null if not found.</returns>
    string? Get(string key);

    /// <summary>
    /// Gets a strongly-typed configuration value by key.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <param name="key">The configuration key.</param>
    /// <returns>The configuration value converted to type T.</returns>
    T? Get<T>(string key);

    /// <summary>
    /// Gets a configuration section as a strongly-typed object.
    /// </summary>
    /// <typeparam name="T">The type to bind the section to.</typeparam>
    /// <param name="sectionKey">The section key.</param>
    /// <returns>The configuration section bound to type T.</returns>
    T? GetSection<T>(string sectionKey) where T : class, new();

    /// <summary>
    /// Sets a configuration value.
    /// </summary>
    /// <param name="key">The configuration key.</param>
    /// <param name="value">The value to set.</param>
    void Set(string key, string? value);

    /// <summary>
    /// Reloads configuration from all sources.
    /// </summary>
    void Reload();

    /// <summary>
    /// Gets all configuration keys.
    /// </summary>
    /// <returns>An enumerable of all configuration keys.</returns>
    IEnumerable<string> GetAllKeys();

    /// <summary>
    /// Checks if a configuration key exists.
    /// </summary>
    /// <param name="key">The configuration key to check.</param>
    /// <returns>True if the key exists; otherwise, false.</returns>
    bool Exists(string key);
}
