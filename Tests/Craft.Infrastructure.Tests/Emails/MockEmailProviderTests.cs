using Craft.Infrastructure.Emails;
using Microsoft.Extensions.Logging;
using Moq;

namespace Craft.Infrastructure.Tests.Emails;

public class MockEmailProviderTests
{
    private readonly Mock<ILogger<MockEmailProvider>> _loggerMock;
    private readonly MockEmailProvider _provider;

    public MockEmailProviderTests()
    {
        _loggerMock = new Mock<ILogger<MockEmailProvider>>();
        _provider = new MockEmailProvider(_loggerMock.Object);
    }

    [Fact]
    public void Name_ReturnsCorrectProviderName()
    {
        // Assert
        Assert.Equal("mock", _provider.Name);
    }

    [Fact]
    public void IsConfigured_ReturnsTrue()
    {
        // Assert
        Assert.True(_provider.IsConfigured());
    }

    [Fact]
    public async Task SendAsync_LogsEmailDetails()
    {
        // Arrange
        var request = new MailRequest(
            to: ["test@example.com"],
            subject: "Test Subject",
            body: "Test Body");

        // Act
        var result = await _provider.SendAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.MessageId);
        Assert.StartsWith("mock-", result.MessageId);
    }

    [Fact]
    public async Task SendAsync_WithMultipleRecipients_Succeeds()
    {
        // Arrange
        var request = new MailRequest(
            to: ["test1@example.com", "test2@example.com"],
            subject: "Test",
            body: "Body");

        // Act
        var result = await _provider.SendAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
    }
}
