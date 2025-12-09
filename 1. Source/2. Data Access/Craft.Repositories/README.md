# Craft.Repositories

Craft.Repositories is a .NET 10 library that provides robust, extensible, and testable repository abstractions and implementations for data access in C# applications. It is designed to encapsulate data access logic, promote the repository pattern, and support both read-only and CRUD operations for Entity Framework Core and other data sources.

## Features
- **Repository Pattern**: Clean separation of data access logic from business logic.
- **Generic Abstractions**: Interfaces for base, read-only, and change (CRUD) repositories.
- **Async Operations**: All data access methods are asynchronous for scalability.
- **Entity Framework Core Support**: Works seamlessly with DbContext and DbSet.
- **Soft Delete Support**: Built-in support for soft deletion via ISoftDelete.
- **Batch Operations**: Add, update, and delete multiple entities efficiently.
- **Pagination**: Built-in support for paginated queries.
- **Logging**: Integrated with ILogger for diagnostics and debugging.
- **Extensible**: Easily extend or customize repository implementations.
- **.NET 10 & C# 13**: Utilizes the latest language and framework features.

## Getting Started

### Installation
Add a project reference to `Craft.Repositories` in your .NET 10 solution:

```
dotnet add reference ../Craft.Repositories/Craft.Repositories.csproj
```

### Usage Example
```csharp
using Craft.Repositories;

// Inject repository in your service or controller
public class ProductService
{
    private readonly IChangeRepository<Product, Guid> _repository;
    public ProductService(IChangeRepository<Product, Guid> repository) => _repository = repository;

    public async Task<Product?> GetProductAsync(Guid id) => await _repository.GetAsync(id);
    public async Task<Product> AddProductAsync(Product product) => await _repository.AddAsync(product);
}
```

## Key Components
- `IRepository`: Marker interface for all repositories.
- `IBaseRepository<T, TKey>`: Base repository abstraction for DbContext and DbSet access.
- `IReadRepository<T, TKey>`: Read-only repository abstraction (get, list, count, paginate).
- `IChangeRepository<T, TKey>`: CRUD repository abstraction (add, update, delete, batch, soft delete).
- `BaseRepository<T, TKey>`: Base implementation for repository logic.
- `ReadRepository<T, TKey>`: Implementation for read-only operations.
- `ChangeRepository<T, TKey>`: Implementation for full CRUD operations.

## Integration
Craft.Repositories is designed to be referenced by .NET projects that require clean, reusable, and testable data access logic, especially with Entity Framework Core or similar ORMs.

## License
This project is licensed under the MIT License. See the `LICENSE` file for details.

---
For more details, review the source code and XML documentation in the project.
