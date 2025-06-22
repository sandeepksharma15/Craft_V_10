using System.Net;
using Craft.Exceptions.Security;

namespace Craft.Exceptions.Tests.Security;

public class InvalidCredentialsExceptionTests
{
    [Fact]
    public void Constructor_Parameterless_SetsDefaultMessage()
    {
        // Arrange & Act
        var ex = new InvalidCredentialsException();

        // Assert
        Assert.Equal("Invalid Credentials: Please check your credentials", ex.Message);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessage_SetsMessage()
    {
        // Arrange & Act
        var ex = new InvalidCredentialsException("custom message");

        // Assert
        Assert.Equal("custom message", ex.Message);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsProperties()
    {
        // Arrange & Act
        var inner = new Exception("inner");
        var ex = new InvalidCredentialsException("msg", inner);

        // Assert
        Assert.Equal("msg", ex.Message);
        Assert.Equal(inner, ex.InnerException);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessageErrorsStatusCode_SetsAllProperties()
    {
        // Arrange & Act
        var errors = new List<string> { "err1", "err2" };
        var ex = new InvalidCredentialsException("msg", errors, HttpStatusCode.NotAcceptable);

        // Assert
        Assert.Equal("msg", ex.Message);
        Assert.Equal(HttpStatusCode.NotAcceptable, ex.StatusCode);
        Assert.Equal(errors, ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessageAndNullErrors_SetsEmptyErrors()
    {
        // Arrange & Act
        var ex = new InvalidCredentialsException("msg", null!, HttpStatusCode.NotAcceptable);

        // Assert
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithCustomStatusCode_SetsStatusCode()
    {
        // Arrange & Act
        var ex = new InvalidCredentialsException("msg", [], HttpStatusCode.BadRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
    }
}
