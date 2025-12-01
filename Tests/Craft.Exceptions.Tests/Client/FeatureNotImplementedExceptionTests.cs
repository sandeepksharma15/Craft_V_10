using System.Net;
using Craft.Exceptions.Client;

namespace Craft.Exceptions.Tests.Client;

public class FeatureNotImplementedExceptionTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaultMessageAndStatusCode()
    {
        // Arrange & Act
        var ex = new FeatureNotImplementedException();

        // Assert
        Assert.Equal("This feature is not implemented", ex.Message);
        Assert.Equal(HttpStatusCode.NotImplemented, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessage_SetsMessageAndStatusCode()
    {
        // Arrange & Act
        var ex = new FeatureNotImplementedException("Feature coming soon");

        // Assert
        Assert.Equal("Feature coming soon", ex.Message);
        Assert.Equal(HttpStatusCode.NotImplemented, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsProperties()
    {
        // Arrange
        var inner = new Exception("inner exception");

        // Act
        var ex = new FeatureNotImplementedException("not implemented", inner);

        // Assert
        Assert.Equal("not implemented", ex.Message);
        Assert.Equal(inner, ex.InnerException);
        Assert.Equal(HttpStatusCode.NotImplemented, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithMessageAndErrors_SetsAllProperties()
    {
        // Arrange
        var errors = new List<string> { "error1", "error2" };

        // Act
        var ex = new FeatureNotImplementedException("not implemented", errors);

        // Assert
        Assert.Equal("not implemented", ex.Message);
        Assert.Equal(errors, ex.Errors);
        Assert.Equal(HttpStatusCode.NotImplemented, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithFeatureNameAndDetails_SetsFormattedMessage()
    {
        // Arrange & Act
        var ex = new FeatureNotImplementedException("Export to PDF", "Planned for next release");

        // Assert
        Assert.Equal("Feature \"Export to PDF\" is not implemented: Planned for next release", ex.Message);
        Assert.Equal(HttpStatusCode.NotImplemented, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithNullErrors_InitializesEmptyErrorsList()
    {
        // Arrange & Act
        var ex = new FeatureNotImplementedException("not implemented", (List<string>?)null);

        // Assert
        Assert.NotNull(ex.Errors);
        Assert.Empty(ex.Errors);
    }
}
