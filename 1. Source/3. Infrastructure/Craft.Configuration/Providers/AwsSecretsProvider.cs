using Craft.Configuration.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Craft.Configuration.Providers;

/// <summary>
/// Provides configuration from AWS Secrets Manager.
/// This is a placeholder/example implementation that can be extended when AWS packages are available.
/// </summary>
/// <remarks>
/// To use AWS Secrets Manager:
/// 1. Install Amazon.Extensions.Configuration.SystemsManager or AWSSDK.SecretsManager
/// 2. Configure the IConfigurationBuilder with AddSystemsManager or custom provider
/// 3. Implement this class to wrap the AWS configuration
/// </remarks>
public class AwsSecretsProvider : IConfigurationService
{
    private readonly ILogger<AwsSecretsProvider> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AwsSecretsProvider"/> class.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public AwsSecretsProvider(ILogger<AwsSecretsProvider>? logger = null)
    {
        _logger = logger ?? NullLogger<AwsSecretsProvider>.Instance;
        _logger.LogWarning("AwsSecretsProvider is not fully implemented. Install AWS configuration packages to enable.");
    }

    /// <inheritdoc/>
    public string? Get(string key)
    {
        throw new NotImplementedException(
            "AWS Secrets Manager support requires Amazon.Extensions.Configuration.SystemsManager or AWSSDK.SecretsManager package. " +
            "Install the package and configure it in your IConfigurationBuilder.");
    }

    /// <inheritdoc/>
    public T? Get<T>(string key)
    {
        throw new NotImplementedException(
            "AWS Secrets Manager support requires Amazon.Extensions.Configuration.SystemsManager or AWSSDK.SecretsManager package. " +
            "Install the package and configure it in your IConfigurationBuilder.");
    }

    /// <inheritdoc/>
    public T? GetSection<T>(string sectionKey) where T : class, new()
    {
        throw new NotImplementedException(
            "AWS Secrets Manager support requires Amazon.Extensions.Configuration.SystemsManager or AWSSDK.SecretsManager package. " +
            "Install the package and configure it in your IConfigurationBuilder.");
    }

    /// <inheritdoc/>
    public void Set(string key, string? value)
    {
        throw new NotSupportedException("AWS Secrets Manager does not support setting values through configuration API.");
    }

    /// <inheritdoc/>
    public void Reload()
    {
        _logger.LogInformation("AWS Secrets Manager configuration reload requested");
    }

    /// <inheritdoc/>
    public IEnumerable<string> GetAllKeys()
    {
        throw new NotImplementedException(
            "AWS Secrets Manager support requires Amazon.Extensions.Configuration.SystemsManager or AWSSDK.SecretsManager package. " +
            "Install the package and configure it in your IConfigurationBuilder.");
    }

    /// <inheritdoc/>
    public bool Exists(string key)
    {
        throw new NotImplementedException(
            "AWS Secrets Manager support requires Amazon.Extensions.Configuration.SystemsManager or AWSSDK.SecretsManager package. " +
            "Install the package and configure it in your IConfigurationBuilder.");
    }
}

/// <summary>
/// Extension methods for AWS Secrets Manager configuration.
/// </summary>
public static class AwsSecretsExtensions
{
    /// <summary>
    /// Adds AWS Secrets Manager configuration support.
    /// </summary>
    /// <param name="builder">The configuration builder.</param>
    /// <param name="region">The AWS region.</param>
    /// <returns>The configuration builder for chaining.</returns>
    /// <remarks>
    /// Example usage:
    /// <code>
    /// builder.Configuration.AddSystemsManager(config =>
    /// {
    ///     config.Path = "/my-app/";
    ///     config.ReloadAfter = TimeSpan.FromMinutes(5);
    /// });
    /// </code>
    /// Requires: Amazon.Extensions.Configuration.SystemsManager package
    /// </remarks>
    public static void AddAwsSecretsManagerPlaceholder(this object builder, string region)
    {
        throw new NotImplementedException(
            "Install Amazon.Extensions.Configuration.SystemsManager and use AddSystemsManager extension method.");
    }
}
