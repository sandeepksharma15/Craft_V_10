# Patch File for CraftDataGrid.razor.cs

## Location
File: `..\Craft_V_10\1. Source\7. UI\Craft.UiBuilders\Components\DataGrid\CraftDataGrid.razor.cs`

## Change 1: Add Parameter (around line 269)

**After this line:**
```csharp
    [Parameter] public bool AllowExport { get; set; }
```

**Add:**
```csharp
    /// <summary>
    /// Automatically includes all navigation properties when loading data.
    /// Uses reflection to discover navigation properties and load them (1 level deep).
    /// Perfect for generic grids where you want to display related data without specifying each Include.
    /// Default is false.
    /// </summary>
    [Parameter] public bool AutoIncludeNavigationProperties { get; set; }
```

## Change 2: Update BuildQuery Method (around line 515)

**Find this method:**
```csharp
    private Query<TEntity> BuildQuery()
    {
        var query = new Query<TEntity>();

        // Set pagination
        query.SetPage(_currentPage, _pageSize);
```

**Replace the first two lines with:**
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

## Complete Updated Method

```csharp
    private Query<TEntity> BuildQuery()
    {
        var query = new Query<TEntity>
        {
            AutoIncludeNavigationProperties = AutoIncludeNavigationProperties
        };

        // Set pagination
        query.SetPage(_currentPage, _pageSize);

        // Apply filters from advanced search
        ApplyFilters(query);

        // Apply sorting from columns
        ApplySorting(query);

        // Apply custom query builder if provided
        if (QueryBuilder is not null)
            query = QueryBuilder(query);

        return query;
    }
```
