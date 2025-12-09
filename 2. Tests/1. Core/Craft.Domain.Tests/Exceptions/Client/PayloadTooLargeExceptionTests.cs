using System.Net;

namespace Craft.Exceptions.Tests.Client;

public class PayloadTooLargeExceptionTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaultMessageAndStatusCode()
    {
        // Arrange & Act
        var ex = new PayloadTooLargeException();

        // Assert
        Assert.Equal("The request payload is too large", ex.Message);
        Assert.Equal((HttpStatusCode)413, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessage_SetsMessageAndStatusCode()
    {
        // Arrange & Act
        var ex = new PayloadTooLargeException("File is too large");

        // Assert
        Assert.Equal("File is too large", ex.Message);
        Assert.Equal((HttpStatusCode)413, ex.StatusCode);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsProperties()
    {
        // Arrange
        var inner = new Exception("Upload failed");

        // Act
        var ex = new PayloadTooLargeException("Payload too large", inner);

        // Assert
        Assert.Equal("Payload too large", ex.Message);
        Assert.Equal(inner, ex.InnerException);
        Assert.Equal((HttpStatusCode)413, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithMessageAndErrors_SetsAllProperties()
    {
        // Arrange
        var errors = new List<string> { "Max size: 10MB", "Actual size: 15MB" };

        // Act
        var ex = new PayloadTooLargeException("File too large", errors);

        // Assert
        Assert.Equal("File too large", ex.Message);
        Assert.Equal(errors, ex.Errors);
        Assert.Equal(2, ex.Errors.Count);
        Assert.Equal((HttpStatusCode)413, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithSizes_SetsFormattedMessage()
    {
        // Arrange & Act
        var ex = new PayloadTooLargeException(15000000, 10000000);

        // Assert
        // Just verify the message contains the byte information and exceeds keyword
        // Number formatting is culture-dependent, so we don't check exact format
        Assert.Contains("bytes", ex.Message);
        Assert.Contains("exceeds", ex.Message);
        Assert.Contains("maximum", ex.Message);
        Assert.Equal((HttpStatusCode)413, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithResourceTypeAndSizes_SetsFormattedMessage()
    {
        // Arrange & Act
        var ex = new PayloadTooLargeException("Image", 5000000, 2000000);

        // Assert
        Assert.Contains("Image", ex.Message);
        Assert.Contains("bytes", ex.Message);
        Assert.Contains("exceeds", ex.Message);
        Assert.Equal((HttpStatusCode)413, ex.StatusCode);
    }

    [Fact]
    public void Constructor_WithLargeSizes_FormatsNumbersCorrectly()
    {
        // Arrange & Act
        var ex = new PayloadTooLargeException(1073741824, 536870912); // 1GB and 512MB in bytes

        // Assert
        // Verify the numbers are present regardless of formatting
        var cleanMessage = ex.Message.Replace(",", "").Replace(" ", "");
        Assert.Contains("1073741824", cleanMessage);
        Assert.Contains("536870912", cleanMessage);
    }

    [Fact]
    public void Constructor_WithZeroSizes_HandlesCorrectly()
    {
        // Arrange & Act
        var ex = new PayloadTooLargeException(0, 1000);

        // Assert
        Assert.Contains("0 bytes", ex.Message);
        Assert.Contains("1,000 bytes", ex.Message);
    }

    [Fact]
    public void Constructor_WithNullErrors_InitializesEmptyErrorsList()
    {
        // Arrange & Act
        var ex = new PayloadTooLargeException("Too large", (List<string>?)null);

        // Assert
        Assert.NotNull(ex.Errors);
        Assert.Empty(ex.Errors);
    }

    [Fact]
    public void StatusCode_IsAlways413()
    {
        // Arrange & Act
        var ex1 = new PayloadTooLargeException();
        var ex2 = new PayloadTooLargeException("message");
        var ex3 = new PayloadTooLargeException(1000, 500);

        // Assert
        Assert.Equal((HttpStatusCode)413, ex1.StatusCode);
        Assert.Equal((HttpStatusCode)413, ex2.StatusCode);
        Assert.Equal((HttpStatusCode)413, ex3.StatusCode);
    }

    [Fact]
    public void Constructor_InheritsFromCraftException()
    {
        // Arrange & Act
        var ex = new PayloadTooLargeException();

        // Assert
        Assert.IsType<CraftException>(ex, exactMatch: false);
        Assert.IsType<Exception>(ex, exactMatch: false);
    }

    [Fact]
    public void Constructor_WithDifferentResourceTypes_FormatsCorrectly()
    {
        // Arrange & Act
        var ex1 = new PayloadTooLargeException("Video", 100000000, 50000000);
        var ex2 = new PayloadTooLargeException("Document", 5000000, 2000000);
        var ex3 = new PayloadTooLargeException("Archive", 200000000, 100000000);

        // Assert
        Assert.Contains("Video", ex1.Message);
        Assert.Contains("Document", ex2.Message);
        Assert.Contains("Archive", ex3.Message);
    }
}
