using Craft.Middleware.RequestMiddleware;
using Craft.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Craft.Middleware.Tests.RequestMiddleware;

public class ApiControllersExtensionTests
{
    [Fact]
    public void AddApiControllers_RegistersControllers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApiControllers();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var mvcBuilder = serviceProvider.GetService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Mvc.MvcOptions>>();
        Assert.NotNull(mvcBuilder);
    }

    [Fact]
    public void AddApiControllers_WithConfiguration_ConfiguresApiBehaviorOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(422);
        
        RegisterProblemDetailsFactory(services, 422);

        // Act
        services.AddApiControllers(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<IOptions<ApiBehaviorOptions>>();
        Assert.NotNull(options);
        Assert.NotNull(options.Value.InvalidModelStateResponseFactory);
    }

    [Fact]
    public void InvalidModelStateResponseFactory_ReturnsValidationProblemDetails()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(422);
        
        RegisterProblemDetailsFactory(services, 422);
        services.AddApiControllers(configuration);

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<ApiBehaviorOptions>>();
        
        var actionContext = CreateActionContext(serviceProvider);
        var modelState = actionContext.ModelState;
        modelState.AddModelError("Email", "Email is required");

        // Act
        var result = options.Value.InvalidModelStateResponseFactory(actionContext);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<UnprocessableEntityObjectResult>(result);
        
        var objectResult = (UnprocessableEntityObjectResult)result;
        Assert.IsType<ValidationProblemDetails>(objectResult.Value);
        
        var problemDetails = (ValidationProblemDetails)objectResult.Value;
        Assert.Equal(422, problemDetails.Status);
        Assert.Contains("Email", problemDetails.Errors.Keys);
    }

    [Fact]
    public void InvalidModelStateResponseFactory_Returns422_WhenConfigured()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(422);
        
        RegisterProblemDetailsFactory(services, 422);
        services.AddApiControllers(configuration);

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<ApiBehaviorOptions>>();
        
        var actionContext = CreateActionContext(serviceProvider);
        actionContext.ModelState.AddModelError("Field", "Error");

        // Act
        var result = options.Value.InvalidModelStateResponseFactory(actionContext);

        // Assert
        var objectResult = (UnprocessableEntityObjectResult)result;
        var problemDetails = (ValidationProblemDetails)objectResult.Value!;
        
        Assert.Equal(422, problemDetails.Status);
        Assert.Equal("One or more validation errors occurred", problemDetails.Title);
    }

    [Fact]
    public void InvalidModelStateResponseFactory_Returns400_WhenConfigured()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(400);
        
        RegisterProblemDetailsFactory(services, 400);
        services.AddApiControllers(configuration);

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<ApiBehaviorOptions>>();
        
        var actionContext = CreateActionContext(serviceProvider);
        actionContext.ModelState.AddModelError("Field", "Error");

        // Act
        var result = options.Value.InvalidModelStateResponseFactory(actionContext);

        // Assert
        var objectResult = (BadRequestObjectResult)result;
        var problemDetails = (ValidationProblemDetails)objectResult.Value!;
        
        Assert.Equal(400, problemDetails.Status);
        Assert.Equal("Bad Request - Validation Failed", problemDetails.Title);
    }

    [Fact]
    public void InvalidModelStateResponseFactory_IncludesAllModelStateErrors()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(422);
        
        RegisterProblemDetailsFactory(services, 422);
        services.AddApiControllers(configuration);

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<ApiBehaviorOptions>>();
        
        var actionContext = CreateActionContext(serviceProvider);
        actionContext.ModelState.AddModelError("Email", "Email is required");
        actionContext.ModelState.AddModelError("Email", "Email format is invalid");
        actionContext.ModelState.AddModelError("Age", "Age must be 18+");
        actionContext.ModelState.AddModelError("Name", "Name is required");

        // Act
        var result = options.Value.InvalidModelStateResponseFactory(actionContext);

        // Assert
        var objectResult = (UnprocessableEntityObjectResult)result;
        var problemDetails = (ValidationProblemDetails)objectResult.Value!;
        
        Assert.Equal(3, problemDetails.Errors.Count);
        Assert.True(problemDetails.Errors.ContainsKey("Email"));
        Assert.True(problemDetails.Errors.ContainsKey("Age"));
        Assert.True(problemDetails.Errors.ContainsKey("Name"));
        Assert.Equal(2, problemDetails.Errors["Email"].Length);
        Assert.Single(problemDetails.Errors["Age"]);
        Assert.Single(problemDetails.Errors["Name"]);
    }

    [Fact]
    public void InvalidModelStateResponseFactory_SetsDetailMessage()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(422);
        
        RegisterProblemDetailsFactory(services, 422);
        services.AddApiControllers(configuration);

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<ApiBehaviorOptions>>();
        
        var actionContext = CreateActionContext(serviceProvider);
        actionContext.ModelState.AddModelError("Field", "Error");

        // Act
        var result = options.Value.InvalidModelStateResponseFactory(actionContext);

        // Assert
        var objectResult = (UnprocessableEntityObjectResult)result;
        var problemDetails = (ValidationProblemDetails)objectResult.Value!;
        
        Assert.Equal("One or more validation errors occurred. See the errors property for details.", problemDetails.Detail);
    }

    [Fact]
    public void InvalidModelStateResponseFactory_SetsContentType()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(422);
        
        RegisterProblemDetailsFactory(services, 422);
        services.AddApiControllers(configuration);

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<ApiBehaviorOptions>>();
        
        var actionContext = CreateActionContext(serviceProvider);
        actionContext.ModelState.AddModelError("Field", "Error");

        // Act
        var result = options.Value.InvalidModelStateResponseFactory(actionContext) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Contains("application/problem+json", result.ContentTypes);
    }

    [Fact]
    public void InvalidModelStateResponseFactory_IncludesDiagnosticExtensions()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(422);
        
        RegisterProblemDetailsFactory(services, 422, includeDiagnostics: true);
        services.AddApiControllers(configuration);

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<ApiBehaviorOptions>>();
        
        var actionContext = CreateActionContext(serviceProvider, correlationId: "test-correlation-id");
        actionContext.ModelState.AddModelError("Field", "Error");

        // Act
        var result = options.Value.InvalidModelStateResponseFactory(actionContext);

        // Assert
        var objectResult = (UnprocessableEntityObjectResult)result;
        var problemDetails = (ValidationProblemDetails)objectResult.Value!;
        
        // These are added by CraftProblemDetailsFactory
        Assert.True(problemDetails.Extensions.ContainsKey("correlationId"));
        Assert.True(problemDetails.Extensions.ContainsKey("timestamp"));
        Assert.True(problemDetails.Extensions.ContainsKey("traceId"));
        Assert.True(problemDetails.Extensions.ContainsKey("path"));
        Assert.True(problemDetails.Extensions.ContainsKey("method"));
    }

    [Fact]
    public void InvalidModelStateResponseFactory_ExcludesDiagnostics_WhenDisabled()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(422);
        
        RegisterProblemDetailsFactory(services, 422, includeDiagnostics: false);
        services.AddApiControllers(configuration);

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<ApiBehaviorOptions>>();
        
        var actionContext = CreateActionContext(serviceProvider);
        actionContext.ModelState.AddModelError("Field", "Error");

        // Act
        var result = options.Value.InvalidModelStateResponseFactory(actionContext);

        // Assert
        var objectResult = (UnprocessableEntityObjectResult)result;
        var problemDetails = (ValidationProblemDetails)objectResult.Value!;
        
        // Only errors property should be present (from ModelState)
        Assert.True(problemDetails.Errors.Count > 0);
        // No diagnostic extensions
        Assert.Empty(problemDetails.Extensions);
    }

    [Fact]
    public void InvalidModelStateResponseFactory_WithoutConfiguration_Defaults422()
    {
        // Arrange
        var services = new ServiceCollection();
        
        RegisterProblemDetailsFactory(services, 422);
        services.AddApiControllers(); // No configuration

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<ApiBehaviorOptions>>();
        
        var actionContext = CreateActionContext(serviceProvider);
        actionContext.ModelState.AddModelError("Field", "Error");

        // Act
        var result = options.Value.InvalidModelStateResponseFactory(actionContext);

        // Assert
        var objectResult = (UnprocessableEntityObjectResult)result;
        var problemDetails = (ValidationProblemDetails)objectResult.Value!;
        
        Assert.Equal(422, problemDetails.Status);
    }

    [Fact]
    public void InvalidModelStateResponseFactory_SetsInstancePath()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(422);
        
        RegisterProblemDetailsFactory(services, 422);
        services.AddApiControllers(configuration);

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<ApiBehaviorOptions>>();
        
        var actionContext = CreateActionContext(serviceProvider);
        actionContext.HttpContext.Request.Path = "/api/users/create";
        actionContext.ModelState.AddModelError("Field", "Error");

        // Act
        var result = options.Value.InvalidModelStateResponseFactory(actionContext);

        // Assert
        var objectResult = (UnprocessableEntityObjectResult)result;
        var problemDetails = (ValidationProblemDetails)objectResult.Value!;
        
        Assert.Equal("/api/users/create", problemDetails.Instance);
    }

    [Fact]
    public void InvalidModelStateResponseFactory_IncludesUserContext_WhenAuthenticated()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(422);
        
        var userId = Guid.NewGuid();
        RegisterProblemDetailsFactory(services, 422, includeDiagnostics: true, 
            isAuthenticated: true, userId: userId, userEmail: "test@example.com", tenant: "tenant-001");
        services.AddApiControllers(configuration);

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<ApiBehaviorOptions>>();
        
        var actionContext = CreateActionContext(serviceProvider);
        actionContext.ModelState.AddModelError("Field", "Error");

        // Act
        var result = options.Value.InvalidModelStateResponseFactory(actionContext);

        // Assert
        var objectResult = (UnprocessableEntityObjectResult)result;
        var problemDetails = (ValidationProblemDetails)objectResult.Value!;
        
        Assert.True(problemDetails.Extensions.ContainsKey("userId"));
        Assert.True(problemDetails.Extensions.ContainsKey("userEmail"));
        Assert.True(problemDetails.Extensions.ContainsKey("tenant"));
        
        Assert.Equal(userId, problemDetails.Extensions["userId"]);
        Assert.Equal("test@example.com", problemDetails.Extensions["userEmail"]);
        Assert.Equal("tenant-001", problemDetails.Extensions["tenant"]);
    }

    // Helper methods
    private static IConfiguration CreateConfiguration(int validationStatusCode)
    {
        var configData = new Dictionary<string, string?>
        {
            { "RequestMiddlewareSettings:ModelValidationStatusCode", validationStatusCode.ToString() }
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
    }

    private static void RegisterProblemDetailsFactory(IServiceCollection services, int validationStatusCode, 
        bool includeDiagnostics = true, bool isAuthenticated = false, Guid? userId = null, 
        string? userEmail = null, string? tenant = null)
    {
        var settings = Options.Create(new RequestMiddlewareSettings
        {
            ModelValidationStatusCode = validationStatusCode,
            ExceptionHandling = new ExceptionHandlingSettings
            {
                IncludeDiagnostics = includeDiagnostics
            }
        });

        var currentUserMock = new Mock<ICurrentUser<Guid>>();
        currentUserMock.Setup(u => u.IsAuthenticated()).Returns(isAuthenticated);
        currentUserMock.Setup(u => u.GetId()).Returns(userId ?? Guid.Empty);
        currentUserMock.Setup(u => u.GetEmail()).Returns(userEmail);
        currentUserMock.Setup(u => u.GetTenant()).Returns(tenant);

        services.AddSingleton(settings);
        services.AddSingleton(currentUserMock.Object);
        services.AddSingleton<ProblemDetailsFactory>(sp => 
            new CraftProblemDetailsFactory(settings, currentUserMock.Object));
    }

    private static ActionContext CreateActionContext(IServiceProvider serviceProvider, string? correlationId = null)
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider,
            TraceIdentifier = "test-trace-id"
        };
        
        httpContext.Request.Path = "/api/test";
        httpContext.Request.Method = "POST";

        if (correlationId != null)
            httpContext.Items["CorrelationId"] = correlationId;

        return new ActionContext(
            httpContext,
            new Microsoft.AspNetCore.Routing.RouteData(),
            new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor(),
            new ModelStateDictionary());
    }
}
