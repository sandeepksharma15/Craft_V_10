using System.Net;
using Craft.Exceptions.Server;

namespace Craft.Exceptions.Tests.Server;

public class GatewayTimeoutExceptionTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaultMessageAndStatusCode()
    {
        // Arrange & Act
        var ex = new GatewayTimeoutException();

        // Assert
        Assert.Equal("Gateway timeout - no response from upstream server", ex.Message);
        Assert.Equal(HttpStatusCode.GatewayTimeout, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessage_SetsMessageAndStatusCode()
    {
        // Arrange & Act
        var ex = new GatewayTimeoutException("Upstream server timeout");

        // Assert
        Assert.Equal("Upstream server timeout", ex.Message);
        Assert.Equal(HttpStatusCode.GatewayTimeout, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsProperties()
    {
        // Arrange
        var inner = new Exception("inner exception");

        // Act
        var ex = new GatewayTimeoutException("timeout occurred", inner);

        // Assert
        Assert.Equal("timeout occurred", ex.Message);
        Assert.Equal(inner, ex.InnerException);
        Assert.Equal(HttpStatusCode.GatewayTimeout, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithMessageAndErrors_SetsAllProperties()
    {
        // Arrange
        var errors = new List<string> { "error1", "error2" };

        // Act
        var ex = new GatewayTimeoutException("timeout error", errors);

        // Assert
        Assert.Equal("timeout error", ex.Message);
        Assert.Equal(errors, ex.Errors);
        Assert.Equal(HttpStatusCode.GatewayTimeout, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithUpstreamServiceAndTimeout_SetsFormattedMessage()
    {
        // Arrange & Act
        var ex = new GatewayTimeoutException("ExternalAPI", 30);

        // Assert
        Assert.Equal("Gateway timeout from \"ExternalAPI\" after 30 seconds", ex.Message);
        Assert.Equal(HttpStatusCode.GatewayTimeout, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithNullErrors_InitializesEmptyErrorsList()
    {
        // Arrange & Act
        var ex = new GatewayTimeoutException("timeout", (List<string>?)null);

        // Assert
        Assert.NotNull(ex.Errors);
        Assert.Empty(ex.Errors);
    }
}
