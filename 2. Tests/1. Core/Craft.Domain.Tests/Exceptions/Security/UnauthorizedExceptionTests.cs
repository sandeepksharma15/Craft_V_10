using System.Net;

namespace Craft.Domain.Tests.Security;

public class UnauthorizedExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_SetsMessageAndStatusCode()
    {
        // Arrange & Act
        var ex = new UnauthorizedException("unauthorized!");

        // Assert
        Assert.Equal("unauthorized!", ex.Message);
        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_Parameterless_Defaults()
    {
        // Arrange & Act
        var ex = new UnauthorizedException();

        // Assert
        Assert.NotNull(ex.Message); // Exception.Message is default
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsProperties()
    {
        // Arrange & Act
        var inner = new Exception("inner");
        var ex = new UnauthorizedException("unauthorized!", inner);

        // Assert
        Assert.Equal("unauthorized!", ex.Message);
        Assert.Equal(inner, ex.InnerException);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessageErrorsStatusCode_SetsAllProperties()
    {
        // Arrange & Act
        var errors = new List<string> { "err1", "err2" };
        var ex = new UnauthorizedException("unauthorized!", errors, HttpStatusCode.Unauthorized);

        // Assert
        Assert.Equal("unauthorized!", ex.Message);
        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
        Assert.Equal(errors, ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessageAndNullErrors_SetsEmptyErrors()
    {
        // Arrange & Act
        var ex = new UnauthorizedException("unauthorized!", null!, HttpStatusCode.Unauthorized);

        // Assert
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithCustomStatusCode_SetsStatusCode()
    {
        // Arrange & Act
        var ex = new UnauthorizedException("unauthorized!", [], HttpStatusCode.BadRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
    }
}
