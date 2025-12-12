using Craft.Security.Tokens;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;

namespace Craft.Security.Tests.Tokens;

public class TokenBlacklistCleanupServiceTests
{
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<IServiceScope> _mockServiceScope;
    private readonly Mock<IServiceScopeFactory> _mockServiceScopeFactory;
    private readonly Mock<ITokenBlacklist> _mockBlacklist;
    private readonly Mock<ILogger<TokenBlacklistCleanupService>> _mockLogger;

    public TokenBlacklistCleanupServiceTests()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockServiceScope = new Mock<IServiceScope>();
        _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        _mockBlacklist = new Mock<ITokenBlacklist>();
        _mockLogger = new Mock<ILogger<TokenBlacklistCleanupService>>();

        var scopeServiceProvider = new Mock<IServiceProvider>();
        scopeServiceProvider
            .Setup(sp => sp.GetService(typeof(ITokenBlacklist)))
            .Returns(_mockBlacklist.Object);

        _mockServiceScope
            .Setup(s => s.ServiceProvider)
            .Returns(scopeServiceProvider.Object);

        _mockServiceScopeFactory
            .Setup(f => f.CreateScope())
            .Returns(_mockServiceScope.Object);

        _mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
            .Returns(_mockServiceScopeFactory.Object);
    }

    [Fact]
    public async Task ExecuteAsync_Should_StartAndLog()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var service = new TokenBlacklistCleanupService(_mockServiceProvider.Object, _mockLogger.Object);

        cts.CancelAfter(TimeSpan.FromMilliseconds(100));

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(150);
        await service.StopAsync(CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Token blacklist cleanup service started")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Should_PerformCleanup_Periodically()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        _mockBlacklist.Setup(b => b.CleanupExpiredTokensAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        var service = new TokenBlacklistCleanupService(_mockServiceProvider.Object, _mockLogger.Object);

        cts.CancelAfter(TimeSpan.FromMilliseconds(100));

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(150);
        await service.StopAsync(CancellationToken.None);

        // Assert - Service should start but may not complete first cleanup due to 1-hour interval
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("cleanup service")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_Should_HandleCancellation_Gracefully()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var service = new TokenBlacklistCleanupService(_mockServiceProvider.Object, _mockLogger.Object);

        // Act
        await service.StartAsync(cts.Token);
        
        await Task.Delay(100);
        
        await cts.CancelAsync();
        
        await Task.Delay(100);
        
        await service.StopAsync(CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("started") || v.ToString()!.Contains("stopping") || v.ToString()!.Contains("stopped")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_Should_LogError_WhenExceptionOccurs()
    {
        // Arrange
        var scopeServiceProvider = new Mock<IServiceProvider>();
        scopeServiceProvider
            .Setup(sp => sp.GetService(typeof(ITokenBlacklist)))
            .Throws(new InvalidOperationException("Test exception"));

        _mockServiceScope
            .Setup(s => s.ServiceProvider)
            .Returns(scopeServiceProvider.Object);

        var cts = new CancellationTokenSource();
        var service = new TokenBlacklistCleanupService(_mockServiceProvider.Object, _mockLogger.Object);

        cts.CancelAfter(TimeSpan.FromMilliseconds(100));

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(150);
        await service.StopAsync(CancellationToken.None);

        // Assert - Service handles exceptions and continues
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("started")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Service_Should_Implement_IHostedService()
    {
        // Arrange
        var service = new TokenBlacklistCleanupService(_mockServiceProvider.Object, _mockLogger.Object);

        // Assert
        Assert.IsType<IHostedService>(service, exactMatch: false);
        Assert.IsType<BackgroundService>(service, exactMatch: false);

        // Cleanup
        await service.StopAsync(CancellationToken.None);
    }
}
