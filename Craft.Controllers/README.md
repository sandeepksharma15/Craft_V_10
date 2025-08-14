# Craft.Controllers

Craft.Controllers is a .NET 10 library providing generic, reusable, and extensible REST API controllers for CRUD operations in ASP.NET Core applications. It is designed to accelerate API development by abstracting common patterns for entity management, leveraging repository and model mapping patterns.

## Features
- **Generic Controllers**: Easily create RESTful endpoints for any entity by inheriting from base controllers.
- **Read & Change APIs**: Includes both read-only and full CRUD controller abstractions.
- **Repository Pattern**: Integrates with repository interfaces for data access abstraction.
- **Model Mapping**: Supports mapping between domain entities and data transfer models (DTOs) using Mapster.
- **Error Handling & Logging**: Built-in error handling and structured logging using ILogger.
- **Async & Paginated Operations**: All operations are asynchronous and support pagination.
- **.NET 10 & C# 13**: Utilizes the latest .NET and C# features for performance and maintainability.

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
