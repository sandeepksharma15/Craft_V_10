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

    #region EntityNotFoundException Tests (Backward Compatibility)

    [Fact]
    public void EntityNotFoundException_WithMessage_SetsMessageAndStatusCode()
    {
        // Arrange & Act
#pragma warning disable CS0618 // Type or member is obsolete
        var ex = new EntityNotFoundException("not found message");
#pragma warning restore CS0618

        // Assert
        Assert.Equal("not found message", ex.Message);
        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
    }

    [Fact]
    public void EntityNotFoundException_DefaultConstructor_CreatesInstance()
    {
        // Arrange & Act
#pragma warning disable CS0618 // Type or member is obsolete
        var ex = new EntityNotFoundException();
#pragma warning restore CS0618

        // Assert
        Assert.NotNull(ex);
    }

    [Fact]
    public void EntityNotFoundException_WithMessageAndInnerException_SetsProperties()
    {
        // Arrange & Act
        var inner = new Exception("inner");
#pragma warning disable CS0618 // Type or member is obsolete
        var ex = new EntityNotFoundException("msg", inner);
#pragma warning restore CS0618

        // Assert
        Assert.Equal("msg", ex.Message);
        Assert.Equal(inner, ex.InnerException);
    }

    [Fact]
    public void EntityNotFoundException_WithMessageErrorsAndStatusCode_SetsAllProperties()
    {
        // Arrange & Act
        var errors = new List<string> { "err1", "err2" };
#pragma warning disable CS0618 // Type or member is obsolete
        var ex = new EntityNotFoundException("msg", errors, HttpStatusCode.NotFound);
#pragma warning restore CS0618

        // Assert
        Assert.Equal("msg", ex.Message);
        Assert.Equal(errors, ex.Errors);
        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
    }

    [Fact]
    public void EntityNotFoundException_WithNameAndKey_SetsFormattedMessage()
    {
        // Arrange & Act
#pragma warning disable CS0618 // Type or member is obsolete
        var ex = new EntityNotFoundException("User", 123);
#pragma warning restore CS0618

        // Assert
        Assert.Equal("Entity \"User\" (123) was not found.", ex.Message);
        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
    }

    [Fact]
    public void EntityNotFoundException_InheritsFromNotFoundException()
    {
        // Arrange & Act
#pragma warning disable CS0618 // Type or member is obsolete
        var ex = new EntityNotFoundException();
#pragma warning restore CS0618

        // Assert
        Assert.IsAssignableFrom<NotFoundException>(ex);
        Assert.IsAssignableFrom<CraftException>(ex);
    }

    #endregion
}
