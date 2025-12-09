using System.Net;

namespace Craft.Exceptions.Tests.Server;

public class BadGatewayExceptionTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaultMessageAndStatusCode()
    {
        // Arrange & Act
        var ex = new BadGatewayException();

        // Assert
        Assert.Equal("Bad gateway - invalid response from upstream server", ex.Message);
        Assert.Equal(HttpStatusCode.BadGateway, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessage_SetsMessageAndStatusCode()
    {
        // Arrange & Act
        var ex = new BadGatewayException("Upstream server error");

        // Assert
        Assert.Equal("Upstream server error", ex.Message);
        Assert.Equal(HttpStatusCode.BadGateway, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsProperties()
    {
        // Arrange
        var inner = new Exception("inner exception");

        // Act
        var ex = new BadGatewayException("bad gateway", inner);

        // Assert
        Assert.Equal("bad gateway", ex.Message);
        Assert.Equal(inner, ex.InnerException);
        Assert.Equal(HttpStatusCode.BadGateway, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithMessageAndErrors_SetsAllProperties()
    {
        // Arrange
        var errors = new List<string> { "error1", "error2" };

        // Act
        var ex = new BadGatewayException("gateway error", errors);

        // Assert
        Assert.Equal("gateway error", ex.Message);
        Assert.Equal(errors, ex.Errors);
        Assert.Equal(HttpStatusCode.BadGateway, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithUpstreamServiceAndReason_SetsFormattedMessage()
    {
        // Arrange & Act
        var ex = new BadGatewayException("ExternalAPI", "Invalid JSON response");

        // Assert
        Assert.Equal("Bad gateway from \"ExternalAPI\": Invalid JSON response", ex.Message);
        Assert.Equal(HttpStatusCode.BadGateway, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithNullErrors_InitializesEmptyErrorsList()
    {
        // Arrange & Act
        var ex = new BadGatewayException("gateway error", (List<string>?)null);

        // Assert
        Assert.NotNull(ex.Errors);
        Assert.Empty(ex.Errors);
    }
}
