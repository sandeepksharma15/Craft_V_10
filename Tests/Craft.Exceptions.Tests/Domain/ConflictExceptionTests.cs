using System.Net;

namespace Craft.Exceptions.Tests.Domain;

public class ConflictExceptionTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaultMessageAndStatusCode()
    {
        // Arrange & Act
        var ex = new ConflictException();

        // Assert
        Assert.Equal("A conflict occurred with the current state of the resource", ex.Message);
        Assert.Equal(HttpStatusCode.Conflict, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessage_SetsMessageAndStatusCode()
    {
        // Arrange & Act
        var ex = new ConflictException("Resource conflict detected");

        // Assert
        Assert.Equal("Resource conflict detected", ex.Message);
        Assert.Equal(HttpStatusCode.Conflict, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsProperties()
    {
        // Arrange
        var inner = new Exception("inner exception");

        // Act
        var ex = new ConflictException("conflict occurred", inner);

        // Assert
        Assert.Equal("conflict occurred", ex.Message);
        Assert.Equal(inner, ex.InnerException);
        Assert.Equal(HttpStatusCode.Conflict, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithMessageAndErrors_SetsAllProperties()
    {
        // Arrange
        var errors = new List<string> { "error1", "error2" };

        // Act
        var ex = new ConflictException("conflict message", errors);

        // Assert
        Assert.Equal("conflict message", ex.Message);
        Assert.Equal(errors, ex.Errors);
        Assert.Equal(HttpStatusCode.Conflict, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithResourceNameAndReason_SetsFormattedMessage()
    {
        // Arrange & Act
        var ex = new ConflictException("Order", "version mismatch");

        // Assert
        Assert.Equal("Conflict with resource \"Order\": version mismatch", ex.Message);
        Assert.Equal(HttpStatusCode.Conflict, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithNullErrors_InitializesEmptyErrorsList()
    {
        // Arrange & Act
        var ex = new ConflictException("conflict", (List<string>?)null);

        // Assert
        Assert.NotNull(ex.Errors);
        Assert.Empty(ex.Errors);
    }
}
