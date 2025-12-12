using System.Net;

namespace Craft.Domain.Tests.Domain;

public class BadRequestExceptionTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaultMessageAndStatusCode()
    {
        // Arrange & Act
        var ex = new BadRequestException();

        // Assert
        Assert.Equal("The request is invalid", ex.Message);
        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessage_SetsMessageAndStatusCode()
    {
        // Arrange & Act
        var ex = new BadRequestException("Invalid request data");

        // Assert
        Assert.Equal("Invalid request data", ex.Message);
        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsProperties()
    {
        // Arrange
        var inner = new Exception("inner exception");

        // Act
        var ex = new BadRequestException("bad request", inner);

        // Assert
        Assert.Equal("bad request", ex.Message);
        Assert.Equal(inner, ex.InnerException);
        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithMessageAndErrors_SetsAllProperties()
    {
        // Arrange
        var errors = new List<string> { "error1", "error2" };

        // Act
        var ex = new BadRequestException("bad request", errors);

        // Assert
        Assert.Equal("bad request", ex.Message);
        Assert.Equal(errors, ex.Errors);
        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithNullErrors_InitializesEmptyErrorsList()
    {
        // Arrange & Act
        var ex = new BadRequestException("bad request", (List<string>?)null);

        // Assert
        Assert.NotNull(ex.Errors);
        Assert.Empty(ex.Errors);
    }
}
