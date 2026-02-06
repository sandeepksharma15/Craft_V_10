# Complete Patch Instructions for All Components

## 1. CraftCardGrid.razor.cs

### Add Parameter (after line ~269, after AllowExport):

```csharp
/// <summary>
/// Automatically includes all navigation properties when loading data.
/// Uses reflection to discover navigation properties and load them (1 level deep).
/// Perfect for displaying related data in cards without specifying each Include.
/// Default is false.
/// </summary>
[Parameter] public bool AutoIncludeNavigationProperties { get; set; }
```

### Update BuildQuery Method (around line 575):

**Find:**
```csharp
private Query<TEntity> BuildQuery()
{
    var query = new Query<TEntity>();

    // Set pagination
    query.SetPage(_currentPage, _pageSize);
```

**Replace with:**
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

### Add Parameter (in the Parameters section, around line 85):

**After the QueryBuilder parameter:**
```csharp
/// <summary>
/// Automatically includes all navigation properties when loading data.
/// Uses reflection to discover navigation properties and load them (1 level deep).
/// Default is false.
/// </summary>
[Parameter] public bool AutoIncludeNavigationProperties { get; set; }
```

### Update the component to pass this to both DataGrid and CardGrid

**In the razor file (CraftGrid.razor), find the CraftDataGrid component and add:**
```razor
<CraftDataGrid ... 
               AutoIncludeNavigationProperties="@AutoIncludeNavigationProperties"
               ...>
```

**And find the CraftCardGrid component and add:**
```razor
<CraftCardGrid ... 
               AutoIncludeNavigationProperties="@AutoIncludeNavigationProperties"
               ...>
```

---

## 3. PublicHolidaysList.razor

### Option A: Use AutoIncludeNavigationProperties (Recommended for Generic Approach)

```razor
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
               OnAdd="HandleAddAsync"
               OnEdit="HandleEditAsync"
               OnDelete="HandleDeleteAsync"
               OnView="HandleViewAsync">
    <Columns>
        <!-- Your column definitions -->
    </Columns>
</CraftDataGrid>
```

### Option B: Use QueryBuilder with Explicit Include (More Control)

```razor
<CraftDataGrid TEntity="PublicHoliday"
               HttpService="@PublicHolidayHttpService"
               Title="Public Holidays"
               QueryBuilder="@BuildPublicHolidayQuery"
               AllowAdd="true"
               AllowEdit="true"
               AllowDelete="true"
               AllowView="true">
    <Columns>
        <!-- Your column definitions -->
    </Columns>
</CraftDataGrid>

@code {
    private Query<PublicHoliday> BuildPublicHolidayQuery(Query<PublicHoliday> query)
    {
        // Explicitly include only Location
        query.Include(ph => ph.Location);
        return query;
    }
}
```

---

## Complete Example: PublicHolidaysList.razor

```razor
@page "/masters/public-holidays"
@using GccPT.Shared.Models.Masters
@inject IHttpService<PublicHoliday> PublicHolidayHttpService
@inject NavigationManager NavigationManager

<PageTitle>Public Holidays</PageTitle>

<CraftDataGrid TEntity="PublicHoliday"
               HttpService="@PublicHolidayHttpService"
               Title="Public Holidays"
               AutoIncludeNavigationProperties="true"
               AllowAdd="true"
               AllowEdit="true"
               AllowDelete="true"
               AllowView="true"
               ShowSearch="true"
               AllowRefresh="true"
               EnablePagination="true"
               InitialPageSize="10"
               Striped="true"
               Hover="true"
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
                            Sortable="true">
            <Template Context="holiday">
                @if (holiday.Location != null)
                {
                    <text>@holiday.Location.Name</text>
                }
                else
                {
                    <MudChip Size="Size.Small" Color="Color.Warning">No Location</MudChip>
                }
            </Template>
        </CraftDataGridColumn>
        
        <CraftDataGridColumn TEntity="PublicHoliday"
                            PropertyName="@nameof(PublicHoliday.IsActive)"
                            Title="Status"
                            Sortable="true">
            <Template Context="holiday">
                @if (holiday.IsActive)
                {
                    <MudChip Size="Size.Small" Color="Color.Success">Active</MudChip>
                }
                else
                {
                    <MudChip Size="Size.Small" Color="Color.Default">Inactive</MudChip>
                }
            </Template>
        </CraftDataGridColumn>
    </Columns>
    
</CraftDataGrid>

@code {
    private async Task HandleAddAsync()
    {
        NavigationManager.NavigateTo("/masters/public-holidays/add");
    }
    
    private async Task HandleEditAsync(PublicHoliday holiday)
    {
        NavigationManager.NavigateTo($"/masters/public-holidays/edit/{holiday.Id}");
    }
    
    private async Task HandleViewAsync(PublicHoliday holiday)
    {
        NavigationManager.NavigateTo($"/masters/public-holidays/view/{holiday.Id}");
    }
    
    private async Task<bool> HandleDeleteAsync(PublicHoliday holiday)
    {
        var result = await PublicHolidayHttpService.DeleteAsync(holiday.Id);
        return result.IsSuccess;
    }
}
```

---

## Summary of Changes

### Files to Update:

1. ✅ **CraftDataGrid.razor.cs**
   - Add `AutoIncludeNavigationProperties` parameter
   - Update `BuildQuery()` method

2. ✅ **CraftCardGrid.razor.cs**
   - Add `AutoIncludeNavigationProperties` parameter
   - Update `BuildQuery()` method

3. ✅ **CraftGrid.razor.cs**
   - Add `AutoIncludeNavigationProperties` parameter

4. ✅ **CraftGrid.razor**
   - Pass `AutoIncludeNavigationProperties` to both CraftDataGrid and CraftCardGrid

5. ✅ **PublicHolidaysList.razor**
   - Add `AutoIncludeNavigationProperties="true"` to CraftDataGrid

---

## Testing Steps

1. Make all the changes listed above
2. Build the solution (should compile successfully)
3. Run the application
4. Navigate to the Public Holidays list page
5. Verify that the Location column shows location names (not null or "No Location")
6. Try sorting, filtering, and pagination to ensure everything works
7. Check browser console for any errors
8. (Optional) Use browser dev tools Network tab to inspect the API response and verify Location data is included

---

## Troubleshooting

### If Location is still null:

1. **Check the entity model:**
   ```csharp
   public class PublicHoliday
   {
       public int Id { get; set; }
       public string Name { get; set; }
       public DateTime Date { get; set; }
       
       public int LocationId { get; set; }  // Foreign key must exist
       public Location Location { get; set; }  // Navigation property
   }
   ```

2. **Check the database:**
   - Verify LocationId values are not null or 0
   - Verify corresponding Location records exist

3. **Check the API:**
   - Inspect the network response in browser dev tools
   - Verify the Location object is included in the JSON response

### If you get compilation errors:

1. Make sure you added the parameter in the correct region
2. Check for typos in property names
3. Ensure you're using the exact code from the patches above

---

## Alternative: Manual Include in Each Page

If you prefer more control and don't want to modify the grid components, you can use QueryBuilder:

```csharp
@code {
    private Query<PublicHoliday> CustomQueryBuilder(Query<PublicHoliday> query)
    {
        // Explicitly include Location
        query.Include(ph => ph.Location);
        
        // Optionally include other navigation properties
        // query.Include(ph => ph.CreatedBy);
        
        return query;
    }
}
```

Then use it in the grid:
```razor
<CraftDataGrid TEntity="PublicHoliday"
               QueryBuilder="@CustomQueryBuilder"
               ...>
```

This approach gives you full control over what's included for each specific page!
