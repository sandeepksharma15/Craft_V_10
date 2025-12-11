# Craft.QuerySpec

Craft.QuerySpec is a .NET 10 library that extends Craft.Repositories with advanced query specification capabilities for building complex, reusable, and testable queries in C# applications. It provides a fluent API for filtering, sorting, pagination, projection, and more.

## Features

- **Query Specification Pattern**: Build reusable query specifications separate from repository logic
- **Fluent API**: Intuitive, chainable methods for building complex queries
- **Advanced Filtering**: Support for multiple filter criteria with various comparison operators
- **Projection Support**: Map entities to DTOs with type-safe projections
- **Pagination**: Built-in support for efficient paging with total count
- **Ordering**: Multi-level sorting with ascending/descending support
- **Search**: Full-text search capabilities across multiple properties
- **Include Details**: Eager loading of related entities
- **Soft Delete Support**: Automatic handling of soft-deleted entities
- **Evaluator Pipeline**: Extensible query evaluation pipeline
- **Performance Optimized**: Single-query execution, no N+1 problems
- **.NET 10 & C# 14**: Utilizes the latest language and framework features

## Installation

Add a project reference to `Craft.QuerySpec` in your .NET 10 solution:

```bash
dotnet add reference ../Craft.QuerySpec/Craft.QuerySpec.csproj
```

## Getting Started

### Basic Usage

```csharp
using Craft.QuerySpec;

// Inject repository in your service
public class ProductService
{
    private readonly IRepository<Product, Guid> _repository;
    
    public ProductService(IRepository<Product, Guid> repository) 
        => _repository = repository;

    // Simple query
    public async Task<Product?> GetProductByNameAsync(string name)
    {
        var query = new Query<Product>();
        query.Where(p => p.Name == name);
        
        return await _repository.GetAsync(query);
    }
    
    // Query with multiple filters
    public async Task<List<Product>> GetActiveProductsAsync()
    {
        var query = new Query<Product>();
        query.Where(p => p.IsActive)
             .Where(p => p.Stock > 0)
             .OrderBy(p => p.Name);
        
        return await _repository.GetAllAsync(query);
    }
}
```

### Pagination

```csharp
public async Task<PageResponse<Product>> GetProductsPageAsync(int page, int pageSize)
{
    var query = new Query<Product>
    {
        Skip = (page - 1) * pageSize,
        Take = pageSize
    };
    
    query.Where(p => p.IsActive)
         .OrderBy(p => p.CreatedDate, OrderDirection.Descending);
    
    return await _repository.GetPagedListAsync(query);
}
```

### Projection to DTO

```csharp
public class ProductDto
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}

public async Task<List<ProductDto>> GetProductDtosAsync()
{
    var query = new Query<Product, ProductDto>();
    
    query.Where(p => p.IsActive)
         .Select(p => p.Name, dto => dto.Name)
         .Select(p => p.Price, dto => dto.Price);
    
    return await _repository.GetAllAsync(query);
}
```

### Advanced Filtering

```csharp
// Comparison operators
var query = new Query<Product>();
query.Where(p => p.Price, 100m, ComparisonType.GreaterThan)
     .Where(p => p.Price, 1000m, ComparisonType.LessThanOrEqual);

// String property by name
query.Where("Category", "Electronics", ComparisonType.EqualTo);

// Complex expressions
query.Where(p => p.Name.Contains("Phone") && p.Stock > 0);
```

### Search

```csharp
var query = new Query<Product>
{
    SearchTerm = "iPhone",
    SearchPropertyNames = new[] { "Name", "Description" }
};

var results = await _repository.GetAllAsync(query);
```

### Ordering

```csharp
var query = new Query<Product>();

// Single order
query.OrderBy(p => p.Price);

// Multiple orders
query.OrderBy(p => p.Category)
     .ThenOrderBy(p => p.Name);

// Descending order
query.OrderBy(p => p.CreatedDate, OrderDirection.Descending);
```

### Count

```csharp
var query = new Query<Product>();
query.Where(p => p.IsActive);

var count = await _repository.GetCountAsync(query);
```

### Batch Delete

```csharp
// Delete all inactive products
var query = new Query<Product>();
query.Where(p => !p.IsActive);

await _repository.DeleteAsync(query);
```

## Key Components

### Interfaces

- **`IRepository<T, TKey>`**: Repository interface with QuerySpec support
- **`IQuery<T>`**: Query specification for entity retrieval
- **`IQuery<T, TResult>`**: Query specification with projection support
- **`IEvaluator`**: Interface for query evaluators
- **`ISelectEvaluator`**: Interface for selection/projection evaluators

### Implementations

- **`Repository<T, TKey>`**: Repository implementation with QuerySpec support
- **`Query<T>`**: Concrete query specification implementation
- **`Query<T, TResult>`**: Concrete query specification with projection
- **`QueryEvaluator`**: Main evaluator that orchestrates the evaluation pipeline

### Evaluators

The evaluation pipeline processes queries in this order:

1. **WhereEvaluator**: Applies filtering criteria
2. **OrderEvaluator**: Applies sorting
3. **SearchEvaluator**: Applies search terms
4. **PaginationEvaluator**: Applies Skip/Take for pagination
5. **AsNoTrackingEvaluator**: Optimizes read-only queries
6. **AsSplitQueryEvaluator**: Optimizes queries with multiple includes
7. **IgnoreAutoIncludeEvaluator**: Controls automatic navigation property loading
8. **IgnoreQueryFiltersEvaluator**: Bypasses global query filters

### Extension Methods

- **`WithQuery<T>(IQuery<T>)`**: Applies query specification to IQueryable
- **`ToListSafeAsync<T>()`**: Safe async list conversion
- **`LongCountSafeAsync<T>()`**: Safe async count
- **`Where<T>(Expression<Func<T, bool>>)`**: Adds filter criteria
- **`OrderBy<T>(Expression<Func<T, object>>)`**: Adds ordering
- **`Select<T, TResult>(Expression)`**: Adds projection

## Query Options

### IQuery<T> Properties

```csharp
public class Query<T>
{
    // Pagination
    public int? Skip { get; set; }
    public int? Take { get; set; }
    
    // Search
    public string? SearchTerm { get; set; }
    public IEnumerable<string>? SearchPropertyNames { get; set; }
    
    // Options
    public bool AsNoTracking { get; set; }
    public bool AsSplitQuery { get; set; }
    public bool IgnoreAutoIncludes { get; set; }
    public bool IgnoreQueryFilters { get; set; }
    
    // Builders
    public EntityFilterBuilder<T> EntityFilterBuilder { get; }
    public EntityOrderBuilder<T> EntityOrderBuilder { get; }
    
    // Post-processing
    public Func<IEnumerable<T>, IEnumerable<T>>? PostProcessingAction { get; set; }
}
```

## Best Practices

### 1. Reusable Query Specifications

Create specification classes for common queries:

```csharp
public class ActiveProductsSpec : Query<Product>
{
    public ActiveProductsSpec()
    {
        this.Where(p => p.IsActive);
        AsNoTracking = true;
    }
}

// Usage
var products = await _repository.GetAllAsync(new ActiveProductsSpec());
```

### 2. Pagination Requirements

Always set both `Skip` and `Take` for paginated queries:

```csharp
// ? Correct
var query = new Query<Product>
{
    Skip = 0,
    Take = 20
};

// ? Throws ArgumentOutOfRangeException
var badQuery = new Query<Product>
{
    Take = 20  // Skip is null
};
```

### 3. Read-Only Queries

Use `AsNoTracking` for read-only operations:

```csharp
var query = new Query<Product>
{
    AsNoTracking = true
};
query.Where(p => p.IsActive);

var products = await _repository.GetAllAsync(query);
```

### 4. Projections for Performance

Use projections to retrieve only needed data:

```csharp
// ? Better - only retrieves needed columns
var query = new Query<Product, ProductListDto>();
query.Select(p => p.Id)
     .Select(p => p.Name)
     .Select(p => p.Price);

// ? Retrieves entire entity
var allData = await _repository.GetAllAsync(new Query<Product>());
```

### 5. Soft Delete Handling

Soft-deletable entities are handled automatically:

```csharp
// Hard delete for non-ISoftDelete entities
// Soft delete (sets IsDeleted = true) for ISoftDelete entities
await _repository.DeleteAsync(query);
```

## Performance Considerations

### Single Query Execution

Queries are optimized to execute as a single database call:

```csharp
// This executes ONE database query
var page = await _repository.GetPagedListAsync(query);
// Items are retrieved with applied filters/sorting/paging
// Total count uses only WHERE clause (no pagination)
```

### Avoid N+1 Problems

Use split queries for complex includes:

```csharp
var query = new Query<Order>
{
    AsSplitQuery = true  // Prevents N+1 for multiple collections
};
```

### Efficient Counting

`GetPagedListAsync` optimizes count queries:

```csharp
// Count query: SELECT COUNT(*) WHERE [filters]
// Data query: SELECT * WHERE [filters] ORDER BY [order] SKIP X TAKE Y
var page = await _repository.GetPagedListAsync(query);
```

## Error Handling

### Multiple Matches

`GetAsync` throws if multiple entities match:

```csharp
try
{
    var product = await _repository.GetAsync(query);
}
catch (InvalidOperationException ex)
{
    // "Sequence contains more than one matching element."
}
```

### Pagination Validation

Pagination parameters are validated:

```csharp
// Throws ArgumentOutOfRangeException
var query = new Query<Product> 
{ 
    Skip = -1,  // Must be >= 0
    Take = 0    // Must be > 0
};
```

## Testing

### Unit Testing with In-Memory Data

```csharp
[Fact]
public async Task GetAsync_WithValidQuery_ReturnsEntity()
{
    // Arrange
    var options = new DbContextOptionsBuilder<TestDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;
    
    await using var context = new TestDbContext(options);
    var repository = new Repository<Product, Guid>(context, logger);
    
    var product = new Product { Name = "Test" };
    context.Products.Add(product);
    await context.SaveChangesAsync();
    
    // Act
    var query = new Query<Product>();
    query.Where(p => p.Name == "Test");
    var result = await repository.GetAsync(query);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal("Test", result.Name);
}
```

### Integration Testing

```csharp
[Collection(nameof(SystemTestCollectionDefinition))]
public class RepositoryIntegrationTests
{
    private readonly IRepository<Product, Guid> _repository;
    
    public RepositoryIntegrationTests(TestFixture fixture)
    {
        _repository = fixture.GetService<IRepository<Product, Guid>>();
    }
    
    [Fact]
    public async Task GetPagedListAsync_ReturnsCorrectPage()
    {
        // Arrange
        var query = new Query<Product>
        {
            Skip = 10,
            Take = 10
        };
        
        // Act
        var page = await _repository.GetPagedListAsync(query);
        
        // Assert
        Assert.Equal(10, page.Items.Count);
        Assert.Equal(2, page.CurrentPage);
    }
}
```

## Advanced Scenarios

### Custom Evaluator

Create custom evaluators for specialized logic:

```csharp
public class TenantEvaluator : IEvaluator
{
    private readonly string _tenantId;
    
    public TenantEvaluator(string tenantId) => _tenantId = tenantId;
    
    public IQueryable<T>? GetQuery<T>(IQueryable<T> queryable, IQuery<T> query) 
        where T : class
    {
        if (typeof(T).GetInterface(nameof(ITenant)) != null)
            return queryable.Where(x => ((ITenant)x).TenantId == _tenantId);
        
        return queryable;
    }
}

// Usage
var evaluator = new QueryEvaluator(new IEvaluator[]
{
    WhereEvaluator.Instance,
    new TenantEvaluator(currentTenantId),
    OrderEvaluator.Instance,
    // ... other evaluators
});
```

### Post-Processing

Apply in-memory transformations after database retrieval:

```csharp
var query = new Query<Product>
{
    PostProcessingAction = items => items
        .OrderBy(p => SomeComplexCalculation(p))
        .Take(10)
};

var products = await _repository.GetAllAsync(query);
```

### SelectMany for Flattening

Use SelectMany to flatten collections:

```csharp
var query = new Query<Order, OrderItem>();
query.SelectMany(order => order.OrderItems);

var allItems = await _repository.GetAllAsync(query);
```

## Migration from Direct LINQ

### Before (Direct LINQ)

```csharp
var products = await dbContext.Products
    .Where(p => p.IsActive)
    .Where(p => p.Price > 100)
    .OrderBy(p => p.Name)
    .Skip(20)
    .Take(10)
    .AsNoTracking()
    .ToListAsync();
```

### After (QuerySpec)

```csharp
var query = new Query<Product>
{
    Skip = 20,
    Take = 10,
    AsNoTracking = true
};

query.Where(p => p.IsActive)
     .Where(p => p.Price > 100)
     .OrderBy(p => p.Name);

var products = await _repository.GetAllAsync(query);
```

## Comparison Operators

```csharp
public enum ComparisonType
{
    EqualTo,              // ==
    NotEqualTo,           // !=
    GreaterThan,          // >
    GreaterThanOrEqual,   // >=
    LessThan,             // <
    LessThanOrEqual,      // <=
    Contains,             // LIKE %value%
    StartsWith,           // LIKE value%
    EndsWith              // LIKE %value
}
```

## Integration with Craft.Repositories

Craft.QuerySpec extends Craft.Repositories:

```
IBaseRepository<T, TKey>
    ?
IReadRepository<T, TKey>
    ?
IChangeRepository<T, TKey>  (from Craft.Repositories)
    ?
IRepository<T, TKey>         (from Craft.QuerySpec)
```

All methods from Craft.Repositories are available plus QuerySpec methods.

## Troubleshooting

### "Sequence contains more than one matching element"

This occurs when `GetAsync` finds multiple matches. Use `GetAllAsync` instead or refine your query.

### "No Selection defined in query"

For `IQuery<T, TResult>`, you must define either `Select` or `SelectMany`:

```csharp
// ? Correct
var query = new Query<Product, ProductDto>();
query.Select(p => p.Name);

// ? Wrong - no selection
var query = new Query<Product, ProductDto>();
```

### "Page size (Take) must be set and greater than zero"

For `GetPagedListAsync`, both `Skip` and `Take` must be set:

```csharp
// ? Correct
var query = new Query<Product> { Skip = 0, Take = 20 };

// ? Wrong
var query = new Query<Product> { Skip = 0 };  // Take is null
```

## Contributing

Contributions are welcome! Please ensure:
- All new features have comprehensive unit tests
- XML documentation is provided for public APIs
- Code follows the Craft coding standards
- Integration tests are added for repository operations

## License

This project is licensed under the MIT License. See the `LICENSE` file for details.

---

For more information, review the source code, XML documentation, and unit tests in the project.
