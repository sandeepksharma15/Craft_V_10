namespace Craft.Emails;

/// <summary>
/// Factory for creating email provider instances.
/// </summary>
public interface IEmailProviderFactory
{
    /// <summary>
    /// Gets an email provider by name.
    /// </summary>
    /// <param name="providerName">The name of the provider (e.g., "smtp", "sendgrid").</param>
    /// <returns>The email provider instance.</returns>
    IEmailProvider GetProvider(string providerName);

    /// <summary>
    /// Gets the default email provider based on configuration.
    /// </summary>
    IEmailProvider GetDefaultProvider();

    /// <summary>
    /// Gets all registered email providers.
    /// </summary>
    IEnumerable<IEmailProvider> GetAllProviders();
}
