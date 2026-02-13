# Craft.QuerySpec

A powerful, type-safe query specification library for .NET with full HTTP serialization support. Build complex queries on the client, serialize them over HTTP, and execute them on the server with Entity Framework Core.

[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?style=flat-square)](https://dotnet.microsoft.com/)
[![Tests](https://img.shields.io/badge/tests-991%20passing-brightgreen?style=flat-square)]()
[![License](https://img.shields.io/badge/license-MIT-blue?style=flat-square)]()

## 🎯 Features

- ✅ **Type-Safe Queries** - Strongly-typed LINQ expressions
- ✅ **HTTP Serialization** - Full JSON serialization/deserialization support
- ✅ **Dynamic Filtering** - Runtime filter construction with multiple comparison operators
- ✅ **Sorting & Pagination** - Multi-field sorting with skip/take pagination
- ✅ **SQL-Like Search** - Contains, StartsWith, EndsWith with case-insensitive matching
- ✅ **Navigation Includes** - Manual and automatic navigation property loading
- ✅ **Result Projection** - Select specific fields to reduce payload size
- ✅ **Query Validation** - Built-in validation with configurable limits
- ✅ **Performance Monitoring** - Query metrics and slow query detection
- ✅ **EF Core Integration** - Seamless integration with Entity Framework Core
- ✅ **Repository Pattern** - Advanced repository implementation with QuerySpec support
- ✅ **HTTP Services** - Ready-to-use HTTP client and controller implementations

## 📦 Installation

```bash
dotnet add package Craft.QuerySpec
```

## 🚀 Quick Start

### Basic Setup

```csharp
// In Program.cs or Startup.cs
services.AddQuerySpec(configuration); // Register QuerySpec services

builder.Services
    .AddControllers()
    .AddQuerySpecJsonOptions(); // Enable JSON serialization for Query<T>
```

### Simple Query Example

```csharp
// Create a query
var query = new Query<Product>()
    .Where(p => p.Category == "Electronics")
    .Where(p => p.Price < 1000)
    .OrderBy(p => p.Price)
    .SetPage(1, 20);

// Execute the query
var products = await repository.GetPagedListAsync(query);
```

### Dynamic Filtering

```csharp
// Build filters dynamically at runtime
var query = new Query<Product>();

if (!string.IsNullOrEmpty(category))
    query.Where("Category", category, ComparisonType.EqualTo);

if (maxPrice.HasValue)
    query.Where("Price", maxPrice.Value, ComparisonType.LessThanOrEqualTo);

var results = await repository.GetAllAsync(query);
```

### Result Projection

```csharp
// Select only the fields you need
var query = new Query<Product, ProductDto>()
    .Where(p => p.IsActive)
    .Select(p => new ProductDto
    {
        Id = p.Id,
        Name = p.Name,
        Price = p.Price
    })
    .OrderBy(p => p.Name)
    .SetPage(1, 50);

var dtos = await repository.GetPagedListAsync(query);
```

### HTTP Serialization

**Client Side (Blazor WebAssembly)**:
```csharp
// Create query on the client
var query = new Query<Product>()
    .Where(p => p.Category == "Electronics")
    .OrderBy(p => p.Price);

// Send query over HTTP (automatically serialized to JSON)
var response = await httpService.GetAllAsync(query);

if (response.IsSuccess)
{
    var products = response.Data;
}
```

**Server Side (Web API)**:
```csharp
[HttpPost("search")]
public async Task<ActionResult<List<Product>>> Search(
    [FromBody] IQuery<Product> query,
    CancellationToken cancellationToken)
{
    // Query is automatically deserialized from JSON
    return await repository.GetAllAsync(query, cancellationToken);
}
```

## 📚 Advanced Usage

### Navigation Property Includes

```csharp
// Manually specify includes
var query = new Query<Order>()
    .Include(o => o.Customer)
    .Include(o => o.OrderItems)
    .ThenInclude<OrderItem>(oi => oi.Product);

// Or use automatic includes (1 level deep)
var query = new Query<Order>()
{
    AutoIncludeNavigationProperties = true
};
```

### Search Functionality

```csharp
// SQL-like search across multiple fields
var query = new Query<Product>()
    .Search(p => p.Name, "laptop")
    .Search(p => p.Description, "gaming")
    .OrderBy(p => p.Name);
```

### Complex Filtering

```csharp
// Combine multiple filters
var query = new Query<Employee>()
    .Where(e => e.IsActive)
    .Where(e => e.Department == "IT")
    .Where(e => e.Salary >= 50000 && e.Salary <= 100000)
    .Where(e => e.Skills.Any(s => s.Name == "C#"))
    .OrderByDescending(e => e.HireDate)
    .SetPage(1, 25);
```

### Query Options Configuration

```csharp
// In appsettings.json
{
  "QuerySpec": {
    "Options": {
      "CommandTimeoutSeconds": 30,
      "MaxResultSize": 10000,
      "MaxFilterCount": 50,
      "MaxIncludeCount": 10,
      "MaxPageSize": 1000,
      "MaxOrderByFields": 5,
      "EnableQueryMetrics": true,
      "SlowQueryThresholdMs": 5000
    }
  }
}

// Or configure programmatically
services.AddQuerySpec(options =>
{
    options.MaxPageSize = 500;
    options.EnableQueryMetrics = true;
    options.SlowQueryThresholdMs = 3000;
});
```

### Query Validation

```csharp
// Inject validator in your controller
public class ProductController : EntityController<Product, ProductDto, long>
{
    private readonly IQueryValidator<Product> _validator;
    
    public ProductController(
        IRepository<Product, long> repository,
        IQueryValidator<Product> validator,
        ILogger<ProductController> logger)
        : base(repository, logger)
    {
        _validator = validator;
    }
    
    [HttpPost("search")]
    public async Task<ActionResult<List<Product>>> Search(
        [FromBody] IQuery<Product> query)
    {
        // Validate query before execution
        var validation = await _validator.ValidateAsync(query);
        
        if (!validation.IsValid)
            return BadRequest(validation.Errors);
        
        return await repository.GetAllAsync(query);
    }
}
```

## 🏗️ Architecture

### Core Components

#### Query<T>
The main query specification class that encapsulates all query logic:
- Filtering (Where clauses)
- Sorting (OrderBy, OrderByDescending)
- Pagination (Skip, Take)
- Includes (Navigation properties)
- Query options (AsNoTracking, AsSplitQuery, etc.)

#### Query<T, TResult>
Extension of Query<T> with result projection support for selecting specific fields.

#### EntityFilterBuilder<T>
Fluent builder for constructing filter expressions dynamically at runtime.

#### SortOrderBuilder<T>
Builder for creating multi-field sorting specifications.

#### QueryEvaluator
Chain of responsibility pattern implementation that applies query specifications to IQueryable<T>.

### Evaluators

The library uses pluggable evaluators to apply different aspects of the query:

- **WhereEvaluator** - Applies filter conditions
- **OrderEvaluator** - Applies sorting
- **SearchEvaluator** - Applies SQL-like search
- **PaginationEvaluator** - Applies skip/take
- **IncludeEvaluator** - Applies navigation property includes
- **AsNoTrackingEvaluator** - Applies change tracking options
- **AsSplitQueryEvaluator** - Applies split query options

### JSON Serialization

Custom JSON converters enable full serialization of:
- LINQ expressions
- Filter criteria
- Sort descriptors
- Include expressions
- All query options

## 🔒 Security Features

### Built-in Validation
- Maximum filter count limits
- Maximum include depth limits
- Maximum page size enforcement
- Property access validation
- Query complexity checks

### Best Practices
```csharp
// ✅ Good: Use validation
var validation = await validator.ValidateAsync(query);
if (!validation.IsValid)
    return BadRequest(validation.Errors);

// ✅ Good: Configure reasonable limits
options.MaxFilterCount = 50;
options.MaxPageSize = 1000;

// ✅ Good: Enable metrics for monitoring
options.EnableQueryMetrics = true;
options.SlowQueryThresholdMs = 5000;
```

## 📊 Performance

### Optimization Features

- **AsNoTracking by default** - Read-only queries don't track changes
- **Expression caching** - Common expressions are cached
- **Lazy evaluation** - Queries execute only when enumerated
- **Compiled queries** - Frequently used queries can be compiled
- **Projection support** - Select only needed fields

### Performance Tips

```csharp
// ✅ Use projection to reduce payload
var query = new Query<Order, OrderSummary>()
    .Select(o => new OrderSummary { Id = o.Id, Total = o.Total });

// ✅ Use AsNoTracking for read-only queries (default)
query.AsNoTracking = true;

// ✅ Use pagination for large result sets
query.SetPage(1, 50);

// ✅ Use AsSplitQuery for multiple includes
query.AsSplitQuery = true;
query.Include(o => o.OrderItems)
     .Include(o => o.Customer);
```

## 🧪 Testing

The library includes 991 comprehensive tests covering:
- All builders and components
- JSON serialization round-trips
- Expression building
- Query evaluation
- Repository operations
- Validation logic

Run tests:
```bash
dotnet test Craft.QuerySpec.Tests
```

## 📖 API Reference

### IQuery<T> Interface

```csharp
public interface IQuery<T> where T : class
{
    bool AsNoTracking { get; set; }
    bool AsSplitQuery { get; set; }
    bool IgnoreAutoIncludes { get; set; }
    bool IgnoreQueryFilters { get; set; }
    bool AutoIncludeNavigationProperties { get; set; }
    int? Skip { get; set; }
    int? Take { get; set; }
    SortOrderBuilder<T>? SortOrderBuilder { get; set; }
    SqlLikeSearchCriteriaBuilder<T>? SqlLikeSearchCriteriaBuilder { get; set; }
    EntityFilterBuilder<T>? EntityFilterBuilder { get; set; }
    List<IncludeExpression>? IncludeExpressions { get; set; }
    Func<IEnumerable<T>, IEnumerable<T>>? PostProcessingAction { get; set; }
    
    bool IsSatisfiedBy(T entity);
    void SetPage(int page, int pageSize);
    void Clear();
}
```

### IRepository<T, TKey> Interface

```csharp
public interface IRepository<T, TKey> where T : class, IEntity<TKey>
{
    Task<T?> GetAsync(IQuery<T> query, CancellationToken cancellationToken = default);
    Task<TResult?> GetAsync<TResult>(IQuery<T, TResult> query, CancellationToken cancellationToken = default);
    Task DeleteAsync(IQuery<T> query, bool autoSave = true, CancellationToken cancellationToken = default);
    Task<long> GetCountAsync(IQuery<T> query, CancellationToken cancellationToken = default);
    Task<List<T>> GetAllAsync(IQuery<T> query, CancellationToken cancellationToken = default);
    Task<List<TResult>> GetAllAsync<TResult>(IQuery<T, TResult> query, CancellationToken cancellationToken = default);
    Task<PageResponse<T>> GetPagedListAsync(IQuery<T> query, CancellationToken cancellationToken = default);
    Task<PageResponse<TResult>> GetPagedListAsync<TResult>(IQuery<T, TResult> query, CancellationToken cancellationToken = default);
}
```

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is licensed under the MIT License.

## 🔗 Related Projects

- [Craft Framework](https://github.com/sandeepksharma15/Craft_V_10) - Complete framework ecosystem
- [Entity Framework Core](https://github.com/dotnet/efcore) - ORM integration

## 📞 Support

For issues, questions, or contributions, please visit the [GitHub repository](https://github.com/sandeepksharma15/Craft_V_10).

---

**Built with ❤️ using .NET 10**
