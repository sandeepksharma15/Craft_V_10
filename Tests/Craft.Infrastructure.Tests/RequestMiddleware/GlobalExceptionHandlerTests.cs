using System.Net;
using System.Text;
using System.Text.Json;
using Craft.Exceptions;
using Craft.Infrastructure.RequestMiddleware;
using Craft.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Craft.Infrastructure.Tests.RequestMiddleware;

public class GlobalExceptionHandlerTests
{
    private readonly Mock<ILogger<GlobalExceptionHandler>> _loggerMock;
    private readonly Mock<IWebHostEnvironment> _environmentMock;
    private readonly Mock<ICurrentUser<Guid>> _currentUserMock;
    private readonly IOptions<SystemSettings> _settings;
    private readonly GlobalExceptionHandler _handler;
    private readonly DefaultHttpContext _httpContext;

    public GlobalExceptionHandlerTests()
    {
        _loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        _environmentMock = new Mock<IWebHostEnvironment>();
        _currentUserMock = new Mock<ICurrentUser<Guid>>();

        _settings = Options.Create(new SystemSettings
        {
            ExceptionHandling = new ExceptionHandlingSettings
            {
                IncludeDiagnostics = true,
                IncludeInnerException = true,
                IncludeStackTrace = false,
                UseProblemDetails = true
            }
        });

        _environmentMock.Setup(e => e.EnvironmentName).Returns(Environments.Production);

        _currentUserMock.Setup(u => u.IsAuthenticated()).Returns(false);
        _currentUserMock.Setup(u => u.GetEmail()).Returns((string?)null);
        _currentUserMock.Setup(u => u.GetId()).Returns(Guid.Empty);
        _currentUserMock.Setup(u => u.GetTenant()).Returns((string?)null);

        _handler = new GlobalExceptionHandler(
            _loggerMock.Object,
            _settings,
            _environmentMock.Object,
            _currentUserMock.Object);

        _httpContext = new DefaultHttpContext();
        _httpContext.Response.Body = new MemoryStream();
    }

    [Fact]
    public async Task TryHandleAsync_StandardException_ReturnsProblemDetails()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(400, _httpContext.Response.StatusCode);
        Assert.Equal("application/json", _httpContext.Response.ContentType);

        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody);

        Assert.NotNull(problemDetails);
        Assert.Equal(400, problemDetails.Status);
        Assert.Equal("Test error", problemDetails.Detail);
    }

    [Fact]
    public async Task TryHandleAsync_CraftException_UsesCustomStatusCode()
    {
        // Arrange
        var exception = new TestCraftException("Custom error", HttpStatusCode.Conflict);

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(409, _httpContext.Response.StatusCode);

        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody);

        Assert.NotNull(problemDetails);
        Assert.Equal(409, problemDetails.Status);
    }

    [Fact]
    public async Task TryHandleAsync_KeyNotFoundException_Returns404()
    {
        // Arrange
        var exception = new KeyNotFoundException("Resource not found");

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(404, _httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_UnauthorizedException_Returns401()
    {
        // Arrange
        var exception = new UnauthorizedAccessException("Access denied");

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(401, _httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_WithUserContext_IncludesUserInfo()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userEmail = "test@example.com";
        var tenant = "tenant-001";

        _currentUserMock.Setup(u => u.IsAuthenticated()).Returns(true);
        _currentUserMock.Setup(u => u.GetId()).Returns(userId);
        _currentUserMock.Setup(u => u.GetEmail()).Returns(userEmail);
        _currentUserMock.Setup(u => u.GetTenant()).Returns(tenant);

        var exception = new InvalidOperationException("Test error");

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        Assert.True(result);

        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var document = JsonDocument.Parse(responseBody);

        Assert.True(document.RootElement.TryGetProperty("userId", out var userIdProperty));
        Assert.Equal(userId.ToString(), userIdProperty.GetString());

        Assert.True(document.RootElement.TryGetProperty("userEmail", out var emailProperty));
        Assert.Equal(userEmail, emailProperty.GetString());

        Assert.True(document.RootElement.TryGetProperty("tenant", out var tenantProperty));
        Assert.Equal(tenant, tenantProperty.GetString());
    }

    [Fact]
    public async Task TryHandleAsync_IncludesDiagnostics_WhenEnabled()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        Assert.True(result);

        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var document = JsonDocument.Parse(responseBody);

        Assert.True(document.RootElement.TryGetProperty("errorId", out _));
        Assert.True(document.RootElement.TryGetProperty("correlationId", out _));
        Assert.True(document.RootElement.TryGetProperty("timestamp", out _));
    }

    [Fact]
    public async Task TryHandleAsync_IncludesStackTrace_InDevelopment()
    {
        // Arrange
        _environmentMock.Setup(e => e.EnvironmentName).Returns(Environments.Development);

        var settings = Options.Create(new SystemSettings
        {
            ExceptionHandling = new ExceptionHandlingSettings
            {
                IncludeDiagnostics = true,
                IncludeStackTrace = true
            }
        });

        var handler = new GlobalExceptionHandler(
            _loggerMock.Object,
            settings,
            _environmentMock.Object,
            _currentUserMock.Object);

        var exception = new InvalidOperationException("Test error");

        // Act
        var result = await handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        Assert.True(result);

        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var document = JsonDocument.Parse(responseBody);

        Assert.True(document.RootElement.TryGetProperty("stackTrace", out _));
        Assert.True(document.RootElement.TryGetProperty("exceptionType", out _));
    }

    [Fact]
    public async Task TryHandleAsync_CraftExceptionWithErrors_IncludesErrorList()
    {
        // Arrange
        var errors = new List<string> { "Error 1", "Error 2", "Error 3" };
        var exception = new TestCraftException("Validation failed", errors, HttpStatusCode.BadRequest);

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        Assert.True(result);

        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var document = JsonDocument.Parse(responseBody);

        Assert.True(document.RootElement.TryGetProperty("errors", out var errorsProperty));
        var errorArray = errorsProperty.EnumerateArray().Select(e => e.GetString()).ToList();

        Assert.Equal(3, errorArray.Count);
        Assert.Contains("Error 1", errorArray);
        Assert.Contains("Error 2", errorArray);
        Assert.Contains("Error 3", errorArray);
    }

    [Fact]
    public async Task TryHandleAsync_WithCorrelationId_UsesExistingId()
    {
        // Arrange
        var correlationId = "test-correlation-id";
        _httpContext.Items["CorrelationId"] = correlationId;

        var exception = new InvalidOperationException("Test error");

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        Assert.True(result);

        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var document = JsonDocument.Parse(responseBody);

        Assert.True(document.RootElement.TryGetProperty("correlationId", out var correlationIdProperty));
        Assert.Equal(correlationId, correlationIdProperty.GetString());
    }

    [Fact]
    public async Task TryHandleAsync_LogsErrorWithCorrectLevel()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");

        // Act
        await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task TryHandleAsync_ServerError_LogsAsError()
    {
        // Arrange
        var exception = new Exception("Server error");

        // Act
        await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private class TestCraftException : CraftException
    {
        public TestCraftException(string message, HttpStatusCode statusCode)
            : base(message, (List<string>?)null, statusCode)
        {
        }

        public TestCraftException(string message, List<string> errors, HttpStatusCode statusCode)
            : base(message, errors, statusCode)
        {
        }
    }
}
