using System.Net;
using Craft.Exceptions.Client;

namespace Craft.Exceptions.Tests.Client;

public class TooManyRequestsExceptionTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaultMessageAndStatusCode()
    {
        // Arrange & Act
        var ex = new TooManyRequestsException();

        // Assert
        Assert.Equal("Too many requests - rate limit exceeded", ex.Message);
        Assert.Equal((HttpStatusCode)429, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessage_SetsMessageAndStatusCode()
    {
        // Arrange & Act
        var ex = new TooManyRequestsException("Rate limit hit");

        // Assert
        Assert.Equal("Rate limit hit", ex.Message);
        Assert.Equal((HttpStatusCode)429, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsProperties()
    {
        // Arrange
        var inner = new Exception("inner exception");

        // Act
        var ex = new TooManyRequestsException("too many requests", inner);

        // Assert
        Assert.Equal("too many requests", ex.Message);
        Assert.Equal(inner, ex.InnerException);
        Assert.Equal((HttpStatusCode)429, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithMessageAndErrors_SetsAllProperties()
    {
        // Arrange
        var errors = new List<string> { "error1", "error2" };

        // Act
        var ex = new TooManyRequestsException("rate limit exceeded", errors);

        // Assert
        Assert.Equal("rate limit exceeded", ex.Message);
        Assert.Equal(errors, ex.Errors);
        Assert.Equal((HttpStatusCode)429, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithLimitAndPeriod_SetsFormattedMessage()
    {
        // Arrange & Act
        var ex = new TooManyRequestsException(100, "hour");

        // Assert
        Assert.Equal("Rate limit exceeded: 100 requests per hour", ex.Message);
        Assert.Equal((HttpStatusCode)429, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithRetryAfterSeconds_SetsFormattedMessage()
    {
        // Arrange & Act
        var ex = new TooManyRequestsException(60);

        // Assert
        Assert.Equal("Too many requests. Retry after 60 seconds", ex.Message);
        Assert.Equal((HttpStatusCode)429, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithNullErrors_InitializesEmptyErrorsList()
    {
        // Arrange & Act
        var ex = new TooManyRequestsException("too many", (List<string>?)null);

        // Assert
        Assert.NotNull(ex.Errors);
        Assert.Empty(ex.Errors);
    }
}
