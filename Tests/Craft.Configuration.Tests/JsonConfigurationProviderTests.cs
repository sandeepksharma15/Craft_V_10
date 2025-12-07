using Craft.Configuration.Abstractions;
using Craft.Configuration.Providers;
using Craft.Utilities.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace Craft.Configuration.Tests;

public class JsonConfigurationProviderTests
{
    private readonly IKeySafeService _keySafeService;

    public JsonConfigurationProviderTests()
    {
        SetupEncryptionEnvironmentVariables();
        _keySafeService = new KeySafeService();
    }

    [Fact]
    public void Constructor_WithValidConfiguration_CreatesInstance()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();

        // Act
        var provider = new JsonConfigurationProvider(config);

        // Assert
        Assert.NotNull(provider);
    }

    [Fact]
    public void Constructor_WithNullConfiguration_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new JsonConfigurationProvider(null!));
    }

    [Fact]
    public void Get_WithExistingKey_ReturnsValue()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["TestKey"] = "TestValue"
            })
            .Build();
        var provider = new JsonConfigurationProvider(config);

        // Act
        var value = provider.Get("TestKey");

        // Assert
        Assert.Equal("TestValue", value);
    }

    [Fact]
    public void Get_WithNonExistingKey_ReturnsNull()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();
        var provider = new JsonConfigurationProvider(config);

        // Act
        var value = provider.Get("NonExistingKey");

        // Assert
        Assert.Null(value);
    }

    [Fact]
    public void Get_WithEncryptedValue_ReturnsDecryptedValue()
    {
        // Arrange
        var plainText = "SecretValue";
        var encrypted = _keySafeService.Encrypt(plainText);
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SecretKey"] = $"ENC:{encrypted}"
            })
            .Build();
        var encryption = new Encryption.AesConfigurationEncryptor(_keySafeService);
        var provider = new JsonConfigurationProvider(config, encryption);

        // Act
        var value = provider.Get("SecretKey");

        // Assert
        Assert.Equal(plainText, value);
    }

    [Fact]
    public void GetGeneric_WithStringType_ReturnsValue()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["StringKey"] = "StringValue"
            })
            .Build();
        var provider = new JsonConfigurationProvider(config);

        // Act
        var value = provider.Get<string>("StringKey");

        // Assert
        Assert.Equal("StringValue", value);
    }

    [Fact]
    public void GetGeneric_WithIntType_ReturnsConvertedValue()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["IntKey"] = "42"
            })
            .Build();
        var provider = new JsonConfigurationProvider(config);

        // Act
        var value = provider.Get<int>("IntKey");

        // Assert
        Assert.Equal(42, value);
    }

    [Fact]
    public void GetGeneric_WithBoolType_ReturnsConvertedValue()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["BoolKey"] = "true"
            })
            .Build();
        var provider = new JsonConfigurationProvider(config);

        // Act
        var value = provider.Get<bool>("BoolKey");

        // Assert
        Assert.True(value);
    }

    [Fact]
    public void GetSection_WithValidSection_ReturnsBindedObject()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["TestOptions:Name"] = "TestApp",
                ["TestOptions:MaxRetries"] = "3"
            })
            .Build();
        var provider = new JsonConfigurationProvider(config);

        // Act
        var options = provider.GetSection<TestOptions>("TestOptions");

        // Assert
        Assert.NotNull(options);
        Assert.Equal("TestApp", options.Name);
        Assert.Equal(3, options.MaxRetries);
    }

    [Fact]
    public void GetSection_WithNonExistingSection_ReturnsNull()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();
        var provider = new JsonConfigurationProvider(config);

        // Act
        var options = provider.GetSection<TestOptions>("NonExistingSection");

        // Assert
        Assert.Null(options);
    }

    [Fact]
    public void GetSection_WithEncryptedProperty_DecryptsProperty()
    {
        // Arrange
        var plainPassword = "MyPassword123";
        var encrypted = _keySafeService.Encrypt(plainPassword);
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["TestOptions:Name"] = "TestApp",
                ["TestOptions:Password"] = $"ENC:{encrypted}"
            })
            .Build();
        var encryption = new Encryption.AesConfigurationEncryptor(_keySafeService);
        var provider = new JsonConfigurationProvider(config, encryption);

        // Act
        var options = provider.GetSection<TestOptions>("TestOptions");

        // Assert
        Assert.NotNull(options);
        Assert.Equal("TestApp", options.Name);
        Assert.Equal(plainPassword, options.Password);
    }

    [Fact]
    public void Set_WithValidKey_SetsValue()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ExistingKey"] = "ExistingValue"
            })
            .Build();
        var provider = new JsonConfigurationProvider(config);

        // Act
        provider.Set("NewKey", "NewValue");
        var value = provider.Get("NewKey");

        // Assert
        Assert.Equal("NewValue", value);
    }

    [Fact]
    public void Exists_WithExistingKey_ReturnsTrue()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["TestKey"] = "TestValue"
            })
            .Build();
        var provider = new JsonConfigurationProvider(config);

        // Act
        var exists = provider.Exists("TestKey");

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public void Exists_WithNonExistingKey_ReturnsFalse()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();
        var provider = new JsonConfigurationProvider(config);

        // Act
        var exists = provider.Exists("NonExistingKey");

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public void GetAllKeys_ReturnsAllConfigurationKeys()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Key1"] = "Value1",
                ["Key2"] = "Value2",
                ["Section:Key3"] = "Value3"
            })
            .Build();
        var provider = new JsonConfigurationProvider(config);

        // Act
        var keys = provider.GetAllKeys().ToList();

        // Assert
        Assert.Contains("Key1", keys);
        Assert.Contains("Key2", keys);
        Assert.Contains("Section:Key3", keys);
    }

    [Fact]
    public void Reload_WithConfigurationRoot_SuccessfullyReloads()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "{\"TestKey\":\"OriginalValue\"}");

        var config = new ConfigurationBuilder()
            .AddJsonFile(tempFile, optional: false, reloadOnChange: false)
            .Build();
        var provider = new JsonConfigurationProvider(config);

        // Act - Read original value
        var originalValue = provider.Get("TestKey");

        // Update file
        File.WriteAllText(tempFile, "{\"TestKey\":\"UpdatedValue\"}");

        // Reload configuration
        provider.Reload();
        var updatedValue = provider.Get("TestKey");

        // Assert
        Assert.Equal("OriginalValue", originalValue);
        Assert.Equal("UpdatedValue", updatedValue);

        // Cleanup
        File.Delete(tempFile);
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

    private class TestOptions
    {
        public string Name { get; set; } = string.Empty;
        public string? Password { get; set; }
        public int MaxRetries { get; set; }
    }
}
