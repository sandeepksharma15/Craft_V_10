using System.Net;

namespace Craft.Domain.Tests.Infrastructure;

public class ConfigurationExceptionTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaultMessageAndStatusCode()
    {
        // Arrange & Act
        var ex = new ConfigurationException();

        // Assert
        Assert.Equal("A configuration error occurred", ex.Message);
        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessage_SetsMessageAndStatusCode()
    {
        // Arrange & Act
        var ex = new ConfigurationException("Missing required configuration");

        // Assert
        Assert.Equal("Missing required configuration", ex.Message);
        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsProperties()
    {
        // Arrange
        var inner = new Exception("Configuration file not found");

        // Act
        var ex = new ConfigurationException("Failed to load configuration", inner);

        // Assert
        Assert.Equal("Failed to load configuration", ex.Message);
        Assert.Equal(inner, ex.InnerException);
        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithMessageAndErrors_SetsAllProperties()
    {
        // Arrange
        var errors = new List<string>
        {
            "ConnectionString is missing",
            "ApiKey is invalid",
            "Timeout value must be positive"
        };

        // Act
        var ex = new ConfigurationException("Configuration validation failed", errors);

        // Assert
        Assert.Equal("Configuration validation failed", ex.Message);
        Assert.Equal(errors, ex.Errors);
        Assert.Equal(3, ex.Errors.Count);
        Assert.Contains("ConnectionString is missing", ex.Errors);
        Assert.Contains("ApiKey is invalid", ex.Errors);
        Assert.Contains("Timeout value must be positive", ex.Errors);
        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithConfigurationKeyAndReason_SetsFormattedMessage()
    {
        // Arrange & Act
        var ex = new ConfigurationException("Database:ConnectionString", "Value is missing or empty");

        // Assert
        Assert.Equal("Configuration error for key \"Database:ConnectionString\": Value is missing or empty", ex.Message);
        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithConfigurationKeyAndReason_HandlesComplexKeys()
    {
        // Arrange & Act
        var ex = new ConfigurationException("Azure:Storage:BlobContainer:Name", "Container name contains invalid characters");

        // Assert
        Assert.Contains("Azure:Storage:BlobContainer:Name", ex.Message);
        Assert.Contains("Container name contains invalid characters", ex.Message);
        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithNullErrors_InitializesEmptyErrorsList()
    {
        // Arrange & Act
        var ex = new ConfigurationException("Configuration error", (List<string>?)null);

        // Assert
        Assert.NotNull(ex.Errors);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithEmptyErrors_InitializesEmptyErrorsList()
    {
        // Arrange
        var errors = new List<string>();

        // Act
        var ex = new ConfigurationException("Configuration error", errors);

        // Assert
        Assert.NotNull(ex.Errors);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void StatusCode_IsAlwaysInternalServerError()
    {
        // Arrange & Act
        var ex1 = new ConfigurationException();
        var ex2 = new ConfigurationException("message");
        var ex3 = new ConfigurationException("key", "reason");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, ex1.StatusCode);
        Assert.Equal(HttpStatusCode.InternalServerError, ex2.StatusCode);
        Assert.Equal(HttpStatusCode.InternalServerError, ex3.StatusCode);
    }

    [Fact]
    public void Constructor_WithMultipleErrors_PreservesAllErrors()
    {
        // Arrange
        var errors = new List<string>
        {
            "AppSettings:ApiUrl is not a valid URL",
            "AppSettings:Timeout must be between 1 and 300 seconds",
            "AppSettings:MaxRetries must be a positive integer",
            "AppSettings:EnableFeatureX is not a valid boolean"
        };

        // Act
        var ex = new ConfigurationException("Multiple configuration errors detected", errors);

        // Assert
        Assert.Equal(4, ex.Errors.Count);
        Assert.All(errors, error => Assert.Contains(error, ex.Errors));
    }

    [Fact]
    public void Constructor_InheritsFromCraftException()
    {
        // Arrange & Act
        var ex = new ConfigurationException();

        // Assert
        Assert.IsType<CraftException>(ex, exactMatch: false);
        Assert.IsType<Exception>(ex, exactMatch: false);
    }

    [Fact]
    public void Constructor_WithSpecialCharactersInKey_HandlesCorrectly()
    {
        // Arrange & Act
        var ex = new ConfigurationException("App:Features:Feature-X:Enabled", "Invalid value");

        // Assert
        Assert.Contains("App:Features:Feature-X:Enabled", ex.Message);
        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithLongConfigurationKey_PreservesFullKey()
    {
        // Arrange
        var longKey = "VeryLongConfigurationKeyThatExceedsTypicalLengthForTestingPurposes:SubKey:AnotherSubKey";

        // Act
        var ex = new ConfigurationException(longKey, "Value not found");

        // Assert
        Assert.Contains(longKey, ex.Message);
        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithInnerException_PreservesStackTrace()
    {
        // Arrange
        var inner = new InvalidOperationException("JSON parse error");

        // Act
        var ex = new ConfigurationException("Failed to parse configuration file", inner);

        // Assert
        Assert.Equal(inner, ex.InnerException);
        Assert.Equal("Failed to parse configuration file", ex.Message);
    }
}
