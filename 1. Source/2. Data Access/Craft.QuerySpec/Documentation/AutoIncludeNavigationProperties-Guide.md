# AutoIncludeNavigationProperties Feature - Complete Guide

## Overview

The **AutoIncludeNavigationProperties** feature automatically discovers and includes ALL navigation properties for an entity using reflection. This is perfect for generic components like data grids where you don't know which navigation properties exist at design time.

## The Problem This Solves

In generic Blazor components like `CraftDataGrid`, `CraftCardGrid`, or `CraftGrid`, you want to:
- Display related data without knowing entity structure at compile time
- Avoid manually specifying Include expressions for each entity type
- Have a simple boolean parameter to control whether related data is loaded

## Quick Start

### Simple Usage

```csharp
// Create a query with auto-include enabled
var query = new Query<PublicHoliday>
{
    AutoIncludeNavigationProperties = true  // ← That's it!
};

var holidays = await repository.GetAllAsync(query);
// ALL navigation properties (Location, CreatedBy, etc.) are now loaded!
```

### In Blazor Components

```csharp
@code {
    [Parameter] public bool IncludeRelatedData { get; set; } = true;

    private async Task LoadDataAsync()
    {
        var query = new Query<PublicHoliday>
        {
            AutoIncludeNavigationProperties = IncludeRelatedData
        };
        
        query.SetPage(_currentPage, _pageSize);
        
        var result = await _repository.GetPagedListAsync(query);
        _holidays = result.Items;
    }
}
```

## How It Works

### 1. Reflection-Based Discovery

When `AutoIncludeNavigationProperties = true`, the evaluator:
1. Uses reflection to scan the entity type for navigation properties
2. Identifies properties that look like EF Core navigation properties:
   - Reference navigation (single entity with an `Id` property)
   - Collection navigation (List, ICollection, IEnumerable of entities)
3. **Caches** the results for performance (subsequent queries are fast!)
4. Automatically creates Include expressions for each discovered property
5. Goes **1 level deep only** to prevent circular references

### 2. Smart Detection

The system detects navigation properties by checking:
- Property is a class (not value type or string)
- For reference navigation: Has an `Id` property (looks like an entity)
- For collection navigation: Is `ICollection<T>`, `List<T>`, `IEnumerable<T>`, etc.
- Collection element type has an `Id` property

### 3. Performance Optimization

- **First query**: Uses reflection to discover properties (~1-2ms overhead)
- **Subsequent queries**: Uses cached results (no reflection overhead!)
- Cache persists for application lifetime

## Real-World Examples

### Example 1: Generic Data Grid Component

```csharp
// CraftDataGrid.razor
@typeparam TEntity where TEntity : class, IEntity

<MudDataGrid T="TEntity" Items="@_items">
    @ChildContent
</MudDataGrid>

@code {
    [Parameter] public bool IncludeNavigationProperties { get; set; } = true;
    [Parameter] public Func<IQuery<TEntity>>? QueryFactory { get; set; }
    
    private List<TEntity> _items = [];
    
    protected override async Task OnInitializedAsync()
    {
        var query = QueryFactory?.Invoke() ?? new Query<TEntity>();
        query.AutoIncludeNavigationProperties = IncludeNavigationProperties;
        
        _items = await Repository.GetAllAsync(query);
    }
}
```

**Usage:**
```razor
<!-- Automatically loads Location and any other navigation properties -->
<CraftDataGrid TEntity="PublicHoliday" 
               IncludeNavigationProperties="true">
    <Columns>
        <Column Title="Date" Field="@nameof(PublicHoliday.Date)" />
        <Column Title="Name" Field="@nameof(PublicHoliday.Name)" />
        <Column Title="Location" Field="@nameof(PublicHoliday.Location.Name)" />
    </Columns>
</CraftDataGrid>
```

### Example 2: Generic Card Grid

```csharp
// CraftCardGrid.razor
@typeparam TEntity where TEntity : class, IEntity

@code {
    [Parameter] public bool LoadRelatedData { get; set; }
    [Parameter] public int PageSize { get; set; } = 12;
    [Parameter] public RenderFragment<TEntity>? CardTemplate { get; set; }
    
    private async Task LoadCardsAsync(int page)
    {
        var query = new Query<TEntity>
        {
            AutoIncludeNavigationProperties = LoadRelatedData
        };
        
        query.SetPage(page, PageSize);
        
        var result = await Repository.GetPagedListAsync(query);
        _cards = result.Items;
    }
}
```

**Usage:**
```razor
<CraftCardGrid TEntity="Product" LoadRelatedData="true">
    <CardTemplate Context="product">
        <MudCard>
            <MudCardContent>
                <MudText>@product.Name</MudText>
                <MudText>Category: @product.Category?.Name</MudText>
                <MudText>Supplier: @product.Supplier?.CompanyName</MudText>
            </MudCardContent>
        </MudCard>
    </CardTemplate>
</CraftCardGrid>
```

### Example 3: PublicHolidays with Generic Grid

**Entity:**
```csharp
public class PublicHoliday : IEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime Date { get; set; }
    
    public int LocationId { get; set; }
    public Location Location { get; set; }  // ← Auto-included!
    
    public int? CreatedById { get; set; }
    public User? CreatedBy { get; set; }  // ← Auto-included!
}
```

**Component:**
```csharp
@page "/holidays"
@inject IRepository<PublicHoliday> HolidayRepository

<MudSwitch @bind-Checked="@_includeRelatedData" 
           Label="Load related data" />

<MudDataGrid T="PublicHoliday" Items="@_holidays" Loading="@_loading">
    <Columns>
        <PropertyColumn Property="h => h.Date" Title="Date" />
        <PropertyColumn Property="h => h.Name" Title="Holiday Name" />
        <PropertyColumn Property="h => h.Location.Name" Title="Location" />
        <PropertyColumn Property="h => h.CreatedBy.Name" Title="Created By" />
    </Columns>
</MudDataGrid>

@code {
    private List<PublicHoliday> _holidays = [];
    private bool _loading;
    private bool _includeRelatedData = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadHolidaysAsync();
    }

    private async Task LoadHolidaysAsync()
    {
        _loading = true;
        
        var query = new Query<PublicHoliday>
        {
            AutoIncludeNavigationProperties = _includeRelatedData  // ← Simple boolean!
        };
        
        query.OrderBy(h => h.Date);
        
        _holidays = await HolidayRepository.GetAllAsync(query);
        _loading = false;
    }
}
```

### Example 4: Combining with Other Query Features

```csharp
public async Task<PageResponse<Order>> GetOrdersAsync(
    int page, 
    int pageSize,
    bool includeRelatedData,
    OrderStatus? statusFilter = null)
{
    var query = new Query<Order>
    {
        AutoIncludeNavigationProperties = includeRelatedData
    };
    
    // Add filtering
    if (statusFilter.HasValue)
        query.Where(o => o.Status == statusFilter.Value);
    
    // Add sorting
    query.OrderByDescending(o => o.OrderDate)
         .ThenBy(o => o.Id);
    
    // Add pagination
    query.SetPage(page, pageSize);
    
    return await _repository.GetPagedListAsync(query);
}
```

## Comparison: AutoInclude vs Explicit Include

### Auto Include (New Feature)

```csharp
// Automatically includes ALL navigation properties
var query = new Query<PublicHoliday>
{
    AutoIncludeNavigationProperties = true
};

// Loads: Location, CreatedBy, and any other navigation properties
var holidays = await repository.GetAllAsync(query);
```

**Pros:**
- ✅ Simple boolean flag
- ✅ Perfect for generic components
- ✅ No code changes needed when entity structure changes
- ✅ Cached for performance

**Cons:**
- ❌ Loads ALL navigation properties (might load more than needed)
- ❌ Only goes 1 level deep (can't load nested relationships)

### Explicit Include

```csharp
// Explicitly specify which properties to include
var query = new Query<PublicHoliday>();
query.Include(h => h.Location)
     .Include(h => h.CreatedBy)
     .ThenInclude<PublicHoliday, User, Department>(u => u.Department);

var holidays = await repository.GetAllAsync(query);
```

**Pros:**
- ✅ Load only what you need
- ✅ Support for nested relationships (ThenInclude)
- ✅ More control and precision

**Cons:**
- ❌ Must know entity structure at compile time
- ❌ More code to write
- ❌ Need to update code when entity structure changes

### Hybrid Approach (Best of Both Worlds)

```csharp
// Auto-include first level, then explicitly include nested
var query = new Query<Order>
{
    AutoIncludeNavigationProperties = true  // Includes Customer, OrderItems, etc.
};

// Then explicitly include nested relationships
query.ThenInclude<Order, List<OrderItem>, Product>(oi => oi.First().Product)
     .ThenInclude<Order, Product, Category>(p => p.Category);

var orders = await repository.GetAllAsync(query);
```

## Blazor Component Patterns

### Pattern 1: Simple Toggle

```razor
<MudSwitch @bind-Checked="@_loadRelated" Label="Load Related Data" 
           Color="Color.Primary" />

@code {
    private bool _loadRelated = true;
    
    private IQuery<TEntity> CreateQuery()
    {
        return new Query<TEntity>
        {
            AutoIncludeNavigationProperties = _loadRelated
        };
    }
}
```

### Pattern 2: Component Parameter

```csharp
@code {
    [Parameter]
    public bool IncludeNavigationProperties { get; set; } = true;
    
    [Parameter]
    public EventCallback<bool> IncludeNavigationPropertiesChanged { get; set; }
}
```

### Pattern 3: Based on View Mode

```csharp
@code {
    [Parameter] public ViewMode Mode { get; set; } = ViewMode.Detailed;
    
    private async Task LoadDataAsync()
    {
        var query = new Query<TEntity>
        {
            // Only load related data in detailed view
            AutoIncludeNavigationProperties = Mode == ViewMode.Detailed
        };
        
        _items = await Repository.GetAllAsync(query);
    }
}

public enum ViewMode { Summary, Detailed }
```

### Pattern 4: User Preference

```csharp
@inject ILocalStorageService LocalStorage

@code {
    private bool _includeRelatedData;
    
    protected override async Task OnInitializedAsync()
    {
        // Load user preference
        _includeRelatedData = await LocalStorage.GetItemAsync<bool>("includeRelatedData") 
                              ?? true;
        
        await LoadDataAsync();
    }
    
    private async Task ToggleRelatedDataAsync()
    {
        _includeRelatedData = !_includeRelatedData;
        await LocalStorage.SetItemAsync("includeRelatedData", _includeRelatedData);
        await LoadDataAsync();
    }
}
```

## Performance Considerations

### Cache Behavior

The navigation properties are discovered once per entity type and cached:

```csharp
// First query for PublicHoliday
var query1 = new Query<PublicHoliday> 
{ 
    AutoIncludeNavigationProperties = true 
};
// Reflection overhead: ~1-2ms (discovers properties)

// Second query for PublicHoliday
var query2 = new Query<PublicHoliday> 
{ 
    AutoIncludeNavigationProperties = true 
};
// No reflection overhead (uses cache)

// Query for different entity
var query3 = new Query<Order> 
{ 
    AutoIncludeNavigationProperties = true 
};
// Reflection overhead: ~1-2ms (discovers properties for Order)
```

### SQL Query Impact

With `AutoIncludeNavigationProperties = true`:

**Single Entity Query:**
```sql
SELECT h.*, l.*, u.*
FROM PublicHolidays h
LEFT JOIN Locations l ON h.LocationId = l.Id
LEFT JOIN Users u ON h.CreatedById = u.Id
WHERE h.Id = 1
```

**Collection Query:**
```sql
-- Same as above but for all matching records
SELECT h.*, l.*, u.*
FROM PublicHolidays h
LEFT JOIN Locations l ON h.LocationId = l.Id
LEFT JOIN Users u ON h.CreatedById = u.Id
ORDER BY h.Date
```

### When to Use Auto-Include

✅ **USE when:**
- Building generic/reusable components
- Entity structure varies across projects
- All navigation properties are typically needed
- Displaying entity details
- Performance is acceptable (extra JOINs are not a problem)

❌ **DON'T USE when:**
- Only need 1-2 specific properties (use explicit Include)
- Loading very large collections
- Need nested relationships (use ThenInclude)
- Performance is critical (hand-optimize the query)
- Entity has many navigation properties you don't need

## Advanced Scenarios

### Scenario 1: Conditional Auto-Include

```csharp
public async Task<List<TEntity>> GetDataAsync<TEntity>(
    bool fullDetails,
    Expression<Func<TEntity, bool>>? filter = null)
    where TEntity : class, IEntity
{
    var query = new Query<TEntity>
    {
        // Auto-include only when full details requested
        AutoIncludeNavigationProperties = fullDetails
    };
    
    if (filter != null)
        query.Where(filter);
    
    return await _repository.GetAllAsync(query);
}
```

### Scenario 2: Auto-Include with Manual Overrides

```csharp
// Start with auto-include
var query = new Query<Order>
{
    AutoIncludeNavigationProperties = true
};

// The evaluator will skip properties already explicitly included
query.Include(o => o.Customer);  // This takes precedence
// Other navigation properties are still auto-included

var orders = await repository.GetAllAsync(query);
```

### Scenario 3: Generic Service with Auto-Include

```csharp
public class GenericDataService<TEntity> where TEntity : class, IEntity
{
    private readonly IRepository<TEntity> _repository;
    
    public async Task<PageResponse<TEntity>> GetPagedAsync(
        int page,
        int pageSize,
        bool includeNavigationProperties = true)
    {
        var query = new Query<TEntity>
        {
            AutoIncludeNavigationProperties = includeNavigationProperties
        };
        
        query.SetPage(page, pageSize);
        
        return await _repository.GetPagedListAsync(query);
    }
}
```

## Troubleshooting

### Navigation Properties Not Loading

**Issue**: Auto-include enabled but properties still null.

**Possible causes:**
1. Property doesn't have an `Id` property (not detected as entity)
2. Property is not marked as `virtual` (lazy loading might interfere)
3. Foreign key data doesn't exist in database

**Solution:**
```csharp
// Check what properties were discovered
var navProps = NavigationPropertyDiscovery.DiscoverNavigationProperties<YourEntity>();
foreach (var prop in navProps)
{
    Console.WriteLine($"Discovered: {prop.Name} ({prop.PropertyType.Name})");
}
```

### Performance Issues

**Issue**: Queries are slow with auto-include.

**Solution:**
1. Check SQL query (are there cartesian products?)
2. Use `AsSplitQuery = true` for collection navigation:

```csharp
var query = new Query<Order>
{
    AutoIncludeNavigationProperties = true,
    AsSplitQuery = true  // ← Generates multiple queries instead of one large JOIN
};
```

3. Or switch to explicit Include for better control:

```csharp
// More performant than auto-include
var query = new Query<Order>();
query.Include(o => o.Customer);  // Only load what you need
```

### Circular References

**Issue**: Worried about circular references.

**Solution**: Built-in! Auto-include only goes **1 level deep**. It won't recursively load navigation properties of navigation properties, preventing circular references automatically.

## API Reference

### IQuery<T> Property

```csharp
/// <summary>
/// Gets or sets a value indicating whether all navigation properties should be automatically included.
/// </summary>
/// <remarks>
/// When set to true, uses reflection to discover all navigation properties
/// on the entity and automatically includes them (1 level deep only).
/// Results are cached for performance.
/// </remarks>
bool AutoIncludeNavigationProperties { get; set; }
```

### NavigationPropertyDiscovery Helper

```csharp
// Discover navigation properties for an entity
List<PropertyInfo> DiscoverNavigationProperties<T>() where T : class

// Create Include expression for a property
IncludeExpression CreateIncludeExpression<T>(PropertyInfo navigationProperty) where T : class

// Clear cache (useful for testing)
void ClearCache()
```

## Best Practices

### ✅ DO

- Use for generic components that work with multiple entity types
- Use when you need all navigation properties loaded
- Set to `false` by default and let users opt-in
- Combine with explicit Include when you need nested relationships
- Monitor SQL queries to ensure performance

### ❌ DON'T

- Use when you only need 1-2 specific properties
- Expect it to load nested relationships (only 1 level deep)
- Use for entities with 10+ navigation properties unless needed
- Forget to consider the SQL JOIN impact

## Summary

**AutoIncludeNavigationProperties** provides a simple, reflection-based way to automatically load all navigation properties:

✅ **Perfect for**: Generic components, rapid development, unknown entity structures  
✅ **Simple**: Just a boolean flag  
✅ **Fast**: Cached for performance  
✅ **Safe**: Only 1 level deep to prevent circular references  

**Use it when simplicity and flexibility matter more than fine-grained control!**
