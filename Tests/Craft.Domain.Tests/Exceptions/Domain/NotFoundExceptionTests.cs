using System.Net;

namespace Craft.Exceptions.Tests.Domain;

public class NotFoundExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_SetsMessageAndStatusCode()
    {
        // Arrange & Act
        var ex = new EntityNotFoundException("not found message");

        // Assert
        Assert.Equal("not found message", ex.Message);
        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
    }

    [Fact]
    public void DefaultConstructor_CreatesInstance()
    {
        // Arrange & Act
        var ex = new EntityNotFoundException();

        // Assert
        Assert.NotNull(ex);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsProperties()
    {
        // Arrange & Act
        var inner = new Exception("inner");
        var ex = new EntityNotFoundException("msg", inner);

        // Assert
        Assert.Equal("msg", ex.Message);
        Assert.Equal(inner, ex.InnerException);
    }

    [Fact]
    public void Constructor_WithMessageErrorsAndStatusCode_SetsAllProperties()
    {
        // Arrange & Act
        var errors = new List<string> { "err1", "err2" };
        var ex = new EntityNotFoundException("msg", errors, HttpStatusCode.NotFound);

        // Assert
        Assert.Equal("msg", ex.Message);
        Assert.Equal(errors, ex.Errors);
        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithNameAndKey_SetsFormattedMessage()
    {
        // Arrange & Act
        var ex = new EntityNotFoundException("User", 123);

        // Assert
        Assert.Equal("Entity \"User\" (123) was not found.", ex.Message);
        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
    }
}
