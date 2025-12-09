using System.Net;

namespace Craft.Exceptions.Tests.Security;

public class ForbiddenExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_SetsMessageAndStatusCode()
    {
        // Arrange & Act
        var ex = new ForbiddenException("forbidden!");

        // Assert
        Assert.Equal("forbidden!", ex.Message);
        Assert.Equal(HttpStatusCode.Forbidden, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_Parameterless_Defaults()
    {
        // Arrange & Act
        var ex = new ForbiddenException();

        // Assert
        Assert.NotNull(ex.Message); // Exception.Message is default
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsProperties()
    {
        // Arrange & Act
        var inner = new Exception("inner");
        var ex = new ForbiddenException("forbidden!", inner);

        // Assert
        Assert.Equal("forbidden!", ex.Message);
        Assert.Equal(inner, ex.InnerException);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessageErrorsStatusCode_SetsAllProperties()
    {
        // Arrange & Act
        var errors = new List<string> { "err1", "err2" };
        var ex = new ForbiddenException("forbidden!", errors, HttpStatusCode.Forbidden);

        // Assert
        Assert.Equal("forbidden!", ex.Message);
        Assert.Equal(HttpStatusCode.Forbidden, ex.StatusCode);
        Assert.Equal(errors, ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessageAndNullErrors_SetsEmptyErrors()
    {
        // Arrange & Act
        var ex = new ForbiddenException("forbidden!", null!, HttpStatusCode.Forbidden);

        // Assert
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithCustomStatusCode_SetsStatusCode()
    {
        // Arrange & Act
        var ex = new ForbiddenException("forbidden!", [], HttpStatusCode.BadRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
    }
}
