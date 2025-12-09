using Craft.Configuration.Abstractions;
using Craft.Configuration.Encryption;
using Craft.Utilities.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Craft.Configuration.Tests;

public class AesConfigurationEncryptorTests
{
    private readonly IKeySafeService _keySafeService;

    public AesConfigurationEncryptorTests()
    {
        SetupEncryptionEnvironmentVariables();
        _keySafeService = new KeySafeService();
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Act
        var encryptor = new AesConfigurationEncryptor(_keySafeService);

        // Assert
        Assert.NotNull(encryptor);
        Assert.Equal("ENC:", encryptor.EncryptionPrefix);
    }

    [Fact]
    public void Constructor_WithCustomPrefix_SetsPrefix()
    {
        // Arrange
        const string customPrefix = "ENCRYPTED:";

        // Act
        var encryptor = new AesConfigurationEncryptor(_keySafeService, customPrefix);

        // Assert
        Assert.Equal(customPrefix, encryptor.EncryptionPrefix);
    }

    [Fact]
    public void Constructor_WithNullKeySafeService_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new AesConfigurationEncryptor(null!));
    }

    [Fact]
    public void Encrypt_WithValidPlainText_ReturnsEncryptedValue()
    {
        // Arrange
        var encryptor = new AesConfigurationEncryptor(_keySafeService);
        const string plainText = "MySecretValue";

        // Act
        var encrypted = encryptor.Encrypt(plainText);

        // Assert
        Assert.NotNull(encrypted);
        Assert.NotEqual(plainText, encrypted);
    }

    [Fact]
    public void Decrypt_WithValidEncryptedText_ReturnsPlainText()
    {
        // Arrange
        var encryptor = new AesConfigurationEncryptor(_keySafeService);
        const string plainText = "MySecretValue";
        var encrypted = encryptor.Encrypt(plainText);

        // Act
        var decrypted = encryptor.Decrypt(encrypted);

        // Assert
        Assert.Equal(plainText, decrypted);
    }

    [Fact]
    public void Encrypt_WithNullPlainText_ThrowsArgumentException()
    {
        // Arrange
        var encryptor = new AesConfigurationEncryptor(_keySafeService);

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => encryptor.Encrypt(null!));
    }

    [Fact]
    public void Encrypt_WithEmptyPlainText_ThrowsArgumentException()
    {
        // Arrange
        var encryptor = new AesConfigurationEncryptor(_keySafeService);

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => encryptor.Encrypt(string.Empty));
    }

    [Fact]
    public void Decrypt_WithNullEncryptedText_ThrowsArgumentException()
    {
        // Arrange
        var encryptor = new AesConfigurationEncryptor(_keySafeService);

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => encryptor.Decrypt(null!));
    }

    [Fact]
    public void Decrypt_WithInvalidEncryptedText_ThrowsInvalidOperationException()
    {
        // Arrange
        var encryptor = new AesConfigurationEncryptor(_keySafeService);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => encryptor.Decrypt("InvalidBase64=="));
    }

    [Fact]
    public void IsEncrypted_WithEncryptedValue_ReturnsTrue()
    {
        // Arrange
        var encryptor = new AesConfigurationEncryptor(_keySafeService);
        const string value = "ENC:someencryptedvalue";

        // Act
        var result = encryptor.IsEncrypted(value);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsEncrypted_WithPlainValue_ReturnsFalse()
    {
        // Arrange
        var encryptor = new AesConfigurationEncryptor(_keySafeService);
        const string value = "plainvalue";

        // Act
        var result = encryptor.IsEncrypted(value);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsEncrypted_WithNullValue_ReturnsFalse()
    {
        // Arrange
        var encryptor = new AesConfigurationEncryptor(_keySafeService);

        // Act
        var result = encryptor.IsEncrypted(null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsEncrypted_WithCustomPrefix_DetectsCorrectly()
    {
        // Arrange
        const string customPrefix = "ENCRYPTED:";
        var encryptor = new AesConfigurationEncryptor(_keySafeService, customPrefix);

        // Act
        var resultWithPrefix = encryptor.IsEncrypted("ENCRYPTED:value");
        var resultWithoutPrefix = encryptor.IsEncrypted("ENC:value");

        // Assert
        Assert.True(resultWithPrefix);
        Assert.False(resultWithoutPrefix);
    }

    [Fact]
    public void Encrypt_Decrypt_RoundTrip_PreservesValue()
    {
        // Arrange
        var encryptor = new AesConfigurationEncryptor(_keySafeService);
        const string originalValue = "This is a test value with special chars: !@#$%^&*()";

        // Act
        var encrypted = encryptor.Encrypt(originalValue);
        var decrypted = encryptor.Decrypt(encrypted);

        // Assert
        Assert.Equal(originalValue, decrypted);
    }

    [Fact]
    public void Constructor_WithLogger_LogsDebugMessages()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<AesConfigurationEncryptor>>();
        var encryptor = new AesConfigurationEncryptor(_keySafeService, "ENC:", mockLogger.Object);
        const string plainText = "TestValue";

        // Act
        var encrypted = encryptor.Encrypt(plainText);
        var decrypted = encryptor.Decrypt(encrypted);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("encrypted")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
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
}
