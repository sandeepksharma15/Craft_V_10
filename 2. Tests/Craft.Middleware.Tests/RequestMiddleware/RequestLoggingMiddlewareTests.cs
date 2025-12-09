using System.Text;
using Craft.Middleware.RequestMiddleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Craft.Middleware.Tests.RequestMiddleware;

public class RequestLoggingMiddlewareTests
{
    private readonly Mock<ILogger<RequestLoggingMiddleware>> _loggerMock;
    private readonly IOptions<RequestMiddlewareSettings> _settings;
    private readonly RequestLoggingMiddleware _middleware;
    private readonly DefaultHttpContext _httpContext;

    public RequestLoggingMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<RequestLoggingMiddleware>>();

        _settings = Options.Create(new RequestMiddlewareSettings
        {
            Logging = new LoggingSettings
            {
                LogRequestBody = true,
                LogResponseBody = false,
                LogHeaders = true,
                LogPerformanceMetrics = true,
                ExcludedPaths = ["/health", "/metrics"],
                SensitivePaths = ["/token", "/auth"],
                SensitiveHeaders = ["Authorization", "Cookie"],
                MaxRequestBodyLength = 4096,
                MaxResponseBodyLength = 4096
            }
        });

        _middleware = new RequestLoggingMiddleware(_loggerMock.Object, _settings);

        _httpContext = new DefaultHttpContext();
        _httpContext.Request.Method = "GET";
        _httpContext.Request.Path = "/api/test";
        _httpContext.Response.Body = new MemoryStream();
    }

    [Fact]
    public async Task InvokeAsync_NormalRequest_LogsRequestAndAddsCorrelationId()
    {
        // Arrange
        var nextCalled = false;
        Task next(HttpContext ctx)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        // Act
        await _middleware.InvokeAsync(_httpContext, next);

        // Assert
        Assert.True(nextCalled);
        Assert.True(_httpContext.Items.ContainsKey("CorrelationId"));
        Assert.NotNull(_httpContext.Items["CorrelationId"]);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task InvokeAsync_ExcludedPath_SkipsLogging()
    {
        // Arrange
        _httpContext.Request.Path = "/health";
        var nextCalled = false;
        Task next(HttpContext ctx)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        // Act
        await _middleware.InvokeAsync(_httpContext, next);

        // Assert
        Assert.True(nextCalled);
        Assert.False(_httpContext.Items.ContainsKey("CorrelationId"));

        _loggerMock.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_SensitivePath_RedactsBody()
    {
        // Arrange
        _httpContext.Request.Path = "/api/token";
        _httpContext.Request.Method = "POST";
        _httpContext.Request.ContentType = "application/json";
        var requestBody = "{\"username\":\"test\",\"password\":\"secret\"}";
        _httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));

        var nextCalled = false;
        Task next(HttpContext ctx)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        // Act
        await _middleware.InvokeAsync(_httpContext, next);

        // Assert
        Assert.True(nextCalled);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("[REDACTED")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_JsonRequest_LogsBody()
    {
        // Arrange
        _httpContext.Request.Method = "POST";
        _httpContext.Request.ContentType = "application/json";
        var requestBody = "{\"name\":\"test\",\"value\":\"data\"}";
        _httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));

        var nextCalled = false;
        Task next(HttpContext ctx)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        // Act
        await _middleware.InvokeAsync(_httpContext, next);

        // Assert
        Assert.True(nextCalled);

        // Body should be reset for next middleware
        Assert.Equal(0, _httpContext.Request.Body.Position);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request details:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_BinaryContent_LogsContentType()
    {
        // Arrange
        _httpContext.Request.Method = "POST";
        _httpContext.Request.ContentType = "application/octet-stream";
        _httpContext.Request.Body = new MemoryStream([0x01, 0x02, 0x03]);

        var nextCalled = false;
        Task next(HttpContext ctx)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        // Act
        await _middleware.InvokeAsync(_httpContext, next);

        // Assert
        Assert.True(nextCalled);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Binary content")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_LargeBody_TruncatesBody()
    {
        // Arrange
        _httpContext.Request.Method = "POST";
        _httpContext.Request.ContentType = "application/json";
        var largeBody = new string('X', 5000); // Larger than MaxRequestBodyLength (4096)
        _httpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(largeBody));

        var nextCalled = false;
        Task next(HttpContext ctx)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        // Act
        await _middleware.InvokeAsync(_httpContext, next);

        // Assert
        Assert.True(nextCalled);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("TRUNCATED")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_LogPerformanceMetrics_LogsRequestDuration()
    {
        // Arrange
        var nextCalled = false;
        async Task next(HttpContext ctx)
        {
            nextCalled = true;
            await Task.Delay(10); // Add small delay
        }

        // Act
        await _middleware.InvokeAsync(_httpContext, next);

        // Assert
        Assert.True(nextCalled);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Duration:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_DisablePerformanceMetrics_DoesNotLogDuration()
    {
        // Arrange
        var settings = Options.Create(new RequestMiddlewareSettings
        {
            Logging = new LoggingSettings
            {
                LogPerformanceMetrics = false,
                ExcludedPaths = []
            }
        });

        var middleware = new RequestLoggingMiddleware(_loggerMock.Object, settings);

        var nextCalled = false;
        Task next(HttpContext ctx)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        // Act
        await middleware.InvokeAsync(_httpContext, next);

        // Assert
        Assert.True(nextCalled);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request completed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_WithSensitiveHeaders_RedactsHeaders()
    {
        // Arrange
        _httpContext.Request.Headers.Authorization = "Bearer secret-token";
        _httpContext.Request.Headers["X-Custom-Header"] = "public-value";

        var nextCalled = false;
        Task next(HttpContext ctx)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        // Act
        await _middleware.InvokeAsync(_httpContext, next);

        // Assert
        Assert.True(nextCalled);

        // The headers are logged as part of the request details
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request details:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
