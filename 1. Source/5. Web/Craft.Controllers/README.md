# Craft.Controllers

Craft.Controllers is a .NET 10 library providing generic, reusable, and extensible REST API controllers for CRUD operations in ASP.NET Core applications. It is designed to accelerate API development by abstracting common patterns for entity management, leveraging repository and model mapping patterns.

## Features
- **Generic Controllers**: Easily create RESTful endpoints for any entity by inheriting from base controllers.
- **Read & Change APIs**: Includes both read-only and full CRUD controller abstractions.
- **Repository Pattern**: Integrates with repository interfaces for data access abstraction.
- **Model Mapping**: Supports mapping between domain entities and data transfer models (DTOs) using Mapster.
- **Error Handling & Logging**: Built-in error handling and structured logging using ILogger.
- **Async & Paginated Operations**: All operations are asynchronous and support pagination.
- **Rate Limiting**: Configurable request throttling with multiple pre-defined policies.
- **API Versioning**: Full support for multiple API versions (URL, query string, and header versioning).
- **Enhanced Swagger/OpenAPI**: Comprehensive API documentation with JWT auth, XML comments, and examples.
- **.NET 10 & C# 13**: Utilizes the latest .NET and C# features for performance and maintainability.

## New in Version 1.1.0 ??

### ?? Rate Limiting
Protect your API from abuse with configurable rate limiting policies:
- Read operations: 100 requests/minute
- Write operations: 30 requests/minute
- Delete operations: 10 requests/minute
- Bulk operations: 5 requests/minute

```csharp
builder.Services.AddControllerRateLimiting();
app.UseRateLimiter();
```

### ?? API Versioning
Support multiple API versions simultaneously with flexible versioning strategies:
```csharp
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class UsersController : EntityReadController<User, UserDto> { }
```

### ?? Enhanced Swagger/OpenAPI
Comprehensive API documentation with:
- JWT Bearer authentication support
- XML documentation comments
- Automatic response code documentation
- Rate limiting information
- Authorization requirements

```csharp
builder.Services.AddEnhancedSwagger();
app.UseSwagger();
app.UseEnhancedSwaggerUI();
```

**?? See [FEATURES.md](FEATURES.md) for detailed documentation and examples.**

## Getting Started

### Installation
Add a project reference to `Craft.Controllers` in your ASP.NET Core project:

```
dotnet add reference ../Craft.Controllers/Craft.Controllers.csproj
```

### Usage Example
1. **Implement Repository Interfaces** for your entity (e.g., `IReadRepository`, `IChangeRepository`).
2. **Create a Controller** by inheriting from `EntityChangeController` or `EntityReadController`:

```csharp
public class ProductController : EntityChangeController<Product, ProductDto, long>
{
    public ProductController(IChangeRepository<Product, long> repo, ILogger<ProductController> logger)
        : base(repo, logger) { }
}
```

3. **Register Controllers** in your ASP.NET Core application's DI container.

## Key Components
- `EntityReadController<T, DataTransferT, TKey>`: Base controller for read operations.
- `EntityChangeController<T, DataTransferT, TKey>`: Base controller for full CRUD operations.
- `IEntityReadController`, `IEntityChangeController`: Abstractions for controller contracts.
- **Dependencies**: Mapster, Microsoft.AspNetCore.App, Craft.Core, Craft.Domain, Craft.Extensions, Craft.Repositories.

## License
See the `LICENSE` file in this directory for details.

---
For more details, review the source code and XML documentation in the project.
