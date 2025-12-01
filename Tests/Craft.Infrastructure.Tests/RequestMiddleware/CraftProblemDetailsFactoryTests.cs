using Craft.Infrastructure.RequestMiddleware;
using Craft.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Craft.Infrastructure.Tests.RequestMiddleware;

public class CraftProblemDetailsFactoryTests
{
    private readonly Mock<ICurrentUser<Guid>> _currentUserMock;
    private readonly IOptions<RequestMiddlewareSettings> _settings;
    private readonly CraftProblemDetailsFactory _factory;
    private readonly DefaultHttpContext _httpContext;

    public CraftProblemDetailsFactoryTests()
    {
        _currentUserMock = new Mock<ICurrentUser<Guid>>();

        _settings = Options.Create(new RequestMiddlewareSettings
        {
            ModelValidationStatusCode = 422,
            ExceptionHandling = new ExceptionHandlingSettings
            {
                IncludeDiagnostics = true
            }
        });

        _currentUserMock.Setup(u => u.IsAuthenticated()).Returns(false);
        _currentUserMock.Setup(u => u.GetEmail()).Returns((string?)null);
        _currentUserMock.Setup(u => u.GetId()).Returns(Guid.Empty);
        _currentUserMock.Setup(u => u.GetTenant()).Returns((string?)null);

        _factory = new CraftProblemDetailsFactory(_settings, _currentUserMock.Object);

        _httpContext = new DefaultHttpContext();
        _httpContext.Request.Path = "/api/test";
        _httpContext.Request.Method = "GET";
        _httpContext.TraceIdentifier = "test-trace-id";
    }

    [Fact]
    public void CreateProblemDetails_WithMinimalInfo_ReturnsProblemDetails()
    {
        // Act
        var result = _factory.CreateProblemDetails(_httpContext);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(500, result.Status);
        Assert.Equal("Internal Server Error", result.Title);
        Assert.Equal("/api/test", result.Instance);
        Assert.Contains("rfc9110", result.Type);
    }

    [Fact]
    public void CreateProblemDetails_WithStatusCode_UsesCorrectTitle()
    {
        // Act
        var result = _factory.CreateProblemDetails(_httpContext, statusCode: 404);

        // Assert
        Assert.Equal(404, result.Status);
        Assert.Equal("Not Found", result.Title);
        Assert.Contains("15.5.5", result.Type);
    }

    [Fact]
    public void CreateProblemDetails_WithCustomValues_UsesProvidedValues()
    {
        // Act
        var result = _factory.CreateProblemDetails(
            _httpContext,
            statusCode: 400,
            title: "Custom Title",
            type: "custom:type",
            detail: "Custom detail",
            instance: "/custom/path");

        // Assert
        Assert.Equal(400, result.Status);
        Assert.Equal("Custom Title", result.Title);
        Assert.Equal("custom:type", result.Type);
        Assert.Equal("Custom detail", result.Detail);
        Assert.Equal("/custom/path", result.Instance);
    }

    [Fact]
    public void CreateProblemDetails_IncludesDiagnostics_WhenEnabled()
    {
        // Act
        var result = _factory.CreateProblemDetails(_httpContext, statusCode: 500);

        // Assert
        Assert.True(result.Extensions.ContainsKey("timestamp"));
        Assert.True(result.Extensions.ContainsKey("traceId"));
        Assert.True(result.Extensions.ContainsKey("path"));
        Assert.True(result.Extensions.ContainsKey("method"));

        Assert.Equal("test-trace-id", result.Extensions["traceId"]);
        Assert.Equal("/api/test", result.Extensions["path"]);
        Assert.Equal("GET", result.Extensions["method"]);
    }

    [Fact]
    public void CreateProblemDetails_IncludesCorrelationId_WhenAvailable()
    {
        // Arrange
        var correlationId = "test-correlation-id";
        _httpContext.Items["CorrelationId"] = correlationId;

        // Act
        var result = _factory.CreateProblemDetails(_httpContext, statusCode: 500);

        // Assert
        Assert.True(result.Extensions.ContainsKey("correlationId"));
        Assert.Equal(correlationId, result.Extensions["correlationId"]);
    }

    [Fact]
    public void CreateProblemDetails_IncludesUserContext_WhenAuthenticated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userEmail = "test@example.com";
        var tenant = "tenant-001";

        _currentUserMock.Setup(u => u.IsAuthenticated()).Returns(true);
        _currentUserMock.Setup(u => u.GetId()).Returns(userId);
        _currentUserMock.Setup(u => u.GetEmail()).Returns(userEmail);
        _currentUserMock.Setup(u => u.GetTenant()).Returns(tenant);

        // Act
        var result = _factory.CreateProblemDetails(_httpContext, statusCode: 403);

        // Assert
        Assert.True(result.Extensions.ContainsKey("userId"));
        Assert.True(result.Extensions.ContainsKey("userEmail"));
        Assert.True(result.Extensions.ContainsKey("tenant"));

        Assert.Equal(userId, result.Extensions["userId"]);
        Assert.Equal(userEmail, result.Extensions["userEmail"]);
        Assert.Equal(tenant, result.Extensions["tenant"]);
    }

    [Fact]
    public void CreateProblemDetails_ExcludesDiagnostics_WhenDisabled()
    {
        // Arrange
        var settings = Options.Create(new RequestMiddlewareSettings
        {
            ExceptionHandling = new ExceptionHandlingSettings
            {
                IncludeDiagnostics = false
            }
        });

        var factory = new CraftProblemDetailsFactory(settings, _currentUserMock.Object);

        // Act
        var result = factory.CreateProblemDetails(_httpContext, statusCode: 500);

        // Assert
        Assert.Empty(result.Extensions);
    }

    [Fact]
    public void CreateValidationProblemDetails_WithModelState_ReturnsValidationProblemDetails()
    {
        // Arrange
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Email", "Email is required");
        modelState.AddModelError("Email", "Email is invalid");
        modelState.AddModelError("Age", "Age must be 18+");

        // Act
        var result = _factory.CreateValidationProblemDetails(_httpContext, modelState);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(422, result.Status);
        Assert.Equal("One or more validation errors occurred", result.Title);
        Assert.Contains("rfc4918", result.Type);
        Assert.Equal("/api/test", result.Instance);

        Assert.Equal(2, result.Errors.Count);
        Assert.True(result.Errors.ContainsKey("Email"));
        Assert.True(result.Errors.ContainsKey("Age"));
        Assert.Equal(2, result.Errors["Email"].Length);
        Assert.Single(result.Errors["Age"]);
    }

    [Fact]
    public void CreateValidationProblemDetails_UsesConfiguredStatusCode()
    {
        // Arrange
        var settings = Options.Create(new RequestMiddlewareSettings
        {
            ModelValidationStatusCode = 400,
            ExceptionHandling = new ExceptionHandlingSettings
            {
                IncludeDiagnostics = true
            }
        });

        var factory = new CraftProblemDetailsFactory(settings, _currentUserMock.Object);
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Field", "Error");

        // Act
        var result = factory.CreateValidationProblemDetails(_httpContext, modelState);

        // Assert
        Assert.Equal(400, result.Status);
        Assert.Equal("Bad Request - Validation Failed", result.Title);
        Assert.Contains("rfc9110", result.Type);
    }

    [Fact]
    public void CreateValidationProblemDetails_WithCustomValues_UsesProvidedValues()
    {
        // Arrange
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Field", "Error");

        // Act
        var result = _factory.CreateValidationProblemDetails(
            _httpContext,
            modelState,
            statusCode: 400,
            title: "Custom Validation Title",
            type: "custom:validation",
            detail: "Custom validation detail",
            instance: "/custom/validation");

        // Assert
        Assert.Equal(400, result.Status);
        Assert.Equal("Custom Validation Title", result.Title);
        Assert.Equal("custom:validation", result.Type);
        Assert.Equal("Custom validation detail", result.Detail);
        Assert.Equal("/custom/validation", result.Instance);
    }

    [Fact]
    public void CreateValidationProblemDetails_IncludesDiagnostics()
    {
        // Arrange
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Field", "Error");
        var correlationId = "validation-correlation-id";
        _httpContext.Items["CorrelationId"] = correlationId;

        // Act
        var result = _factory.CreateValidationProblemDetails(_httpContext, modelState);

        // Assert
        Assert.True(result.Extensions.ContainsKey("correlationId"));
        Assert.True(result.Extensions.ContainsKey("timestamp"));
        Assert.True(result.Extensions.ContainsKey("traceId"));
        Assert.True(result.Extensions.ContainsKey("path"));
        Assert.True(result.Extensions.ContainsKey("method"));

        Assert.Equal(correlationId, result.Extensions["correlationId"]);
    }

    [Theory]
    [InlineData(400, "Bad Request", "15.5.1")]
    [InlineData(401, "Unauthorized", "15.5.2")]
    [InlineData(403, "Forbidden", "15.5.4")]
    [InlineData(404, "Not Found", "15.5.5")]
    [InlineData(409, "Conflict", "15.5.10")]
    [InlineData(422, "Unprocessable Entity", "rfc4918")]
    [InlineData(500, "Internal Server Error", "15.6.1")]
    [InlineData(503, "Service Unavailable", "15.6.4")]
    public void CreateProblemDetails_ReturnsCorrectTitleAndType_ForStatusCode(
        int statusCode, string expectedTitle, string expectedTypeFragment)
    {
        // Act
        var result = _factory.CreateProblemDetails(_httpContext, statusCode: statusCode);

        // Assert
        Assert.Equal(expectedTitle, result.Title);
        Assert.Contains(expectedTypeFragment, result.Type);
    }

    [Fact]
    public void CreateProblemDetails_HandlesNullTraceIdentifier()
    {
        // Arrange
        _httpContext.TraceIdentifier = null!;

        // Act
        var result = _factory.CreateProblemDetails(_httpContext, statusCode: 500);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Extensions.ContainsKey("traceId"));
    }

    [Fact]
    public void CreateProblemDetails_IncludesUserWithoutTenant()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userEmail = "test@example.com";

        _currentUserMock.Setup(u => u.IsAuthenticated()).Returns(true);
        _currentUserMock.Setup(u => u.GetId()).Returns(userId);
        _currentUserMock.Setup(u => u.GetEmail()).Returns(userEmail);
        _currentUserMock.Setup(u => u.GetTenant()).Returns((string?)null);

        // Act
        var result = _factory.CreateProblemDetails(_httpContext, statusCode: 500);

        // Assert
        Assert.True(result.Extensions.ContainsKey("userId"));
        Assert.True(result.Extensions.ContainsKey("userEmail"));
        Assert.False(result.Extensions.ContainsKey("tenant"));
    }

    [Fact]
    public void CreateValidationProblemDetails_WorksWithEmptyModelState()
    {
        // Arrange
        var modelState = new ModelStateDictionary();

        // Act
        var result = _factory.CreateValidationProblemDetails(_httpContext, modelState);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Errors);
        Assert.Equal(422, result.Status);
    }
}
