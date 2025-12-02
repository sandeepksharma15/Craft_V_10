using Craft.Emails;

namespace Craft.Tests.Emails;

public class EmailResultTests
{
    [Fact]
    public void Success_CreatesSuccessResult()
    {
        // Act
        var result = EmailResult.Success("message-id");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("message-id", result.MessageId);
        Assert.Null(result.ErrorMessage);
        Assert.Null(result.Exception);
        Assert.True(result.SentAt > DateTimeOffset.MinValue);
    }

    [Fact]
    public void Success_WithoutMessageId_CreatesSuccessResult()
    {
        // Act
        var result = EmailResult.Success();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.MessageId);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Failure_CreatesFailureResult()
    {
        // Act
        var result = EmailResult.Failure("Error message");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Error message", result.ErrorMessage);
        Assert.Null(result.MessageId);
        Assert.Null(result.Exception);
        Assert.True(result.SentAt > DateTimeOffset.MinValue);
    }

    [Fact]
    public void Failure_WithException_CreatesFailureResult()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");

        // Act
        var result = EmailResult.Failure("Error message", exception);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Error message", result.ErrorMessage);
        Assert.Equal(exception, result.Exception);
        Assert.Null(result.MessageId);
    }
}
