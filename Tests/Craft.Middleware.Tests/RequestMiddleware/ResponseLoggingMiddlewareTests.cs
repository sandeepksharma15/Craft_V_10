using Craft.Middleware.RequestMiddleware;
using Craft.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Craft.Middleware.Tests.RequestMiddleware;

public class ResponseLoggingMiddlewareTests
{
    private readonly Mock<ILogger<ResponseLoggingMiddleware>> _loggerMock;
    private readonly Mock<ICurrentUser<Guid>> _currentUserMock;
    private readonly IOptions<RequestMiddlewareSettings> _settings;
    private readonly ResponseLoggingMiddleware _middleware;
    private readonly DefaultHttpContext _httpContext;

    public ResponseLoggingMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<ResponseLoggingMiddleware>>();
        _currentUserMock = new Mock<ICurrentUser<Guid>>();

        _settings = Options.Create(new RequestMiddlewareSettings
        {
            Logging = new LoggingSettings
            {
                LogRequestBody = false,
                LogResponseBody = true,
                LogHeaders = true,
                ExcludedPaths = ["/health", "/metrics"],
                SensitivePaths = ["/token", "/auth"],
                SensitiveHeaders = ["Authorization", "Set-Cookie"],
                MaxResponseBodyLength = 4096
            }
        });

        _currentUserMock.Setup(u => u.IsAuthenticated()).Returns(false);
        _currentUserMock.Setup(u => u.GetEmail()).Returns((string?)null);
        _currentUserMock.Setup(u => u.GetId()).Returns(Guid.Empty);
        _currentUserMock.Setup(u => u.GetTenant()).Returns((string?)null);

        _middleware = new ResponseLoggingMiddleware(_loggerMock.Object, _settings, _currentUserMock.Object);

        _httpContext = new DefaultHttpContext();
        _httpContext.Request.Method = "GET";
        _httpContext.Request.Path = "/api/test";
        _httpContext.Response.Body = new MemoryStream();
    }

    [Fact]
    public async Task InvokeAsync_NormalResponse_LogsResponse()
    {
        // Arrange
        _httpContext.Items["CorrelationId"] = "test-correlation-id";

        var nextCalled = false;
        async Task next(HttpContext ctx)
        {
            nextCalled = true;
            ctx.Response.ContentType = "application/json";
            ctx.Response.StatusCode = 200;
            await ctx.Response.WriteAsync("{\"message\":\"success\"}");
        }

        // Act
        await _middleware.InvokeAsync(_httpContext, next);

        // Assert
        Assert.True(nextCalled);

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
            ctx.Response.StatusCode = 200;
            return Task.CompletedTask;
        }

        // Act
        await _middleware.InvokeAsync(_httpContext, next);

        // Assert
        Assert.True(nextCalled);

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
    public async Task InvokeAsync_SensitivePath_RedactsResponseBody()
    {
        // Arrange
        _httpContext.Request.Path = "/api/token";
        _httpContext.Items["CorrelationId"] = "test-correlation-id";

        var nextCalled = false;
        async Task next(HttpContext ctx)
        {
            nextCalled = true;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync("{\"access_token\":\"secret-token\"}");
        }

        // Act
        await _middleware.InvokeAsync(_httpContext, next);

        // Assert
        Assert.True(nextCalled);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);

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
    public async Task InvokeAsync_WithAuthenticatedUser_LogsUserInfo()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userEmail = "test@example.com";
        var tenant = "tenant-001";

        _currentUserMock.Setup(u => u.IsAuthenticated()).Returns(true);
        _currentUserMock.Setup(u => u.GetId()).Returns(userId);
        _currentUserMock.Setup(u => u.GetEmail()).Returns(userEmail);
        _currentUserMock.Setup(u => u.GetTenant()).Returns(tenant);

        _httpContext.Items["CorrelationId"] = "test-correlation-id";

        var nextCalled = false;
        async Task next(HttpContext ctx)
        {
            nextCalled = true;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync("{\"data\":\"test\"}");
        }

        // Act
        await _middleware.InvokeAsync(_httpContext, next);

        // Assert
        Assert.True(nextCalled);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(userEmail)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ErrorResponse_LogsAsWarning()
    {
        // Arrange
        _httpContext.Items["CorrelationId"] = "test-correlation-id";

        var nextCalled = false;
        async Task next(HttpContext ctx)
        {
            nextCalled = true;
            ctx.Response.StatusCode = 400;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync("{\"error\":\"Bad request\"}");
        }

        // Act
        await _middleware.InvokeAsync(_httpContext, next);

        // Assert
        Assert.True(nextCalled);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task InvokeAsync_BinaryResponse_LogsContentType()
    {
        // Arrange
        _httpContext.Items["CorrelationId"] = "test-correlation-id";

        var nextCalled = false;
        async Task next(HttpContext ctx)
        {
            nextCalled = true;
            ctx.Response.ContentType = "application/pdf";
            byte[] pdfHeader = [0x25, 0x50, 0x44, 0x46]; // PDF header
            await ctx.Response.Body.WriteAsync(pdfHeader.AsMemory());
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
    public async Task InvokeAsync_LargeResponse_TruncatesBody()
    {
        // Arrange
        _httpContext.Items["CorrelationId"] = "test-correlation-id";

        var largeResponse = new string('X', 5000); // Larger than MaxResponseBodyLength (4096)

        var nextCalled = false;
        async Task next(HttpContext ctx)
        {
            nextCalled = true;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync(largeResponse);
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
    public async Task InvokeAsync_DisableResponseBodyLogging_OnlyLogsMetadata()
    {
        // Arrange
        var settings = Options.Create(new RequestMiddlewareSettings
        {
            Logging = new LoggingSettings
            {
                LogResponseBody = false,
                ExcludedPaths = []
            }
        });

        var middleware = new ResponseLoggingMiddleware(_loggerMock.Object, settings, _currentUserMock.Object);

        _httpContext.Items["CorrelationId"] = "test-correlation-id";

        var nextCalled = false;
        async Task next(HttpContext ctx)
        {
            nextCalled = true;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync("{\"data\":\"test\"}");
        }

        // Act
        await middleware.InvokeAsync(_httpContext, next);

        // Assert
        Assert.True(nextCalled);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Should not log response body details
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ResponseData")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_EmptyResponse_HandlesGracefully()
    {
        // Arrange
        _httpContext.Items["CorrelationId"] = "test-correlation-id";

        var nextCalled = false;
        Task next(HttpContext ctx)
        {
            nextCalled = true;
            ctx.Response.StatusCode = 204; // No Content
            return Task.CompletedTask;
        }

        // Act
        await _middleware.InvokeAsync(_httpContext, next);

        // Assert
        Assert.True(nextCalled);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_RestoresOriginalResponseStream()
    {
        // Arrange
        var originalStream = _httpContext.Response.Body;
        _httpContext.Items["CorrelationId"] = "test-correlation-id";

        var nextCalled = false;
        async Task next(HttpContext ctx)
        {
            nextCalled = true;
            await ctx.Response.WriteAsync("test");
        }

        // Act
        await _middleware.InvokeAsync(_httpContext, next);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal(originalStream, _httpContext.Response.Body);
    }
}
