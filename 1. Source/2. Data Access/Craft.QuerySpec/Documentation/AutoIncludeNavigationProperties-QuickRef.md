# AutoIncludeNavigationProperties - Quick Reference

## What Is It?

A **single boolean flag** that automatically discovers and includes ALL navigation properties for an entity using reflection.

## Why Use It?

Perfect for **generic Blazor components** (CraftDataGrid, CraftCardGrid, etc.) where you don't know the entity structure at design time.

## Quick Example

```csharp
var query = new Query<PublicHoliday>
{
    AutoIncludeNavigationProperties = true  // ← Magic happens here!
};

var holidays = await repository.GetAllAsync(query);
// ALL navigation properties (Location, CreatedBy, etc.) are loaded!
```

## In Blazor Components

```razor
@code {
    [Parameter] public bool IncludeRelatedData { get; set; } = true;

    private async Task LoadDataAsync()
    {
        var query = new Query<TEntity>
        {
            AutoIncludeNavigationProperties = IncludeRelatedData
        };
        
        _items = await Repository.GetAllAsync(query);
    }
}
```

## Features

- ✅ **Automatic**: Uses reflection to discover navigation properties
- ✅ **Fast**: Results are cached (no reflection overhead after first use)
- ✅ **Safe**: Only goes 1 level deep (prevents circular references)
- ✅ **Smart**: Detects reference and collection navigation properties
- ✅ **Compatible**: Works with all repository methods

## How It Works

1. Set `AutoIncludeNavigationProperties = true`
2. System uses reflection to find all navigation properties
3. Properties with an `Id` field are considered entities
4. Automatically creates `Include` expressions for each
5. **Caches** the discovered properties for performance

## What Gets Included

### Reference Navigation (Single Entity)
```csharp
public Location Location { get; set; }  // ✅ Included
```

### Collection Navigation
```csharp
public List<OrderItem> OrderItems { get; set; }  // ✅ Included
public ICollection<Address> Addresses { get; set; }  // ✅ Included
```

### What's NOT Included
```csharp
public string Name { get; set; }  // ❌ Not navigation property
public int LocationId { get; set; }  // ❌ Foreign key, not navigation
public DateTime Date { get; set; }  // ❌ Value type
```

## Common Use Cases

### 1. Generic Data Grid

```razor
<CraftDataGrid TEntity="PublicHoliday" 
               IncludeNavigationProperties="true" />
```

### 2. Details Page

```csharp
var query = new Query<Order>
{
    AutoIncludeNavigationProperties = true
};
query.Where(o => o.Id == orderId);

var order = await repository.GetAsync(query);
// order.Customer, order.OrderItems, etc. all loaded!
```

### 3. With Pagination

```csharp
var query = new Query<Product>
{
    AutoIncludeNavigationProperties = true
};
query.SetPage(page, pageSize);

var result = await repository.GetPagedListAsync(query);
```

## When to Use

### ✅ Use AutoIncludeNavigationProperties When:

- Building generic/reusable components
- Entity structure varies across projects
- You need ALL navigation properties
- Displaying entity details in grids/forms
- Rapid development/prototyping

### ❌ Use Explicit Include When:

- You only need 1-2 specific properties
- Need nested relationships (Level 2+)
- Performance is critical
- Loading very large collections
- Want fine-grained control

## Comparison

### Auto Include

```csharp
// Simple boolean
var query = new Query<Order>
{
    AutoIncludeNavigationProperties = true
};
```

**Result**: Loads ALL navigation properties (Customer, OrderItems, ShippingAddress, etc.)

### Explicit Include

```csharp
// Specify exactly what to include
var query = new Query<Order>();
query.Include(o => o.Customer)
     .Include(o => o.OrderItems)
     .ThenInclude<Order, List<OrderItem>, Product>(oi => oi.First().Product);
```

**Result**: Loads only Customer, OrderItems, and nested Products

## Performance

### First Query (Per Entity Type)
- Uses reflection to discover properties
- Overhead: ~1-2ms
- Results cached automatically

### Subsequent Queries
- Uses cached results
- No reflection overhead
- Same speed as explicit Include

### SQL Impact

Generates LEFT JOINs for each navigation property:

```sql
SELECT h.*, l.*, u.*
FROM PublicHolidays h
LEFT JOIN Locations l ON h.LocationId = l.Id
LEFT JOIN Users u ON h.CreatedById = u.Id
```

## Combining with Other Features

### With Filtering

```csharp
var query = new Query<PublicHoliday>
{
    AutoIncludeNavigationProperties = true
};
query.Where(h => h.Date >= DateTime.Now);
```

### With Sorting

```csharp
var query = new Query<Order>
{
    AutoIncludeNavigationProperties = true
};
query.OrderByDescending(o => o.OrderDate);
```

### With Split Queries (For Large Collections)

```csharp
var query = new Query<Order>
{
    AutoIncludeNavigationProperties = true,
    AsSplitQuery = true  // ← Prevents cartesian explosion
};
```

### Hybrid Approach

```csharp
// Auto-include level 1, then explicit for level 2+
var query = new Query<Order>
{
    AutoIncludeNavigationProperties = true  // Includes Customer, OrderItems
};

// Then add nested includes
query.ThenInclude<Order, List<OrderItem>, Product>(oi => oi.First().Product);
```

## Troubleshooting

### Properties Still Null?

Check if the property looks like an entity:
- Does it have an `Id` property?
- Is it a class (not value type)?
- Is the foreign key data in the database?

### Performance Issues?

- Use `AsSplitQuery = true` for collection navigation
- Or switch to explicit Include for better control
- Check the generated SQL query

### Want to See Discovered Properties?

```csharp
var navProps = NavigationPropertyDiscovery.DiscoverNavigationProperties<YourEntity>();
foreach (var prop in navProps)
{
    Console.WriteLine($"{prop.Name}: {prop.PropertyType.Name}");
}
```

## Examples for Your Scenario

### PublicHolidays Grid Component

```csharp
@code {
    [Parameter] public bool LoadLocationData { get; set; } = true;
    
    private async Task LoadHolidaysAsync()
    {
        var query = new Query<PublicHoliday>
        {
            AutoIncludeNavigationProperties = LoadLocationData  // ← Simple!
        };
        
        query.SetPage(_page, _pageSize)
             .OrderBy(h => h.Date);
        
        var result = await _repository.GetPagedListAsync(query);
        _holidays = result.Items;
        // _holidays[0].Location is now populated!
    }
}
```

### Generic Grid with Toggle

```razor
<MudSwitch @bind-Checked="@_includeRelated" Label="Load Related Data" />

<CraftDataGrid TEntity="@TEntity" Items="@_items" />

@code {
    [Parameter] public bool IncludeNavigationProperties { get; set; } = true;
    
    private bool _includeRelated = true;
    
    private async Task LoadDataAsync()
    {
        var query = new Query<TEntity>
        {
            AutoIncludeNavigationProperties = _includeRelated
        };
        
        _items = await Repository.GetAllAsync(query);
    }
}
```

## Key Takeaways

1. **One boolean flag** to auto-load all navigation properties
2. **Uses reflection** (but cached for performance)
3. **1 level deep only** (safe from circular references)
4. **Perfect for generic components**
5. **Complements explicit Include** (use together!)

## Next Steps

- Read the [complete guide](AutoIncludeNavigationProperties-Guide.md) for detailed information
- See [Include feature docs](Include-Feature.md) for explicit Include usage
- Check [PublicHoliday example](Include-PublicHoliday-Example.md) for real-world usage
