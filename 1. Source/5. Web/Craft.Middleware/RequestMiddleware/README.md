# Craft Middleware

Modern, comprehensive middleware collection for ASP.NET Core (.NET 10) applications with advanced exception handling, logging, diagnostics, and CORS configuration.

## Features

### Request/Response Middleware
? **Modern Exception Handling** - `IExceptionHandler` implementation with RFC 7807 & RFC 9110 compliance  
? **Comprehensive CraftException Support** - Automatic handling of all custom exception types  
? **Validation Error Formatting** - Rich `ValidationProblemDetails` with structured errors  
? **Correlation ID Tracking** - Track requests across distributed systems  
? **Performance Metrics** - Request duration tracking and slow request detection  
? **Sensitive Data Redaction** - Configurable PII and security filtering  
? **Configurable Logging** - Fine-grained control over what gets logged  
? **Multi-Tenant Support** - Automatic tenant context in logs and error responses  
? **User Context** - Captures user ID, email, and tenant information  
? **Development Diagnostics** - Stack traces and inner exception details in development mode

### CORS Middleware
? **Configuration-based** - Define allowed origins in `appsettings.json`  
? **Options Pattern** - Uses `IOptions<CorsSettings>` for better testability  
? **Automatic Validation** - Validates configuration at startup  
? **Multiple Frontend Support** - Separate settings for Angular, Blazor, and React  
? **Semicolon-separated** - Multiple origins per frontend type  
? **Permissive Policy** - Allows any header, any method, and credentials  
? **Fail-Fast** - Configuration errors detected at startup

## Quick Start

### 1. Add to `appsettings.json`

```json
{
  "SystemSettings": {
    "EnableExceptionHandler": true,
    "EnableSerilogRequestLogging": true,
    "EnableDetailedLogging": false,
    "Logging": {
      "LogRequestBody": true,
      "LogResponseBody": false,
      "LogHeaders": true,
      "LogPerformanceMetrics": true,
      "ExcludedPaths": ["/health", "/metrics", "/favicon.ico"],
      "SensitivePaths": ["/token", "/auth", "/password", "/secret"],
      "SensitiveHeaders": ["Authorization", "Cookie", "X-API-Key"],
      "MaxRequestBodyLength": 4096,
      "MaxResponseBodyLength": 4096
    },
    "ExceptionHandling": {
      "IncludeStackTrace": false,
      "IncludeInnerException": true,
      "UseProblemDetails": true,
      "IncludeDiagnostics": true
    }
  },
  "CorsSettings": {
    "Angular": "http://localhost:4200;https://angular.app.com",
    "Blazor": "http://localhost:5000;https://blazor.app.com",
    "React": "http://localhost:3000;https://react.app.com"
  }
}
```

### 2. Register Services in `Program.cs`

```csharp
using Craft.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Register middleware services
builder.Services.AddExceptionHandling(builder.Configuration);
builder.Services.AddDetailedLogging(builder.Configuration);
builder.Services.AddCorsPolicy(builder.Configuration);

var app = builder.Build();

// Configure middleware pipeline (order matters!)
app.UseExceptionHandling(app.Configuration);
app.UseCorsPolicy();                    // Before routing
app.UseSerilogRequestLogging(app.Configuration);
app.UseDetailedLogging(app.Configuration);

// Add your other middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
```

## Middleware Components

### Request/Response Middleware

#### 1. GlobalExceptionHandler

Modern `IExceptionHandler` implementation that:
- Returns RFC 7807 ProblemDetails responses
- Automatically maps exceptions to appropriate HTTP status codes
- Includes correlation IDs for request tracking
- Captures user and tenant context
- Logs all errors with structured data

**Supported Exception Mappings:**
- `CraftException` ? Uses exception's StatusCode
- `KeyNotFoundException` ? 404 Not Found
- `UnauthorizedAccessException` ? 401 Unauthorized
- `ArgumentException` ? 400 Bad Request
- `InvalidOperationException` ? 400 Bad Request
- `NotImplementedException` ? 501 Not Implemented
- `TimeoutException` ? 408 Request Timeout
- All others ? 500 Internal Server Error

#### 2. RequestLoggingMiddleware

Logs incoming requests with:
- Correlation ID generation
- Request body capture (configurable)
- Header filtering (sensitive headers redacted)
- Path-based exclusion
- Performance timing

#### 3. ResponseLoggingMiddleware

Logs outgoing responses with:
- Proper response body buffering
- User context (ID, email, tenant)
- Response time tracking
- Header filtering
- Status-code based log levels

#### 4. Serilog Request Logging

Enhanced Serilog integration with:
- Automatic enrichment with correlation ID
- User identity capture
- Performance-based log levels
- Custom message templates

### CORS Middleware

#### 5. CORS Policy Configuration

Provides configuration-based CORS setup with:
- Multiple frontend support (Angular, Blazor, React)
- Semicolon-separated origins per frontend
- Automatic validation at startup
- Options pattern for testability

**Configuration:**

```json
{
  "CorsSettings": {
    "Angular": "http://localhost:4200;https://angular.app.com",
    "Blazor": "http://localhost:5000;https://blazor.app.com",
    "React": "http://localhost:3000;https://react.app.com"
  }
}
```

**Usage:**

```csharp
// Register CORS policy
builder.Services.AddCorsPolicy(builder.Configuration);

// Use CORS policy (before routing/endpoints)
app.UseCorsPolicy();
```

**CORS Policy Details:**

The configured policy allows:
- ? **Any Header** - `AllowAnyHeader()`
- ? **Any Method** - `AllowAnyMethod()` (GET, POST, PUT, DELETE, etc.)
- ? **Credentials** - `AllowCredentials()` (cookies, authorization headers)
- ? **Specified Origins** - Only from configured `CorsSettings`

## Configuration Options

### Request/Response Settings

#### LoggingSettings

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `LogRequestBody` | bool | true | Enable request body logging |
| `LogResponseBody` | bool | false | Enable response body logging (performance impact) |
| `LogHeaders` | bool | true | Enable header logging |
| `LogPerformanceMetrics` | bool | true | Log request duration |
| `ExcludedPaths` | List<string> | ["/health", "/metrics", "/favicon.ico"] | Paths to skip logging |
| `SensitivePaths` | List<string> | ["/token", "/auth", "/password", "/secret"] | Paths to redact |
| `SensitiveHeaders` | List<string> | ["Authorization", "Cookie", "X-API-Key"] | Headers to redact |
| `MaxRequestBodyLength` | int | 4096 | Max request body size to log |
| `MaxResponseBodyLength` | int | 4096 | Max response body size to log |

#### ExceptionHandlingSettings

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `IncludeStackTrace` | bool | false | Include stack trace in development |
| `IncludeInnerException` | bool | true | Unwrap inner exceptions |
| `UseProblemDetails` | bool | true | Use RFC 7807 format |
| `IncludeDiagnostics` | bool | true | Include errorId, correlationId, etc. |

### CORS Settings

#### CorsSettings

| Property | Type | Description | Example |
|----------|------|-------------|---------|
| `Angular` | `string?` | Semicolon-separated list of Angular app origins | `"http://localhost:4200;https://angular.app.com"` |
| `Blazor` | `string?` | Semicolon-separated list of Blazor app origins | `"http://localhost:5000;https://blazor.app.com"` |
| `React` | `string?` | Semicolon-separated list of React app origins | `"http://localhost:3000;https://react.app.com"` |

**Note:** At least one origin must be configured, or the application will fail at startup.

## Best Practices

### 1. Order of Middleware

```csharp
app.UseExceptionHandling();        // First - catch all exceptions
app.UseCorsPolicy();               // Second - CORS before routing
app.UseSerilogRequestLogging();    // Third - log all requests
app.UseDetailedLogging();          // Optional - detailed logging
app.UseAuthentication();           // After logging setup
app.UseAuthorization();
```

### 2. Production Settings

```json
{
  "SystemSettings": {
    "EnableDetailedLogging": false,  // Disable in production
    "Logging": {
      "LogResponseBody": false,      // Avoid performance impact
      "LogRequestBody": true
    },
    "ExceptionHandling": {
      "IncludeStackTrace": false     // Don't expose internals
    }
  },
  "CorsSettings": {
    "Angular": "https://angular.app.com",
    "Blazor": "https://blazor.app.com",
    "React": "https://react.app.com"
  }
}
```

### 3. Development Settings

```json
{
  "SystemSettings": {
    "EnableDetailedLogging": true,
    "Logging": {
      "LogResponseBody": true,
      "LogRequestBody": true
    },
    "ExceptionHandling": {
      "IncludeStackTrace": true
    }
  },
  "CorsSettings": {
    "Angular": "http://localhost:4200",
    "Blazor": "http://localhost:5000",
    "React": "http://localhost:3000"
  }
}
```

### 4. CORS Best Practices

1. ? **Use specific origins** - Never use `AllowAnyOrigin()` in production
2. ? **Use HTTPS in production** - Always use `https://` for production origins
3. ? **Limit origins** - Only add origins you control and trust
4. ? **Use environment-specific configs** - Different origins for dev/staging/production
5. ? **Test CORS policies** - Verify from actual frontend applications
6. ? **Monitor logs** - Check for CORS warnings during deployment

## Response Format

### Error Response (ProblemDetails)

```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "User with ID 123 was not found",
  "instance": "/api/users/123",
  "errorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "correlationId": "7c8b5f2a-1234-5678-abcd-ef1234567890",
  "timestamp": "2024-01-15T10:30:00Z",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "userEmail": "user@example.com",
  "tenant": "tenant-001"
}
```

### With CraftException Errors

```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Validation failed",
  "instance": "/api/orders",
  "errorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "correlationId": "7c8b5f2a-1234-5678-abcd-ef1234567890",
  "errors": [
    "Email is required",
    "Phone number is invalid"
  ]
}
```

## Migration Guide

### From Old ExceptionMiddleware

**Before:**
```csharp
services.AddExceptionMiddleware(configuration);
app.UseExceptionMiddleware(configuration);
```

**After:**
```csharp
services.AddExceptionHandling(configuration);
app.UseExceptionHandling(configuration);
```

### From Old Logging Middleware

**Before:**
```csharp
services.AddRequestLogging(configuration);
app.UseRequestLogging(configuration);
app.UseSerilogHttpsLogging(configuration);
```

**After:**
```csharp
services.AddDetailedLogging(configuration);
app.UseDetailedLogging(configuration);
app.UseSerilogRequestLogging(configuration);
```

### Configuration Changes

**Before:**
```json
{
  "SystemSettings": {
    "EnableExceptionMiddleware": true,
    "EnableHttpsLogging": false,
    "EnableSerilogRequestLogging": true
  }
}
```

**After:**
```json
{
  "SystemSettings": {
    "EnableExceptionHandler": true,
    "EnableDetailedLogging": false,
    "EnableSerilogRequestLogging": true,
    "Logging": { /* ... */ },
    "ExceptionHandling": { /* ... */ }
  }
}
```

## Performance Considerations

1. **Response Body Logging**: Has significant performance impact - disable in production
2. **Request Body Logging**: Minimal impact for JSON payloads < 4KB
3. **Excluded Paths**: Use for high-frequency endpoints like health checks
4. **Sampling**: Consider implementing sampling for high-traffic scenarios

## Troubleshooting

### Request/Response Middleware Issues

#### Issue: Response body is empty in logs

**Solution**: Ensure `LogResponseBody = true` and the response is a text-based format (JSON/XML/Text).

#### Issue: Sensitive data in logs

**Solution**: Add paths to `SensitivePaths` and headers to `SensitiveHeaders`.

#### Issue: Performance degradation

**Solution**: Disable `LogResponseBody` and add high-traffic paths to `ExcludedPaths`.

#### Issue: Missing correlation IDs

**Solution**: Ensure `RequestLoggingMiddleware` is registered before other logging middleware.

### CORS Issues

#### Issue: CORS errors in browser

**Symptom:** Browser shows errors like:
```
Access to XMLHttpRequest at 'https://api.example.com' from origin 'http://localhost:4200' 
has been blocked by CORS policy
```

**Solutions:**

1. **Check configuration** - Ensure `CorsSettings` is in `appsettings.json`
2. **Check origins** - Verify the origin matches exactly (including protocol and port)
3. **Check middleware order** - `UseCorsPolicy()` must be called before `UseRouting()` or `MapControllers()`
4. **Check logs** - Look for CORS warnings in application logs

#### Issue: No origins allowed

**Symptom:** All CORS requests are blocked

**Solutions:**

1. **Verify configuration section exists** - Check `appsettings.json` has `CorsSettings`
2. **Check origin format** - Origins must include protocol: `http://` or `https://`
3. **Check for typos** - Property names are case-sensitive in JSON
4. **Review logs** - Check for warning messages about missing configuration

#### Issue: Credentials not working

**Symptom:** Cookies or authorization headers not sent

**Solution:** Ensure frontend is configured to send credentials:

**JavaScript/Fetch:**
```javascript
fetch('https://api.example.com/data', {
    credentials: 'include'
});
```

**Axios:**
```javascript
axios.defaults.withCredentials = true;
```

**Angular HttpClient:**
```typescript
this.http.get('https://api.example.com/data', { withCredentials: true });
```

## Support

For issues or questions, refer to the Craft framework documentation or submit an issue to the repository.
