# Request Middleware

Modern, scalable request/response middleware for ASP.NET Core applications with comprehensive logging and exception handling.

## Features

? **Global Exception Handling** - RFC 7807 ProblemDetails format  
? **Correlation ID Tracking** - Track requests across distributed systems  
? **Performance Metrics** - Request duration tracking  
? **Sensitive Data Redaction** - Configurable PII and security filtering  
? **Configurable Logging** - Fine-grained control over what gets logged  
? **Multi-Tenant Support** - Automatic tenant context in logs  
? **User Context** - Captures user ID, email, and tenant information  

## Configuration

Add to your `appsettings.json`:

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
  }
}
```

## Usage

### In `Program.cs` or Startup:

```csharp
// Register services
builder.Services.AddExceptionHandling(builder.Configuration);
builder.Services.AddDetailedLogging(builder.Configuration);

// Configure middleware pipeline (order matters!)
app.UseExceptionHandling(app.Configuration);
app.UseSerilogRequestLogging(app.Configuration);
app.UseDetailedLogging(app.Configuration);

// Add your other middleware
app.UseAuthentication();
app.UseAuthorization();
```

## Middleware Components

### 1. GlobalExceptionHandler

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

### 2. RequestLoggingMiddleware

Logs incoming requests with:
- Correlation ID generation
- Request body capture (configurable)
- Header filtering (sensitive headers redacted)
- Path-based exclusion
- Performance timing

### 3. ResponseLoggingMiddleware

Logs outgoing responses with:
- Proper response body buffering
- User context (ID, email, tenant)
- Response time tracking
- Header filtering
- Status-code based log levels

### 4. Serilog Request Logging

Enhanced Serilog integration with:
- Automatic enrichment with correlation ID
- User identity capture
- Performance-based log levels
- Custom message templates

## Configuration Options

### LoggingSettings

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

### ExceptionHandlingSettings

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `IncludeStackTrace` | bool | false | Include stack trace in development |
| `IncludeInnerException` | bool | true | Unwrap inner exceptions |
| `UseProblemDetails` | bool | true | Use RFC 7807 format |
| `IncludeDiagnostics` | bool | true | Include errorId, correlationId, etc. |

## Best Practices

### 1. Order of Middleware

```csharp
app.UseExceptionHandling();        // First - catch all exceptions
app.UseSerilogRequestLogging();    // Second - log all requests
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
  }
}
```

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

### Issue: Response body is empty in logs

**Solution**: Ensure `LogResponseBody = true` and the response is a text-based format (JSON/XML/Text).

### Issue: Sensitive data in logs

**Solution**: Add paths to `SensitivePaths` and headers to `SensitiveHeaders`.

### Issue: Performance degradation

**Solution**: Disable `LogResponseBody` and add high-traffic paths to `ExcludedPaths`.

### Issue: Missing correlation IDs

**Solution**: Ensure `RequestLoggingMiddleware` is registered before other logging middleware.

## Support

For issues or questions, refer to the Craft framework documentation or submit an issue to the repository.
