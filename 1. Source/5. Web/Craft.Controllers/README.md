# Craft.Controllers

> **Version:** 1.0.32  
> **Target Framework:** .NET 10  
> **Author:** Sandeep SHARMA

A comprehensive ASP.NET Core controller library providing base controllers for CRUD operations, database error handling, API versioning, and rate limiting capabilities.

---

## 📑 Table of Contents

1. [Overview](#-overview)
2. [Installation](#-installation)
3. [Quick Start](#-quick-start)
4. [Core Features](#-core-features)
   - [Base Controllers](#base-controllers)
   - [Database Error Handling](#database-error-handling)
   - [API Versioning](#api-versioning)
   - [Rate Limiting](#rate-limiting)
5. [Base Controllers](#-base-controllers)
   - [EntityReadController](#entityreadcontroller)
   - [EntityChangeController](#entitychangecontroller)
6. [Database Error Handling](#-database-error-handling)
7. [API Versioning](#-api-versioning)
8. [Rate Limiting](#-rate-limiting)
9. [Architecture](#-architecture)
10. [Best Practices](#-best-practices)
11. [Troubleshooting](#-troubleshooting)
12. [Dependencies](#-dependencies)

---

## 🎯 Overview

Craft.Controllers is a production-ready library that provides:

- **🎮 Base Controllers**: Generic, reusable controllers for CRUD operations
- **🛡️ Database Error Handling**: Intelligent error message translation using Strategy Pattern
- **📊 API Versioning**: Support for multiple API versions simultaneously
- **🚦 Rate Limiting**: Protection against API abuse with configurable policies
- **✨ Clean Architecture**: Separation of concerns with interfaces and abstractions
- **🔍 Structured Logging**: Comprehensive logging with Serilog support
- **🎯 Best Practices**: Built-in support for .NET 10 patterns and conventions

---

## 📦 Installation

Add the project reference to your API project:

```xml
<ItemGroup>
  <ProjectReference Include="path\to\Craft.Controllers\Craft.Controllers.csproj" />
</ItemGroup>
```

### Dependencies

Craft.Controllers depends on:

- **Craft.Core** - Core utilities and extensions
- **Craft.Domain** - Domain entities and interfaces
- **Craft.Repositories** - Repository pattern implementation
- **Mapster** (v10.0.0-pre01) - Object-to-object mapping
- **Humanizer.Core** (v3.0.1) - String humanization
- **Asp.Versioning.*** (v8.1.1) - API versioning support

---

## 🚀 Quick Start

### 1. Register Services

```csharp
// Program.cs
using Craft.Controllers.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add database error handling
builder.Services.AddDatabaseErrorHandling();

// Add API versioning
builder.Services.AddControllerApiVersioning();

// Add your repositories and services
builder.Services.AddScoped<IChangeRepository<Product>, ChangeRepository<Product>>();

builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
```

### 2. Create Your Controller

```csharp
using Craft.Controllers;
using Craft.Controllers.ErrorHandling;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductsController : EntityChangeController<Product, ProductDto>
{
    public ProductsController(
        IChangeRepository<Product> repository,
        ILogger<EntityChangeController<Product, ProductDto>> logger,
        IDatabaseErrorHandler databaseErrorHandler)
        : base(repository, logger, databaseErrorHandler)
    {
    }
}
```

### 3. Use Your API

```bash
# Get all products
GET /api/v1/products/false

# Get product by ID
GET /api/v1/products/{id}/false

# Create product
POST /api/v1/products
Content-Type: application/json
{
  "name": "New Product",
  "price": 99.99
}

# Update product
PUT /api/v1/products
Content-Type: application/json
{
  "id": 1,
  "name": "Updated Product",
  "price": 89.99
}

# Delete product
DELETE /api/v1/products/{id}
```

---

## 🎨 Core Features

### Base Controllers

Craft.Controllers provides two main base controllers:

1. **EntityReadController** - Read-only operations (GET)
2. **EntityChangeController** - Full CRUD operations (GET, POST, PUT, DELETE)

Both controllers are generic and work with any entity type that implements `IEntity<TKey>`.

### Database Error Handling

Automatic translation of database exceptions into user-friendly error messages using the Strategy Pattern. Supports:

- **PostgreSQL** - Error code translation
- **SQL Server** - Error number translation
- **Generic Fallback** - Pattern matching for unknown providers

### API Versioning

Built-in support for multiple API versioning strategies:

- **URL Segment** - `/api/v1/products`
- **Query String** - `/api/products?api-version=1.0`
- **Header** - `X-Api-Version: 1.0`
- **Media Type** - `Accept: application/json;version=1.0`

### Rate Limiting

Protection against API abuse with configurable policies (requires .NET Rate Limiting middleware).

---

## 🎮 Base Controllers

### EntityReadController

Provides read-only operations for entities.

#### Endpoints

| Method | Route | Description | Response |
|--------|-------|-------------|----------|
| GET | `/{includeDetails:bool}` | Get all entities | `200 OK` with entity list |
| GET | `/{id}/{includeDetails:bool}` | Get entity by ID | `200 OK` or `404 Not Found` |
| GET | `/count` | Get total count | `200 OK` with count |
| GET | `/getpaged/{page}/{pageSize}/{includeDetails}` | Get paginated list | `200 OK` with `PageResponse<T>` |
| GET | `/exists/{id}` | Check if entity exists | `200 OK` with boolean |
| GET | `/any` | Check if any entities exist | `200 OK` with boolean |

#### Usage Example

```csharp
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductsReadController : EntityReadController<Product, ProductDto>
{
    public ProductsReadController(
        IReadRepository<Product> repository,
        ILogger<EntityReadController<Product, ProductDto>> logger)
        : base(repository, logger)
    {
    }
    
    // All read operations are inherited and ready to use
    // Optionally override to customize behavior
    
    public override async Task<ActionResult<Product>> GetAsync(
        KeyType id, 
        bool includeDetails, 
        CancellationToken cancellationToken = default)
    {
        // Custom logic here
        return await base.GetAsync(id, includeDetails, cancellationToken);
    }
}
```

#### Features

✅ **Automatic Logging** - Debug-level logging for all operations  
✅ **Cancellation Support** - All methods accept `CancellationToken`  
✅ **Include Details** - Support for loading related entities  
✅ **Pagination** - Built-in pagination with metadata  
✅ **Error Handling** - Automatic exception handling with proper HTTP status codes  
✅ **XML Documentation** - Comprehensive documentation for OpenAPI/Swagger

### EntityChangeController

Extends `EntityReadController` with full CRUD operations.

#### Endpoints

| Method | Route | Description | Response |
|--------|-------|-------------|----------|
| POST | `/` | Create entity | `201 Created` with entity |
| POST | `/addrange` | Create multiple entities | `200 OK` with entity list |
| PUT | `/` | Update entity | `200 OK` with entity |
| POST | `/updaterange` | Update multiple entities | `200 OK` with entity list |
| DELETE | `/{id}` | Delete entity | `200 OK` or `404 Not Found` |
| PUT | `/deleterange` | Delete multiple entities | `200 OK` |
| PUT | `/restore` | Restore soft-deleted entity | `200 OK` with entity |
| POST | `/restorerange` | Restore multiple entities | `200 OK` with entity list |

#### Usage Example

```csharp
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductsController : EntityChangeController<Product, ProductDto>
{
    public ProductsController(
        IChangeRepository<Product> repository,
        ILogger<EntityChangeController<Product, ProductDto>> logger,
        IDatabaseErrorHandler databaseErrorHandler)
        : base(repository, logger, databaseErrorHandler)
    {
    }
    
    // All CRUD operations are inherited and ready to use
    // Optionally override to customize behavior
    
    protected override ActionResult<Product> ReturnProperError(Exception ex)
    {
        // Custom error handling
        return base.ReturnProperError(ex);
    }
}
```

#### Features

✅ **Soft Delete Support** - Automatic soft delete for entities implementing `ISoftDelete`  
✅ **Batch Operations** - Range methods for bulk operations  
✅ **Restore Support** - Restore soft-deleted entities  
✅ **Concurrency Handling** - Automatic handling of `DbUpdateConcurrencyException`  
✅ **Database Error Translation** - User-friendly error messages  
✅ **Entity Validation** - Checks existence before update/delete operations  
✅ **Automatic Mapping** - DTO to Entity mapping using Mapster  
✅ **Transaction Support** - Automatic transactions for large batch operations (>100 items)

---

## 🛡️ Database Error Handling

Craft.Controllers uses the **Strategy Pattern** to provide database-specific error messages while keeping controller code clean.

### Architecture

```
┌─────────────────────────────────────────┐
│       EntityChangeController            │
│  (Catches DbUpdateException)            │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│      IDatabaseErrorHandler              │
│  (Orchestrates error handling)          │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│    IDatabaseErrorStrategy[]             │
│  (Database-specific strategies)         │
└─────────────────────────────────────────┘
       │           │           │
       ▼           ▼           ▼
 PostgreSQL   SQL Server   Generic
  Strategy     Strategy    Strategy
```

### Supported Databases

#### PostgreSQL
- Unique constraint violations (23505)
- Foreign key violations (23503)
- Not null violations (23502)
- Check constraint violations (23514)
- Data type mismatch (22P02)
- String truncation (22001)
- Numeric overflow (22003)
- And more...

#### SQL Server
- Unique constraint violations (2601, 2627)
- Foreign key violations (547)
- Cannot insert null (515)
- String truncation (2628, 8152)
- Invalid object name (208)
- Arithmetic overflow (8115)
- And more...

#### Generic Fallback
- Pattern matching for common error types
- Works with any database provider

### Error Message Examples

#### Before (Technical)
```
Npgsql.PostgresException: 23505: duplicate key value violates unique constraint "IX_Products_Name"
```

#### After (User-Friendly)
```
A Product with this Name already exists. Please use a different value.
```

### Usage

Error handling is automatic in all `EntityChangeController` operations:

```csharp
[HttpPost]
public virtual async Task<ActionResult<Product>> AddAsync(
    ProductDto model, 
    CancellationToken cancellationToken = default)
{
    try
    {
        var entity = await repository.AddAsync(model.Adapt<Product>(), cancellationToken);
        return Created(new Uri($"{entity.Id}/false", UriKind.Relative), entity);
    }
    catch (Exception ex)
    {
        // Automatically translates database errors to user-friendly messages
        return ReturnProperError(ex);
    }
}
```

### Registration

```csharp
// Program.cs
using Craft.Controllers.Extensions;

builder.Services.AddDatabaseErrorHandling();
```

This registers:
- `IDatabaseErrorHandler` (DatabaseErrorHandler)
- `IDatabaseErrorStrategy` implementations:
  - PostgreSqlErrorStrategy (Priority 1)
  - SqlServerErrorStrategy (Priority 2)
  - GenericErrorStrategy (Priority 999)

### Extending

Add support for a new database by creating a strategy:

```csharp
public class MySqlErrorStrategy : IDatabaseErrorStrategy
{
    public bool CanHandle(ErrorContext context)
    {
        return context.Exception.GetType().Name.Contains("MySql");
    }
    
    public string GetErrorMessage(ErrorContext context)
    {
        // Extract error code and return user-friendly message
        return "User-friendly error message";
    }
    
    public int Priority => 3; // Lower = higher priority
}
```

Register it:

```csharp
services.AddSingleton<IDatabaseErrorStrategy, MySqlErrorStrategy>();
```

### Error Context Model

```csharp
public class ErrorContext
{
    public Exception Exception { get; init; }
    public Type EntityType { get; init; }
    public string? SqlState { get; set; }
    public int? ErrorNumber { get; set; }
    public string? ConstraintName { get; set; }
    public string? TableName { get; set; }
    public string? ColumnName { get; set; }
}
```

---

## 📊 API Versioning

Craft.Controllers provides comprehensive API versioning support using `Asp.Versioning` libraries.

### Quick Start

```csharp
// Program.cs
builder.Services.AddControllerApiVersioning();
```

### Default Configuration

- **Default Version:** 1.0
- **Assume Default:** When client doesn't specify, use default version
- **Report Versions:** Include supported versions in response headers
- **Multiple Readers:** URL segment, query string, header, media type

### Versioning Strategies

#### 1. URL Segment Versioning (Recommended)

```csharp
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class ProductsController : EntityChangeController<Product, ProductDto>
{
    [HttpGet]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetAllV1()
    {
        // Version 1.0 implementation
        // URL: /api/v1/products
    }
    
    [HttpGet]
    [MapToApiVersion("2.0")]
    public async Task<IActionResult> GetAllV2([FromQuery] int page = 1)
    {
        // Version 2.0 with pagination
        // URL: /api/v2/products
    }
}
```

#### 2. Query String Versioning

```csharp
[ApiVersion("1.0")]
[Route("api/[controller]")]
public class ProductsController : EntityChangeController<Product, ProductDto>
{
    // URL: /api/products?api-version=1.0
    // URL: /api/products?v=1.0
}
```

#### 3. Header Versioning

```csharp
[ApiVersion("1.0")]
[Route("api/[controller]")]
public class ProductsController : EntityChangeController<Product, ProductDto>
{
    // Client sends: X-Api-Version: 1.0
    // OR: Api-Version: 1.0
}
```

#### 4. Media Type Versioning

```csharp
[ApiVersion("1.0")]
[Route("api/[controller]")]
public class ProductsController : EntityChangeController<Product, ProductDto>
{
    // Client sends: Accept: application/json;version=1.0
}
```

### Deprecating Versions

```csharp
[ApiVersion("1.0", Deprecated = true)]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductsController : EntityChangeController<Product, ProductDto>
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
        // Current implementation
    }
}
```

### Version Discovery

Clients can discover supported versions via response headers:

```http
GET /api/v1/products HTTP/1.1

HTTP/1.1 200 OK
Api-Supported-Versions: 1.0, 2.0
Api-Deprecated-Versions: 1.0
```

### Custom Configuration

```csharp
builder.Services.AddControllerApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(2, 0);
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = false;
});
```

### Swagger/OpenAPI Integration

For comprehensive Swagger/OpenAPI documentation with API versioning, use the **Craft.OpenAPI** module which provides:

- Automatic versioned document generation
- Version-specific endpoint filtering
- UI customization for multiple versions
- Deep linking support

See [Craft.OpenAPI documentation](../Craft.OpenAPI/README.md) for details.

---

## 🚦 Rate Limiting

Protect your API from abuse with configurable rate limiting policies.

> **Important:** Due to .NET class library limitations, rate limiting must be configured directly in your `Program.cs`. Craft.Controllers provides guidance but the middleware must be added in the hosting application.

### Quick Start

```csharp
// Program.cs
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add rate limiting
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
});

builder.Services.AddControllers();

var app = builder.Build();

// IMPORTANT: Add rate limiter middleware BEFORE MapControllers
app.UseRateLimiter();
app.MapControllers();

app.Run();
```

### Recommended Policies

| Policy Name | Permit Limit | Window | Use Case |
|------------|--------------|--------|----------|
| `read-policy` | 100 requests | 1 minute | GET operations |
| `write-policy` | 30 requests | 1 minute | POST, PUT operations |
| `delete-policy` | 10 requests | 1 minute | DELETE operations |
| `bulk-policy` | 5 requests | 1 minute | Bulk operations |
| `authenticated-policy` | 200/50 requests | 1 minute | Auth/unauth users |
| `concurrent-policy` | 10 concurrent | N/A | Resource-intensive ops |

### Usage in Controllers

```csharp
using Microsoft.AspNetCore.RateLimiting;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductsController : EntityChangeController<Product, ProductDto>
{
    [HttpGet("{includeDetails:bool}")]
    [EnableRateLimiting("read-policy")]  // 100 requests/minute
    public override async Task<ActionResult<IAsyncEnumerable<Product>>> GetAllAsync(
        bool includeDetails, 
        CancellationToken cancellationToken = default)
    {
        return await base.GetAllAsync(includeDetails, cancellationToken);
    }
    
    [HttpPost]
    [EnableRateLimiting("write-policy")]  // 30 requests/minute
    public override async Task<ActionResult<Product>> AddAsync(
        ProductDto model, 
        CancellationToken cancellationToken = default)
    {
        return await base.AddAsync(model, cancellationToken);
    }
    
    [HttpDelete("{id}")]
    [EnableRateLimiting("delete-policy")]  // 10 requests/minute
    public override async Task<ActionResult> DeleteAsync(
        KeyType id, 
        CancellationToken cancellationToken = default)
    {
        return await base.DeleteAsync(id, cancellationToken);
    }
    
    [HttpPost("addrange")]
    [EnableRateLimiting("bulk-policy")]  // 5 requests/minute
    public override async Task<ActionResult<List<Product>>> AddRangeAsync(
        IEnumerable<ProductDto> models, 
        CancellationToken cancellationToken = default)
    {
        return await base.AddRangeAsync(models, cancellationToken);
    }
}
```

### Rate Limit Response

When rate limit is exceeded:

**Status Code:** `429 Too Many Requests`

**Headers:**
```
Retry-After: 45
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 0
```

**Body:**
```json
{
  "error": "Too many requests",
  "message": "Rate limit exceeded. Please retry after 45 seconds.",
  "retryAfter": 45
}
```

---

## 🏗️ Architecture

### Project Structure

```
Craft.Controllers/
├── Abstractions/
│   ├── IEntityController.cs              # Marker interface
│   ├── IEntityReadController.cs          # Read operations interface
│   └── IEntityChangeController.cs        # CRUD operations interface
├── Controllers/
│   ├── EntityReadController.cs           # Read-only base controller
│   └── EntityChangeController.cs         # Full CRUD base controller
├── ErrorHandling/
│   ├── IDatabaseErrorHandler.cs          # Error handler interface
│   ├── DatabaseErrorHandler.cs           # Main error handler
│   ├── Models/
│   │   └── ErrorContext.cs               # Error information model
│   └── Strategies/
│       ├── IDatabaseErrorStrategy.cs     # Strategy interface
│       ├── PostgreSqlErrorStrategy.cs    # PostgreSQL strategy
│       ├── SqlServerErrorStrategy.cs     # SQL Server strategy
│       └── GenericErrorStrategy.cs       # Fallback strategy
└── Extensions/
    ├── ApiVersioningExtensions.cs        # API versioning setup
    └── DatabaseErrorHandlingExtensions.cs # Error handling registration
```

### Design Patterns

#### Strategy Pattern (Error Handling)
- Encapsulates database-specific error handling
- Easy to extend with new database providers
- Priority-based strategy selection

#### Template Method Pattern (Controllers)
- Base controllers define operation structure
- Derived controllers customize behavior
- Virtual methods for extension points

#### Dependency Injection
- All services registered via DI
- Testable and maintainable
- Follows SOLID principles

---

## ✅ Best Practices

### Controller Design

1. **Inherit from base controllers** instead of duplicating code
2. **Override only what you need** to customize
3. **Use appropriate controller type**:
   - `EntityReadController` for read-only APIs
   - `EntityChangeController` for full CRUD

### Error Handling

1. **Always register error handling**:
   ```csharp
   builder.Services.AddDatabaseErrorHandling();
   ```

2. **Let the base controller handle errors** - don't catch exceptions unless you have custom logic

3. **Log technical details, return user-friendly messages**

### API Versioning

1. **Use URL segment versioning** for major versions:
   - `/api/v1/products`
   - `/api/v2/products`

2. **Deprecate gracefully**:
   ```csharp
   [ApiVersion("1.0", Deprecated = true)]
   ```

3. **Document breaking changes** in XML comments

### Rate Limiting

1. **Apply appropriate policies** based on operation cost:
   - Read: More permissive (100/min)
   - Write: Moderate (30/min)
   - Delete: Restrictive (10/min)
   - Bulk: Very restrictive (5/min)

2. **Provide meaningful error messages** with retry-after information

3. **Use authenticated policies** for known users (higher limits)

### Logging

1. **Enable debug logging** for troubleshooting:
   ```json
   {
     "Logging": {
       "LogLevel": {
         "Craft.Controllers": "Debug"
       }
     }
   }
   ```

2. **Review logs** for performance and security insights

---

## 🔧 Troubleshooting

### Issue: Rate limiting not working

**Symptoms:**
- Rate limits not being enforced
- No 429 responses

**Solutions:**
1. Check middleware order - `UseRateLimiter()` must come BEFORE `UseAuthorization()`
2. Verify policy names match exactly
3. Ensure rate limiter is configured in `Program.cs` (not in a class library)

### Issue: API versioning not detected

**Symptoms:**
- Version parameter not recognized
- 400 Bad Request for valid version

**Solutions:**
1. Add route template: `[Route("api/v{version:apiVersion}/[controller]")]`
2. Verify `AssumeDefaultVersionWhenUnspecified = true` for backward compatibility
3. Check that `AddControllerApiVersioning()` is called

### Issue: Database errors showing technical details

**Symptoms:**
- Raw SQL exception messages in responses
- Stack traces exposed to clients

**Solutions:**
1. Ensure `AddDatabaseErrorHandling()` is registered
2. Verify `IDatabaseErrorHandler` is injected in controller
3. Check that controller inherits from `EntityChangeController`

### Issue: DTO mapping fails

**Symptoms:**
- Null properties in mapped objects
- Type conversion errors

**Solutions:**
1. Verify DTO and Entity property names match
2. Configure Mapster mappings if needed
3. Check that DTO implements `IModel<TKey>`

### Issue: Soft delete not working

**Symptoms:**
- Entities are hard-deleted instead of soft-deleted
- Restore methods throw exceptions

**Solutions:**
1. Ensure entity implements `ISoftDelete` interface
2. Verify `IsDeleted` property is defined
3. Check that repository is `ChangeRepository` (not `ReadRepository`)

---

## 📚 Dependencies

### NuGet Packages

| Package | Version | Purpose |
|---------|---------|---------|
| Mapster | 10.0.0-pre01 | Object-to-object mapping |
| Humanizer.Core | 3.0.1 | String humanization |
| Asp.Versioning.Http | 8.1.1 | HTTP API versioning |
| Asp.Versioning.Mvc | 8.1.1 | MVC API versioning |
| Asp.Versioning.Mvc.ApiExplorer | 8.1.1 | API version exploration |

### Project References

| Project | Purpose |
|---------|---------|
| Craft.Core | Core utilities and extensions |
| Craft.Domain | Domain entities and interfaces |
| Craft.Extensions | Extension methods |
| Craft.Repositories | Repository pattern implementation |

### Framework References

- **Microsoft.AspNetCore.App** (.NET 10)

---

## 🔗 Related Libraries

- **[Craft.Repositories](../../2.%20Data%20Access/Craft.Repositories/README.md)** - Repository pattern implementation
- **[Craft.OpenAPI](../Craft.OpenAPI/README.md)** - Swagger/OpenAPI documentation
- **[Craft.Middleware](../Craft.Middleware/README.md)** - Custom middleware components
- **[Craft.Domain](../../1.%20Core/Craft.Domain/README.md)** - Domain entities and interfaces

---

## 📝 License

This project is part of the Craft framework by Sandeep SHARMA.

---

## 🤝 Contributing

For questions, issues, or contributions, please refer to the main Craft repository.

---

**Version:** 1.0.32  
**Last Updated:** January 2025  
**Target Framework:** .NET 10  
**Status:** ✅ Production Ready
