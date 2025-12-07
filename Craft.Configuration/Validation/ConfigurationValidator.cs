using Craft.Configuration.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace Craft.Configuration.Validation;

/// <summary>
/// Validates configuration options using data annotations and custom validation logic.
/// </summary>
public class ConfigurationValidator : IConfigurationValidator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ConfigurationValidator> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationValidator"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider for resolving options.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public ConfigurationValidator(
        IServiceProvider serviceProvider,
        ILogger<ConfigurationValidator>? logger = null)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? NullLogger<ConfigurationValidator>.Instance;
    }

    /// <inheritdoc/>
    public ConfigurationValidationResult Validate()
    {
        _logger.LogInformation("Starting configuration validation");

        var errors = new List<string>();

        try
        {
            var optionsMonitors = _serviceProvider.GetServices<IOptionsMonitor<object>>();
            
            foreach (var monitor in optionsMonitors)
            {
                try
                {
                    var value = monitor.CurrentValue;
                    var validationResults = ValidateObject(value);
                    
                    if (validationResults.Any())
                    {
                        var typeName = value.GetType().Name;
                        errors.AddRange(validationResults.Select(r => 
                            $"{typeName}: {r.ErrorMessage}"));
                    }
                }
                catch (OptionsValidationException ex)
                {
                    errors.Add(ex.Message);
                    _logger.LogError(ex, "Options validation failed");
                }
            }

            if (errors.Count > 0)
            {
                _logger.LogWarning("Configuration validation completed with {ErrorCount} errors", errors.Count);
                return ConfigurationValidationResult.Failure(errors);
            }

            _logger.LogInformation("Configuration validation completed successfully");
            return ConfigurationValidationResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Configuration validation failed with exception");
            errors.Add($"Validation exception: {ex.Message}");
            return ConfigurationValidationResult.Failure(errors);
        }
    }

    /// <inheritdoc/>
    public ConfigurationValidationResult Validate<T>() where T : class
    {
        _logger.LogInformation("Validating configuration section: {Type}", typeof(T).Name);

        try
        {
            var options = _serviceProvider.GetService<IOptions<T>>();
            
            if (options == null)
            {
                var error = $"Configuration section {typeof(T).Name} is not registered";
                _logger.LogWarning(error);
                return ConfigurationValidationResult.Failure(error);
            }

            var value = options.Value;
            var validationResults = ValidateObject(value);

            if (validationResults.Any())
            {
                var errors = validationResults.Select(r => r.ErrorMessage ?? "Unknown error").ToList();
                _logger.LogWarning("Configuration section {Type} validation failed with {ErrorCount} errors", 
                    typeof(T).Name, errors.Count);
                return ConfigurationValidationResult.Failure(errors);
            }

            _logger.LogInformation("Configuration section {Type} validated successfully", typeof(T).Name);
            return ConfigurationValidationResult.Success();
        }
        catch (OptionsValidationException ex)
        {
            _logger.LogError(ex, "Configuration section {Type} validation failed", typeof(T).Name);
            return ConfigurationValidationResult.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Configuration section {Type} validation failed with exception", typeof(T).Name);
            return ConfigurationValidationResult.Failure($"Validation exception: {ex.Message}");
        }
    }

    private static List<ValidationResult> ValidateObject(object obj)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(obj);
        
        Validator.TryValidateObject(obj, validationContext, validationResults, validateAllProperties: true);
        
        return validationResults;
    }
}
