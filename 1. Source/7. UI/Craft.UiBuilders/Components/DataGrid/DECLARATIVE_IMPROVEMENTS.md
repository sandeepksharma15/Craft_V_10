# CraftDataGrid - Declarative UI Improvements

## Summary of Changes

Successfully refactored `CraftDataGrid.razor` to use `Craft.UiBuilders.Generic` components for a more declarative approach.

## Generic Components Used

### 1. **DataLoader** (Already in use)
Used at the root level to handle loading states, errors, and retry functionality.

```razor
<DataLoader IsLoading="@_isLoading" 
            HasError="@_hasError" 
            ErrorMessage="@_errorMessage" 
            OnRetry="@LoadDataAsync">
    @* Content *@
</DataLoader>
```

### 2. **Show** (Enhanced usage)
Replaces simple `@if` statements for cleaner conditional rendering.

**Before:**
```razor
@if (!string.IsNullOrWhiteSpace(Title))
{
    <MudText Typo="Typo.h6">@Title</MudText>
}
```

**After:**
```razor
<Show When="@_showTitle">
    <MudText Typo="Typo.h6">@Title</MudText>
</Show>
```

### 3. **If** (Enhanced usage)
Replaces `@if/@else` blocks for two-way conditional rendering.

**Before:**
```razor
@if (column.Sortable)
{
    <MudTableSortLabel>@column.Title</MudTableSortLabel>
}
else
{
    @column.Title
}
```

**After:**
```razor
<If Condition="@column.Sortable">
    <True>
        <MudTableSortLabel>@column.Title</MudTableSortLabel>
    </True>
    <False>
        @column.Title
    </False>
</If>
```

## Key Improvements

### 1. **Computed Properties for Complex Conditions**

Added private properties in the code-behind to handle complex boolean expressions that can't be directly used in component parameters:

```csharp
/// <summary>
/// Determines if the title should be shown.
/// </summary>
private bool _showTitle => !string.IsNullOrWhiteSpace(Title);

/// <summary>
/// Determines if the add button should be shown.
/// </summary>
private bool _showAddButton => AllowAdd && OnAdd.HasDelegate;

/// <summary>
/// Determines if the export menu should be shown.
/// </summary>
private bool _showExportMenu => AllowExport && OnExport.HasDelegate;

/// <summary>
/// Determines if the actions column should be shown.
/// </summary>
private bool _showActionsColumn => ShowActions && (AllowEdit || AllowDelete || AllowView);

/// <summary>
/// Determines if any items are loaded.
/// </summary>
private bool _hasItems => _items?.Count > 0;
```

### 2. **Consistent Use of Generic Components**

- **Show**: Used for simple conditional rendering throughout
- **If**: Used for two-way conditions (True/False branches)
- **DataLoader**: Wraps the entire component for loading state management

### 3. **Why Not ForEach?**

The `ForEach` Generic component has a different API:
- Uses `Collection` instead of `Items`
- Uses `ItemContent` with `ItemContext` record
- More complex than needed for simple iteration

For simple iteration over collections, `@foreach` loops are:
- ? More readable
- ? Better performance (no extra component overhead)
- ? More familiar to developers
- ? Better IDE support

## Benefits of This Approach

### 1. **Improved Readability**
```razor
<!-- More declarative -->
<Show When="@AllowRefresh">
    <MudIconButton Icon="@Icons.Material.Filled.Refresh" />
</Show>

<!-- vs. imperative -->
@if (AllowRefresh)
{
    <MudIconButton Icon="@Icons.Material.Filled.Refresh" />
}
```

### 2. **Better Separation of Concerns**
Complex conditions are computed in C# code, not embedded in markup:

```csharp
// In code-behind - testable, clear intent
private bool _showActionsColumn => ShowActions && (AllowEdit || AllowDelete || AllowView);
```

```razor
<!-- In markup - clean and simple -->
<Show When="@_showActionsColumn">
    @* Actions column content *@
</Show>
```

### 3. **Consistent Pattern**
All conditional rendering uses the same Generic components, making the codebase more consistent and maintainable.

### 4. **Better for Tooling**
Generic components can provide better IntelliSense and tooling support compared to raw `@if` statements.

## Current Component Usage Map

| Location | Component | Purpose |
|----------|-----------|---------|
| Root | `DataLoader` | Loading states, error handling |
| Toolbar Title | `Show` | Conditional title display |
| Toolbar Search | `Show` | Conditional search box |
| Toolbar Add | `Show` | Conditional add button |
| Toolbar Export | `Show` | Conditional export menu |
| Toolbar Refresh | `Show` | Conditional refresh button |
| Header Actions | `Show` | Conditional actions column |
| Header Columns | `@foreach` + `If` | Column headers with sorting |
| Row Actions | `Show` | Conditional actions cell |
| Row Action Buttons | `Show` (3x) | View, Edit, Delete buttons |
| Row Columns | `@foreach` + `If` | Column cells with templates |
| Pager | `Show` | Conditional pagination |

## Best Practices Applied

### ? Use Computed Properties
Move complex boolean logic to computed properties in code-behind.

### ? Use Show for Simple Conditions
When you just need to show/hide content based on a single condition.

### ? Use If for Two-Way Conditions
When you need True/False branches.

### ? Use @foreach for Iteration
When iterating over collections - it's simpler and more performant.

### ? Document Intent
Add XML comments to computed properties explaining their purpose.

## Performance Considerations

1. **Computed Properties**: These are evaluated on each render, but are simple boolean operations (fast)
2. **Generic Components**: Minimal overhead compared to raw `@if` statements
3. **LINQ Where Clauses**: Used sparingly and only on small collections (Columns)

## Migration Notes

If you have code using the old pattern:

**Old:**
```razor
@if (AllowAdd && OnAdd.HasDelegate)
{
    <MudButton>Add</MudButton>
}
```

**New:**
```csharp
// Add to code-behind
private bool _showAddButton => AllowAdd && OnAdd.HasDelegate;
```

```razor
<!-- Update markup -->
<Show When="@_showAddButton">
    <MudButton>Add</MudButton>
</Show>
```

## Future Enhancements

Consider creating additional Generic components for common patterns:

1. **ConditionalActions** - A specialized component for action buttons
2. **ColumnHeader** - A component wrapping the sortable header logic
3. **ColumnCell** - A component wrapping the template/render logic

## Conclusion

The CraftDataGrid now uses a more declarative approach with `Craft.UiBuilders.Generic` components, making it:
- More readable
- More maintainable
- More consistent with the Craft framework patterns
- Better separated between logic (C#) and presentation (Razor)

The component maintains excellent performance while providing a cleaner, more declarative syntax.
