# Component Updates for AutoIncludeNavigationProperties Feature

## 1. CraftDataGrid.razor.cs

### Add Parameter (after line ~269, after AllowExport parameter):

```csharp
/// <summary>
/// Automatically includes all navigation properties when loading data.
/// Uses reflection to discover navigation properties and load them (1 level deep).
/// Default is false.
/// </summary>
[Parameter] public bool AutoIncludeNavigationProperties { get; set; }
```

### Update BuildQuery Method (around line 515):

**Replace:**
```csharp
private Query<TEntity> BuildQuery()
{
    var query = new Query<TEntity>();

    // Set pagination
    query.SetPage(_currentPage, _pageSize);
```

**With:**
```csharp
private Query<TEntity> BuildQuery()
{
    var query = new Query<TEntity>
    {
        AutoIncludeNavigationProperties = AutoIncludeNavigationProperties
    };

    // Set pagination
    query.SetPage(_currentPage, _pageSize);
```

---

## 2. CraftGrid.razor.cs

### Add Parameter (in the Parameters region):

```csharp
/// <summary>
/// Automatically includes all navigation properties when loading data.
/// Uses reflection to discover navigation properties and load them (1 level deep).
/// Default is false.
/// </summary>
[Parameter] public bool AutoIncludeNavigationProperties { get; set; }
```

### Update BuildQuery Method:

**Find the BuildQuery or LoadDataAsync method and update the query creation:**

**Replace:**
```csharp
var query = new Query<TEntity>();
```

**With:**
```csharp
var query = new Query<TEntity>
{
    AutoIncludeNavigationProperties = AutoIncludeNavigationProperties
};
```

---

## 3. CraftCardGrid.razor.cs

### Add Parameter (in the Parameters region):

```csharp
/// <summary>
/// Automatically includes all navigation properties when loading data.
/// Uses reflection to discover navigation properties and load them (1 level deep).
/// Default is false.
/// </summary>
[Parameter] public bool AutoIncludeNavigationProperties { get; set; }
```

### Update Query Creation:

**Find where Query<TEntity> is created and update:**

**Replace:**
```csharp
var query = new Query<TEntity>();
```

**With:**
```csharp
var query = new Query<TEntity>
{
    AutoIncludeNavigationProperties = AutoIncludeNavigationProperties
};
```

---

## 4. PublicHolidaysList.razor (or PublicHolidaysList.razor.cs)

### Update Component Usage:

**In the razor file, update the CraftDataGrid usage:**

```razor
<CraftDataGrid TEntity="PublicHoliday"
               HttpService="@PublicHolidayHttpService"
               Title="Public Holidays"
               AutoIncludeNavigationProperties="true"
               AllowAdd="true"
               AllowEdit="true"
               AllowDelete="true"
               AllowView="true"
               OnAdd="HandleAddAsync"
               OnEdit="HandleEditAsync"
               OnDelete="HandleDeleteAsync"
               OnView="HandleViewAsync">
    <!-- Column definitions -->
</CraftDataGrid>
```

**Note:** The key addition is `AutoIncludeNavigationProperties="true"` which will automatically load the Location navigation property!

---

## Alternative: Use Explicit Include (if you prefer control)

If you don't want to use AutoIncludeNavigationProperties, you can use explicit Include in a QueryBuilder:

```razor
<CraftDataGrid TEntity="PublicHoliday"
               HttpService="@PublicHolidayHttpService"
               Title="Public Holidays"
               QueryBuilder="@BuildPublicHolidayQuery"
               AllowAdd="true"
               AllowEdit="true"
               AllowDelete="true">
    <!-- Column definitions -->
</CraftDataGrid>

@code {
    private Query<PublicHoliday> BuildPublicHolidayQuery(Query<PublicHoliday> query)
    {
        // Explicitly include Location
        query.Include(ph => ph.Location);
        
        // Add any other query customizations here
        return query;
    }
}
```

---

## Summary of Changes

### CraftDataGrid.razor.cs
- ✅ Add `AutoIncludeNavigationProperties` parameter
- ✅ Update `BuildQuery()` to use the parameter

### CraftGrid.razor.cs
- ✅ Add `AutoIncludeNavigationProperties` parameter
- ✅ Update query creation to use the parameter

### CraftCardGrid.razor.cs
- ✅ Add `AutoIncludeNavigationProperties` parameter
- ✅ Update query creation to use the parameter

### PublicHolidaysList.razor
- ✅ Add `AutoIncludeNavigationProperties="true"` to the CraftDataGrid component
- ✅ OR use `QueryBuilder` with explicit Include

---

## Testing

After making these changes:

1. Run the application
2. Navigate to the Public Holidays list
3. Verify that the Location column displays location names (not null)
4. Check the browser console for any errors
5. Optionally, use browser dev tools to inspect the SQL queries (should see LEFT JOIN for Locations table)

---

## Example: Complete PublicHolidaysList.razor

```razor
@page "/master/public-holidays"
@using GccPT.Shared.Models.Masters

<CraftDataGrid TEntity="PublicHoliday"
               HttpService="@PublicHolidayHttpService"
               Title="Public Holidays"
               AutoIncludeNavigationProperties="true"
               AllowAdd="true"
               AllowEdit="true"
               AllowDelete="true"
               AllowView="true"
               ShowSearch="true"
               EnablePagination="true"
               InitialPageSize="10"
               OnAdd="HandleAddAsync"
               OnEdit="HandleEditAsync"
               OnDelete="HandleDeleteAsync"
               OnView="HandleViewAsync">
    
    <Columns>
        <CraftDataGridColumn TEntity="PublicHoliday"
                            PropertyName="@nameof(PublicHoliday.Name)"
                            Title="Holiday Name"
                            Searchable="true"
                            Sortable="true" />
        
        <CraftDataGridColumn TEntity="PublicHoliday"
                            PropertyName="@nameof(PublicHoliday.Date)"
                            Title="Date"
                            Sortable="true"
                            Format="d" />
        
        <CraftDataGridColumn TEntity="PublicHoliday"
                            PropertyName="Location.Name"
                            Title="Location"
                            Searchable="true"
                            Sortable="true" />
        
        <CraftDataGridColumn TEntity="PublicHoliday"
                            PropertyName="@nameof(PublicHoliday.IsActive)"
                            Title="Active"
                            Sortable="true" />
    </Columns>
    
</CraftDataGrid>

@code {
    [Inject] private IHttpService<PublicHoliday>? PublicHolidayHttpService { get; set; }
    [Inject] private NavigationManager? NavigationManager { get; set; }
    
    private async Task HandleAddAsync()
    {
        NavigationManager?.NavigateTo("/master/public-holidays/add");
    }
    
    private async Task HandleEditAsync(PublicHoliday holiday)
    {
        NavigationManager?.NavigateTo($"/master/public-holidays/edit/{holiday.Id}");
    }
    
    private async Task HandleViewAsync(PublicHoliday holiday)
    {
        NavigationManager?.NavigateTo($"/master/public-holidays/view/{holiday.Id}");
    }
    
    private async Task<bool> HandleDeleteAsync(PublicHoliday holiday)
    {
        if (PublicHolidayHttpService is null)
            return false;
            
        var result = await PublicHolidayHttpService.DeleteAsync(holiday.Id);
        return result.IsSuccess;
    }
}
```
