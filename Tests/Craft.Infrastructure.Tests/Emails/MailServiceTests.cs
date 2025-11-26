using Craft.Infrastructure.Emails;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Craft.Infrastructure.Tests.Emails;

public class MailServiceTests
{
    private readonly Mock<IEmailProviderFactory> _providerFactoryMock;
    private readonly Mock<IEmailTemplateRenderer> _templateRendererMock;
    private readonly Mock<IEmailQueue> _emailQueueMock;
    private readonly Mock<IEmailProvider> _emailProviderMock;
    private readonly Mock<ILogger<MailService>> _loggerMock;
    private readonly EmailOptions _options;
    private readonly MailService _mailService;

    public MailServiceTests()
    {
        _providerFactoryMock = new Mock<IEmailProviderFactory>();
        _templateRendererMock = new Mock<IEmailTemplateRenderer>();
        _emailQueueMock = new Mock<IEmailQueue>();
        _emailProviderMock = new Mock<IEmailProvider>();
        _loggerMock = new Mock<ILogger<MailService>>();

        _options = new EmailOptions
        {
            Provider = "smtp",
            From = "test@example.com",
            MaxRetryAttempts = 3,
            RetryDelaySeconds = 1
        };

        _providerFactoryMock
            .Setup(x => x.GetDefaultProvider())
            .Returns(_emailProviderMock.Object);

        var optionsMock = new Mock<IOptions<EmailOptions>>();
        optionsMock.Setup(x => x.Value).Returns(_options);

        _mailService = new MailService(
            _providerFactoryMock.Object,
            _templateRendererMock.Object,
            _emailQueueMock.Object,
            optionsMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task SendAsync_WithValidRequest_ReturnsSuccessResult()
    {
        // Arrange
        var request = new MailRequest(
            to: ["test@example.com"],
            subject: "Test",
            body: "Body");

        _emailProviderMock
            .Setup(x => x.SendAsync(It.IsAny<MailRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmailResult.Success("message-id"));

        // Act
        var result = await _mailService.SendAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("message-id", result.MessageId);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task SendAsync_WithFailure_RetriesAndReturnsFailure()
    {
        // Arrange
        var request = new MailRequest(
            to: ["test@example.com"],
            subject: "Test",
            body: "Body");

        _emailProviderMock
            .Setup(x => x.SendAsync(It.IsAny<MailRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmailResult.Failure("Connection failed"));

        // Act
        var result = await _mailService.SendAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Connection failed", result.ErrorMessage);
        _emailProviderMock.Verify(
            x => x.SendAsync(It.IsAny<MailRequest>(), It.IsAny<CancellationToken>()),
            Times.Exactly(4)); // 1 initial + 3 retries
    }

    [Fact]
    public async Task SendAsync_SucceedsOnRetry_ReturnsSuccess()
    {
        // Arrange
        var request = new MailRequest(
            to: ["test@example.com"],
            subject: "Test",
            body: "Body");

        var attempts = 0;
        _emailProviderMock
            .Setup(x => x.SendAsync(It.IsAny<MailRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                attempts++;
                return attempts < 3
                    ? EmailResult.Failure("Temporary failure")
                    : EmailResult.Success("message-id");
            });

        // Act
        var result = await _mailService.SendAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("message-id", result.MessageId);
        Assert.Equal(3, attempts);
    }

    [Fact]
    public async Task QueueAsync_WithValidRequest_ReturnsEmailId()
    {
        // Arrange
        var request = new MailRequest(
            to: ["test@example.com"],
            subject: "Test",
            body: "Body");

        // Act
        var emailId = await _mailService.QueueAsync(request);

        // Assert
        Assert.NotNull(emailId);
        Assert.NotEmpty(emailId);
        _emailQueueMock.Verify(
            x => x.EnqueueAsync(It.Is<QueuedEmail>(e =>
                e.Request == request &&
                e.Status == EmailStatus.Pending &&
                e.Priority == EmailPriority.Normal), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task QueueAsync_WithPriority_SetsCorrectPriority()
    {
        // Arrange
        var request = new MailRequest(
            to: ["test@example.com"],
            subject: "Test",
            body: "Body");

        // Act
        await _mailService.QueueAsync(request, EmailPriority.High);

        // Assert
        _emailQueueMock.Verify(
            x => x.EnqueueAsync(It.Is<QueuedEmail>(e => e.Priority == EmailPriority.High), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task QueueAsync_WithScheduledTime_SetsScheduledStatus()
    {
        // Arrange
        var request = new MailRequest(
            to: ["test@example.com"],
            subject: "Test",
            body: "Body");
        var scheduledFor = DateTimeOffset.UtcNow.AddHours(1);

        // Act
        await _mailService.QueueAsync(request, scheduledFor: scheduledFor);

        // Assert
        _emailQueueMock.Verify(
            x => x.EnqueueAsync(It.Is<QueuedEmail>(e =>
                e.Status == EmailStatus.Scheduled &&
                e.ScheduledFor == scheduledFor), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SendTemplateAsync_RendersTemplateAndSends()
    {
        // Arrange
        var model = new { Name = "Test User" };
        var renderedHtml = "<html>Hello Test User</html>";

        _templateRendererMock
            .Setup(x => x.RenderAsync("Welcome", model, It.IsAny<CancellationToken>()))
            .ReturnsAsync(renderedHtml);

        _emailProviderMock
            .Setup(x => x.SendAsync(It.IsAny<MailRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmailResult.Success());

        // Act
        var result = await _mailService.SendTemplateAsync(
            to: ["test@example.com"],
            subject: "Welcome",
            templateName: "Welcome",
            model: model);

        // Assert
        Assert.True(result.IsSuccess);
        _templateRendererMock.Verify(
            x => x.RenderAsync("Welcome", model, It.IsAny<CancellationToken>()),
            Times.Once);
        _emailProviderMock.Verify(
            x => x.SendAsync(It.Is<MailRequest>(r => r.Body == renderedHtml), It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task QueueTemplateAsync_RendersTemplateAndQueues()
    {
        // Arrange
        var model = new { Name = "Test User" };
        var renderedHtml = "<html>Hello Test User</html>";

        _templateRendererMock
            .Setup(x => x.RenderAsync("Welcome", model, It.IsAny<CancellationToken>()))
            .ReturnsAsync(renderedHtml);

        // Act
        var emailId = await _mailService.QueueTemplateAsync(
            to: ["test@example.com"],
            subject: "Welcome",
            templateName: "Welcome",
            model: model);

        // Assert
        Assert.NotNull(emailId);
        _templateRendererMock.Verify(
            x => x.RenderAsync("Welcome", model, It.IsAny<CancellationToken>()),
            Times.Once);
        _emailQueueMock.Verify(
            x => x.EnqueueAsync(It.Is<QueuedEmail>(e => e.Request.Body == renderedHtml), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PreviewTemplateAsync_ReturnsRenderedHtml()
    {
        // Arrange
        var model = new { Name = "Preview User" };
        var renderedHtml = "<html>Hello Preview User</html>";

        _templateRendererMock
            .Setup(x => x.RenderAsync("Welcome", model, It.IsAny<CancellationToken>()))
            .ReturnsAsync(renderedHtml);

        // Act
        var result = await _mailService.PreviewTemplateAsync("Welcome", model);

        // Assert
        Assert.Equal(renderedHtml, result);
        _templateRendererMock.Verify(
            x => x.RenderAsync("Welcome", model, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetQueuedEmailAsync_ReturnsEmailFromQueue()
    {
        // Arrange
        var emailId = "test-id";
        var queuedEmail = new QueuedEmail { Id = emailId };

        _emailQueueMock
            .Setup(x => x.GetByIdAsync(emailId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(queuedEmail);

        // Act
        var result = await _mailService.GetQueuedEmailAsync(emailId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(emailId, result.Id);
    }
}
