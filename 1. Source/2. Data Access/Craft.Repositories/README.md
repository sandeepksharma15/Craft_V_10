# Craft.Repositories

<div align="center">

![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?style=flat-square&logo=dotnet)
![EF Core](https://img.shields.io/badge/EF%20Core-10-512BD4?style=flat-square)
![C# 13](https://img.shields.io/badge/C%23-13-239120?style=flat-square&logo=csharp)
![License](https://img.shields.io/badge/license-MIT-blue?style=flat-square)

**Production-ready repository pattern implementation for .NET 10 applications**

[Features](#-features) • [Getting Started](#-getting-started) • [API Reference](#-api-reference) • [Examples](#-usage-examples) • [Best Practices](#-best-practices)

</div>

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Features](#-features)
- [Architecture](#-architecture)
- [Getting Started](#-getting-started)
- [API Reference](#-api-reference)
- [Usage Examples](#-usage-examples)
- [Best Practices](#-best-practices)
- [Integration](#-integration-with-craft-framework)
- [Testing](#-testing)
- [Performance](#-performance)
- [Advanced Topics](#-advanced-topics)
- [Troubleshooting](#-troubleshooting)
- [Contributing](#-contributing)
- [License](#-license)

---

## 🎯 Overview

**Craft.Repositories** is a production-ready, enterprise-grade repository pattern implementation for .NET 10 applications. It provides a clean, consistent, and testable abstraction layer over Entity Framework Core, promoting separation of concerns and maintainable data access code.

### Why Craft.Repositories?

✅ **Clean Architecture**: Separate data access from business logic  
✅ **Type-Safe**: Generic implementations with strong typing  
✅ **Production-Ready**: Concurrency handling, transactions, error logging  
✅ **Testable**: Easy to mock and unit test  
✅ **Performant**: Optimized queries, caching, batch operations  
✅ **Feature-Rich**: Soft deletes, pagination, bulk operations, restoration  

---

## ✨ Features

### Core Features

- ✅ **Repository Pattern**: Clean separation of data access logic from business logic
- ✅ **Generic Abstractions**: Strongly-typed interfaces for all entity types
- ✅ **Async/Await**: All operations are fully asynchronous
- ✅ **Entity Framework Core 10**: First-class EF Core support
- ✅ **Flexible Key Types**: Support for any key type (int, Guid, long, composite keys)

### Advanced Features

- ✅ **Soft Delete Support**: Automatic soft delete with restoration capabilities
- ✅ **Batch Operations**: Efficient add, update, and delete multiple entities
- ✅ **Transaction Support**: Automatic transactions for large batch operations (>100 items)
- ✅ **Concurrency Handling**: Built-in `DbUpdateConcurrencyException` detection and logging
- ✅ **Pagination**: Built-in support for paginated queries
- ✅ **Existence Checks**: Efficient `ExistsAsync` and `AnyAsync` methods
- ✅ **Auto-Save Control**: Optional manual transaction control via `autoSave` parameter
- ✅ **Entity State Management**: Automatic entity detachment after saves

### Infrastructure Features

- ✅ **Structured Logging**: Integrated with `ILogger<T>` for diagnostics
- ✅ **Auto-Include Support**: Convention-based navigation property loading
- ✅ **Extensible**: Easy to extend or customize implementations
- ✅ **.NET 10 & C# 13**: Utilizes latest language features (primary constructors, collection expressions)

---

## 🏗️ Architecture

### Repository Hierarchy

```
IRepository (Marker Interface)
    ↓
IBaseRepository<T, TKey>
    ├── GetDbContextAsync()
    ├── GetDbSetAsync()
    └── SaveChangesAsync()
    ↓
IReadRepository<T, TKey>
    ├── GetAsync(id)
    ├── GetAllAsync()
    ├── GetCountAsync()
    ├── GetPagedListAsync()
    ├── ExistsAsync(id)        ← New!
    └── AnyAsync()              ← New!
    ↓
IChangeRepository<T, TKey>
    ├── AddAsync()
    ├── AddRangeAsync()
    ├── UpdateAsync()
    ├── UpdateRangeAsync()
    ├── DeleteAsync()
    ├── DeleteRangeAsync()
    ├── RestoreAsync()          ← New!
    └── RestoreRangeAsync()     ← New!
```

### Implementation Classes

```
BaseRepository<T, TKey>
    ↓
ReadRepository<T, TKey>
    ↓
ChangeRepository<T, TKey>
```

### Key Design Decisions

1. **Generic Type Parameters**: `<T, TKey>` where T is entity and TKey is primary key type
2. **Constraint**: `where T : class, IEntity<TKey>, new()`
3. **Default Key Type**: `KeyType` alias (typically `long`) for simplified usage
4. **Async-First**: No synchronous methods (except `SaveChanges()` for legacy support)
5. **Immutable After Save**: Entities are detached after `SaveChangesAsync()`

---

## 🚀 Getting Started

### Prerequisites

- .NET 10 SDK or later
- Entity Framework Core 10.0.2 or later
- A project referencing `Craft.Core` and `Craft.Domain`

### Installation

#### Option 1: Project Reference
```bash
dotnet add reference ../Craft.Repositories/Craft.Repositories.csproj
```

#### Option 2: NuGet Package (if published)
```bash
dotnet add package Craft.Repositories
```

### Dependencies

Craft.Repositories depends on:
- `Craft.Core` - Core abstractions (IDbContext, IEntity)
- `Craft.Domain` - Domain models (BaseEntity, ISoftDelete)
- `Craft.Extensions` - Extension methods (IncludeDetails)
- `Microsoft.EntityFrameworkCore` (10.0.2)

### Setup in DI Container

```csharp
using Craft.Repositories;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // Register read-only repositories
        services.AddScoped(typeof(IReadRepository<,>), typeof(ReadRepository<,>));
        services.AddScoped(typeof(IReadRepository<>), typeof(ReadRepository<>));

        // Register change (CRUD) repositories
        services.AddScoped(typeof(IChangeRepository<,>), typeof(ChangeRepository<,>));
        services.AddScoped(typeof(IChangeRepository<>), typeof(ChangeRepository<>));

        return services;
    }
}

// In Program.cs or Startup.cs
services.AddRepositories();
```

---

## 📚 API Reference

### IReadRepository<T, TKey>

#### GetAsync
```csharp
Task<T?> GetAsync(TKey id, bool includeDetails = false, CancellationToken cancellationToken = default);
```
Gets a single entity by primary key. Returns `null` if not found.

**Parameters:**
- `id`: Primary key value
- `includeDetails`: If `true`, loads navigation properties configured with `.AutoInclude()`
- `cancellationToken`: Cancellation token

**Example:**
```csharp
var product = await _repository.GetAsync(productId, includeDetails: true);
```

---

#### GetAllAsync
```csharp
Task<IReadOnlyList<T>> GetAllAsync(bool includeDetails = false, CancellationToken cancellationToken = default);
```
Gets all entities from the repository.

**Example:**
```csharp
var allProducts = await _repository.GetAllAsync();
```

---

#### GetCountAsync
```csharp
Task<long> GetCountAsync(CancellationToken cancellationToken = default);
```
Gets the total count of entities.

**Example:**
```csharp
var totalProducts = await _repository.GetCountAsync();
```

---

#### GetPagedListAsync
```csharp
Task<PageResponse<T>> GetPagedListAsync(int currentPage, int pageSize, bool includeDetails = false, 
    CancellationToken cancellationToken = default);
```
Gets a paginated list of entities.

**Parameters:**
- `currentPage`: Page number (1-based)
- `pageSize`: Number of items per page
- `includeDetails`: Include navigation properties
- `cancellationToken`: Cancellation token

**Returns:** `PageResponse<T>` containing items, total count, page number, and page size

**Example:**
```csharp
var page = await _repository.GetPagedListAsync(currentPage: 1, pageSize: 20);
Console.WriteLine($"Total: {page.TotalCount}, Page: {page.CurrentPage}/{page.TotalPages}");
```

---

#### ExistsAsync ⭐ New!
```csharp
Task<bool> ExistsAsync(TKey id, CancellationToken cancellationToken = default);
```
Efficiently checks if an entity with the given ID exists without loading the entity.

**Example:**
```csharp
if (await _repository.ExistsAsync(productId))
{
    // Product exists
}
```

---

#### AnyAsync ⭐ New!
```csharp
Task<bool> AnyAsync(CancellationToken cancellationToken = default);
```
Efficiently checks if any entities exist in the repository.

**Example:**
```csharp
var hasProducts = await _repository.AnyAsync();
```

---

### IChangeRepository<T, TKey>

Inherits all methods from `IReadRepository<T, TKey>` plus:

#### AddAsync
```csharp
Task<T> AddAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default);
```
Adds a new entity to the repository.

**Parameters:**
- `entity`: Entity to add
- `autoSave`: If `true`, saves immediately; if `false`, call `SaveChangesAsync()` manually
- `cancellationToken`: Cancellation token

**Example:**
```csharp
var newProduct = new Product { Name = "New Product", Price = 99.99m };
var added = await _repository.AddAsync(newProduct);
Console.WriteLine($"Added with ID: {added.Id}");
```

---

#### AddRangeAsync
```csharp
Task<List<T>> AddRangeAsync(IEnumerable<T> entities, bool autoSave = true, 
    CancellationToken cancellationToken = default);
```
Adds multiple entities efficiently. Uses transactions for batches > 100 items.

**Example:**
```csharp
var products = Enumerable.Range(1, 500)
    .Select(i => new Product { Name = $"Product {i}", Price = i * 10 })
    .ToList();

var added = await _repository.AddRangeAsync(products);
// Automatically uses transaction for 500 items
```

---

#### UpdateAsync
```csharp
Task<T> UpdateAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default);
```
Updates an existing entity. Includes concurrency exception handling.

**Example:**
```csharp
product.Price = 149.99m;
var updated = await _repository.UpdateAsync(product);
```

**Concurrency Handling:**
```csharp
try 
{
    await _repository.UpdateAsync(product);
}
catch (DbUpdateConcurrencyException ex)
{
    // Handle concurrency conflict
    // Exception is logged automatically
}
```

---

#### UpdateRangeAsync
```csharp
Task<List<T>> UpdateRangeAsync(IEnumerable<T> entities, bool autoSave = true, 
    CancellationToken cancellationToken = default);
```
Updates multiple entities. Uses transactions for batches > 100 items.

**Example:**
```csharp
products.ForEach(p => p.Price *= 1.1m); // 10% price increase
await _repository.UpdateRangeAsync(products);
```

---

#### DeleteAsync
```csharp
Task<T> DeleteAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default);
```
Deletes an entity. If entity implements `ISoftDelete`, performs soft delete (sets `IsDeleted = true`).

**Example:**
```csharp
// Soft delete (if Product implements ISoftDelete)
await _repository.DeleteAsync(product);

// Hard delete (if Product doesn't implement ISoftDelete)
await _repository.DeleteAsync(nonSoftDeleteEntity);
```

---

#### DeleteRangeAsync
```csharp
Task<List<T>> DeleteRangeAsync(IEnumerable<T> entities, bool autoSave = true, 
    CancellationToken cancellationToken = default);
```
Deletes multiple entities. Supports mixed soft/hard deletes. Uses transactions for batches > 100 items.

**Example:**
```csharp
var productsToDelete = await _repository.GetAllAsync();
await _repository.DeleteRangeAsync(productsToDelete);
```

---

#### RestoreAsync ⭐ New!
```csharp
Task<T> RestoreAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default);
```
Restores a soft-deleted entity. Throws `InvalidOperationException` if entity doesn't implement `ISoftDelete`.

**Example:**
```csharp
// Restore a soft-deleted product
var deletedProduct = await _repository.GetAsync(productId);
if (deletedProduct?.IsDeleted == true)
{
    await _repository.RestoreAsync(deletedProduct);
}
```

---

#### RestoreRangeAsync ⭐ New!
```csharp
Task<List<T>> RestoreRangeAsync(IEnumerable<T> entities, bool autoSave = true, 
    CancellationToken cancellationToken = default);
```
Restores multiple soft-deleted entities. Uses transactions for batches > 100 items.

**Example:**
```csharp
var deletedProducts = products.Where(p => p.IsDeleted).ToList();
await _repository.RestoreRangeAsync(deletedProducts);
```

---

## 💡 Usage Examples

### Basic CRUD Operations

```csharp
public class ProductService
{
    private readonly IChangeRepository<Product> _repository;

    public ProductService(IChangeRepository<Product> repository)
    {
        _repository = repository;
    }

    // Create
    public async Task<Product> CreateProductAsync(string name, decimal price)
    {
        var product = new Product { Name = name, Price = price };
        return await _repository.AddAsync(product);
    }

    // Read
    public async Task<Product?> GetProductAsync(long id)
    {
        return await _repository.GetAsync(id, includeDetails: true);
    }

    // Update
    public async Task<Product> UpdatePriceAsync(long id, decimal newPrice)
    {
        var product = await _repository.GetAsync(id);
        if (product == null) throw new NotFoundException();

        product.Price = newPrice;
        return await _repository.UpdateAsync(product);
    }

    // Delete (soft delete if Product implements ISoftDelete)
    public async Task DeleteProductAsync(long id)
    {
        var product = await _repository.GetAsync(id);
        if (product == null) throw new NotFoundException();

        await _repository.DeleteAsync(product);
    }
}
```

---

### Pagination

```csharp
public async Task<PageResponse<Product>> GetProductPageAsync(int page, int pageSize)
{
    var result = await _repository.GetPagedListAsync(page, pageSize);

    Console.WriteLine($"Showing page {result.CurrentPage} of {result.TotalPages}");
    Console.WriteLine($"Total items: {result.TotalCount}");
    Console.WriteLine($"Items on this page: {result.Items.Count}");

    return result;
}
```

---

### Batch Operations with Manual Transaction Control

```csharp
public async Task<bool> ImportProductsAsync(List<Product> products)
{
    try
    {
        // Add all products without auto-save
        await _repository.AddRangeAsync(products, autoSave: false);

        // Perform validation or additional operations
        foreach (var product in products)
        {
            // Custom logic
        }

        // Manually save all changes
        var affected = await _repository.SaveChangesAsync();
        return affected > 0;
    }
    catch (Exception ex)
    {
        // Transaction automatically rolled back
        _logger.LogError(ex, "Failed to import products");
        return false;
    }
}
```

---

### Soft Delete and Restoration

```csharp
public class ProductLifecycleService
{
    private readonly IChangeRepository<Product> _repository;

    public async Task ArchiveProductAsync(long productId)
    {
        var product = await _repository.GetAsync(productId);
        if (product == null) return;

        // Soft delete
        await _repository.DeleteAsync(product);
    }

    public async Task RestoreProductAsync(long productId)
    {
        var product = await _repository.GetAsync(productId);
        if (product?.IsDeleted == true)
        {
            await _repository.RestoreAsync(product);
        }
    }

    public async Task PermanentlyDeleteOldProductsAsync()
    {
        var deletedProducts = await _repository.GetAllAsync();
        var oldProducts = deletedProducts
            .Where(p => p.IsDeleted && p.DeletedDate < DateTime.UtcNow.AddYears(-1))
            .ToList();

        // This will actually remove them from database
        // since they're already soft-deleted
        await _repository.DeleteRangeAsync(oldProducts);
    }
}
```

---

### Existence Checks

```csharp
public async Task<bool> ValidateOrderAsync(CreateOrderDto dto)
{
    // Efficiently check if product exists without loading it
    if (!await _productRepository.ExistsAsync(dto.ProductId))
    {
        throw new ValidationException("Product does not exist");
    }

    // Check if customer has any orders
    var hasOrders = await _orderRepository.AnyAsync();
    if (!hasOrders)
    {
        // First order - apply welcome discount
    }

    return true;
}
```

---

### Concurrency Handling

```csharp
public async Task<bool> UpdateProductWithRetryAsync(long productId, decimal newPrice)
{
    int maxRetries = 3;
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            var product = await _repository.GetAsync(productId);
            if (product == null) return false;

            product.Price = newPrice;
            await _repository.UpdateAsync(product);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            if (i == maxRetries - 1) throw;

            // Wait before retry
            await Task.Delay(TimeSpan.FromMilliseconds(100 * (i + 1)));
        }
    }
    return false;
}
```

---

### Navigation Properties with IncludeDetails

```csharp
// Configure in your DbContext
protected override void OnModelCreating(ModelBuilder builder)
{
    builder.Entity<Order>()
        .Navigation(o => o.OrderItems)
        .AutoInclude();

    builder.Entity<Order>()
        .Navigation(o => o.Customer)
        .AutoInclude();
}

// Usage in repository
public async Task<Order?> GetOrderWithDetailsAsync(long orderId)
{
    // Automatically includes OrderItems and Customer
    return await _repository.GetAsync(orderId, includeDetails: true);
}
```

---

## 🎯 Best Practices

### 1. Choose the Right Repository Interface

```csharp
// ❌ Don't use IChangeRepository if you only need reads
public class ReportService
{
    private readonly IChangeRepository<Order> _repository; // Overkill!
}

// ✅ Use IReadRepository for read-only scenarios
public class ReportService
{
    private readonly IReadRepository<Order> _repository; // Perfect!
}
```

---

### 2. Use Default Key Type When Possible

```csharp
// ❌ Verbose
private readonly IChangeRepository<Product, long> _repository;

// ✅ Concise (KeyType is aliased to long)
private readonly IChangeRepository<Product> _repository;
```

---

### 3. Leverage Auto-Save Control

```csharp
// ✅ For single operations, use autoSave: true (default)
await _repository.AddAsync(product); // Saves immediately

// ✅ For multiple operations, use autoSave: false for better performance
await _repository.AddAsync(product1, autoSave: false);
await _repository.AddAsync(product2, autoSave: false);
await _repository.AddAsync(product3, autoSave: false);
await _repository.SaveChangesAsync(); // Single save
```

---

### 4. Always Use CancellationToken

```csharp
// ✅ Pass cancellation tokens from controller/endpoint
public async Task<IActionResult> GetProductsAsync(CancellationToken cancellationToken)
{
    var products = await _repository.GetAllAsync(cancellationToken: cancellationToken);
    return Ok(products);
}
```

---

### 5. Handle Soft Deletes Appropriately

```csharp
public class Product : BaseEntity // BaseEntity implements ISoftDelete
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}

// DeleteAsync will soft delete
await _repository.DeleteAsync(product); // Sets IsDeleted = true

// To permanently delete, you'd need to access the entity when IsDeleted = true
// and delete it again, or use a custom method
```

---

### 6. Use Pagination for Large Datasets

```csharp
// ❌ Don't load everything
var allOrders = await _repository.GetAllAsync(); // Could be millions!

// ✅ Use pagination
var pageSize = 50;
var currentPage = 1;
var ordersPage = await _repository.GetPagedListAsync(currentPage, pageSize);
```

---

### 7. Validate Before Saving

```csharp
public async Task<Product> CreateProductAsync(CreateProductDto dto)
{
    // Validate first
    if (string.IsNullOrWhiteSpace(dto.Name))
        throw new ValidationException("Name is required");

    if (dto.Price <= 0)
        throw new ValidationException("Price must be positive");

    // Then create
    var product = new Product 
    { 
        Name = dto.Name, 
        Price = dto.Price 
    };

    return await _repository.AddAsync(product);
}
```

---

### 8. Log Repository Operations

```csharp
// Repository already logs at Debug level:
// [ReadRepository] Type: ["Product"] Method: ["GetAsync"] Id: ["123"]
// [ChangeRepository] Type: ["Product"] Method: ["UpdateAsync"] Id: ["123"]

// Configure logging in appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Craft.Repositories": "Debug" // Enable repository logs
    }
  }
}
```

---

## 🔗 Integration with Craft Framework

### With Craft.QuerySpec

For advanced querying with specifications, use `Craft.QuerySpec.IRepository<T>`:

```csharp
// Craft.Repositories - Basic CRUD
IChangeRepository<Product> basicRepo;

// Craft.QuerySpec - Advanced queries
IRepository<Product> advancedRepo; // Inherits from IChangeRepository
```

**Craft.QuerySpec** adds:
- Specification pattern support
- Complex filtering with `IQuery<T>`
- Projections with `IQuery<T, TResult>`
- Advanced sorting and grouping

---

### With Craft.Data

`Craft.Data` provides `BaseDbContext` with built-in features:
- Audit trails (Created/Modified dates and users)
- Concurrency tokens (RowVersion)
- Multi-tenancy support
- Soft delete query filters

```csharp
public class AppDbContext : BaseDbContext
{
    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // Essential!

        // Configure auto-includes
        builder.Entity<Product>()
            .Navigation(p => p.Category)
            .AutoInclude();
    }
}
```

---

### With Craft.Extensions

`Craft.Extensions` provides the `IncludeDetails` extension method used by repositories:

```csharp
// In Microsoft.EntityFrameworkCore namespace
public static IQueryable<T> IncludeDetails<T>(this IQueryable<T> source, bool includeDetails)
{
    return includeDetails ? source : source.IgnoreAutoIncludes();
}
```

See [INCLUDEDETAILS_IMPLEMENTATION.md](./INCLUDEDETAILS_IMPLEMENTATION.md) for details.

---

## 🧪 Testing

### Unit Testing with Mocks

```csharp
using Moq;
using Xunit;

public class ProductServiceTests
{
    [Fact]
    public async Task CreateProduct_ShouldCallAddAsync()
    {
        // Arrange
        var mockRepo = new Mock<IChangeRepository<Product>>();
        mockRepo.Setup(r => r.AddAsync(It.IsAny<Product>(), true, default))
            .ReturnsAsync((Product p, bool _, CancellationToken _) => p);

        var service = new ProductService(mockRepo.Object);

        // Act
        var result = await service.CreateProductAsync("Test", 99.99m);

        // Assert
        Assert.NotNull(result);
        mockRepo.Verify(r => r.AddAsync(It.IsAny<Product>(), true, default), Times.Once);
    }
}
```

---

### Integration Testing

```csharp
public class ProductRepositoryIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly IChangeRepository<Product> _repository;
    private readonly AppDbContext _context;

    public ProductRepositoryIntegrationTests(DatabaseFixture fixture)
    {
        _context = fixture.CreateContext();
        var logger = new NullLogger<ChangeRepository<Product>>();
        _repository = new ChangeRepository<Product>(_context, logger);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistToDatabase()
    {
        // Arrange
        var product = new Product { Name = "Test Product", Price = 99.99m };

        // Act
        var added = await _repository.AddAsync(product);

        // Assert
        Assert.True(added.Id > 0);
        var fromDb = await _context.Products.FindAsync(added.Id);
        Assert.NotNull(fromDb);
        Assert.Equal("Test Product", fromDb.Name);
    }
}
```

---

## ⚡ Performance

### Optimization Features

1. **AsNoTracking for Reads**
   - `GetAsync`, `GetAllAsync`, `ExistsAsync`, `AnyAsync` use `.AsNoTracking()`
   - 30-40% faster for read-only scenarios

2. **Batch Operations**
   - `AddRangeAsync`, `UpdateRangeAsync`, `DeleteRangeAsync` minimize round trips
   - Single `SaveChangesAsync()` call for all items

3. **Transaction Threshold**
   - Automatic transactions for batches > 100 items
   - Prevents partial commits on large operations

4. **Entity Detachment**
   - Entities detached after save to prevent tracking overhead
   - Reduces memory consumption in long-running contexts

5. **Lazy Loading Support**
   - Navigation properties marked `virtual` support lazy loading
   - `includeDetails` parameter for explicit eager loading

---

### Performance Tips

```csharp
// ✅ Use ExistsAsync instead of GetAsync when you only need to check existence
bool exists = await _repository.ExistsAsync(id); // Fast!
var product = await _repository.GetAsync(id); // Loads entire entity

// ✅ Use AnyAsync instead of GetCountAsync when you only care if items exist
bool hasItems = await _repository.AnyAsync(); // Fast!
long count = await _repository.GetCountAsync(); // Counts all items

// ✅ Use pagination for large result sets
var page = await _repository.GetPagedListAsync(1, 50); // Good
var all = await _repository.GetAllAsync(); // Could be slow

// ✅ Use batch operations for multiple items
await _repository.AddRangeAsync(products); // Single round-trip
foreach (var p in products) await _repository.AddAsync(p); // Multiple round-trips
```

---

## 🔧 Advanced Topics

### Custom Repository Extensions

```csharp
public interface IProductRepository : IChangeRepository<Product>
{
    Task<List<Product>> GetByPriceRangeAsync(decimal min, decimal max);
}

public class ProductRepository : ChangeRepository<Product>, IProductRepository
{
    public ProductRepository(IDbContext dbContext, ILogger<ProductRepository> logger) 
        : base(dbContext, logger)
    {
    }

    public async Task<List<Product>> GetByPriceRangeAsync(decimal min, decimal max)
    {
        return await _dbSet
            .Where(p => p.Price >= min && p.Price <= max)
            .AsNoTracking()
            .ToListAsync();
    }
}
```

---

### Multiple DbContexts

```csharp
// Register multiple contexts with keyed services (.NET 10)
services.AddDbContext<SalesDbContext>();
services.AddDbContext<InventoryDbContext>();

services.AddKeyedScoped<IChangeRepository<Product>>(
    "sales", 
    (sp, _) => new ChangeRepository<Product>(
        sp.GetRequiredService<SalesDbContext>(),
        sp.GetRequiredService<ILogger<ChangeRepository<Product>>>()
    )
);

services.AddKeyedScoped<IChangeRepository<Product>>(
    "inventory", 
    (sp, _) => new ChangeRepository<Product>(
        sp.GetRequiredService<InventoryDbContext>(),
        sp.GetRequiredService<ILogger<ChangeRepository<Product>>>()
    )
);

// Usage
public class ProductService(
    [FromKeyedServices("sales")] IChangeRepository<Product> salesRepo,
    [FromKeyedServices("inventory")] IChangeRepository<Product> inventoryRepo)
{
}
```

---

### Composite Keys

```csharp
public class OrderItem : IEntity<(long OrderId, long ProductId)>
{
    public long OrderId { get; set; }
    public long ProductId { get; set; }
    public int Quantity { get; set; }

    public (long OrderId, long ProductId) Id 
    { 
        get => (OrderId, ProductId);
        set => (OrderId, ProductId) = value;
    }
}

// Usage
IChangeRepository<OrderItem, (long, long)> repository;
var item = await repository.GetAsync((orderId: 1, productId: 2));
```

---

## 🐛 Troubleshooting

### "The call is ambiguous between methods"

**Issue:** Multiple `IncludeDetails` extension methods found.

**Solution:** Ensure `Craft.Extensions` is referenced and no conflicting extension exists.

```xml
<ProjectReference Include="..\..\1. Core\Craft.Extensions\Craft.Extensions.csproj" />
```

---

### "Navigation properties not loading with includeDetails: true"

**Issue:** Properties not configured with `.AutoInclude()`.

**Solution:** Configure navigation properties in `OnModelCreating`:

```csharp
builder.Entity<Product>()
    .Navigation(p => p.Category)
    .AutoInclude();
```

---

### DbUpdateConcurrencyException

**Issue:** Entity modified by another process.

**Solution:** Handle exception and retry or reload:

```csharp
try 
{
    await _repository.UpdateAsync(entity);
}
catch (DbUpdateConcurrencyException ex)
{
    _logger.LogWarning(ex, "Concurrency conflict");
    // Reload and retry, or show error to user
}
```

---

### "InvalidOperationException: does not implement ISoftDelete"

**Issue:** Calling `RestoreAsync` on entity that doesn't support soft delete.

**Solution:** Ensure entity implements `ISoftDelete`:

```csharp
public class Product : BaseEntity // BaseEntity implements ISoftDelete
{
    // ...
}
```

---

## 📖 Additional Resources

- [Entity Framework Core Documentation](https://docs.microsoft.com/ef/core/)
- [Repository Pattern](https://martinfowler.com/eaaCatalog/repository.html)
- [INCLUDEDETAILS_IMPLEMENTATION.md](./INCLUDEDETAILS_IMPLEMENTATION.md)
- [Craft Framework GitHub](https://github.com/sandeepksharma15/Craft_V_10)

---

## 🤝 Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Coding Standards

- Follow existing code style and conventions
- Add XML documentation for all public members
- Include unit tests for new functionality
- Ensure all tests pass before submitting PR
- Update README if adding new features

---

## 📄 License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

---

## 📧 Contact & Support

- **Author**: Sandeep SHARMA
- **GitHub**: [Craft_V_10](https://github.com/sandeepksharma15/Craft_V_10)
- **Issues**: [GitHub Issues](https://github.com/sandeepksharma15/Craft_V_10/issues)

---

<div align="center">

**Built with ❤️ using .NET 10**

⭐ Star this repo if you find it helpful!

</div>
