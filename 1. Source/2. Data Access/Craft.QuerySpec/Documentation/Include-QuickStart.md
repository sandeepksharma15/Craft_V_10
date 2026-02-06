# QuerySpec Include Feature - Quick Start

## What's New?

The QuerySpec library now supports dynamic `Include` and `ThenInclude` for eagerly loading navigation properties without needing DbContext AutoInclude configuration!

## Quick Example

```csharp
// Load orders with their customers and order items
var query = new Query<Order>();
query.Include(o => o.Customer)
     .Include(o => o.OrderItems)
     .Where(o => o.OrderDate >= DateTime.Now.AddMonths(-1));

var orders = await repository.GetAllAsync(query);
```

## Why Use This?

- ✅ **No DbContext Configuration**: No need to set up AutoInclude
- ✅ **Flexible**: Choose what to load per query, not globally
- ✅ **Type-Safe**: Full IntelliSense support
- ✅ **Performant**: Avoid N+1 query problems
- ✅ **Clear**: Explicitly shows what's being loaded

## Key Methods

### Include - Load a navigation property

```csharp
query.Include(entity => entity.NavigationProperty);
```

### ThenInclude - Load nested navigation properties

```csharp
query.Include(o => o.OrderItems)
     .ThenInclude<Order, List<OrderItem>, Product>(oi => oi.First().Product);
```

## Common Patterns

### Single Entity Navigation

```csharp
var query = new Query<Order>();
query.Include(o => o.Customer);
```

### Collection Navigation

```csharp
var query = new Query<Customer>();
query.Include(c => c.Orders);
```

### Multiple Includes

```csharp
var query = new Query<Order>();
query.Include(o => o.Customer)
     .Include(o => o.OrderItems)
     .Include(o => o.ShippingAddress);
```

### Deep Navigation

```csharp
var query = new Query<Order>();
query.Include(o => o.Customer)
     .ThenInclude<Order, Customer, Address>(c => c.Address)
     .ThenInclude<Order, Address, City>(a => a.City);
```

## Complete Example

```csharp
public class OrderService
{
    private readonly IRepository<Order> _repository;

    public async Task<Order?> GetOrderDetailsAsync(int orderId)
    {
        var query = new Query<Order>();
        
        query.Include(o => o.Customer)
             .ThenInclude<Order, Customer, Address>(c => c.Address)
             .Include(o => o.OrderItems)
             .ThenInclude<Order, List<OrderItem>, Product>(oi => oi.First().Product)
             .Where(o => o.Id == orderId);

        return await _repository.GetAsync(query);
    }

    public async Task<PageResponse<Order>> GetRecentOrdersAsync(int page, int pageSize)
    {
        var query = new Query<Order>();
        
        query.Include(o => o.Customer)
             .Include(o => o.OrderItems)
             .Where(o => o.OrderDate >= DateTime.Now.AddMonths(-1))
             .OrderByDescending(o => o.OrderDate)
             .SetPage(page, pageSize);

        return await _repository.GetPagedListAsync(query);
    }
}
```

## Implementation Details

### Components Added

1. **IncludeExpression** - Stores include expression metadata
2. **IncludeEvaluator** - Applies includes to queryables
3. **QueryIncludeExtensions** - Provides Include/ThenInclude methods
4. **IQuery.IncludeExpressions** - Collection of includes in a query

### How It Works

1. Call `Include()` or `ThenInclude()` on your query
2. Include expressions are stored in `query.IncludeExpressions`
3. `IncludeEvaluator` processes these expressions when the query executes
4. EF Core's `Include()` and `ThenInclude()` are called on the underlying queryable
5. Navigation properties are loaded via SQL JOINs

## Repository Method Support

The Include feature works with all repository query methods:

- ✅ `GetAsync()`
- ✅ `GetAllAsync()`
- ✅ `GetPagedListAsync()`
- ✅ `GetCountAsync()` (includes ignored for count, but allowed)
- ✅ `DeleteAsync()` (includes applied before deletion)

## For More Information

See the complete documentation at: `Craft.QuerySpec/Documentation/Include-Feature.md`

## Breaking Changes

None! This is a pure additive feature. Existing code continues to work without modification.

## Migration from AutoInclude

If you're using `IgnoreAutoIncludes = false`:

```csharp
// Old way
var query = new Query<Order> { IgnoreAutoIncludes = false };

// New way (more explicit and flexible)
var query = new Query<Order>();
query.Include(o => o.Customer)
     .Include(o => o.OrderItems);
```
