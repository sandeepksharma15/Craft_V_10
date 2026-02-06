# Include and ThenInclude Feature Documentation

## Overview

The QuerySpec library now supports **dynamic Include and ThenInclude** functionality, allowing you to specify navigation properties to eagerly load without needing to configure `AutoInclude` in your Entity Framework Core DbContext.

## Features

- **Fluent API**: Chain `Include()` and `ThenInclude()` calls for easy-to-read code
- **Type-Safe**: Full IntelliSense support with compile-time type checking
- **Collection Navigation**: Support for both single and collection navigation properties
- **Multiple Includes**: Add multiple independent Include expressions in a single query
- **Deep Navigation**: Chain ThenInclude calls for loading nested relationships

## Basic Usage

### Single Include

Load a single navigation property:

```csharp
var query = new Query<Order>();
query.Include(o => o.Customer);

var orders = await repository.GetAllAsync(query);
// Customer navigation property will be loaded for all orders
```

### Multiple Includes

Load multiple navigation properties:

```csharp
var query = new Query<Order>();
query.Include(o => o.Customer)
     .Include(o => o.OrderItems);

var orders = await repository.GetAllAsync(query);
// Both Customer and OrderItems will be loaded
```

### ThenInclude for Nested Relationships

Load nested navigation properties:

```csharp
var query = new Query<Order>();
query.Include(o => o.OrderItems)
     .ThenInclude<Order, List<OrderItem>, Product>(oi => oi.First().Product);

var orders = await repository.GetAllAsync(query);
// OrderItems and their Products will be loaded
```

### Complex Navigation Chains

Load deeply nested relationships:

```csharp
var query = new Query<Order>();
query.Include(o => o.Customer)
     .ThenInclude<Order, Customer, Address>(c => c.Address)
     .ThenInclude<Order, Address, City>(a => a.City);

var orders = await repository.GetAllAsync(query);
// Customer, Address, and City will all be loaded
```

## Repository Integration

The Include functionality works seamlessly with all repository methods:

### GetAsync

```csharp
var query = new Query<Order>();
query.Include(o => o.Customer)
     .Include(o => o.OrderItems)
     .Where(o => o.Id == orderId);

var order = await repository.GetAsync(query);
```

### GetAllAsync

```csharp
var query = new Query<Order>();
query.Include(o => o.Customer)
     .Where(o => o.OrderDate >= DateTime.Now.AddMonths(-1));

var recentOrders = await repository.GetAllAsync(query);
```

### GetPagedListAsync

```csharp
var query = new Query<Order>();
query.Include(o => o.Customer)
     .Include(o => o.OrderItems)
     .SetPage(page: 1, pageSize: 10)
     .OrderBy(o => o.OrderDate);

var pagedOrders = await repository.GetPagedListAsync(query);
```

### GetCountAsync

```csharp
// Include doesn't affect count, but you can still use it
var query = new Query<Order>();
query.Include(o => o.Customer)
     .Where(o => o.Status == OrderStatus.Pending);

var count = await repository.GetCountAsync(query);
```

## Real-World Examples

### Example 1: E-Commerce Order Loading

```csharp
public async Task<Order?> GetOrderWithDetailsAsync(int orderId)
{
    var query = new Query<Order>();
    
    query.Include(o => o.Customer)
         .ThenInclude<Order, Customer, Address>(c => c.BillingAddress)
         .Include(o => o.OrderItems)
         .ThenInclude<Order, List<OrderItem>, Product>(oi => oi.First().Product)
         .ThenInclude<Order, Product, Category>(p => p.Category)
         .Include(o => o.ShippingAddress)
         .Where(o => o.Id == orderId);

    return await _repository.GetAsync(query);
}
```

### Example 2: Blog Post with Comments and Authors

```csharp
public async Task<List<BlogPost>> GetPostsWithCommentsAsync()
{
    var query = new Query<BlogPost>();
    
    query.Include(p => p.Author)
         .Include(p => p.Comments)
         .ThenInclude<BlogPost, List<Comment>, User>(c => c.First().Author)
         .Include(p => p.Tags)
         .Where(p => p.PublishedDate <= DateTime.Now)
         .OrderByDescending(p => p.PublishedDate);

    return await _repository.GetAllAsync(query);
}
```

### Example 3: Organizational Hierarchy

```csharp
public async Task<Employee?> GetEmployeeWithHierarchyAsync(int employeeId)
{
    var query = new Query<Employee>();
    
    query.Include(e => e.Manager)
         .Include(e => e.DirectReports)
         .ThenInclude<Employee, List<Employee>, Department>(dr => dr.First().Department)
         .Include(e => e.Department)
         .ThenInclude<Employee, Department, Company>(d => d.Company)
         .Where(e => e.Id == employeeId);

    return await _repository.GetAsync(query);
}
```

### Example 4: Project Management

```csharp
public async Task<PageResponse<Project>> GetProjectsWithTeamAsync(int page, int pageSize)
{
    var query = new Query<Project>();
    
    query.Include(p => p.ProjectManager)
         .Include(p => p.TeamMembers)
         .ThenInclude<Project, List<ProjectMember>, Employee>(tm => tm.First().Employee)
         .ThenInclude<Project, Employee, Department>(e => e.Department)
         .Include(p => p.Tasks)
         .ThenInclude<Project, List<ProjectTask>, Employee>(t => t.First().AssignedTo)
         .SetPage(page, pageSize)
         .OrderByDescending(p => p.CreatedDate);

    return await _repository.GetPagedListAsync(query);
}
```

## Comparison with AutoInclude

### Before (AutoInclude Configuration)

```csharp
// In DbContext OnModelCreating
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Order>()
        .Navigation(o => o.Customer)
        .AutoInclude();
    
    modelBuilder.Entity<Order>()
        .Navigation(o => o.OrderItems)
        .AutoInclude();
}

// In your code
var query = new Query<Order>
{
    IgnoreAutoIncludes = false  // Must remember to set this
};
var orders = await repository.GetAllAsync(query);
```

### After (Dynamic Include)

```csharp
// No DbContext configuration needed!

// In your code
var query = new Query<Order>();
query.Include(o => o.Customer)
     .Include(o => o.OrderItems);

var orders = await repository.GetAllAsync(query);
```

## Benefits

1. **Flexibility**: Choose which navigation properties to load per query, not globally
2. **No Configuration**: No need to modify DbContext or entity configurations
3. **Explicit**: Clear in the code what relationships are being loaded
4. **Performance**: Load only what you need for each specific use case
5. **Maintainable**: Easy to add/remove includes without changing EF Core configuration

## Best Practices

### ✅ DO

- Use Include when you know you'll need the navigation property data
- Chain multiple ThenInclude calls for deep navigation hierarchies
- Combine Include with Where, OrderBy, and pagination for efficient queries
- Use Include in read-only queries (GetAsync, GetAllAsync, GetPagedListAsync)

### ❌ DON'T

- Don't over-include – only load relationships you actually need
- Don't use Include for properties you'll never access
- Don't forget to specify type parameters for ThenInclude when working with collections
- Don't mix Include with IgnoreAutoIncludes = true expecting AutoInclude behavior

## Performance Considerations

### Good Performance

```csharp
// Load only what you need
var query = new Query<Order>();
query.Include(o => o.Customer)  // Only Customer
     .Where(o => o.Id == orderId);
```

### Potential N+1 Problem (Without Include)

```csharp
// BAD: This causes N+1 queries
var orders = await repository.GetAllAsync(new Query<Order>());
foreach (var order in orders)
{
    // Each access to Customer causes a separate query!
    Console.WriteLine(order.Customer.Name);
}
```

### Fixed with Include

```csharp
// GOOD: Single query with JOIN
var query = new Query<Order>();
query.Include(o => o.Customer);  // Load all customers in one query

var orders = await repository.GetAllAsync(query);
foreach (var order in orders)
{
    Console.WriteLine(order.Customer.Name);  // No extra queries!
}
```

## Troubleshooting

### Navigation Property is Null

**Problem**: Navigation property returns null even with Include.

**Solution**: 
1. Check that the foreign key relationship is properly configured
2. Ensure data actually exists in the database
3. Verify the Include expression matches your property name exactly

```csharp
// Wrong
query.Include(o => o.Customers);  // Property name is Customer, not Customers

// Correct
query.Include(o => o.Customer);
```

### ThenInclude Compilation Error

**Problem**: ThenInclude doesn't compile or shows type mismatch.

**Solution**: You must specify all three type parameters for ThenInclude:

```csharp
// Wrong
query.Include(o => o.OrderItems)
     .ThenInclude(oi => oi.Product);  // Missing type parameters

// Correct
query.Include(o => o.OrderItems)
     .ThenInclude<Order, List<OrderItem>, Product>(oi => oi.First().Product);
```

### Include Not Working with Projections

**Problem**: Include seems to have no effect when using Select.

**Explanation**: When you use Select (projections), EF Core only loads the properties you explicitly select. Include is ignored.

```csharp
// Include is ignored here
var query = new Query<Order, OrderDto>();
query.Include(o => o.Customer);
query.Select(o => new OrderDto { Id = o.Id });  // Only Id is loaded

// If you need related data in projection, reference it in Select
query.Select(o => new OrderDto 
{ 
    Id = o.Id,
    CustomerName = o.Customer.Name  // This will load Customer automatically
});
```

## API Reference

### Include Method

```csharp
public static IQuery<T> Include<T, TProperty>(
    this IQuery<T> query,
    Expression<Func<T, TProperty>> navigationPropertyPath)
```

Specifies a related entity to include in the query results.

**Type Parameters:**
- `T`: The entity type
- `TProperty`: The type of the navigation property

**Parameters:**
- `query`: The query to add the include to
- `navigationPropertyPath`: Lambda expression representing the navigation property

**Returns:** The query with the include added (fluent interface)

### ThenInclude Method

```csharp
public static IQuery<T> ThenInclude<T, TPreviousProperty, TProperty>(
    this IQuery<T> query,
    Expression<Func<TPreviousProperty, TProperty>> navigationPropertyPath)
```

Specifies additional related entities to include after a previous Include.

**Type Parameters:**
- `T`: The root entity type
- `TPreviousProperty`: The type of the previously included navigation property
- `TProperty`: The type of the navigation property to include

**Parameters:**
- `query`: The query to add the include to
- `navigationPropertyPath`: Lambda expression representing the navigation property

**Returns:** The query with the include added (fluent interface)

**Throws:**
- `InvalidOperationException`: If called before Include

## Migration Guide

If you're currently using `IgnoreAutoIncludes = false` with AutoInclude configuration:

### Step 1: Identify AutoInclude Configurations

Find all AutoInclude configurations in your DbContext:

```csharp
modelBuilder.Entity<Order>()
    .Navigation(o => o.Customer)
    .AutoInclude();
```

### Step 2: Update Your Queries

Replace `IgnoreAutoIncludes = false` with explicit Include:

```csharp
// Old way
var query = new Query<Order>
{
    IgnoreAutoIncludes = false
};

// New way
var query = new Query<Order>();
query.Include(o => o.Customer);
```

### Step 3: Remove AutoInclude Configurations (Optional)

Once all queries are updated, you can remove AutoInclude from your DbContext.

## Conclusion

The Include and ThenInclude feature provides a flexible, type-safe way to load related entities in your queries. It gives you fine-grained control over what data is loaded, improving both performance and code maintainability.

For questions or issues, refer to the test suite in `Craft.QuerySpec.Tests` or consult the EF Core documentation on eager loading.
