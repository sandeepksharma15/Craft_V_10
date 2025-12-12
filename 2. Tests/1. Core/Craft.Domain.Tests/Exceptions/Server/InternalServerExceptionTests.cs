using System.Net;

namespace Craft.Domain.Tests.Server;

public class InternalServerExceptionTests
{
    [Fact]
    public void Constructor_Parameterless_SetsDefaultMessageAndStatusCode()
    {
        // Arrange & Act
        var ex = new InternalServerException();

        // Assert
        Assert.Equal("An internal server error occurred", ex.Message);
        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessage_SetsMessageAndStatusCode()
    {
        // Arrange & Act
        var ex = new InternalServerException("custom error");

        // Assert
        Assert.Equal("custom error", ex.Message);
        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsProperties()
    {
        // Arrange
        var inner = new Exception("inner");

        // Act
        var ex = new InternalServerException("custom error", inner);

        // Assert
        Assert.Equal("custom error", ex.Message);
        Assert.Equal(inner, ex.InnerException);
        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessageAndErrors_SetsProperties()
    {
        // Arrange
        var errors = new List<string> { "err1", "err2" };

        // Act
        var ex = new InternalServerException("custom error", errors);

        // Assert
        Assert.Equal("custom error", ex.Message);
        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
        Assert.Equal(errors, ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessageAndNullErrors_SetsEmptyErrors()
    {
        // Arrange & Act
        var ex = new InternalServerException("custom error", (List<string>?)null);

        // Assert
        Assert.Equal("custom error", ex.Message);
        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessageErrorsStatusCode_SetsAllProperties()
    {
        // Arrange
        var errors = new List<string> { "err1", "err2" };

        // Act
        var ex = new InternalServerException("custom error", errors, HttpStatusCode.BadGateway);

        // Assert
        Assert.Equal("custom error", ex.Message);
        Assert.Equal(HttpStatusCode.BadGateway, ex.StatusCode);
        Assert.Equal(errors, ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessageAndEmptyErrors_SetsEmptyErrors()
    {
        // Arrange
        var errors = new List<string>();

        // Act
        var ex = new InternalServerException("custom error", errors);

        // Assert
        Assert.Equal("custom error", ex.Message);
        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithCustomStatusCode_SetsStatusCode()
    {
        // Arrange & Act
        var ex = new InternalServerException("custom error", [], HttpStatusCode.NotImplemented);

        // Assert
        Assert.Equal("custom error", ex.Message);
        Assert.Equal(HttpStatusCode.NotImplemented, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }
}
