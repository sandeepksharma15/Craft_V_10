# Craft.Controllers - Enhanced Features Guide

> **Version:** 1.1.0  
> **Target Framework:** .NET 10  
> **Features:** Rate Limiting, API Versioning

---

## ?? Table of Contents

1. [Overview](#overview)
2. [Rate Limiting](#rate-limiting)
3. [API Versioning](#api-versioning)
4. [Swagger/OpenAPI Documentation](#swaggeropenapi-documentation)
5. [Complete Setup Example](#complete-setup-example)
6. [Best Practices](#best-practices)
7. [Troubleshooting](#troubleshooting)

---

## ?? Overview

Craft.Controllers provides production-ready features to enhance your API:

- **?? Rate Limiting** - Protect your API from abuse with configurable request throttling
- **?? API Versioning** - Support multiple API versions simultaneously
- **?? Swagger/OpenAPI** - Use the **Craft.OpenAPI** module for comprehensive API documentation

---

## ?? Rate Limiting

### Quick Start

**Important:** Due to .NET class library limitations, rate limiting must be configured directly in your `Program.cs`:

```csharp
// Program.cs
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add rate limiting with default policies
builder.Services.AddRateLimiter(options =>
{
    // Configure rejection behavior
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = retryAfter.TotalSeconds.ToString();
            
            await context.HttpContext.Response.WriteAsJsonAsync(new
            {
                error = "Too many requests",
                message = $"Rate limit exceeded. Please retry after {retryAfter.TotalSeconds} seconds.",
                retryAfter = retryAfter.TotalSeconds
            }, cancellationToken);
        }
        else
        {
            await context.HttpContext.Response.WriteAsJsonAsync(new
            {
                error = "Too many requests",
                message = "Rate limit exceeded. Please try again later."
            }, cancellationToken);
        }
    };

    // Read operations policy (GET) - More permissive
    options.AddFixedWindowLimiter("read-policy", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 10;
    });

    // Write operations policy (POST, PUT) - Moderate
    options.AddFixedWindowLimiter("write-policy", opt =>
    {
        opt.PermitLimit = 30;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 5;
    });

    // Delete operations policy - Restrictive
    options.AddFixedWindowLimiter("delete-policy", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2;
    });

    // Bulk operations policy - Very restrictive
    options.AddSlidingWindowLimiter("bulk-policy", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.SegmentsPerWindow = 4;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 1;
    });
});

builder.Services.AddControllers();

var app = builder.Build();

// IMPORTANT: Add rate limiter middleware BEFORE MapControllers
app.UseRateLimiter();
app.MapControllers();

app.Run();
```

### Default Policies

| Policy Name | Permit Limit | Window | Use Case |
|------------|--------------|--------|----------|
| `read-policy` | 100 requests | 1 minute | GET operations |
| `write-policy` | 30 requests | 1 minute | POST, PUT operations |
| `delete-policy` | 10 requests | 1 minute | DELETE operations |
| `bulk-policy` | 5 requests | 1 minute | Bulk operations |
| `authenticated-policy` | 200/50 requests | 1 minute | Auth/unauth users |
| `concurrent-policy` | 10 concurrent | N/A | Resource-intensive ops |

### Using Rate Limiting in Controllers

```csharp
using Microsoft.AspNetCore.RateLimiting;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : EntityChangeController<Product, ProductDto>
{
    [HttpGet]
    [EnableRateLimiting("read-policy")]  // 100 requests/minute
    public async Task<IActionResult> GetAll() { }
    
    [HttpPost]
    [EnableRateLimiting("write-policy")]  // 30 requests/minute
    public async Task<IActionResult> Create([FromBody] ProductDto model) { }
    
    [HttpDelete("{id}")]
    [EnableRateLimiting("delete-policy")]  // 10 requests/minute
    public async Task<IActionResult> Delete(int id) { }
    
    [HttpPost("bulk")]
    [EnableRateLimiting("bulk-policy")]  // 5 requests/minute
    public async Task<IActionResult> BulkCreate([FromBody] List<ProductDto> models) { }
}
```

### Custom Rate Limit Policy

```csharp
builder.Services.AddCustomRateLimitPolicy("custom-policy", context =>
    RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 50,
            Window = TimeSpan.FromSeconds(30),
            QueueLimit = 5
        }));
```

### Rate Limit Response

When rate limit is exceeded, the API returns:

```json
{
  "error": "Too many requests",
  "message": "Rate limit exceeded. Please retry after 45 seconds.",
  "retryAfter": 45
}
```

**HTTP Headers:**
- `Retry-After: 45` - Seconds to wait before retrying
- `X-RateLimit-Limit: 100` - Rate limit ceiling
- `X-RateLimit-Remaining: 0` - Remaining requests

---

## ?? API Versioning

### Quick Start

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add API versioning
builder.Services.AddControllerApiVersioning();
builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();
app.Run();
```

### Versioning in Controllers

#### Option 1: URL Segment Versioning

```csharp
using Asp.Versioning;

[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class UsersController : EntityReadController<User, UserDto>
{
    [HttpGet]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetAllV1()
    {
        // Version 1.0 implementation
        // URL: /api/v1/users
    }
    
    [HttpGet]
    [MapToApiVersion("2.0")]
    public async Task<IActionResult> GetAllV2([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        // Version 2.0 implementation with pagination
        // URL: /api/v2/users
    }
}
```

#### Option 2: Query String Versioning

```csharp
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/[controller]")]
[ApiController]
public class ProductsController : EntityReadController<Product, ProductDto>
{
    [HttpGet]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetAllV1()
    {
        // URL: /api/products?api-version=1.0
    }
    
    [HttpGet]
    [MapToApiVersion("2.0")]
    public async Task<IActionResult> GetAllV2()
    {
        // URL: /api/products?api-version=2.0
    }
}
```

#### Option 3: Header Versioning

```csharp
// Client sends: X-Api-Version: 1.0
// OR: Api-Version: 1.0

[ApiVersion("1.0")]
[Route("api/[controller]")]
public class OrdersController : EntityReadController<Order, OrderDto>
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        // Version determined by header
    }
}
```

### Deprecating API Versions

```csharp
[ApiVersion("1.0", Deprecated = true)]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class LegacyController : EntityReadController<Legacy, LegacyDto>
{
    [HttpGet]
    [MapToApiVersion("1.0")]
    [Obsolete("This endpoint is deprecated. Use v2.0 instead.")]
    public async Task<IActionResult> GetAllV1()
    {
        // Deprecated implementation
    }
    
    [HttpGet]
    [MapToApiVersion("2.0")]
    public async Task<IActionResult> GetAllV2()
    {
        // New implementation
    }
}
```

### API Version Discovery

Clients can discover supported API versions via response headers:

```http
GET /api/v1/users HTTP/1.1

HTTP/1.1 200 OK
Api-Supported-Versions: 1.0, 2.0
Api-Deprecated-Versions: 1.0
```

---

## ?? Swagger/OpenAPI Documentation

For comprehensive Swagger/OpenAPI documentation features, use the **Craft.OpenAPI** module which provides:

- **Multiple Security Schemes**: JWT Bearer, API Key, OAuth2
- **UI Customization**: Themes, custom CSS/JS, layout options
- **XML Documentation**: Automatic inclusion with validation
- **Environment-Specific**: Different configs per environment
- **Configuration Validation**: Startup validation with detailed errors
- **API Versioning Support**: Automatic versioned document generation
- **Tag Descriptions**: Custom descriptions for endpoint groups
- **Deep Linking**: Direct links to operations
- **Authorization Persistence**: Remember auth across sessions

### Quick Start with Craft.OpenAPI

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add API versioning
builder.Services.AddControllerApiVersioning();

// Add OpenAPI documentation from Craft.OpenAPI module
builder.Services.AddOpenApiDocumentation(builder.Configuration);

builder.Services.AddControllers();

var app = builder.Build();

// Use OpenAPI documentation
app.UseOpenApiDocumentation();

app.MapControllers();
app.Run();
```

### Configuration (appsettings.json)

```json
{
  "SwaggerOptions": {
    "Enable": true,
    "Title": "My API",
    "Version": "v1",
    "Description": "My API Documentation",
    "Security": {
      "EnableJwtBearer": true
    }
  }
}
```

For complete documentation, see the **Craft.OpenAPI README.md** file.

---

## ?? Complete Setup Example

Here's a complete `Program.cs` with all features enabled:

```csharp
using Craft.Controllers.Extensions;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add rate limiting (must be configured directly in Program.cs)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = retryAfter.TotalSeconds.ToString();
            
            await context.HttpContext.Response.WriteAsJsonAsync(new
            {
                error = "Too many requests",
                message = $"Rate limit exceeded. Please retry after {retryAfter.TotalSeconds} seconds.",
                retryAfter = retryAfter.TotalSeconds
            }, cancellationToken);
        }
    };

    // Define policies
    options.AddFixedWindowLimiter("read-policy", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 10;
    });

    options.AddFixedWindowLimiter("write-policy", opt =>
    {
        opt.PermitLimit = 30;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 5;
    });

    options.AddFixedWindowLimiter("delete-policy", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 2;
    });
});

// Add API versioning
builder.Services.AddControllerApiVersioning();

// Add OpenAPI documentation (from Craft.OpenAPI module)
builder.Services.AddOpenApiDocumentation(builder.Configuration);

// Add controllers
builder.Services.AddControllers();

// Add authentication
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Use OpenAPI documentation (from Craft.OpenAPI module)
if (app.Environment.IsDevelopment())
{
    app.UseOpenApiDocumentation();
}

app.UseHttpsRedirection();

// Rate limiter MUST come before authorization
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
```

---

## ? Best Practices

### Rate Limiting

1. **Apply appropriate policies** based on operation cost:
   - Read operations: More permissive (100/min)
   - Write operations: Moderate (30/min)
   - Delete operations: Restrictive (10/min)
   - Bulk operations: Very restrictive (5/min)

2. **Use authenticated-policy** for known users:
   ```csharp
   [Authorize]
   [EnableRateLimiting("authenticated-policy")]  // Higher limits for auth users
   ```

3. **Provide meaningful error messages**:
   ```csharp
   options.OnRejected = async (context, cancellationToken) =>
   {
       await context.HttpContext.Response.WriteAsJsonAsync(new
       {
           error = "Rate limit exceeded",
           message = $"Please retry after {retryAfter} seconds",
           retryAfter = retryAfter.TotalSeconds,
           documentation = "https://api.example.com/docs/rate-limits"
       }, cancellationToken);
   };
   ```

### API Versioning

1. **Use URL segment versioning** for major versions:
   - `/api/v1/users`
   - `/api/v2/users`

2. **Use query string versioning** for minor versions:
   - `/api/users?api-version=1.1`

3. **Deprecate gracefully**:
   ```csharp
   [ApiVersion("1.0", Deprecated = true)]
   [ApiVersion("2.0")]
   ```

4. **Document breaking changes** in API descriptions:
   ```csharp
   /// <remarks>
   /// **Breaking Changes in v2.0:**
   /// - Field `userName` renamed to `username`
   /// - Field `email` now required
   /// - Added pagination support
   /// </remarks>
   ```

### Swagger/OpenAPI Documentation

For comprehensive Swagger/OpenAPI documentation best practices, refer to the **Craft.OpenAPI** module documentation which includes:

- XML documentation configuration
- Security scheme setup
- UI customization options
- Response type documentation
- Example value provision

---

## ?? Troubleshooting

### Rate Limiting Not Working

**Issue:** Rate limits not applied

**Solution:**
1. Check middleware order - `UseRateLimiter()` must come BEFORE `UseAuthorization()`
2. Verify policy name matches: `[EnableRateLimiting("read-policy")]`
3. Ensure rate limiter is configured in `Program.cs`

### API Versioning Not Working

**Issue:** Version not detected

**Solution:**
1. Add route template: `[Route("api/v{version:apiVersion}/[controller]")]`
2. Check API version reader configuration
3. Verify `AssumeDefaultVersionWhenUnspecified = true` for backward compatibility

### Swagger/OpenAPI Documentation Issues

For Swagger/OpenAPI troubleshooting, please refer to the **Craft.OpenAPI** module documentation which includes:
- Swagger UI not accessible
- XML documentation not showing
- Security scheme not working
- Custom CSS not loading

---

## ?? Additional Resources

- [ASP.NET Core Rate Limiting](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit)
- [ASP.NET Core API Versioning](https://github.com/dotnet/aspnet-api-versioning)
- [Craft.OpenAPI Documentation](../Craft.OpenAPI/README.md)

---

**Version:** 1.1.0  
**Last Updated:** December 2025  
**Target Framework:** .NET 10  
**Status:** ? Production Ready
