using System.Net;

namespace Craft.Exceptions.Tests.Domain;

public class PreconditionFailedExceptionTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaultMessageAndStatusCode()
    {
        // Arrange & Act
        var ex = new PreconditionFailedException();

        // Assert
        Assert.Equal("Precondition failed for the request", ex.Message);
        Assert.Equal((HttpStatusCode)412, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessage_SetsMessageAndStatusCode()
    {
        // Arrange & Act
        var ex = new PreconditionFailedException("ETag mismatch");

        // Assert
        Assert.Equal("ETag mismatch", ex.Message);
        Assert.Equal((HttpStatusCode)412, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsProperties()
    {
        // Arrange
        var inner = new Exception("Validation error");

        // Act
        var ex = new PreconditionFailedException("Precondition failed", inner);

        // Assert
        Assert.Equal("Precondition failed", ex.Message);
        Assert.Equal(inner, ex.InnerException);
        Assert.Equal((HttpStatusCode)412, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithMessageAndErrors_SetsAllProperties()
    {
        // Arrange
        var errors = new List<string> { "If-Match header required", "ETag does not match" };

        // Act
        var ex = new PreconditionFailedException("Precondition check failed", errors);

        // Assert
        Assert.Equal("Precondition check failed", ex.Message);
        Assert.Equal(errors, ex.Errors);
        Assert.Equal(2, ex.Errors.Count);
        Assert.Equal((HttpStatusCode)412, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithHeaderAndValues_SetsFormattedMessage()
    {
        // Arrange & Act
        var ex = new PreconditionFailedException("If-Match", "\"abc123\"", "\"xyz789\"");

        // Assert
        Assert.Equal("Precondition header 'If-Match' failed. Expected: '\"abc123\"', Actual: '\"xyz789\"'", ex.Message);
        Assert.Equal((HttpStatusCode)412, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithIfMatchHeader_SetsFormattedMessage()
    {
        // Arrange & Act
        var ex = new PreconditionFailedException("If-Match", "W/\"12345\"", "W/\"67890\"");

        // Assert
        Assert.Contains("If-Match", ex.Message);
        Assert.Contains("W/\"12345\"", ex.Message);
        Assert.Contains("W/\"67890\"", ex.Message);
        Assert.Equal((HttpStatusCode)412, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithIfNoneMatchHeader_SetsFormattedMessage()
    {
        // Arrange & Act
        var ex = new PreconditionFailedException("If-None-Match", "*", "\"resource-etag\"");

        // Assert
        Assert.Contains("If-None-Match", ex.Message);
        Assert.Contains("*", ex.Message);
        Assert.Contains("resource-etag", ex.Message);
    }

    [Fact]
    public void Constructor_WithIfModifiedSinceHeader_SetsFormattedMessage()
    {
        // Arrange
        var expected = "Mon, 15 Jan 2024 10:00:00 GMT";
        var actual = "Mon, 15 Jan 2024 11:00:00 GMT";

        // Act
        var ex = new PreconditionFailedException("If-Modified-Since", expected, actual);

        // Assert
        Assert.Contains("If-Modified-Since", ex.Message);
        Assert.Contains(expected, ex.Message);
        Assert.Contains(actual, ex.Message);
    }

    [Fact]
    public void Constructor_WithIfUnmodifiedSinceHeader_SetsFormattedMessage()
    {
        // Arrange
        var expected = "Mon, 15 Jan 2024 10:00:00 GMT";
        var actual = "Mon, 15 Jan 2024 09:00:00 GMT";

        // Act
        var ex = new PreconditionFailedException("If-Unmodified-Since", expected, actual);

        // Assert
        Assert.Contains("If-Unmodified-Since", ex.Message);
        Assert.Contains(expected, ex.Message);
        Assert.Contains(actual, ex.Message);
    }

    [Fact]
    public void Constructor_WithEmptyValues_HandlesCorrectly()
    {
        // Arrange & Act
        var ex = new PreconditionFailedException("If-Match", "", "\"current-value\"");

        // Assert
        Assert.Contains("If-Match", ex.Message);
        Assert.Contains("''", ex.Message);
    }

    [Fact]
    public void Constructor_WithNullErrors_InitializesEmptyErrorsList()
    {
        // Arrange & Act
        var ex = new PreconditionFailedException("Failed", (List<string>?)null);

        // Assert
        Assert.NotNull(ex.Errors);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void StatusCode_IsAlways412()
    {
        // Arrange & Act
        var ex1 = new PreconditionFailedException();
        var ex2 = new PreconditionFailedException("message");
        var ex3 = new PreconditionFailedException("If-Match", "v1", "v2");

        // Assert
        Assert.Equal((HttpStatusCode)412, ex1.StatusCode);
        Assert.Equal((HttpStatusCode)412, ex2.StatusCode);
        Assert.Equal((HttpStatusCode)412, ex3.StatusCode);
    }

    [Fact]
    public void Constructor_InheritsFromCraftException()
    {
        // Arrange & Act
        var ex = new PreconditionFailedException();

        // Assert
        Assert.IsAssignableFrom<CraftException>(ex);
        Assert.IsAssignableFrom<Exception>(ex);
    }

    [Fact]
    public void Constructor_WithCustomHeaderName_FormatsCorrectly()
    {
        // Arrange & Act
        var ex = new PreconditionFailedException("X-Custom-Precondition", "expected-value", "actual-value");

        // Assert
        Assert.Contains("X-Custom-Precondition", ex.Message);
        Assert.Contains("expected-value", ex.Message);
        Assert.Contains("actual-value", ex.Message);
    }

    [Fact]
    public void Constructor_WithSpecialCharacters_HandlesCorrectly()
    {
        // Arrange & Act
        var ex = new PreconditionFailedException("If-Match", "\"<value>\"", "\"<different>\"");

        // Assert
        Assert.Contains("<value>", ex.Message);
        Assert.Contains("<different>", ex.Message);
    }
}
