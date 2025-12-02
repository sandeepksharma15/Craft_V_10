using Craft.Emails;

namespace Craft.Tests.Emails;

public class InMemoryEmailQueueTests
{
    private readonly InMemoryEmailQueue _queue;

    public InMemoryEmailQueueTests()
    {
        _queue = new InMemoryEmailQueue();
    }

    [Fact]
    public async Task EnqueueAsync_AddsEmailToQueue()
    {
        // Arrange
        var email = new QueuedEmail
        {
            Request = new MailRequest(
                to: ["test@example.com"],
                subject: "Test",
                body: "Body")
        };

        // Act
        await _queue.EnqueueAsync(email);
        var retrieved = await _queue.GetByIdAsync(email.Id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(email.Id, retrieved.Id);
    }

    [Fact]
    public async Task DequeueAsync_ReturnsPendingEmail()
    {
        // Arrange
        var email = new QueuedEmail
        {
            Request = new MailRequest(
                to: ["test@example.com"],
                subject: "Test",
                body: "Body"),
            Status = EmailStatus.Pending
        };

        await _queue.EnqueueAsync(email);

        // Act
        var dequeued = await _queue.DequeueAsync();

        // Assert
        Assert.NotNull(dequeued);
        Assert.Equal(email.Id, dequeued.Id);
    }

    [Fact]
    public async Task DequeueAsync_ReturnsNullWhenQueueEmpty()
    {
        // Act
        var dequeued = await _queue.DequeueAsync();

        // Assert
        Assert.Null(dequeued);
    }

    [Fact]
    public async Task DequeueAsync_SkipsScheduledEmails()
    {
        // Arrange
        var scheduledEmail = new QueuedEmail
        {
            Request = new MailRequest(
                to: ["test@example.com"],
                subject: "Scheduled",
                body: "Body"),
            Status = EmailStatus.Pending,
            ScheduledFor = DateTimeOffset.UtcNow.AddHours(1)
        };

        await _queue.EnqueueAsync(scheduledEmail);

        // Act
        var dequeued = await _queue.DequeueAsync();

        // Assert
        Assert.Null(dequeued);
    }

    [Fact]
    public async Task DequeueAsync_ReturnsDueScheduledEmail()
    {
        // Arrange
        var dueEmail = new QueuedEmail
        {
            Request = new MailRequest(
                to: ["test@example.com"],
                subject: "Due",
                body: "Body"),
            Status = EmailStatus.Pending,
            ScheduledFor = DateTimeOffset.UtcNow.AddMinutes(-1)
        };

        await _queue.EnqueueAsync(dueEmail);

        // Act
        var dequeued = await _queue.DequeueAsync();

        // Assert
        Assert.NotNull(dequeued);
        Assert.Equal(dueEmail.Id, dequeued.Id);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesEmailStatus()
    {
        // Arrange
        var email = new QueuedEmail
        {
            Request = new MailRequest(
                to: ["test@example.com"],
                subject: "Test",
                body: "Body"),
            Status = EmailStatus.Pending
        };

        await _queue.EnqueueAsync(email);

        // Act
        email.Status = EmailStatus.Sent;
        await _queue.UpdateAsync(email);

        var updated = await _queue.GetByIdAsync(email.Id);

        // Assert
        Assert.NotNull(updated);
        Assert.Equal(EmailStatus.Sent, updated.Status);
    }

    [Fact]
    public async Task GetByStatusAsync_ReturnsEmailsWithMatchingStatus()
    {
        // Arrange
        var pendingEmail = new QueuedEmail
        {
            Request = new MailRequest(
                to: ["test1@example.com"],
                subject: "Pending",
                body: "Body"),
            Status = EmailStatus.Pending
        };

        var sentEmail = new QueuedEmail
        {
            Request = new MailRequest(
                to: ["test2@example.com"],
                subject: "Sent",
                body: "Body"),
            Status = EmailStatus.Sent
        };

        await _queue.EnqueueAsync(pendingEmail);
        await _queue.EnqueueAsync(sentEmail);

        // Act
        var pendingEmails = await _queue.GetByStatusAsync(EmailStatus.Pending);

        // Assert
        Assert.Single(pendingEmails);
        Assert.Contains(pendingEmails, e => e.Id == pendingEmail.Id);
    }

    [Fact]
    public async Task DeleteAsync_RemovesEmailFromQueue()
    {
        // Arrange
        var email = new QueuedEmail
        {
            Request = new MailRequest(
                to: ["test@example.com"],
                subject: "Test",
                body: "Body")
        };

        await _queue.EnqueueAsync(email);

        // Act
        await _queue.DeleteAsync(email.Id);
        var retrieved = await _queue.GetByIdAsync(email.Id);

        // Assert
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task GetPendingCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        for (int i = 0; i < 5; i++)
        {
            await _queue.EnqueueAsync(new QueuedEmail
            {
                Request = new MailRequest(
                    to: [$"test{i}@example.com"],
                    subject: "Test",
                    body: "Body"),
                Status = i < 3 ? EmailStatus.Pending : EmailStatus.Sent
            });
        }

        // Act
        var count = await _queue.GetPendingCountAsync();

        // Assert
        Assert.Equal(3, count);
    }
}
