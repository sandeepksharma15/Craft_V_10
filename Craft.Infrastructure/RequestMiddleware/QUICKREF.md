# Quick Reference Card - Request Middleware (.NET 10)

## ?? Quick Setup (3 Steps)

### 1. Configuration (appsettings.json)
```json
{
  "RequestMiddlewareSettings": {
    "EnableExceptionHandler": true,
    "EnableSerilogRequestLogging": true,
    "EnableDetailedLogging": false,
    "ModelValidationStatusCode": 422,
    "Logging": {
      "ExcludedPaths": ["/health", "/metrics"],
      "SensitivePaths": ["/token", "/auth"],
      "LogResponseBody": false
    },
    "ExceptionHandling": {
      "IncludeDiagnostics": true,
      "IncludeStackTrace": false,
      "IncludeValidationDetails": true
    }
  }
}
```

### 2. Service Registration (Program.cs)
```csharp
// API Controllers with validation
builder.Services.AddApiControllers(builder.Configuration);

// Exception handling
builder.Services.AddExceptionHandling(builder.Configuration);
builder.Services.AddProblemDetails(); // Optional: enhanced ProblemDetails

// Optional detailed logging
builder.Services.AddDetailedLogging(builder.Configuration);
```

### 3. Middleware Pipeline (Program.cs)
```csharp
app.UseExceptionHandling(app.Configuration);       // First - catches all exceptions!
app.UseSerilogRequestLogging(app.Configuration);   // Second - logs requests
app.UseDetailedLogging(app.Configuration);         // Optional - detailed body logging

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

---

## ?? Exception Handling - All Supported Types

### CraftException Types (Auto-detected)
```csharp
throw new NotFoundException("User", 123);                    // ? 404
throw new AlreadyExistsException("Email", "user@test.com");  // ? 422
throw new ModelValidationException("Invalid", errors);       // ? 400/422
throw new InvalidCredentialsException("Bad password");       // ? 401
throw new UnauthorizedException("Not logged in");            // ? 401
throw new ForbiddenException("Access denied");               // ? 403
```

### Standard .NET Exceptions
```csharp
throw new KeyNotFoundException("Not found");              // ? 404
throw new UnauthorizedAccessException("Denied");          // ? 401
throw new ArgumentNullException("param");                 // ? 400
throw new ArgumentException("Invalid");                   // ? 400
throw new InvalidOperationException("Invalid state");     // ? 400
throw new NotImplementedException("Coming soon");         // ? 501
throw new TimeoutException("Too slow");                   // ? 408
throw new OperationCanceledException("Cancelled");        // ? 408
throw new Exception("Server error");                      // ? 500
```

### Custom Status Codes
```csharp
throw new CraftException(
    "Custom error",
    HttpStatusCode.Conflict);  // ? 409 (any status code)
```

---

## ?? Validation Errors (ModelValidationException)

### Throwing Validation Errors
```csharp
var errors = new Dictionary<string, string[]>
{
    { "Email", new[] { "Required", "Invalid format" } },
    { "Age", new[] { "Must be 18+" } }
};

throw new ModelValidationException("Validation failed", errors);
```

### Response Format (ValidationProblemDetails)
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc4918#section-11.2",
  "title": "One or more validation errors occurred",
  "status": 422,
  "detail": "Validation failed",
  "instance": "/api/users",
  "errors": {
    "Email": ["Required", "Invalid format"],
    "Age": ["Must be 18+"]
  },
  "errorId": "uuid",
  "correlationId": "uuid",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### Configurable Status Code
```json
{
  "RequestMiddlewareSettings": {
    "ModelValidationStatusCode": 422  // or 400 for traditional behavior
  }
}
```

---

## ?? Error Response Format (RFC 7807 & RFC 9110)

### Standard Error Response
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.5",
  "title": "Resource not found",
  "status": 404,
  "detail": "User with ID 123 was not found",
  "instance": "/api/users/123",
  "errorId": "a1b2c3d4-...",
  "correlationId": "x1y2z3...",
  "timestamp": "2024-01-15T10:30:00.000Z",
  "userId": "user-guid",
  "userEmail": "user@example.com",
  "tenant": "tenant-name"
}
```

### With CraftException Errors
```json
{
  "title": "Validation failed",
  "status": 400,
  "detail": "Multiple validation errors occurred",
  "errors": ["Name is required", "Email is invalid", "Age must be 18+"]
}
```

### Development Mode (with stack trace)
```json
{
  "title": "Internal Server Error",
  "status": 500,
  "stackTrace": "at MyNamespace.MyClass...",
  "exceptionType": "System.InvalidOperationException",
  "innerException": "Inner error message",
  "innerExceptionType": "System.ArgumentException"
}
```

---

## ?? Configuration Options

### ModelValidationStatusCode
```json
{
  "RequestMiddlewareSettings": {
    "ModelValidationStatusCode": 422  // 422 (semantic) or 400 (traditional)
  }
}
```

- **422 Unprocessable Entity** (default): Semantically correct per RFC 4918
- **400 Bad Request**: Traditional approach, widely used

### ExceptionHandling Settings
```json
{
  "ExceptionHandling": {
    "IncludeStackTrace": false,        // Show stack traces (dev environment only)
    "IncludeInnerException": true,     // Unwrap to innermost exception
    "UseProblemDetails": true,         // RFC 7807 compliance (always true)
    "IncludeDiagnostics": true,        // Error IDs, correlation, user info
    "IncludeValidationDetails": true   // Structured validation errors
  }
}
```

---

## ?? Security & Sensitive Data

### Mark Paths as Sensitive
```json
"Logging": {
  "SensitivePaths": ["/token", "/password", "/api/secrets", "/auth"]
}
```
Result: Body logged as `[REDACTED - Sensitive endpoint]`

### Mark Headers as Sensitive
```json
"Logging": {
  "SensitiveHeaders": ["Authorization", "Cookie", "X-API-Key", "X-Auth-Token"]
}
```
Result: Header logged as `[REDACTED]`

### Exclude Paths from Logging
```json
"Logging": {
  "ExcludedPaths": ["/health", "/metrics", "/swagger", "/favicon.ico"]
}
```
Result: Path not logged at all

---

## ?? Migration from ApiExceptionFilterAttribute

### Old Approach (DEPRECATED ??)
```csharp
[ApiController]
[Route("api/[controller]")]
[ApiExceptionFilter]  // ? Remove this
public class UsersController : ControllerBase { }
```

### New Approach (MODERN ?)
```csharp
// In Program.cs
services.AddExceptionHandling(configuration);
app.UseExceptionHandling(configuration);

// Controllers - no attribute needed
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase { }
```

**See [MIGRATION.md](MIGRATION.md) for complete migration guide**

---

## ?? What Gets Logged?

### Always Logged (Basic)
? Correlation ID  
? Request Method & Path  
? Response Status Code  
? Error ID (for exceptions)  
? Timestamp

### Configurable (Detailed)
?? Request Body (on/off)  
?? Response Body (on/off)  
?? Headers (on/off)  
?? Performance Metrics (on/off)  
?? User Context (when authenticated)  
?? Stack Traces (development only)

### Never Logged (Automatic Redaction)
?? Sensitive paths ? `[REDACTED]`  
?? Sensitive headers ? `[REDACTED]`  
?? Binary content ? `[Binary content: type]`  
?? Large bodies ? `[TRUNCATED]`

---

## ? Performance Tips

1. **Disable response body logging in production** (biggest performance impact)
   ```json
   "LogResponseBody": false
   ```

2. **Exclude high-frequency endpoints**
   ```json
   "ExcludedPaths": ["/health", "/metrics", "/swagger"]
   ```

3. **Set appropriate body length limits**
   ```json
   "MaxRequestBodyLength": 4096,   // 4KB for most APIs
   "MaxResponseBodyLength": 4096
   ```

4. **Disable header logging if not needed**
   ```json
   "LogHeaders": false  // Reduces log size
   ```

5. **Use Serilog's built-in logging instead of detailed logging**
   ```json
   "EnableDetailedLogging": false,      // Production
   "EnableSerilogRequestLogging": true  // Lightweight
   ```

---

## ?? Troubleshooting

### Missing Correlation IDs
**Cause:** `RequestLoggingMiddleware` not enabled  
**Fix:** `EnableDetailedLogging = true` or ensure middleware is registered

### Sensitive Data in Logs
**Cause:** Path/header not marked as sensitive  
**Fix:** Add to `SensitivePaths` or `SensitiveHeaders`

### Too Many Logs
**Cause:** High-frequency endpoints being logged  
**Fix:** Add paths to `ExcludedPaths`

### Empty Response Bodies in Logs
**Cause:** `LogResponseBody = false`  
**Fix:** Enable it (but watch performance!)

### Exceptions Not Caught
**Cause:** Middleware order incorrect  
**Fix:** Call `UseExceptionHandling()` **FIRST** in pipeline

### Wrong Validation Status Code
**Cause:** Default is 422, you want 400  
**Fix:** Set `ModelValidationStatusCode = 400`

---

## ?? Configuration Defaults

| Setting | Default | Production | Development |
|---------|---------|------------|-------------|
| EnableExceptionHandler | ? true | ? | ? |
| EnableSerilogRequestLogging | ? true | ? | ? |
| EnableDetailedLogging | ? false | ? | ? |
| ModelValidationStatusCode | 422 | 422 | 422 |
| LogRequestBody | ? true | ? | ? |
| LogResponseBody | ? false | ? | ? |
| LogHeaders | ? true | ? | ? |
| LogPerformanceMetrics | ? true | ? | ? |
| IncludeStackTrace | ? false | ? | ? |
| IncludeDiagnostics | ? true | ? | ? |
| IncludeValidationDetails | ? true | ? | ? |
| MaxRequestBodyLength | 4096 | 4096 | 8192 |
| MaxResponseBodyLength | 4096 | 2048 | 8192 |

---

## ? New Project Checklist

- [ ] Add `RequestMiddlewareSettings` to appsettings.json
- [ ] Customize `ExcludedPaths` for your API
- [ ] Add sensitive paths to `SensitivePaths`
- [ ] Register services: `AddExceptionHandling`, `AddApiControllers`
- [ ] Configure middleware pipeline (correct order!)
- [ ] Remove `[ApiExceptionFilter]` from controllers (if migrating)
- [ ] Test exception handling with various exception types
- [ ] Verify validation errors return correct status code
- [ ] Test logging output and verify redaction
- [ ] Set production-appropriate settings
- [ ] Review performance metrics

---

## ?? Documentation Links

- **Migration Guide:** [MIGRATION.md](MIGRATION.md) - Complete migration from old approach
- **Full Documentation:** [README.md](README.md) - Detailed documentation
- **Example Config:** [appsettings.example.json](appsettings.example.json) - Full configuration example

---

**Version:** 2.0 | **.NET:** 10 | **Last Updated:** January 2025 | **Status:** ? Production Ready

### 1. Configuration (appsettings.json)
```json
{
  "SystemSettings": {
    "EnableExceptionHandler": true,
    "EnableSerilogRequestLogging": true,
    "EnableDetailedLogging": false,  // true for dev, false for prod
    "Logging": {
      "ExcludedPaths": ["/health", "/metrics"],
      "SensitivePaths": ["/token", "/auth"],
      "LogResponseBody": false  // Performance impact!
    }
  }
}
```

### 2. Service Registration (Program.cs)
```csharp
builder.Services.AddExceptionHandling(builder.Configuration);
builder.Services.AddDetailedLogging(builder.Configuration);
```

### 3. Middleware Pipeline (Program.cs)
```csharp
app.UseExceptionHandling(app.Configuration);       // First!
app.UseSerilogRequestLogging(app.Configuration);   // Second
app.UseDetailedLogging(app.Configuration);         // Optional
app.UseAuthentication();
app.UseAuthorization();
```

---

## ?? Common Tasks

### Exclude Path from Logging
```json
"Logging": {
  "ExcludedPaths": ["/health", "/swagger", "/my-noisy-endpoint"]
}
```

### Mark Path as Sensitive
```json
"Logging": {
  "SensitivePaths": ["/token", "/password", "/api/secrets"]
}
```

### Add Sensitive Header
```json
"Logging": {
  "SensitiveHeaders": ["Authorization", "X-API-Key", "X-Custom-Token"]
}
```

### Enable Response Body Logging (Dev Only!)
```json
"Logging": {
  "LogResponseBody": true,  // ?? Performance impact!
  "MaxResponseBodyLength": 8192
}
```

### Enable Stack Traces in Development
```json
"ExceptionHandling": {
  "IncludeStackTrace": true  // Only works in Development env
}
```

---

## ?? What Gets Logged?

### Automatically Logged (Always)
? Correlation ID  
? Request Method & Path  
? Response Status Code  
? Request Duration (if enabled)  
? User ID, Email, Tenant (if authenticated)  

### Configurable Logging
?? Request Body (on/off)  
?? Response Body (on/off)  
?? Headers (on/off)  
?? Performance Metrics (on/off)  

### Never Logged (Security)
?? Sensitive paths ? `[REDACTED]`  
?? Sensitive headers ? `[REDACTED]`  
?? Binary content ? `[Binary content: type]`  
?? Large bodies ? `[TRUNCATED]`  

---

## ?? Exception Handling

### Automatic Status Code Mapping
```csharp
throw new KeyNotFoundException("Not found");              // ? 404
throw new UnauthorizedAccessException("Denied");          // ? 401
throw new ArgumentException("Invalid");                   // ? 400
throw new InvalidOperationException("Invalid");           // ? 400
throw new NotImplementedException("Coming soon");         // ? 501
throw new TimeoutException("Too slow");                   // ? 408
throw new Exception("Server error");                      // ? 500
```

### Custom Status Codes
```csharp
throw new CraftException(
    "Custom error",
    HttpStatusCode.Conflict);  // ? 409
```

### Multiple Errors
```csharp
throw new CraftException(
    "Validation failed",
    new List<string> { "Name required", "Email invalid" },
    HttpStatusCode.BadRequest);
```

---

## ?? Error Response Format (RFC 7807)

```json
{
  "type": "https://...",
  "title": "Not Found",
  "status": 404,
  "detail": "User with ID 123 was not found",
  "instance": "/api/users/123",
  "errorId": "uuid",
  "correlationId": "uuid",
  "timestamp": "2024-01-15T10:30:00Z",
  "userId": "uuid",
  "userEmail": "user@example.com",
  "tenant": "tenant-001",
  "errors": ["Error 1", "Error 2"]  // For CraftException
}
```

---

## ??? Configuration Defaults

| Setting | Default | Prod | Dev |
|---------|---------|------|-----|
| EnableExceptionHandler | ? true | ? | ? |
| EnableSerilogRequestLogging | ? true | ? | ? |
| EnableDetailedLogging | ? false | ? | ? |
| LogRequestBody | ? true | ? | ? |
| LogResponseBody | ? false | ? | ? |
| LogHeaders | ? true | ? | ? |
| LogPerformanceMetrics | ? true | ? | ? |
| IncludeStackTrace | ? false | ? | ? |
| IncludeDiagnostics | ? true | ? | ? |
| MaxRequestBodyLength | 4096 | 4096 | 8192 |
| MaxResponseBodyLength | 4096 | 2048 | 8192 |

---

## ?? Migration from Old Version

### 1. Update Config Names
```diff
- "EnableExceptionMiddleware": true
+ "EnableExceptionHandler": true

- "EnableHttpsLogging": false
+ "EnableDetailedLogging": false

- "EnableSerilogRequestLogging": true  (no change)
```

### 2. Update Method Names
```diff
- builder.Services.AddExceptionMiddleware(config);
+ builder.Services.AddExceptionHandling(config);

- builder.Services.AddRequestLogging(config);
+ builder.Services.AddDetailedLogging(config);

- app.UseExceptionMiddleware(config);
+ app.UseExceptionHandling(config);

- app.UseRequestLogging(config);
+ app.UseDetailedLogging(config);

- app.UseSerilogHttpsLogging(config);
+ app.UseSerilogRequestLogging(config);
```

---

## ?? Log Examples

### Request Log
```
[Information] Incoming request | GET /api/orders | CorrelationId: abc123
[Debug] Request details: {"Method":"GET","Path":"/api/orders",...}
```

### Response Log
```
[Information] Response sent | StatusCode: 200 | Path: /api/orders | User: user@example.com | CorrelationId: abc123
[Debug] Response details: {"StatusCode":200,"Body":"{\"data\":...}",...}
```

### Performance Log
```
[Information] Request completed | Method: GET | Path: /api/orders | StatusCode: 200 | Duration: 45.2ms | CorrelationId: abc123
```

### Error Log
```
[Error] Request failed with status 500 | ErrorId: uuid | Path: /api/orders | Method: GET | User: user@example.com (uuid) | Tenant: tenant-001 | Message: Database connection failed
```

---

## ? Performance Tips

1. **Disable response body logging in production** (biggest impact)
2. **Exclude noisy endpoints** (`/health`, `/metrics`, `/swagger`)
3. **Set appropriate max body lengths** (4KB is usually enough)
4. **Disable header logging in production** (if not needed)
5. **Use sampling for high-traffic endpoints** (future feature)

---

## ?? Troubleshooting

### Issue: Logs missing correlation IDs
**Fix:** Enable `EnableDetailedLogging = true`

### Issue: Sensitive data in logs
**Fix:** Add path to `SensitivePaths` or header to `SensitiveHeaders`

### Issue: Too many logs
**Fix:** Add paths to `ExcludedPaths` or disable `LogResponseBody`

### Issue: Response body is empty
**Fix:** Enable `LogResponseBody = true` (check performance)

### Issue: Exceptions not caught
**Fix:** Ensure `UseExceptionHandling()` is called **first** in middleware pipeline

---

## ?? Documentation Links

- **Full Documentation:** [README.md](README.md)
- **Example Config:** [appsettings.example.json](appsettings.example.json)

---

## ? Checklist for New Projects

- [ ] Copy `appsettings.example.json` to your project
- [ ] Customize `ExcludedPaths` for your endpoints
- [ ] Add your sensitive paths to `SensitivePaths`
- [ ] Register services in `Program.cs`
- [ ] Configure middleware pipeline in correct order
- [ ] Test exception handling
- [ ] Test logging output
- [ ] Verify sensitive data is redacted
- [ ] Set production-appropriate settings
- [ ] Add health checks (optional)

---

**Version:** 2.0 | **Last Updated:** January 2025 | **Status:** ? Production Ready
