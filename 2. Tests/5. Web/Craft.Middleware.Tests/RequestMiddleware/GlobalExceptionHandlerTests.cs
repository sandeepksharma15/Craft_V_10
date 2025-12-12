using System.Net;
using System.Text.Json;
using Craft.Core;
using Craft.Domain;
using Craft.Middleware.RequestMiddleware;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Craft.Middleware.Tests.RequestMiddleware;

public class GlobalExceptionHandlerTests
{
    private readonly Mock<ILogger<GlobalExceptionHandler>> _loggerMock;
    private readonly Mock<IWebHostEnvironment> _environmentMock;
    private readonly Mock<ICurrentUser<Guid>> _currentUserMock;
    private readonly IOptions<RequestMiddlewareSettings> _settings;
    private readonly GlobalExceptionHandler _handler;
    private readonly DefaultHttpContext _httpContext;

    public GlobalExceptionHandlerTests()
    {
        _loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        _environmentMock = new Mock<IWebHostEnvironment>();
        _currentUserMock = new Mock<ICurrentUser<Guid>>();

        _settings = Options.Create(new RequestMiddlewareSettings
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
        Assert.StartsWith("application/json", _httpContext.Response.ContentType);

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

        var settings = Options.Create(new RequestMiddlewareSettings
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

    [Fact]
    public async Task TryHandleAsync_ModelValidationException_ReturnsValidationProblemDetails()
    {
        // Arrange
        var validationErrors = new Dictionary<string, string[]>
        {
            { "Email", ["Email is required", "Email is not valid"] },
            { "Age", ["Age must be between 18 and 100"] }
        };
        var exception = new ModelValidationException("Validation failed", validationErrors);

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(400, _httpContext.Response.StatusCode);

        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var document = JsonDocument.Parse(responseBody);

        Assert.True(document.RootElement.TryGetProperty("title", out var titleProperty));
        Assert.Equal("One or more validation errors occurred", titleProperty.GetString());

        // ValidationProblemDetails has an "errors" property that is an object with field names as keys
        Assert.True(document.RootElement.TryGetProperty("errors", out var errorsProperty));
        
        // If errors is an object (field-based validation errors from ValidationProblemDetails)
        if (errorsProperty.ValueKind == JsonValueKind.Object)
        {
            Assert.True(errorsProperty.TryGetProperty("Email", out var emailErrors));
            Assert.Equal(JsonValueKind.Array, emailErrors.ValueKind);
            Assert.Equal(2, emailErrors.GetArrayLength());
            
            Assert.True(errorsProperty.TryGetProperty("Age", out var ageErrors));
            Assert.Equal(JsonValueKind.Array, ageErrors.ValueKind);
            Assert.Equal(1, ageErrors.GetArrayLength());
        }
        // If errors is an array (flat error list from CraftException.Errors)
        else if (errorsProperty.ValueKind == JsonValueKind.Array)
        {
            // This would be the flat errors array from the extension
            Assert.True(errorsProperty.GetArrayLength() > 0);
        }
    }

    [Fact]
    public async Task TryHandleAsync_AlreadyExistsException_Returns422WithCustomTitle()
    {
        // Arrange
        var exception = new AlreadyExistsException("Product", "ABC123");

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(422, _httpContext.Response.StatusCode);

        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var document = JsonDocument.Parse(responseBody);

        Assert.True(document.RootElement.TryGetProperty("title", out var titleProperty));
        Assert.Equal("Resource already exists", titleProperty.GetString());
    }

    [Fact]
    public async Task TryHandleAsync_InvalidCredentialsException_Returns401WithCustomTitle()
    {
        // Arrange
        var exception = new InvalidCredentialsException("Invalid username or password");

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(401, _httpContext.Response.StatusCode);

        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var document = JsonDocument.Parse(responseBody);

        Assert.True(document.RootElement.TryGetProperty("title", out var titleProperty));
        Assert.Equal("Invalid credentials", titleProperty.GetString());
    }

    [Fact]
    public async Task TryHandleAsync_ForbiddenException_Returns403WithCustomTitle()
    {
        // Arrange
        var exception = new ForbiddenException("Access to this resource is forbidden");

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(403, _httpContext.Response.StatusCode);

        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var document = JsonDocument.Parse(responseBody);

        Assert.True(document.RootElement.TryGetProperty("title", out var titleProperty));
        Assert.Equal("Access forbidden", titleProperty.GetString());
    }

    [Fact]
    public async Task TryHandleAsync_UnauthorizedException_Returns401WithCustomTitle()
    {
        // Arrange
        var exception = new UnauthorizedException("You must be authenticated to access this resource");

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(401, _httpContext.Response.StatusCode);

        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var document = JsonDocument.Parse(responseBody);

        Assert.True(document.RootElement.TryGetProperty("title", out var titleProperty));
        Assert.Equal("Unauthorized access", titleProperty.GetString());
    }

    [Fact]
    public async Task TryHandleAsync_ConcurrencyException_Returns409WithConflictTitle()
    {
        // Arrange
        var exception = new ConcurrencyException("Order", 123);

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(409, _httpContext.Response.StatusCode);

        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var document = JsonDocument.Parse(responseBody);

        Assert.True(document.RootElement.TryGetProperty("title", out var titleProperty));
        Assert.Equal("Conflict", titleProperty.GetString());

        Assert.True(document.RootElement.TryGetProperty("detail", out var detailProperty));
        Assert.Contains("Concurrency conflict for entity \"Order\" (123)", detailProperty.GetString());
    }

    [Fact]
    public async Task TryHandleAsync_ConcurrencyExceptionWithVersions_Returns409WithDetailedMessage()
    {
        // Arrange
        var exception = new ConcurrencyException("Invoice", 456, "v1", "v2");

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(409, _httpContext.Response.StatusCode);

        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var document = JsonDocument.Parse(responseBody);

        Assert.True(document.RootElement.TryGetProperty("detail", out var detailProperty));
        var detail = detailProperty.GetString();
        Assert.Contains("Concurrency conflict for entity \"Invoice\" (456)", detail);
        Assert.Contains("Expected version: v1", detail);
        Assert.Contains("Actual version: v2", detail);
    }

    [Fact]
    public async Task TryHandleAsync_ConcurrencyExceptionWithErrors_IncludesErrorList()
    {
        // Arrange
        var errors = new List<string> 
        { 
            "Row version mismatch", 
            "Record modified by user@example.com at 10:30 AM" 
        };
        var exception = new ConcurrencyException("Concurrency conflict detected", errors);

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(409, _httpContext.Response.StatusCode);

        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var document = JsonDocument.Parse(responseBody);

        Assert.True(document.RootElement.TryGetProperty("errors", out var errorsProperty));
        var errorArray = errorsProperty.EnumerateArray().Select(e => e.GetString()).ToList();

        Assert.Equal(2, errorArray.Count);
        Assert.Contains("Row version mismatch", errorArray);
        Assert.Contains("Record modified by user@example.com at 10:30 AM", errorArray);
    }

    [Fact]
    public async Task TryHandleAsync_ConfigurationException_Returns500WithInternalServerErrorTitle()
    {
        // Arrange
        var exception = new ConfigurationException("Database:ConnectionString", "Value is missing or empty");

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(500, _httpContext.Response.StatusCode);

        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var document = JsonDocument.Parse(responseBody);

        Assert.True(document.RootElement.TryGetProperty("title", out var titleProperty));
        Assert.Equal("Internal Server Error", titleProperty.GetString());

        Assert.True(document.RootElement.TryGetProperty("detail", out var detailProperty));
        Assert.Contains("Database:ConnectionString", detailProperty.GetString());
    }

    [Fact]
    public async Task TryHandleAsync_DatabaseException_Returns500WithInternalServerErrorTitle()
    {
        // Arrange
        var exception = new DatabaseException("INSERT", "Unique constraint violation on column 'Email'");

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(500, _httpContext.Response.StatusCode);

        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var document = JsonDocument.Parse(responseBody);

        Assert.True(document.RootElement.TryGetProperty("title", out var titleProperty));
        Assert.Equal("Internal Server Error", titleProperty.GetString());

        Assert.True(document.RootElement.TryGetProperty("detail", out var detailProperty));
        Assert.Contains("Database error during INSERT", detailProperty.GetString());
    }

    [Fact]
    public async Task TryHandleAsync_ExternalServiceException_Returns502WithBadGatewayTitle()
    {
        // Arrange
        var exception = new ExternalServiceException("PaymentGateway", 503, "Service temporarily unavailable");

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(502, _httpContext.Response.StatusCode);

        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var document = JsonDocument.Parse(responseBody);

        Assert.True(document.RootElement.TryGetProperty("title", out var titleProperty));
        Assert.Equal("Bad Gateway", titleProperty.GetString());

        Assert.True(document.RootElement.TryGetProperty("detail", out var detailProperty));
        Assert.Contains("PaymentGateway", detailProperty.GetString());
        Assert.Contains("503", detailProperty.GetString());
    }

    [Fact]
    public async Task TryHandleAsync_ConfigurationExceptionWithErrors_IncludesErrorList()
    {
        // Arrange
        var errors = new List<string>
        {
            "ConnectionString is missing",
            "ApiKey is invalid"
        };
        var exception = new ConfigurationException("Configuration validation failed", errors);

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(500, _httpContext.Response.StatusCode);

        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var document = JsonDocument.Parse(responseBody);

        Assert.True(document.RootElement.TryGetProperty("errors", out var errorsProperty));
        var errorArray = errorsProperty.EnumerateArray().Select(e => e.GetString()).ToList();

        Assert.Equal(2, errorArray.Count);
        Assert.Contains("ConnectionString is missing", errorArray);
        Assert.Contains("ApiKey is invalid", errorArray);
    }

    [Fact]
    public async Task TryHandleAsync_DatabaseExceptionWithErrors_IncludesErrorList()
    {
        // Arrange
        var errors = new List<string>
        {
            "Connection pool exhausted",
            "Maximum retry attempts exceeded"
        };
        var exception = new DatabaseException("Database operation failed", errors);

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(500, _httpContext.Response.StatusCode);

        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var document = JsonDocument.Parse(responseBody);

        Assert.True(document.RootElement.TryGetProperty("errors", out var errorsProperty));
        var errorArray = errorsProperty.EnumerateArray().Select(e => e.GetString()).ToList();

        Assert.Equal(2, errorArray.Count);
        Assert.Contains("Connection pool exhausted", errorArray);
        Assert.Contains("Maximum retry attempts exceeded", errorArray);
    }

    [Fact]
    public async Task TryHandleAsync_ExternalServiceExceptionWithErrors_IncludesErrorList()
    {
        // Arrange
        var errors = new List<string>
        {
            "Service timeout after 30 seconds",
            "Retry attempts exhausted"
        };
        var exception = new ExternalServiceException("External service call failed", errors);

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(502, _httpContext.Response.StatusCode);

        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var document = JsonDocument.Parse(responseBody);

        Assert.True(document.RootElement.TryGetProperty("errors", out var errorsProperty));
        var errorArray = errorsProperty.EnumerateArray().Select(e => e.GetString()).ToList();

        Assert.Equal(2, errorArray.Count);
        Assert.Contains("Service timeout after 30 seconds", errorArray);
        Assert.Contains("Retry attempts exhausted", errorArray);
    }

    [Fact]
    public async Task TryHandleAsync_OperationCanceledException_Returns408()
    {
        // Arrange
        var exception = new OperationCanceledException("Request was cancelled");

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(408, _httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_ArgumentNullException_Returns400()
    {
        // Arrange
        var exception = new ArgumentNullException("parameter", "Parameter cannot be null");

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(400, _httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_NotImplementedException_Returns501()
    {
        // Arrange
        var exception = new NotImplementedException("Feature not implemented");

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(501, _httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_TimeoutException_Returns408()
    {
        // Arrange
        var exception = new TimeoutException("Request timeout");

        // Act
        var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal(408, _httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_IncludesInnerExceptionType_InDevelopment()
    {
        // Arrange
        _environmentMock.Setup(e => e.EnvironmentName).Returns(Environments.Development);

        var settings = Options.Create(new RequestMiddlewareSettings
        {
            ExceptionHandling = new ExceptionHandlingSettings
            {
                IncludeDiagnostics = true,
                IncludeStackTrace = true,
                IncludeInnerException = true
            }
        });

        var handler = new GlobalExceptionHandler(
            _loggerMock.Object,
            settings,
            _environmentMock.Object,
            _currentUserMock.Object);

        var innerException = new InvalidOperationException("Inner error");
        var exception = new Exception("Outer error", innerException);

        // Act
        var result = await handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        Assert.True(result);

        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var document = JsonDocument.Parse(responseBody);

        Assert.True(document.RootElement.TryGetProperty("innerException", out var innerExProperty));
        Assert.Equal("Inner error", innerExProperty.GetString());

        Assert.True(document.RootElement.TryGetProperty("innerExceptionType", out var innerTypeProperty));
        Assert.Contains("InvalidOperationException", innerTypeProperty.GetString());
    }

    [Fact]
    public async Task TryHandleAsync_ResponseStarted_ReturnsTrue()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");
        
        // Create a mock response that reports HasStarted as true
        var mockResponse = new Mock<HttpResponse>();
        mockResponse.Setup(r => r.HasStarted).Returns(true);
        
        var items = new Dictionary<object, object?>();
        
        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(c => c.Response).Returns(mockResponse.Object);
        mockHttpContext.Setup(c => c.Request.Path).Returns(new PathString("/test"));
        mockHttpContext.Setup(c => c.Request.Method).Returns("GET");
        mockHttpContext.Setup(c => c.Items).Returns(items);

        // Act
        var result = await _handler.TryHandleAsync(mockHttpContext.Object, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Response has already started")),
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
