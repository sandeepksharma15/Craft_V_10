using Craft.Configuration.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace Craft.Configuration.Tests;

public class ConfigurationValidatorTests
{
    [Fact]
    public void Validate_WithValidConfiguration_ReturnsSuccess()
    {
        // Arrange
        var services = new ServiceCollection();
        services.Configure<ValidTestOptions>(options =>
        {
            options.Name = "TestApp";
            options.MaxRetries = 3;
        });

        var sp = services.BuildServiceProvider();
        var validator = new ConfigurationValidator(sp);

        // Act
        var result = validator.Validate<ValidTestOptions>();

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WithInvalidConfiguration_ReturnsFailure()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOptions<ValidTestOptions>()
            .Configure(options =>
            {
                options.Name = string.Empty;
                options.MaxRetries = -1;
            })
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var sp = services.BuildServiceProvider();
        var validator = new ConfigurationValidator(sp);

        // Act & Assert - The validator itself won't throw, but accessing the options will
        var result = validator.Validate<ValidTestOptions>();
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithNonRegisteredOptions_ReturnsFailure()
    {
        // Arrange
        var services = new ServiceCollection();
        var sp = services.BuildServiceProvider();
        var validator = new ConfigurationValidator(sp);

        // Act
        var result = validator.Validate<ValidTestOptions>();

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("not registered", result.Errors.First());
    }

    [Fact]
    public void Validate_WithMultipleValidationErrors_ReturnsAllErrors()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOptions<ValidTestOptions>()
            .Configure(options =>
            {
                options.Name = string.Empty;
                options.MaxRetries = -1;
            })
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var sp = services.BuildServiceProvider();

        // Act & Assert
        var exception = Assert.Throws<OptionsValidationException>(() =>
            sp.GetRequiredService<IOptions<ValidTestOptions>>().Value);

        Assert.Contains("Name", exception.Message);
        Assert.Contains("MaxRetries", exception.Message);
    }

    [Fact]
    public void ValidateAll_WithMultipleOptionsTypes_ValidatesAll()
    {
        // Arrange
        var services = new ServiceCollection();
        services.Configure<ValidTestOptions>(options =>
        {
            options.Name = "TestApp";
            options.MaxRetries = 3;
        });
        services.Configure<AnotherValidOptions>(options =>
        {
            options.Url = "https://example.com";
        });

        var sp = services.BuildServiceProvider();
        var validator = new ConfigurationValidator(sp);

        // Act
        var result = validator.Validate();

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Constructor_WithNullServiceProvider_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ConfigurationValidator(null!));
    }

    private class ValidTestOptions
    {
        [Required]
        [MinLength(1)]
        public string Name { get; set; } = string.Empty;

        [Range(0, 10)]
        public int MaxRetries { get; set; }
    }

    private class AnotherValidOptions
    {
        [Required]
        [Url]
        public string Url { get; set; } = string.Empty;
    }
}
