# Craft.Core

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-14.0-239120)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

**Craft.Core** is the foundational library for the Craft framework, providing essential abstractions, result types, pagination support, and dependency injection markers used across all Craft modules.

## 📦 Installation

```xml
<PackageReference Include="Craft.Core" Version="1.0.32.9" />
```

Or via .NET CLI:

```bash
dotnet add package Craft.Core
```

## 🎯 Features

- **Service Result Pattern** - Unified success/failure handling across all layers
- **Server Response Types** - Standardized API response formats
- **Pagination Support** - Built-in `PageInfo` and `PageResponse<T>` types
- **DI Lifetime Markers** - Convention-based dependency injection registration
- **Core Abstractions** - Interfaces for `IDbContext`, `ICurrentUser`, `ICurrentTenant`
- **Extension Methods** - Fluent conversions between result types and `IActionResult`

## 📁 Project Structure

```
Craft.Core/
├── Abstractions/
│   ├── ICurrentTenant.cs      # Multi-tenant context abstraction
│   ├── ICurrentUser.cs        # Current user context abstraction
│   ├── IDbContext.cs          # EF Core DbContext interface
│   ├── IService.cs            # Marker for DI services
│   └── IServiceResult.cs      # Service result contract
├── Common/
│   ├── Constants/
│   │   ├── ErrorTypes.cs      # Error type enumeration
│   │   └── HttpStatusCodes.cs # HTTP status code constants
│   ├── PageInfo.cs            # Pagination metadata
│   ├── PageResponse.cs        # Paginated response wrapper
│   ├── ServerResponse.cs      # API response type
│   └── ServiceResult.cs       # Service operation result
├── Converters/
│   ├── PageResponseJsonConverter.cs        # JSON converter for PageResponse
│   └── PageResponseJsonConverterFactory.cs # Factory for the converter
├── DependencyInjection/
│   ├── IScopedDependency.cs    # Scoped lifetime marker
│   ├── ISingletonDependency.cs # Singleton lifetime marker
│   └── ITransientDependency.cs # Transient lifetime marker
└── Extensions/
    ├── ActionResultExtensions.cs   # IServiceResult → IActionResult
    └── ServiceResultExtensions.cs  # Functional extensions for results
```

## 🚀 Quick Start

### Service Result Pattern

The `ServiceResult` and `ServiceResult<T>` types provide a consistent way to handle operation outcomes:

```csharp
public async Task<ServiceResult<User>> GetUserAsync(int id)
{
    var user = await _repository.FindAsync(id);
    
    if (user is null)
        return ServiceResult<User>.NotFound($"User {id} not found");
    
    return ServiceResult<User>.Success(user);
}

// Usage
var result = await GetUserAsync(42);

if (result.IsSuccess)
    Console.WriteLine($"Found: {result.Value.Name}");
else
    Console.WriteLine($"Error: {result.ErrorMessage}");
```

### Factory Methods

```csharp
// Success results
ServiceResult.Success();
ServiceResult<T>.Success(value);
ServiceResult<T>.Success(value, statusCode: 201);

// Failure results
ServiceResult.Failure("Validation failed");
ServiceResult.Failure(["Error 1", "Error 2"]);
ServiceResult<T>.Failure("Not found", ErrorType.NotFound, statusCode: 404);

// Common shortcuts
ServiceResult.NotFound("Resource not found");
ServiceResult.Unauthorized("Access denied");
ServiceResult<T>.FromException(exception);
```

### Pagination

```csharp
// Create a paginated response
var items = await _repository.GetPageAsync(page: 1, pageSize: 20);
var totalCount = await _repository.CountAsync();

var response = new PageResponse<Product>(
    items: items,
    totalCount: totalCount,
    currentPage: 1,
    pageSize: 20
);

// Access pagination info
Console.WriteLine($"Page {response.CurrentPage} of {response.TotalPages}");
Console.WriteLine($"Showing {response.From}-{response.To} of {response.TotalCount}");
Console.WriteLine($"Has next: {response.HasNextPage}, Has previous: {response.HasPreviousPage}");
```

### API Controller Integration

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await _userService.GetUserAsync(id);
        
        // Option 1: Simple conversion
        return result.ToActionResult();
        
        // Option 2: With ServerResponse wrapper
        return result.ToServerResponseResult();
        
        // Option 3: With ProblemDetails on failure
        return result.ToActionResultWithProblemDetails();
    }
}
```

### Dependency Injection Markers

Mark your services with lifetime interfaces for convention-based registration:

```csharp
// Transient: New instance per request
public class EmailBuilder : IEmailBuilder, ITransientDependency { }

// Scoped: One instance per HTTP request
public class OrderService : IOrderService, IScopedDependency { }

// Singleton: Shared across all requests (must be thread-safe!)
public class CacheService : ICacheService, ISingletonDependency { }
```

Register all marked services with a single call:

```csharp
// In Program.cs or Startup.cs
services.AddServicesWithLifetimeMarkers(typeof(Craft.Core.IService).Assembly);
```

### Current User & Tenant Abstractions

```csharp
public class OrderService
{
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentTenant _currentTenant;

    public OrderService(ICurrentUser currentUser, ICurrentTenant currentTenant)
    {
        _currentUser = currentUser;
        _currentTenant = currentTenant;
    }

    public async Task<ServiceResult<Order>> CreateOrderAsync(OrderDto dto)
    {
        if (!_currentUser.IsAuthenticated())
            return ServiceResult<Order>.Unauthorized();

        var order = new Order
        {
            UserId = _currentUser.GetId(),
            TenantId = _currentTenant.Id,
            // ...
        };
        
        // ...
    }
}
```

## 📋 Result Types Comparison

| Type | Layer | Purpose |
|------|-------|---------|
| `ServiceResult` | Domain/Application | Business operation results (no data) |
| `ServiceResult<T>` | Domain/Application | Business operation results (with data) |
| `ServerResponse` | API/Presentation | Client-facing API responses (no data) |
| `ServerResponse<T>` | API/Presentation | Client-facing API responses (with data) |

### Conversion Flow

```
ServiceResult<T>  →  ServerResponse<T>  →  IActionResult
     ↓                     ↓
   Domain              Controller           HTTP Response
```

## 🔧 Extension Methods

### ServiceResult Extensions

```csharp
// Get first error
string? error = result.FirstError();
string error = result.FirstErrorOrDefault("Default message");

// Combine errors from multiple results
var allErrors = results.CombineErrors();

// Check if all successful
bool allOk = results.AllSuccessful();

// Get most severe error type
ErrorType severity = results.GetMostSevereErrorType();

// Async mapping
var mapped = await resultTask.MapAsync(user => user.ToDto());

// Async binding (railway-oriented programming)
var final = await resultTask.BindAsync(async user => 
    await _emailService.SendWelcomeAsync(user));
```

### ActionResult Extensions

```csharp
// Basic conversion
return result.ToActionResult();

// With ServerResponse wrapper
return result.ToServerResponseResult();

// With ProblemDetails
return result.ToActionResultWithProblemDetails();

// Typed action result
return result.ToTypedActionResult<UserDto>();
```

## ⚙️ Error Types

```csharp
public enum ErrorType
{
    None = 0,
    Validation = 1,   // 400 Bad Request
    NotFound = 2,     // 404 Not Found
    Unauthorized = 3, // 401 Unauthorized
    Forbidden = 4,    // 403 Forbidden
    Conflict = 5,     // 409 Conflict
    Internal = 6,     // 500 Internal Server Error
    Timeout = 7       // 408/504 Timeout
}
```

## 🔗 Related Packages

| Package | Description |
|---------|-------------|
| `Craft.Domain` | Domain entities, base classes, and interfaces |
| `Craft.Data` | EF Core DbContext, repositories, and data access |
| `Craft.HttpServices` | HTTP client services with result pattern |
| `Craft.Security` | Authentication, authorization, and Identity |
| `Craft.MultiTenant` | Multi-tenancy support |

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👤 Author

**Sandeep SHARMA**

---

<p align="center">
  Made with ❤️ for the .NET community
</p>
