# Craft.Controllers - Enhanced Features Guide

> **Version:** 1.1.0  
> **Target Framework:** .NET 10  
> **New Features:** Rate Limiting, API Versioning, Enhanced Swagger/OpenAPI Documentation

---

## ?? Table of Contents

1. [Overview](#overview)
2. [Rate Limiting](#rate-limiting)
3. [API Versioning](#api-versioning)
4. [Enhanced Swagger/OpenAPI](#enhanced-swaggeropenapi)
5. [Complete Setup Example](#complete-setup-example)
6. [Best Practices](#best-practices)
7. [Troubleshooting](#troubleshooting)

---

## ?? Overview

Craft.Controllers now includes three production-ready features to enhance your API:

- **?? Rate Limiting** - Protect your API from abuse with configurable request throttling
- **?? API Versioning** - Support multiple API versions simultaneously
- **?? Enhanced Swagger/OpenAPI** - Comprehensive API documentation with examples

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

## ?? Enhanced Swagger/OpenAPI

### Quick Start

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add enhanced Swagger
builder.Services.AddEnhancedSwagger();
builder.Services.AddControllers();

var app = builder.Build();

// Use Swagger
app.UseSwagger();
app.UseEnhancedSwaggerUI();

app.MapControllers();
app.Run();
```

### With API Versioning

```csharp
using Asp.Versioning.ApiExplorer;

// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add API versioning with Swagger support
builder.Services.AddControllerApiVersioningWithSwagger();
builder.Services.AddEnhancedSwagger();
builder.Services.ConfigureSwaggerForVersioning();
builder.Services.AddControllers();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    
    foreach (var description in provider.ApiVersionDescriptions)
    {
        options.SwaggerEndpoint(
            $"/swagger/{description.GroupName}/swagger.json",
            description.GroupName.ToUpperInvariant());
    }
    
    options.RoutePrefix = "swagger";
    options.DocumentTitle = "My API Documentation";
    options.EnableDeepLinking();
    options.DisplayRequestDuration();
});

app.MapControllers();
app.Run();
```

### XML Documentation

**1. Enable XML documentation in .csproj:**

```xml
<PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

**2. Add comprehensive XML comments:**

```csharp
/// <summary>
/// Creates a new product.
/// </summary>
/// <param name="model">The product data.</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>The created product.</returns>
/// <remarks>
/// Sample request:
/// 
///     POST /api/products
///     {
///         "name": "Laptop",
///         "price": 999.99,
///         "category": "Electronics"
///     }
/// 
/// </remarks>
/// <response code="201">Returns the newly created product</response>
/// <response code="400">If the model is invalid</response>
/// <response code="401">If user is not authenticated</response>
[HttpPost]
[ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public async Task<IActionResult> Create([FromBody] ProductDto model, CancellationToken cancellationToken)
{
    // Implementation
}
```

### Swagger Features

The enhanced Swagger configuration includes:

? **JWT Bearer Authentication** - Authorize button for token input  
? **API Key Authentication** - Support for API key headers  
? **XML Documentation** - Automatic inclusion from XML comments  
? **Response Headers** - Documents all response headers (X-Request-Id, X-RateLimit-*, etc.)  
? **Rate Limiting Info** - Shows rate limits in operation descriptions  
? **Authorization Info** - Shows required roles/policies  
? **Enum Descriptions** - Readable enum values  
? **Required Fields** - Marks non-nullable properties as required  
? **429 Responses** - Automatic documentation for rate-limited endpoints  
? **401/403 Responses** - Automatic documentation for authorized endpoints  

### Swagger UI Customization

```csharp
builder.Services.AddEnhancedSwagger(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "My Custom API",
        Version = "v1",
        Description = "My custom API description",
        Contact = new OpenApiContact
        {
            Name = "John Doe",
            Email = "john@example.com",
            Url = new Uri("https://example.com")
        }
    });
    
    // Add custom security definitions
    options.AddSecurityDefinition("OAuth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri("https://auth.example.com/authorize"),
                TokenUrl = new Uri("https://auth.example.com/token"),
                Scopes = new Dictionary<string, string>
                {
                    { "read", "Read access" },
                    { "write", "Write access" }
                }
            }
        }
    });
});
```

---

## ?? Complete Setup Example

Here's a complete `Program.cs` with all features enabled:

```csharp
using Craft.Controllers.Extensions;
using Asp.Versioning.ApiExplorer;
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
builder.Services.AddControllerApiVersioningWithSwagger();

// Add enhanced Swagger
builder.Services.AddEnhancedSwagger();
builder.Services.ConfigureSwaggerForVersioning();

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

// Middleware order is important!
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                $"API {description.GroupName.ToUpperInvariant()}");
        }
        
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "Craft API Documentation";
        options.EnableDeepLinking();
        options.DisplayRequestDuration();
        options.EnableTryItOutByDefault();
    });
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

### Swagger/OpenAPI

1. **Always enable XML documentation**:
   ```xml
   <GenerateDocumentationFile>true</GenerateDocumentationFile>
   ```

2. **Provide examples in XML comments**:
   ```csharp
   /// <remarks>
   /// Sample request:
   ///     POST /api/users
   ///     { "name": "John", "email": "john@example.com" }
   /// </remarks>
   ```

3. **Document all response codes**:
   ```csharp
   [ProducesResponseType(typeof(UserDto), 200)]
   [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
   [ProducesResponseType(401)]
   [ProducesResponseType(403)]
   [ProducesResponseType(429)]
   [ProducesResponseType(500)]
   ```

4. **Use schema filters** for consistent documentation:
   ```csharp
   options.SchemaFilter<EnumSchemaFilter>();
   options.SchemaFilter<RequiredNotNullableSchemaFilter>();
   ```

---

## ?? Troubleshooting

### Rate Limiting Not Working

**Issue:** Rate limits not applied

**Solution:**
1. Check middleware order - `UseRateLimiter()` must come BEFORE `UseAuthorization()`
2. Verify policy name matches: `[EnableRateLimiting("read-policy")]`
3. Check that `AddControllerRateLimiting()` is called

### API Versioning Not Working

**Issue:** Version not detected

**Solution:**
1. Add route template: `[Route("api/v{version:apiVersion}/[controller]")]`
2. Check API version reader configuration
3. Verify `AssumeDefaultVersionWhenUnspecified = true` for backward compatibility

### Swagger Not Showing

**Issue:** Swagger UI not accessible

**Solution:**
1. Check environment: Swagger typically only in Development
2. Verify route prefix: Default is `/swagger`
3. Check XML documentation file is generated
4. Ensure `UseSwagger()` comes before `UseSwaggerUI()`

### XML Comments Not Showing

**Issue:** Descriptions missing in Swagger

**Solution:**
1. Enable XML generation in .csproj:
   ```xml
   <GenerateDocumentationFile>true</GenerateDocumentationFile>
   ```
2. Rebuild project to generate .xml file
3. Check XML file exists in output directory
4. Verify `IncludeXmlComments()` is called

---

## ?? Additional Resources

- [ASP.NET Core Rate Limiting](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit)
- [ASP.NET Core API Versioning](https://github.com/dotnet/aspnet-api-versioning)
- [Swashbuckle Documentation](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)

---

**Version:** 1.1.0  
**Last Updated:** January 2025  
**Target Framework:** .NET 10  
**Status:** ? Production Ready
