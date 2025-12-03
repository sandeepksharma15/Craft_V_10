using System.Net;

namespace Craft.Exceptions.Tests.Domain;

public class NotFoundExceptionTests
{
    #region NotFoundException Tests

    [Fact]
    public void NotFoundException_DefaultConstructor_SetsDefaultMessageAndStatusCode()
    {
        // Arrange & Act
        var ex = new NotFoundException();

        // Assert
        Assert.Equal("The requested resource was not found", ex.Message);
        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void NotFoundException_WithMessage_SetsMessageAndStatusCode()
    {
        // Arrange & Act
        var ex = new NotFoundException("Resource not found");

        // Assert
        Assert.Equal("Resource not found", ex.Message);
        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void NotFoundException_WithMessageAndInnerException_SetsProperties()
    {
        // Arrange
        var inner = new Exception("Inner exception");

        // Act
        var ex = new NotFoundException("Not found error", inner);

        // Assert
        Assert.Equal("Not found error", ex.Message);
        Assert.Equal(inner, ex.InnerException);
        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
    }

    [Fact]
    public void NotFoundException_WithEntityNameAndKey_SetsFormattedMessage()
    {
        // Arrange & Act
        var ex = new NotFoundException("User", 123);

        // Assert
        Assert.Equal("Entity \"User\" (123) was not found.", ex.Message);
        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    #endregion
}
