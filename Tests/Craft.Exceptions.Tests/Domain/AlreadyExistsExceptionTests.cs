using System.Net;

namespace Craft.Exceptions.Tests.Domain;

public class AlreadyExistsExceptionTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaultMessage()
    {
        // Arrange & Act
        var ex = new AlreadyExistsException();

        // Assert
        Assert.Equal("This resource already exists", ex.Message);
    }

    [Fact]
    public void Constructor_WithMessage_SetsMessage()
    {
        // Arrange & Act
        var ex = new AlreadyExistsException("custom message");

        // Assert
        Assert.Equal("custom message", ex.Message);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsProperties()
    {
        // Arrange & Act
        var inner = new Exception("inner");
        var ex = new AlreadyExistsException("msg", inner);

        // Assert
        Assert.Equal("msg", ex.Message);
        Assert.Equal(inner, ex.InnerException);
    }

    [Fact]
    public void Constructor_WithMessageErrorsAndStatusCode_SetsAllProperties()
    {
        // Arrange & Act
        var errors = new List<string> { "err1", "err2" };
        var ex = new AlreadyExistsException("msg", errors, HttpStatusCode.Conflict);

        // Assert
        Assert.Equal("msg", ex.Message);
        Assert.Equal(errors, ex.Errors);
        Assert.Equal(HttpStatusCode.Conflict, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithNameAndKey_SetsFormattedMessage()
    {
        // Arrange & Act
        var ex = new AlreadyExistsException("User", 123);

        // Assert
        Assert.Equal("Entity \"User\" (123) already exists", ex.Message);
    }
}
