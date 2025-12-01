# Quick Reference Card

## ?? Quick Setup (3 Steps)

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
- **Migration Guide:** [MIGRATION.md](MIGRATION.md)
- **Example Config:** [appsettings.example.json](appsettings.example.json)
- **Change Log:** [CHANGES.md](CHANGES.md)

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
