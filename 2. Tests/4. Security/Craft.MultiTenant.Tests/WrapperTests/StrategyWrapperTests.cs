using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Craft.MultiTenant.Tests.WrapperTests;

public class StrategyWrapperTests
{
    private readonly ILogger<StrategyWrapperTests> _logger = NullLogger<StrategyWrapperTests>.Instance;

    [Fact]
    public async Task GetIdentifierAsync_CallsUnderlyingStrategy()
    {
        var mockStrategy = new Mock<ITenantStrategy>();
        mockStrategy.Setup(s => s.GetIdentifierAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync("test-tenant");
        
        var wrapper = new StrategyWrapper(mockStrategy.Object, _logger);
        var result = await wrapper.GetIdentifierAsync(new DefaultHttpContext());
        
        Assert.Equal("test-tenant", result);
        mockStrategy.Verify(s => s.GetIdentifierAsync(It.IsAny<HttpContext>()), Times.Once);
    }

    [Fact]
    public async Task GetIdentifierAsync_LogsDebugWhenIdentifierFound()
    {
        var mockStrategy = new Mock<ITenantStrategy>();
        var mockLogger = new Mock<ILogger>();
        mockStrategy.Setup(s => s.GetIdentifierAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync("test-tenant");
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
        
        var wrapper = new StrategyWrapper(mockStrategy.Object, mockLogger.Object);
        await wrapper.GetIdentifierAsync(new DefaultHttpContext());
        
        mockLogger.Verify(l => l.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Found identifier")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task GetIdentifierAsync_LogsDebugWhenIdentifierNotFound()
    {
        var mockStrategy = new Mock<ITenantStrategy>();
        var mockLogger = new Mock<ILogger>();
        mockStrategy.Setup(s => s.GetIdentifierAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync((string?)null);
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
        
        var wrapper = new StrategyWrapper(mockStrategy.Object, mockLogger.Object);
        await wrapper.GetIdentifierAsync(new DefaultHttpContext());
        
        mockLogger.Verify(l => l.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No identifier found")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task GetIdentifierAsync_WrapsExceptionInMultiTenantException()
    {
        var mockStrategy = new Mock<ITenantStrategy>();
        mockStrategy.Setup(s => s.GetIdentifierAsync(It.IsAny<HttpContext>()))
            .ThrowsAsync(new InvalidOperationException("Test error"));
        
        var wrapper = new StrategyWrapper(mockStrategy.Object, _logger);
        
        var exception = await Assert.ThrowsAsync<MultiTenantException>(
            () => wrapper.GetIdentifierAsync(new DefaultHttpContext()));
        
        Assert.Contains("GetIdentifierAsync", exception.Message);
        Assert.IsType<InvalidOperationException>(exception.InnerException);
    }

    [Fact]
    public async Task GetIdentifierAsync_LogsErrorOnException()
    {
        var mockStrategy = new Mock<ITenantStrategy>();
        var mockLogger = new Mock<ILogger>();
        mockStrategy.Setup(s => s.GetIdentifierAsync(It.IsAny<HttpContext>()))
            .ThrowsAsync(new InvalidOperationException("Test error"));
        
        var wrapper = new StrategyWrapper(mockStrategy.Object, mockLogger.Object);
        
        try
        {
            await wrapper.GetIdentifierAsync(new DefaultHttpContext());
        }
        catch
        {
        }
        
        mockLogger.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void Constructor_ThrowsOnNullStrategy()
    {
        Assert.Throws<ArgumentNullException>(
            () => new StrategyWrapper(null!, _logger));
    }

    [Fact]
    public void Constructor_ThrowsOnNullLogger()
    {
        var mockStrategy = new Mock<ITenantStrategy>();
        
        Assert.Throws<ArgumentNullException>(
            () => new StrategyWrapper(mockStrategy.Object, null!));
    }
}
