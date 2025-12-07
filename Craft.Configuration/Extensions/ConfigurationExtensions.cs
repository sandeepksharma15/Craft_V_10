using Craft.Configuration.Abstractions;
using Craft.Configuration.Encryption;
using Craft.Configuration.Providers;
using Craft.Configuration.Validation;
using Craft.Utilities.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Craft.Configuration.Extensions;

/// <summary>
/// Extension methods for configuring Craft.Configuration services.
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Adds Craft configuration services with default settings.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration root.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCraftConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services.AddCraftConfiguration(configuration, null);
    }

    /// <summary>
    /// Adds Craft configuration services with custom environment configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration root.</param>
    /// <param name="environmentConfig">Optional environment-specific configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCraftConfiguration(
        this IServiceCollection services,
        IConfiguration configuration,
        EnvironmentConfiguration? environmentConfig)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        environmentConfig ??= new EnvironmentConfiguration();

        services.AddSingleton(environmentConfig);
        services.AddSingleton(configuration);

        if (environmentConfig.UseEncryption)
        {
            services.AddSingleton<IKeySafeService, KeySafeService>();
            services.AddSingleton<IConfigurationEncryption>(sp =>
            {
                var keySafeService = sp.GetRequiredService<IKeySafeService>();
                var logger = sp.GetService<ILogger<AesConfigurationEncryptor>>();
                
                return new AesConfigurationEncryptor(
                    keySafeService,
                    environmentConfig.EncryptionPrefix,
                    logger);
            });
        }

        services.AddSingleton<IConfigurationService>(sp =>
        {
            var encryption = environmentConfig.UseEncryption 
                ? sp.GetService<IConfigurationEncryption>() 
                : null;
            var logger = sp.GetService<ILogger<JsonConfigurationProvider>>();
            
            return new JsonConfigurationProvider(configuration, encryption, logger);
        });

        if (environmentConfig.ValidateOnStartup)
        {
            services.AddSingleton<IConfigurationValidator, ConfigurationValidator>();
        }

        return services;
    }

    /// <summary>
    /// Adds Craft configuration with a custom configuration builder action.
    /// </summary>
    /// <param name="builder">The host builder.</param>
    /// <param name="configureAction">Action to configure the configuration builder.</param>
    /// <returns>The host builder for chaining.</returns>
    public static IHostBuilder ConfigureCraftConfiguration(
        this IHostBuilder builder,
        Action<HostBuilderContext, IConfigurationBuilder> configureAction)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configureAction);

        return builder.ConfigureAppConfiguration((context, config) =>
        {
            configureAction(context, config);
        });
    }

    /// <summary>
    /// Configures multi-environment support with automatic environment detection.
    /// </summary>
    /// <param name="builder">The configuration builder.</param>
    /// <param name="environmentConfig">The environment configuration.</param>
    /// <param name="basePath">Optional base path for configuration files.</param>
    /// <returns>The configuration builder for chaining.</returns>
    public static IConfigurationBuilder ConfigureEnvironment(
        this IConfigurationBuilder builder,
        EnvironmentConfiguration environmentConfig,
        string? basePath = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(environmentConfig);

        if (!string.IsNullOrWhiteSpace(basePath))
            builder.SetBasePath(basePath);

        foreach (var jsonFile in environmentConfig.JsonFiles)
        {
            builder.AddJsonFile(
                jsonFile,
                optional: true,
                reloadOnChange: environmentConfig.ReloadOnChange);

            if (environmentConfig.UseEnvironmentSpecificFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(jsonFile);
                var extension = Path.GetExtension(jsonFile);
                var environmentFile = $"{fileName}.{environmentConfig.Environment}{extension}";

                builder.AddJsonFile(
                    environmentFile,
                    optional: true,
                    reloadOnChange: environmentConfig.ReloadOnChange);
            }
        }

        builder.AddEnvironmentVariables();

        if (environmentConfig.UseUserSecrets && 
            environmentConfig.Environment.Equals("Development", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(environmentConfig.UserSecretsId))
        {
            builder.AddUserSecrets(environmentConfig.UserSecretsId);
        }

        return builder;
    }

    /// <summary>
    /// Adds configuration encryption support to the configuration builder.
    /// </summary>
    /// <param name="builder">The configuration builder.</param>
    /// <param name="encryptionPrefix">The encryption prefix.</param>
    /// <returns>The configuration builder for chaining.</returns>
    public static IConfigurationBuilder AddConfigurationEncryption(
        this IConfigurationBuilder builder,
        string encryptionPrefix = "ENC:")
    {
        ArgumentNullException.ThrowIfNull(builder);

        var keySafeService = new KeySafeService();
        var sources = builder.Sources.ToList();
        builder.Sources.Clear();

        foreach (var source in sources)
        {
            var decryptedSource = new DecryptedConfigurationSource(
                source,
                keySafeService,
                encryptionPrefix);
            builder.Add(decryptedSource);
        }

        return builder;
    }

    /// <summary>
    /// Validates configuration on application startup.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection ValidateConfigurationOnStartup(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHostedService<ConfigurationValidationHostedService>();

        return services;
    }

    /// <summary>
    /// Registers and validates a configuration section with data annotations.
    /// </summary>
    /// <typeparam name="TOptions">The options type to configure.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="sectionName">The configuration section name.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection ConfigureAndValidate<TOptions>(
        this IServiceCollection services,
        IConfiguration configuration,
        string? sectionName = null)
        where TOptions : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        sectionName ??= typeof(TOptions).Name;

        services.AddOptions<TOptions>()
            .Bind(configuration.GetSection(sectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}

/// <summary>
/// Hosted service that validates configuration on application startup.
/// </summary>
internal class ConfigurationValidationHostedService : IHostedService
{
    private readonly IConfigurationValidator? _validator;
    private readonly ILogger<ConfigurationValidationHostedService> _logger;

    public ConfigurationValidationHostedService(
        IServiceProvider serviceProvider,
        ILogger<ConfigurationValidationHostedService> logger)
    {
        _validator = serviceProvider.GetService<IConfigurationValidator>();
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_validator == null)
        {
            _logger.LogWarning("Configuration validator not registered, skipping validation");
            return Task.CompletedTask;
        }

        _logger.LogInformation("Validating configuration on startup");

        var result = _validator.Validate();

        if (!result.IsValid)
        {
            var errorMessage = string.Join(Environment.NewLine, result.Errors);
            _logger.LogError("Configuration validation failed:{NewLine}{Errors}", 
                Environment.NewLine, errorMessage);

            throw new InvalidOperationException(
                $"Configuration validation failed:{Environment.NewLine}{errorMessage}");
        }

        _logger.LogInformation("Configuration validation completed successfully");
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
