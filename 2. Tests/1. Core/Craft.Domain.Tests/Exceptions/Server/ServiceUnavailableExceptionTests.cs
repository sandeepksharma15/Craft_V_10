using System.Net;

namespace Craft.Exceptions.Tests.Server;

public class ServiceUnavailableExceptionTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaultMessageAndStatusCode()
    {
        // Arrange & Act
        var ex = new ServiceUnavailableException();

        // Assert
        Assert.Equal("The service is temporarily unavailable", ex.Message);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessage_SetsMessageAndStatusCode()
    {
        // Arrange & Act
        var ex = new ServiceUnavailableException("Service is down for maintenance");

        // Assert
        Assert.Equal("Service is down for maintenance", ex.Message);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsProperties()
    {
        // Arrange
        var inner = new Exception("inner exception");

        // Act
        var ex = new ServiceUnavailableException("service unavailable", inner);

        // Assert
        Assert.Equal("service unavailable", ex.Message);
        Assert.Equal(inner, ex.InnerException);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithMessageAndErrors_SetsAllProperties()
    {
        // Arrange
        var errors = new List<string> { "error1", "error2" };

        // Act
        var ex = new ServiceUnavailableException("unavailable", errors);

        // Assert
        Assert.Equal("unavailable", ex.Message);
        Assert.Equal(errors, ex.Errors);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithNullErrors_InitializesEmptyErrorsList()
    {
        // Arrange & Act
        var ex = new ServiceUnavailableException("unavailable", (List<string>?)null);

        // Assert
        Assert.NotNull(ex.Errors);
        Assert.Empty(ex.Errors);
    }
}
