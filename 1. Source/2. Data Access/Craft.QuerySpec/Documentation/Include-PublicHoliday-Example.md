# Fixing PublicHoliday Location Loading Issue

## The Problem

When querying `PublicHoliday` entities, the `Location` navigation property was returning `null` even with `IgnoreAutoIncludes = false`.

```csharp
// This didn't work:
var query = new Query<PublicHoliday>
{
    IgnoreAutoIncludes = false
};
query.SetPage(page, pageSize);
var result = await repository.GetPagedListAsync(query);

// result.Items[0].Location was NULL!
```

## Why It Didn't Work

`IgnoreAutoIncludes = false` only works if you have configured `AutoInclude` in your DbContext:

```csharp
// You would need this in your DbContext:
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<PublicHoliday>()
        .Navigation(ph => ph.Location)
        .AutoInclude();
}
```

Without this configuration, EF Core doesn't know to automatically load the `Location` property.

## The Solution: Use Include

Now you can explicitly load the `Location` navigation property using the new `Include` feature:

### Option 1: Simple Include

```csharp
var query = new Query<PublicHoliday>();
query.Include(ph => ph.Location)  // Explicitly load Location
     .SetPage(page, pageSize);

var result = await repository.GetPagedListAsync(query);

// result.Items[0].Location is now loaded!
```

### Option 2: Include with Filtering

```csharp
var query = new Query<PublicHoliday>();
query.Include(ph => ph.Location)
     .Where(ph => ph.Date >= DateTime.Now.Year)
     .OrderBy(ph => ph.Date)
     .SetPage(page, pageSize);

var result = await repository.GetPagedListAsync(query);
```

### Option 3: Include with Multiple Navigation Properties

If `PublicHoliday` has other navigation properties:

```csharp
var query = new Query<PublicHoliday>();
query.Include(ph => ph.Location)
     .Include(ph => ph.CreatedByUser)  // If you have this
     .SetPage(page, pageSize);

var result = await repository.GetPagedListAsync(query);
```

## Complete Example: PublicHolidays Service

```csharp
public class PublicHolidayService
{
    private readonly IRepository<PublicHoliday> _repository;

    public PublicHolidayService(IRepository<PublicHoliday> repository)
    {
        _repository = repository;
    }

    public async Task<PageResponse<PublicHoliday>> GetPagedHolidaysAsync(
        int page, 
        int pageSize,
        int? locationId = null,
        int? year = null)
    {
        var query = new Query<PublicHoliday>();
        
        // Always include Location so it's not null
        query.Include(ph => ph.Location);

        // Optional filters
        if (locationId.HasValue)
            query.Where(ph => ph.LocationId == locationId.Value);

        if (year.HasValue)
            query.Where(ph => ph.Date.Year == year.Value);

        // Pagination and sorting
        query.OrderBy(ph => ph.Date)
             .SetPage(page, pageSize);

        return await _repository.GetPagedListAsync(query);
    }

    public async Task<PublicHoliday?> GetHolidayByIdAsync(int id)
    {
        var query = new Query<PublicHoliday>();
        
        query.Include(ph => ph.Location)  // Load Location
             .Where(ph => ph.Id == id);

        return await _repository.GetAsync(query);
    }

    public async Task<List<PublicHoliday>> GetHolidaysByLocationAsync(int locationId)
    {
        var query = new Query<PublicHoliday>();
        
        query.Include(ph => ph.Location)
             .Where(ph => ph.LocationId == locationId)
             .OrderBy(ph => ph.Date);

        return await _repository.GetAllAsync(query);
    }

    public async Task<List<PublicHoliday>> GetUpcomingHolidaysAsync(int count = 10)
    {
        var query = new Query<PublicHoliday>();
        
        query.Include(ph => ph.Location)
             .Where(ph => ph.Date >= DateTime.Now)
             .OrderBy(ph => ph.Date)
             .Take = count;

        return await _repository.GetAllAsync(query);
    }
}
```

## Update Your Blazor Component

### Before (Location was null)

```csharp
private async Task LoadDataAsync()
{
    var query = new Query<PublicHoliday>
    {
        IgnoreAutoIncludes = false  // Didn't work!
    };
    query.SetPage(_currentPage, _pageSize);
    
    var result = await _repository.GetPagedListAsync(query);
    _holidays = result.Items;
    // _holidays[0].Location was NULL
}
```

### After (Location is loaded)

```csharp
private async Task LoadDataAsync()
{
    var query = new Query<PublicHoliday>();
    
    query.Include(ph => ph.Location)  // This works!
         .OrderBy(ph => ph.Date)
         .SetPage(_currentPage, _pageSize);
    
    var result = await _repository.GetPagedListAsync(query);
    _holidays = result.Items;
    // _holidays[0].Location is now populated!
}
```

## LocationLookup Component Usage

If you're using the `LocationLookup` component, the Location should now be loaded:

```razor
@foreach (var holiday in _holidays)
{
    <tr>
        <td>@holiday.Date.ToShortDateString()</td>
        <td>@holiday.Name</td>
        <td>
            @if (holiday.Location != null)
            {
                <text>@holiday.Location.Name</text>
            }
            else
            {
                <text>No Location</text>
            }
        </td>
    </tr>
}
```

## Performance Benefits

### Without Include (N+1 Problem)

```csharp
// Bad: Causes N+1 queries
var holidays = await _repository.GetAllAsync(new Query<PublicHoliday>());

foreach (var holiday in holidays)
{
    // Each access to Location causes a separate database query!
    Console.WriteLine(holiday.Location.Name);
}
// Total queries: 1 + N (where N = number of holidays)
```

### With Include (Single Query)

```csharp
// Good: Single query with JOIN
var query = new Query<PublicHoliday>();
query.Include(ph => ph.Location);

var holidays = await _repository.GetAllAsync(query);

foreach (var holiday in holidays)
{
    // No extra queries - Location is already loaded
    Console.WriteLine(holiday.Location.Name);
}
// Total queries: 1
```

## Common Patterns

### Pattern 1: Grid/List Display

```csharp
// For displaying holidays in a grid with location
var query = new Query<PublicHoliday>();
query.Include(ph => ph.Location)
     .OrderBy(ph => ph.Date)
     .SetPage(page, pageSize);
```

### Pattern 2: Edit Form

```csharp
// For editing a holiday with location dropdown
var query = new Query<PublicHoliday>();
query.Include(ph => ph.Location)
     .Where(ph => ph.Id == holidayId);

var holiday = await _repository.GetAsync(query);
```

### Pattern 3: Calendar View

```csharp
// For displaying holidays by location in a calendar
var query = new Query<PublicHoliday>();
query.Include(ph => ph.Location)
     .Where(ph => ph.Date.Year == selectedYear)
     .OrderBy(ph => ph.Date);

var holidays = await _repository.GetAllAsync(query);
```

### Pattern 4: Reports

```csharp
// For generating reports with location details
var query = new Query<PublicHoliday>();
query.Include(ph => ph.Location)
     .Where(ph => ph.Date >= startDate && ph.Date <= endDate)
     .OrderBy(ph => ph.Location.Name)
     .ThenBy(ph => ph.Date);

var holidays = await _repository.GetAllAsync(query);
```

## Troubleshooting

### Location Still Null?

1. **Check Foreign Key**: Ensure `PublicHoliday.LocationId` has a valid value
2. **Check Data**: Verify the Location exists in the database
3. **Check Relationship**: Ensure the navigation property is properly configured:

```csharp
public class PublicHoliday : IEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime Date { get; set; }
    
    public int LocationId { get; set; }  // Foreign key
    public Location Location { get; set; }  // Navigation property
}
```

### Still Using AutoInclude?

If you prefer to keep using AutoInclude:

```csharp
// In your DbContext
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<PublicHoliday>()
        .Navigation(ph => ph.Location)
        .AutoInclude();

    base.OnModelCreating(modelBuilder);
}

// In your query
var query = new Query<PublicHoliday>
{
    IgnoreAutoIncludes = false  // Now this will work!
};
```

But the new Include approach is more flexible and explicit!

## Summary

**Problem**: `Location` was null despite `IgnoreAutoIncludes = false`

**Root Cause**: No AutoInclude configuration in DbContext

**Solution**: Use the new `Include` feature:

```csharp
query.Include(ph => ph.Location)
```

This explicitly tells EF Core to load the Location navigation property using a SQL JOIN, avoiding null references and N+1 query problems.
