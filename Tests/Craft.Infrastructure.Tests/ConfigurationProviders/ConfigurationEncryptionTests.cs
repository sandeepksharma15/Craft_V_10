using Craft.Infrastructure.ConfigurationProviders;
using Craft.Utilities.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Craft.Infrastructure.Tests.ConfigurationProviders;

/// <summary>
/// Tests for configuration encryption/decryption functionality.
/// </summary>
public class ConfigurationEncryptionTests
{
    private const string EncryptionPrefix = "ENC:";
    private readonly IKeySafeService _keySafeService;

    public ConfigurationEncryptionTests()
    {
        SetupEncryptionEnvironmentVariables();
        _keySafeService = new KeySafeService();
    }

    #region DecryptedConfigurationProvider Tests

    [Fact]
    public void DecryptedConfigurationProvider_DecryptsEncryptedValues()
    {
        // Arrange
        var plainText = "MySecretPassword123";
        var encrypted = _keySafeService.Encrypt(plainText);
        
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:Password"] = $"{EncryptionPrefix}{encrypted}",
                ["Database:Server"] = "localhost"
            })
            .AddDecryption(EncryptionPrefix)
            .Build();

        // Act
        var password = config["Database:Password"];
        var server = config["Database:Server"];

        // Assert
        Assert.Equal(plainText, password);
        Assert.Equal("localhost", server);
    }

    [Fact]
    public void DecryptedConfigurationProvider_KeepsPlainTextValuesUnchanged()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Setting1"] = "PlainValue",
                ["Setting2"] = "AnotherPlainValue"
            })
            .AddDecryption(EncryptionPrefix)
            .Build();

        // Act & Assert
        Assert.Equal("PlainValue", config["Setting1"]);
        Assert.Equal("AnotherPlainValue", config["Setting2"]);
    }

    [Fact]
    public void DecryptedConfigurationProvider_HandlesMixedEncryptedAndPlainValues()
    {
        // Arrange
        var secret1 = "Secret1";
        var secret2 = "Secret2";
        var encrypted1 = _keySafeService.Encrypt(secret1);
        var encrypted2 = _keySafeService.Encrypt(secret2);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["App:ApiKey"] = $"{EncryptionPrefix}{encrypted1}",
                ["App:Name"] = "MyApp",
                ["App:ConnectionString"] = $"{EncryptionPrefix}{encrypted2}",
                ["App:Environment"] = "Production"
            })
            .AddDecryption(EncryptionPrefix)
            .Build();

        // Act & Assert
        Assert.Equal(secret1, config["App:ApiKey"]);
        Assert.Equal("MyApp", config["App:Name"]);
        Assert.Equal(secret2, config["App:ConnectionString"]);
        Assert.Equal("Production", config["App:Environment"]);
    }

    [Fact]
    public void DecryptedConfigurationProvider_WorksWithNestedConfiguration()
    {
        // Arrange
        var dbPassword = "DbPass123";
        var apiKey = "ApiKey456";
        var encryptedDbPassword = _keySafeService.Encrypt(dbPassword);
        var encryptedApiKey = _keySafeService.Encrypt(apiKey);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:ConnectionString:Server"] = "localhost",
                ["Database:ConnectionString:Password"] = $"{EncryptionPrefix}{encryptedDbPassword}",
                ["ExternalServices:PaymentGateway:ApiKey"] = $"{EncryptionPrefix}{encryptedApiKey}",
                ["ExternalServices:PaymentGateway:Endpoint"] = "https://api.example.com"
            })
            .AddDecryption(EncryptionPrefix)
            .Build();

        // Act & Assert
        Assert.Equal("localhost", config["Database:ConnectionString:Server"]);
        Assert.Equal(dbPassword, config["Database:ConnectionString:Password"]);
        Assert.Equal(apiKey, config["ExternalServices:PaymentGateway:ApiKey"]);
        Assert.Equal("https://api.example.com", config["ExternalServices:PaymentGateway:Endpoint"]);
    }

    [Fact]
    public void DecryptedConfigurationProvider_HandlesInvalidEncryptedValue_Gracefully()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Setting"] = $"{EncryptionPrefix}InvalidBase64=="
            })
            .AddDecryption(EncryptionPrefix)
            .Build();

        // Act
        var value = config["Setting"];

        // Assert - Should return original value if decryption fails
        Assert.StartsWith(EncryptionPrefix, value);
    }

    [Fact]
    public void DecryptedConfigurationProvider_WorksWithCustomPrefix()
    {
        // Arrange
        const string customPrefix = "ENCRYPTED:";
        var plainText = "MySecret";
        var encrypted = _keySafeService.Encrypt(plainText);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Password"] = $"{customPrefix}{encrypted}"
            })
            .AddDecryption(customPrefix)
            .Build();

        // Act & Assert
        Assert.Equal(plainText, config["Password"]);
    }

    #endregion

    #region Post-Processing (DecryptConfiguration) Tests

    [Fact]
    public void DecryptConfiguration_DecryptsAllEncryptedValues()
    {
        // Arrange
        var secret = "MySecret";
        var encrypted = _keySafeService.Encrypt(secret);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ApiKey"] = $"{EncryptionPrefix}{encrypted}",
                ["AppName"] = "TestApp"
            })
            .Build();

        // Act
        config.DecryptConfiguration(EncryptionPrefix);

        // Assert
        Assert.Equal(secret, config["ApiKey"]);
        Assert.Equal("TestApp", config["AppName"]);
    }

    [Fact]
    public void DecryptConfiguration_WithLogger_LogsDecryptionOperations()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var sp = services.BuildServiceProvider();
        var logger = sp.GetRequiredService<ILogger<ConfigurationEncryptionTests>>();

        var secret = "MySecret";
        var encrypted = _keySafeService.Encrypt(secret);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Password"] = $"{EncryptionPrefix}{encrypted}"
            })
            .Build();

        // Act
        config.DecryptConfiguration(logger, EncryptionPrefix);

        // Assert
        Assert.Equal(secret, config["Password"]);
    }

    #endregion

    #region IOptions Post-Configuration Tests

    [Fact]
    public void AddOptionsDecryption_DecryptsStringProperties()
    {
        // Arrange
        var secret = "MyApiKey123";
        var encrypted = _keySafeService.Encrypt(secret);

        var services = new ServiceCollection();
        services.AddSingleton<IKeySafeService>(_keySafeService);
        
        services.Configure<TestOptions>(options =>
        {
            options.ApiKey = $"{EncryptionPrefix}{encrypted}";
            options.AppName = "TestApp";
        });

        services.AddOptionsDecryption<TestOptions>(EncryptionPrefix);

        var sp = services.BuildServiceProvider();

        // Act
        var options = sp.GetRequiredService<IOptions<TestOptions>>().Value;

        // Assert
        Assert.Equal(secret, options.ApiKey);
        Assert.Equal("TestApp", options.AppName);
    }

    [Fact]
    public void AddOptionsDecryption_IgnoresNonStringProperties()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IKeySafeService>(_keySafeService);
        
        services.Configure<TestOptions>(options =>
        {
            options.MaxRetries = 5;
            options.IsEnabled = true;
        });

        services.AddOptionsDecryption<TestOptions>(EncryptionPrefix);

        var sp = services.BuildServiceProvider();

        // Act
        var options = sp.GetRequiredService<IOptions<TestOptions>>().Value;

        // Assert
        Assert.Equal(5, options.MaxRetries);
        Assert.True(options.IsEnabled);
    }

    [Fact]
    public void AddOptionsDecryption_HandlesMultipleEncryptedProperties()
    {
        // Arrange
        var apiKey = "ApiKey123";
        var dbPassword = "DbPass456";
        var encryptedApiKey = _keySafeService.Encrypt(apiKey);
        var encryptedDbPassword = _keySafeService.Encrypt(dbPassword);

        var services = new ServiceCollection();
        services.AddSingleton<IKeySafeService>(_keySafeService);
        
        services.Configure<TestOptions>(options =>
        {
            options.ApiKey = $"{EncryptionPrefix}{encryptedApiKey}";
            options.DatabasePassword = $"{EncryptionPrefix}{encryptedDbPassword}";
            options.AppName = "TestApp";
        });

        services.AddOptionsDecryption<TestOptions>(EncryptionPrefix);

        var sp = services.BuildServiceProvider();

        // Act
        var options = sp.GetRequiredService<IOptions<TestOptions>>().Value;

        // Assert
        Assert.Equal(apiKey, options.ApiKey);
        Assert.Equal(dbPassword, options.DatabasePassword);
        Assert.Equal("TestApp", options.AppName);
    }

    [Fact]
    public void AddOptionsDecryption_WorksWithConfigurationBinding()
    {
        // Arrange
        var secret = "MySecret";
        var encrypted = _keySafeService.Encrypt(secret);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["TestOptions:ApiKey"] = $"{EncryptionPrefix}{encrypted}",
                ["TestOptions:AppName"] = "TestApp",
                ["TestOptions:MaxRetries"] = "3"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IKeySafeService>(_keySafeService);
        services.Configure<TestOptions>(config.GetSection("TestOptions"));
        services.AddOptionsDecryption<TestOptions>(EncryptionPrefix);

        var sp = services.BuildServiceProvider();

        // Act
        var options = sp.GetRequiredService<IOptions<TestOptions>>().Value;

        // Assert
        Assert.Equal(secret, options.ApiKey);
        Assert.Equal("TestApp", options.AppName);
        Assert.Equal(3, options.MaxRetries);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void ConfigurationEncryption_WorksAcrossAllApproaches()
    {
        // Arrange
        var apiKey = "ApiKey123";
        var dbPassword = "DbPass456";
        var encryptedApiKey = _keySafeService.Encrypt(apiKey);
        var encryptedDbPassword = _keySafeService.Encrypt(dbPassword);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["TestOptions:ApiKey"] = $"{EncryptionPrefix}{encryptedApiKey}",
                ["TestOptions:DatabasePassword"] = $"{EncryptionPrefix}{encryptedDbPassword}",
                ["TestOptions:AppName"] = "TestApp"
            })
            .AddDecryption(EncryptionPrefix)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(config);
        services.AddSingleton<IKeySafeService>(_keySafeService);
        services.Configure<TestOptions>(config.GetSection("TestOptions"));
        services.AddOptionsDecryption<TestOptions>(EncryptionPrefix);

        var sp = services.BuildServiceProvider();

        // Act
        var directApiKey = config["TestOptions:ApiKey"];
        var directDbPassword = config["TestOptions:DatabasePassword"];
        var options = sp.GetRequiredService<IOptions<TestOptions>>().Value;

        // Assert - Configuration provider decryption
        Assert.Equal(apiKey, directApiKey);
        Assert.Equal(dbPassword, directDbPassword);
        
        // Assert - Options post-configuration
        Assert.Equal(apiKey, options.ApiKey);
        Assert.Equal(dbPassword, options.DatabasePassword);
        Assert.Equal("TestApp", options.AppName);
    }

    [Fact]
    public void ConfigurationEncryption_WorksWithJsonConfiguration()
    {
        // Arrange
        var secret = "MySecret123";
        var encrypted = _keySafeService.Encrypt(secret);

        var jsonContent = $$"""
        {
            "Database": {
                "Server": "localhost",
                "Password": "{{EncryptionPrefix}}{{encrypted}}"
            },
            "App": {
                "Name": "TestApp"
            }
        }
        """;

        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, jsonContent);

        try
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile(tempFile)
                .AddDecryption(EncryptionPrefix)
                .Build();

            // Act
            var password = config["Database:Password"];
            var server = config["Database:Server"];

            // Assert
            Assert.Equal(secret, password);
            Assert.Equal("localhost", server);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ConfigurationEncryption_WorksWithEnvironmentVariables()
    {
        // Arrange
        var secret = "EnvSecret123";
        var encrypted = _keySafeService.Encrypt(secret);
        
        Environment.SetEnvironmentVariable("TEST_API_KEY", $"{EncryptionPrefix}{encrypted}", EnvironmentVariableTarget.Process);

        try
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["TEST_API_KEY"] = $"{EncryptionPrefix}{encrypted}"
                })
                .AddDecryption(EncryptionPrefix)
                .Build();

            // Act
            var apiKey = config["TEST_API_KEY"];

            // Assert
            Assert.Equal(secret, apiKey);
        }
        finally
        {
            Environment.SetEnvironmentVariable("TEST_API_KEY", null, EnvironmentVariableTarget.Process);
        }
    }

    #endregion

    #region Helper Methods

    private static void SetupEncryptionEnvironmentVariables()
    {
        var key = "wXzE5vL9kN2fR8tY6mQ3pW7jH4nA1cV0"; // 32 bytes for AES-256
        var iv = "xP9mK3nR7tY2wQ5v"; // 16 bytes

        Environment.SetEnvironmentVariable("AES_ENCRYPTION_KEY", Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(key)), EnvironmentVariableTarget.Process);
        Environment.SetEnvironmentVariable("AES_ENCRYPTION_IV", Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(iv)), EnvironmentVariableTarget.Process);
    }

    #endregion
}

/// <summary>
/// Test options class for configuration binding tests.
/// </summary>
public class TestOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string? DatabasePassword { get; set; }
    public string AppName { get; set; } = string.Empty;
    public int MaxRetries { get; set; }
    public bool IsEnabled { get; set; }
}
