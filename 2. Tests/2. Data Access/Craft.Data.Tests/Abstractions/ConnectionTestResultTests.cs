namespace Craft.Data.Tests.Abstractions;

/// <summary>
/// Unit tests for ConnectionTestResult class.
/// </summary>
public class ConnectionTestResultTests
{
    [Fact]
    public void ConnectionTestResult_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var result = new ConnectionTestResult();

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal(string.Empty, result.Provider);
        Assert.Null(result.LatencyMs);
        Assert.Null(result.ErrorMessage);
        Assert.Equal(string.Empty, result.Message);
        Assert.Null(result.ServerVersion);
        Assert.Null(result.DatabaseName);
    }

    [Fact]
    public void ConnectionTestResult_ShouldSetAllProperties()
    {
        // Arrange & Act
        var result = new ConnectionTestResult
        {
            IsSuccessful = true,
            Provider = "SqlServerProvider",
            LatencyMs = 123.45,
            ErrorMessage = null,
            Message = "Connection successful",
            ServerVersion = "15.0.2000.5",
            DatabaseName = "TestDatabase"
        };

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Equal("SqlServerProvider", result.Provider);
        Assert.Equal(123.45, result.LatencyMs);
        Assert.Null(result.ErrorMessage);
        Assert.Equal("Connection successful", result.Message);
        Assert.Equal("15.0.2000.5", result.ServerVersion);
        Assert.Equal("TestDatabase", result.DatabaseName);
    }

    [Fact]
    public void ConnectionTestResult_ForFailure_ShouldHaveErrorMessage()
    {
        // Arrange & Act
        var result = new ConnectionTestResult
        {
            IsSuccessful = false,
            Provider = "SqlServerProvider",
            LatencyMs = 50.0,
            ErrorMessage = "Login failed for user 'test'",
            Message = "Connection test failed"
        };

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.NotNull(result.ErrorMessage);
        Assert.Equal("Login failed for user 'test'", result.ErrorMessage);
    }

    [Fact]
    public void ConnectionTestResult_LatencyMs_CanBeNull()
    {
        // Arrange & Act
        var result = new ConnectionTestResult
        {
            IsSuccessful = false,
            Provider = "SqlServerProvider",
            LatencyMs = null,
            ErrorMessage = "Timeout occurred"
        };

        // Assert
        Assert.Null(result.LatencyMs);
    }

    [Fact]
    public void ConnectionTestResult_OptionalProperties_CanBeNull()
    {
        // Arrange & Act
        var result = new ConnectionTestResult
        {
            IsSuccessful = true,
            Provider = "SqlServerProvider",
            ServerVersion = null,
            DatabaseName = null,
            ErrorMessage = null
        };

        // Assert
        Assert.Null(result.ServerVersion);
        Assert.Null(result.DatabaseName);
        Assert.Null(result.ErrorMessage);
    }
}
