# Migration Guide: ApiExceptionFilterAttribute ? GlobalExceptionHandler

## Overview

The `ApiExceptionFilterAttribute` (filter-based exception handling) is now **deprecated** in favor of the modern `GlobalExceptionHandler` (middleware-based exception handling using `IExceptionHandler`).

## Why Migrate?

### Benefits of GlobalExceptionHandler

| Feature | ApiExceptionFilterAttribute (Old) | GlobalExceptionHandler (New) |
|---------|-----------------------------------|------------------------------|
| **Scope** | MVC Controllers only | All request types (MVC, Minimal APIs, Razor Pages, gRPC) |
| **Interface** | `ExceptionFilterAttribute` (legacy) | `IExceptionHandler` (modern ASP.NET Core 8+) |
| **Exception Support** | 4 hardcoded exception types | All `CraftException` types automatically |
| **Logging** | Static `Serilog.Log` (not testable) | Injected `ILogger<T>` (testable) |
| **Configuration** | None (hardcoded behavior) | Fully configurable via `RequestMiddlewareSettings` |
| **Validation Errors** | Basic support | Rich `ValidationProblemDetails` with structured errors |
| **Diagnostics** | None | Error ID, Correlation ID, Timestamp, User info |
| **RFC Compliance** | Partial (outdated RFC URLs) | Full RFC 7807 & RFC 9110 compliance |
| **Extensibility** | Requires code changes | Configuration-driven |

## Migration Steps

### Step 1: Remove Filter Attribute from Controllers

**Before:**
```csharp
[ApiController]
[Route("api/[controller]")]
[ApiExceptionFilter]  // ? Remove this
public class UsersController : ControllerBase
{
    // ...
}
```

**After:**
```csharp
[ApiController]
[Route("api/[controller]")]
// ? No filter attribute needed
public class UsersController : ControllerBase
{
    // ...
}
```

### Step 2: Update Service Registration

**Before:**
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddApiControllers();
    
    // No exception handler registration
}
```

**After:**
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddApiControllers(configuration);
    
    // ? Add exception handling
    services.AddExceptionHandling(configuration);
    services.AddProblemDetails(); // Optional: for enhanced ProblemDetails customization
}
```

### Step 3: Update Application Pipeline

**Before:**
```csharp
public void Configure(IApplicationBuilder app)
{
    // No middleware needed (filter handles exceptions)
    
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseEndpoints(endpoints => endpoints.MapControllers());
}
```

**After:**
```csharp
public void Configure(IApplicationBuilder app)
{
    // ? Add exception handling middleware (must be early in pipeline)
    app.UseExceptionHandling(configuration);
    
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseEndpoints(endpoints => endpoints.MapControllers());
}
```

### Step 4: Add Configuration (Optional)

Add to your `appsettings.json`:

```json
{
  "RequestMiddlewareSettings": {
    "EnableExceptionHandler": true,
    "ModelValidationStatusCode": 422,
    "ExceptionHandling": {
      "IncludeStackTrace": false,
      "IncludeInnerException": true,
      "UseProblemDetails": true,
      "IncludeDiagnostics": true,
      "IncludeValidationDetails": true
    }
  }
}
```

For development environment (`appsettings.Development.json`):

```json
{
  "RequestMiddlewareSettings": {
    "ExceptionHandling": {
      "IncludeStackTrace": true
    }
  }
}
```

## Feature Comparison

### Exception Type Handling

#### Old (ApiExceptionFilterAttribute)
```csharp
// Only 4 exceptions explicitly supported:
- ModelValidationException ? 400 Bad Request
- NotFoundException ? 404 Not Found
- AlreadyExistsException ? 400 Bad Request (? should be 409/422)
- InvalidCredentialsException ? 401 Unauthorized
- Unknown exceptions ? 500 Internal Server Error
```

#### New (GlobalExceptionHandler)
```csharp
// All CraftException types automatically handled with correct status codes:
- ModelValidationException ? 400/422 (configurable)
- NotFoundException ? 404 Not Found
- AlreadyExistsException ? 422 Unprocessable Entity
- InvalidCredentialsException ? 401 Unauthorized
- UnauthorizedException ? 401 Unauthorized
- ForbiddenException ? 403 Forbidden
- Any CraftException ? Uses exception's StatusCode property
- Standard .NET exceptions ? Appropriate status codes
- Unknown exceptions ? 500 Internal Server Error
```

### Response Format

#### Old (ApiExceptionFilterAttribute)
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "The specified resource was not found.",
  "status": 404,
  "detail": "User with ID 123 not found"
}
```

#### New (GlobalExceptionHandler)
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.5",
  "title": "Resource not found",
  "status": 404,
  "detail": "User with ID 123 not found",
  "instance": "/api/users/123",
  "errorId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "correlationId": "x-correlation-id-value",
  "timestamp": "2024-01-15T10:30:00.000Z",
  "userId": "user-guid",
  "userEmail": "user@example.com",
  "tenant": "tenant-name"
}
```

### Validation Error Handling

#### Old (ApiExceptionFilterAttribute)
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "errors": {
    "Email": ["The Email field is required."],
    "Age": ["Age must be between 18 and 100."]
  }
}
```

#### New (GlobalExceptionHandler + AddApiControllersExtension)
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc4918#section-11.2",
  "title": "One or more validation errors occurred",
  "status": 422,
  "detail": "One or more validation errors occurred. See the errors property for details.",
  "instance": "/api/users",
  "errors": {
    "Email": ["The Email field is required."],
    "Age": ["Age must be between 18 and 100."]
  },
  "errorId": "validation-error-id",
  "correlationId": "correlation-id",
  "timestamp": "2024-01-15T10:30:00.000Z"
}
```

## Configuration Options

### ModelValidationStatusCode

Choose validation error status code:

```json
{
  "RequestMiddlewareSettings": {
    "ModelValidationStatusCode": 422  // 422 (semantic) or 400 (traditional)
  }
}
```

- **422 Unprocessable Entity** (recommended): Semantically correct per RFC 4918
- **400 Bad Request**: Traditional approach, still widely used

### ExceptionHandling Options

```json
{
  "RequestMiddlewareSettings": {
    "ExceptionHandling": {
      "IncludeStackTrace": false,           // Show stack traces (dev only)
      "IncludeInnerException": true,        // Include inner exception details
      "UseProblemDetails": true,            // RFC 7807 compliance
      "IncludeDiagnostics": true,           // Error IDs, correlation, user info
      "IncludeValidationDetails": true      // Structured validation errors
    }
  }
}
```

## Testing Impact

### Old Approach
```csharp
// Hard to test due to static logging
public class MyControllerTests
{
    [Fact]
    public void TestExceptionHandling()
    {
        // ? Cannot verify logging behavior
        // ? Cannot mock Serilog.Log
    }
}
```

### New Approach
```csharp
// Fully testable with dependency injection
public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TestExceptionHandling()
    {
        // ? Can mock ILogger<T>
        var mockLogger = new Mock<ILogger<GlobalExceptionHandler>>();
        
        // ? Can verify logging calls
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
```

## Rollback Plan

If you need to temporarily rollback:

1. Re-apply `[ApiExceptionFilter]` to controllers
2. Comment out exception handler middleware:
   ```csharp
   // app.UseExceptionHandling(configuration);
   ```
3. The filter will handle exceptions again

## Breaking Changes

### Status Code Changes

| Exception | Old Status | New Status | Impact |
|-----------|-----------|-----------|---------|
| `AlreadyExistsException` | 400 | 422 | More semantically correct |
| `ModelValidationException` | 400 | 422 (default) | Configurable |

### Response Structure

- Added fields: `errorId`, `correlationId`, `timestamp`, `userId`, `userEmail`, `tenant`, `instance`
- Updated RFC URLs: `rfc7231` ? `rfc9110`
- Better structured validation errors

## Support

For questions or issues during migration:

1. Check the `GlobalExceptionHandler` XML documentation
2. Review `RequestMiddlewareSettings` configuration options
3. See `QUICKREF.md` for quick reference
4. See `README.md` for detailed documentation

## Timeline

- **Current**: Both approaches work (filter is deprecated with warnings)
- **Next Minor Version**: Filter remains but shows obsolete warnings
- **Next Major Version**: Filter will be removed

**Recommendation**: Migrate as soon as possible to take advantage of improved features and avoid future breaking changes.
