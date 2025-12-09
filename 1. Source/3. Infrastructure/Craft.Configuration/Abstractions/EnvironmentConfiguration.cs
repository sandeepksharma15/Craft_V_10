namespace Craft.Configuration.Abstractions;

/// <summary>
/// Represents configuration options for a specific environment.
/// </summary>
public class EnvironmentConfiguration
{
    /// <summary>
    /// Gets or sets the environment name (e.g., Development, Staging, Production).
    /// </summary>
    public string Environment { get; set; } = "Development";

    /// <summary>
    /// Gets or sets whether to use encrypted configuration values.
    /// </summary>
    public bool UseEncryption { get; set; }

    /// <summary>
    /// Gets or sets the encryption prefix for identifying encrypted values.
    /// </summary>
    public string EncryptionPrefix { get; set; } = "ENC:";

    /// <summary>
    /// Gets or sets whether to validate configuration on startup.
    /// </summary>
    public bool ValidateOnStartup { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to reload configuration when files change.
    /// </summary>
    public bool ReloadOnChange { get; set; } = true;

    /// <summary>
    /// Gets or sets the list of JSON configuration file paths.
    /// </summary>
    public List<string> JsonFiles { get; set; } = ["appsettings.json"];

    /// <summary>
    /// Gets or sets whether to include environment-specific configuration files.
    /// </summary>
    public bool UseEnvironmentSpecificFiles { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to load user secrets in development.
    /// </summary>
    public bool UseUserSecrets { get; set; } = true;

    /// <summary>
    /// Gets or sets the user secrets ID for the application.
    /// </summary>
    public string? UserSecretsId { get; set; }
}
