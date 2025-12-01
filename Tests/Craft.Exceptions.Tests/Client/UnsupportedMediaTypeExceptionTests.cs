using System.Net;
using Craft.Exceptions.Client;

namespace Craft.Exceptions.Tests.Client;

public class UnsupportedMediaTypeExceptionTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaultMessageAndStatusCode()
    {
        // Arrange & Act
        var ex = new UnsupportedMediaTypeException();

        // Assert
        Assert.Equal("The media type is not supported", ex.Message);
        Assert.Equal(HttpStatusCode.UnsupportedMediaType, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessage_SetsMessageAndStatusCode()
    {
        // Arrange & Act
        var ex = new UnsupportedMediaTypeException("Invalid content type");

        // Assert
        Assert.Equal("Invalid content type", ex.Message);
        Assert.Equal(HttpStatusCode.UnsupportedMediaType, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsProperties()
    {
        // Arrange
        var inner = new Exception("inner exception");

        // Act
        var ex = new UnsupportedMediaTypeException("unsupported media", inner);

        // Assert
        Assert.Equal("unsupported media", ex.Message);
        Assert.Equal(inner, ex.InnerException);
        Assert.Equal(HttpStatusCode.UnsupportedMediaType, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithMessageAndErrors_SetsAllProperties()
    {
        // Arrange
        var errors = new List<string> { "error1", "error2" };

        // Act
        var ex = new UnsupportedMediaTypeException("media type error", errors);

        // Assert
        Assert.Equal("media type error", ex.Message);
        Assert.Equal(errors, ex.Errors);
        Assert.Equal(HttpStatusCode.UnsupportedMediaType, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithMediaTypeAndSupportedTypes_SetsFormattedMessage()
    {
        // Arrange
        var supportedTypes = new[] { "application/json", "application/xml" };

        // Act
        var ex = new UnsupportedMediaTypeException("text/plain", supportedTypes);

        // Assert
        Assert.Equal("Media type \"text/plain\" is not supported. Supported types: application/json, application/xml", ex.Message);
        Assert.Equal(HttpStatusCode.UnsupportedMediaType, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithNullErrors_InitializesEmptyErrorsList()
    {
        // Arrange & Act
        var ex = new UnsupportedMediaTypeException("unsupported", (List<string>?)null);

        // Assert
        Assert.NotNull(ex.Errors);
        Assert.Empty(ex.Errors);
    }
}
