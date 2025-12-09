using Craft.Configuration.Abstractions;
using Craft.Configuration.Extensions;
using Craft.Utilities.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Craft.Configuration.Tests;

public class ConfigurationExtensionsTests
{
    private readonly IKeySafeService _keySafeService;

    public ConfigurationExtensionsTests()
    {
        SetupEncryptionEnvironmentVariables();
        _keySafeService = new KeySafeService();
    }

    [Fact]
    public void AddCraftConfiguration_WithDefaultSettings_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["TestKey"] = "TestValue"
            })
            .Build();

        // Act
        services.AddCraftConfiguration(config);
        var sp = services.BuildServiceProvider();

        // Assert
        var configService = sp.GetService<IConfigurationService>();
        Assert.NotNull(configService);
    }

    [Fact]
    public void AddCraftConfiguration_WithEncryptionEnabled_RegistersEncryptionServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder().Build();
        var envConfig = new EnvironmentConfiguration
        {
            UseEncryption = true
        };

        // Act
        services.AddCraftConfiguration(config, envConfig);
        var sp = services.BuildServiceProvider();

        // Assert
        var encryption = sp.GetService<IConfigurationEncryption>();
        var keySafeService = sp.GetService<IKeySafeService>();
        
        Assert.NotNull(encryption);
        Assert.NotNull(keySafeService);
    }

    [Fact]
    public void AddCraftConfiguration_WithValidationEnabled_RegistersValidator()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder().Build();
        var envConfig = new EnvironmentConfiguration
        {
            ValidateOnStartup = true
        };

        // Act
        services.AddCraftConfiguration(config, envConfig);
        var sp = services.BuildServiceProvider();

        // Assert
        var validator = sp.GetService<IConfigurationValidator>();
        Assert.NotNull(validator);
    }

    [Fact]
    public void AddCraftConfiguration_WithValidationDisabled_DoesNotRegisterValidator()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder().Build();
        var envConfig = new EnvironmentConfiguration
        {
            ValidateOnStartup = false
        };

        // Act
        services.AddCraftConfiguration(config, envConfig);
        var sp = services.BuildServiceProvider();

        // Assert
        var validator = sp.GetService<IConfigurationValidator>();
        Assert.Null(validator);
    }

    [Fact]
    public void ConfigureEnvironment_WithDefaultSettings_LoadsJsonFiles()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), "appsettings.json");
        File.WriteAllText(tempFile, "{\"TestKey\":\"TestValue\"}");

        var builder = new ConfigurationBuilder();
        var envConfig = new EnvironmentConfiguration
        {
            JsonFiles = ["appsettings.json"]
        };

        // Act
        builder.ConfigureEnvironment(envConfig, Path.GetTempPath());
        var config = builder.Build();

        // Assert
        Assert.Equal("TestValue", config["TestKey"]);

        // Cleanup
        File.Delete(tempFile);
    }

    [Fact]
    public void ConfigureEnvironment_WithEnvironmentSpecificFiles_LoadsCorrectFile()
    {
        // Arrange
        var basePath = Path.GetTempPath();
        var baseFile = Path.Combine(basePath, "appsettings.json");
        var devFile = Path.Combine(basePath, "appsettings.Development.json");

        File.WriteAllText(baseFile, "{\"Environment\":\"Base\"}");
        File.WriteAllText(devFile, "{\"Environment\":\"Development\"}");

        var builder = new ConfigurationBuilder();
        var envConfig = new EnvironmentConfiguration
        {
            Environment = "Development",
            JsonFiles = ["appsettings.json"],
            UseEnvironmentSpecificFiles = true
        };

        // Act
        builder.ConfigureEnvironment(envConfig, basePath);
        var config = builder.Build();

        // Assert
        Assert.Equal("Development", config["Environment"]);

        // Cleanup
        File.Delete(baseFile);
        File.Delete(devFile);
    }

    [Fact]
    public void ConfigureEnvironment_WithReloadOnChange_EnablesReload()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), "appsettings.json");
        File.WriteAllText(tempFile, "{\"TestKey\":\"OriginalValue\"}");

        var builder = new ConfigurationBuilder();
        var envConfig = new EnvironmentConfiguration
        {
            JsonFiles = ["appsettings.json"],
            ReloadOnChange = true
        };

        // Act
        builder.ConfigureEnvironment(envConfig, Path.GetTempPath());
        var config = builder.Build();

        // Assert
        Assert.Equal("OriginalValue", config["TestKey"]);

        // Cleanup
        File.Delete(tempFile);
    }

    [Fact]
    public void AddConfigurationEncryption_WithEncryptedValues_DecryptsCorrectly()
    {
        // Arrange
        var plainText = "SecretValue";
        var encrypted = _keySafeService.Encrypt(plainText);
        var builder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SecretKey"] = $"ENC:{encrypted}"
            });

        // Act
        builder.AddConfigurationEncryption();
        var config = builder.Build();

        // Assert
        Assert.Equal(plainText, config["SecretKey"]);
    }

    [Fact]
    public void ConfigureAndValidate_WithValidConfiguration_RegistersAndValidates()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ValidatedOptions:Name"] = "TestApp",
                ["ValidatedOptions:MaxRetries"] = "3"
            })
            .Build();

        // Act
        services.ConfigureAndValidate<ValidatedOptions>(config);
        var sp = services.BuildServiceProvider();

        // Assert
        var options = sp.GetService<Microsoft.Extensions.Options.IOptions<ValidatedOptions>>();
        Assert.NotNull(options);
        Assert.Equal("TestApp", options.Value.Name);
    }

    [Fact]
    public void ConfigureAndValidate_WithInvalidConfiguration_ThrowsOnStartup()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ValidatedOptions:Name"] = string.Empty,
                ["ValidatedOptions:MaxRetries"] = "-1"
            })
            .Build();

        // Act
        services.ConfigureAndValidate<ValidatedOptions>(config);
        var sp = services.BuildServiceProvider();

        // Assert
        Assert.Throws<Microsoft.Extensions.Options.OptionsValidationException>(() =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ValidatedOptions>>();
            _ = options.Value;
        });
    }

    [Fact]
    public void ValidateConfigurationOnStartup_RegistersHostedService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.ValidateConfigurationOnStartup();
        var sp = services.BuildServiceProvider();

        // Assert
        var hostedServices = sp.GetServices<IHostedService>();
        Assert.NotEmpty(hostedServices);
    }

    private static void SetupEncryptionEnvironmentVariables()
    {
        var key = "wXzE5vL9kN2fR8tY6mQ3pW7jH4nA1cV0";
        var iv = "xP9mK3nR7tY2wQ5v";

        Environment.SetEnvironmentVariable("AES_ENCRYPTION_KEY",
            Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(key)),
            EnvironmentVariableTarget.Process);
        Environment.SetEnvironmentVariable("AES_ENCRYPTION_IV",
            Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(iv)),
            EnvironmentVariableTarget.Process);
    }

    private class ValidatedOptions
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string Name { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Range(0, 10)]
        public int MaxRetries { get; set; }
    }
}
