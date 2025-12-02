using Craft.Emails;

namespace Craft.Tests.Emails;

public class QueuedEmailTests
{
    [Fact]
    public void Constructor_InitializesDefaults()
    {
        // Act
        var email = new QueuedEmail();

        // Assert
        Assert.NotNull(email.Id);
        Assert.NotEmpty(email.Id);
        Assert.Equal(EmailStatus.Pending, email.Status);
        Assert.Equal(0, email.Attempts);
        Assert.Equal(EmailPriority.Normal, email.Priority);
        Assert.True(email.QueuedAt > DateTimeOffset.MinValue);
        Assert.Null(email.ScheduledFor);
        Assert.Null(email.LastAttemptAt);
        Assert.Null(email.SentAt);
        Assert.Null(email.ErrorMessage);
        Assert.Null(email.MessageId);
    }

    [Fact]
    public void Id_IsUnique()
    {
        // Act
        var email1 = new QueuedEmail();
        var email2 = new QueuedEmail();

        // Assert
        Assert.NotEqual(email1.Id, email2.Id);
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        // Arrange
        var request = new MailRequest(
            to: ["test@example.com"],
            subject: "Test",
            body: "Body");
        var scheduledFor = DateTimeOffset.UtcNow.AddHours(1);

        // Act
        var email = new QueuedEmail
        {
            Request = request,
            Status = EmailStatus.Scheduled,
            Attempts = 2,
            ScheduledFor = scheduledFor,
            Priority = EmailPriority.High,
            ErrorMessage = "Test error",
            MessageId = "msg-123"
        };

        // Assert
        Assert.Equal(request, email.Request);
        Assert.Equal(EmailStatus.Scheduled, email.Status);
        Assert.Equal(2, email.Attempts);
        Assert.Equal(scheduledFor, email.ScheduledFor);
        Assert.Equal(EmailPriority.High, email.Priority);
        Assert.Equal("Test error", email.ErrorMessage);
        Assert.Equal("msg-123", email.MessageId);
    }
}

public class EmailStatusTests
{
    [Fact]
    public void EmailStatus_HasExpectedValues()
    {
        // Assert
        Assert.Equal(0, (int)EmailStatus.Pending);
        Assert.Equal(1, (int)EmailStatus.Sending);
        Assert.Equal(2, (int)EmailStatus.Sent);
        Assert.Equal(3, (int)EmailStatus.Failed);
        Assert.Equal(4, (int)EmailStatus.FailedPermanently);
        Assert.Equal(5, (int)EmailStatus.Scheduled);
    }
}

public class EmailPriorityTests
{
    [Fact]
    public void EmailPriority_HasExpectedValues()
    {
        // Assert
        Assert.Equal(0, (int)EmailPriority.Low);
        Assert.Equal(1, (int)EmailPriority.Normal);
        Assert.Equal(2, (int)EmailPriority.High);
    }
}
