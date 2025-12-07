# CORS Configuration

This module provides extension methods for configuring CORS (Cross-Origin Resource Sharing) policies in ASP.NET Core applications using the **Options Pattern** for better testability and configuration management.

## Features

- ? **Configuration-based** - Define allowed origins in `appsettings.json`
- ? **Options Pattern** - Uses `IOptions<CorsSettings>` for better testability
- ? **Automatic Validation** - Validates configuration at startup with `ValidateDataAnnotations()` and `ValidateOnStart()`
- ? **Multiple Frontend Support** - Separate settings for Angular, Blazor, and React
- ? **Semicolon-separated** - Multiple origins per frontend type
- ? **Permissive Policy** - Allows any header, any method, and credentials
- ? **Logging Support** - Warns when no origins are configured
- ? **Input Validation** - Throws on null arguments
- ? **Fail-Fast** - Configuration errors detected at startup

## Quick Start

### 1. Configure in `appsettings.json`

```json
{
  "CorsSettings": {
    "Angular": "http://localhost:4200;https://angular.app.com",
    "Blazor": "http://localhost:5000;https://blazor.app.com",
    "React": "http://localhost:3000;https://react.app.com"
  }
}
```

### 2. Register CORS Policy in `Program.cs`

```csharp
using Craft.Infrastructure.Cors;

var builder = WebApplication.CreateBuilder(args);

// Add CORS policy from configuration
builder.Services.AddCorsPolicy(builder.Configuration);

var app = builder.Build();

// Use CORS policy (before routing/endpoints)
app.UseCorsPolicy();

app.MapControllers();
app.Run();
```

## Configuration Options

### CorsSettings

| Property | Type | Description | Example |
|----------|------|-------------|---------|
| `Angular` | `string?` | Semicolon-separated list of Angular app origins | `"http://localhost:4200;https://angular.app.com"` |
| `Blazor` | `string?` | Semicolon-separated list of Blazor app origins | `"http://localhost:5000;https://blazor.app.com"` |
| `React` | `string?` | Semicolon-separated list of React app origins | `"http://localhost:3000;https://react.app.com"` |

### CORS Policy Details

The configured policy allows:

- ? **Any Header** - `AllowAnyHeader()`
- ? **Any Method** - `AllowAnyMethod()` (GET, POST, PUT, DELETE, etc.)
- ? **Credentials** - `AllowCredentials()` (cookies, authorization headers)
- ? **Specified Origins** - Only from configured `CorsSettings`

## Usage Examples

### Example 1: Single Origin per Frontend

```json
{
  "CorsSettings": {
    "Angular": "http://localhost:4200",
    "Blazor": "http://localhost:5000",
    "React": "http://localhost:3000"
  }
}
```

### Example 2: Multiple Origins per Frontend

```json
{
  "CorsSettings": {
    "Angular": "http://localhost:4200;https://angular.app.com;https://angular.staging.com",
    "Blazor": "http://localhost:5000;https://blazor.app.com",
    "React": "http://localhost:3000;https://react.app.com;https://react.staging.com"
  }
}
```

### Example 3: Only One Frontend

```json
{
  "CorsSettings": {
    "React": "http://localhost:3000;https://react.app.com"
  }
}
```

### Example 4: Development vs Production

**appsettings.Development.json**
```json
{
  "CorsSettings": {
    "Angular": "http://localhost:4200",
    "Blazor": "http://localhost:5000",
    "React": "http://localhost:3000"
  }
}
```

**appsettings.Production.json**
```json
{
  "CorsSettings": {
    "Angular": "https://angular.app.com",
    "Blazor": "https://blazor.app.com",
    "React": "https://react.app.com"
  }
}
```

## Advanced Usage

### Custom CORS Policy

If you need more control, you can configure CORS manually:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("CustomPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .WithMethods("GET", "POST")
              .WithHeaders("Content-Type", "Authorization")
              .AllowCredentials();
    });
});

app.UseCors("CustomPolicy");
```

### Conditional CORS Configuration

```csharp
if (app.Environment.IsDevelopment())
{
    // Allow all origins in development
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("DevelopmentPolicy", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });
}
else
{
    // Use configured origins in production
    builder.Services.AddCorsPolicy(builder.Configuration);
}
```

## Logging

The module logs warnings when:

1. **CorsSettings section not found** in configuration
2. **No origins configured** (all frontend settings are empty/null)

Example log output:

```
[Warning] CorsSettings section not found in configuration. CORS policy will not allow any origins.
[Warning] No CORS origins configured. The CORS policy will not allow any origins.
```

## Troubleshooting

### Issue: CORS errors in browser

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

### Issue: No origins allowed

**Symptom:** All CORS requests are blocked

**Solutions:**

1. **Verify configuration section exists** - Check `appsettings.json` has `CorsSettings`
2. **Check origin format** - Origins must include protocol: `http://` or `https://`
3. **Check for typos** - Property names are case-sensitive in JSON
4. **Review logs** - Check for warning messages about missing configuration

### Issue: Credentials not working

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

## Best Practices

1. ? **Use specific origins** - Never use `AllowAnyOrigin()` in production
2. ? **Use HTTPS in production** - Always use `https://` for production origins
3. ? **Limit origins** - Only add origins you control and trust
4. ? **Use environment-specific configs** - Different origins for dev/staging/production
5. ? **Test CORS policies** - Verify from actual frontend applications
6. ? **Monitor logs** - Check for CORS warnings during deployment
7. ?? **Don't use wildcards** - Avoid `*.example.com` patterns (not supported by this implementation)

## Security Considerations

?? **Important Security Notes:**

1. **AllowCredentials + AllowAnyOrigin is not allowed** - This combination is forbidden by browsers for security
2. **Validate origins carefully** - Only allow origins you trust
3. **Use HTTPS** - Always use HTTPS for production origins to prevent MITM attacks
4. **Review regularly** - Periodically review and remove unused origins

## Integration with Other Craft Modules

This module integrates seamlessly with:

- **Craft.Logging** - Logs CORS configuration warnings
- **Craft.Security** - Works with authentication/authorization
- **Craft.MultiTenant** - Can be used with tenant-specific origins (custom implementation needed)

## Dependencies

- `Microsoft.AspNetCore.Cors` v2.3.0
- `Microsoft.Extensions.Configuration.Binder` v10.0.0
- Project reference: `Craft.Extensions`

## Target Framework

- **.NET 10** - Uses C# 14.0 features

---

**Last Updated:** January 2025  
**Version:** 1.0.0  
**Status:** ? Production Ready

---

For more details, review the XML documentation in the source code.
