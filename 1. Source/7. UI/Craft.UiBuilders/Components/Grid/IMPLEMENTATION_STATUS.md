# CraftGrid Improvement - Unified Column Definition

## Status: Partially Complete

### ? Completed Work

1. **Created CraftGridColumn Component** (`Components/Grid/CraftGridColumn.razor` and `.razor.cs`)
   - Unified column component that works for both DataGrid and CardGrid
   - Smart defaults for card visibility (first 5 columns visible)
   - Auto-detection of Id field from property name
   - `ShowInCardView` parameter for explicit control
   - `CardFieldType` parameter for Title/SubTitle designation
   - Ignores table-specific properties (Width, Alignment) in card view
   - Supports all existing features (sorting, searching, templates, formatting)

2. **Updated ICraftCardGrid Interface** (`Components/CardGrid/ICraftCardGrid.cs`)
   - Renamed `ICraftCardGridField` ? `ICraftCardGridColumn`
   - Changed `Fields` property ? `Columns` property  
   - Changed `AddField`/`RemoveField` methods ? `AddColumn`/`RemoveColumn`

3. **Renamed CraftCardGridField ? CraftCardGridColumn**
   - Files renamed
   - Class name updated
   - Comments updated from "field" to "column"
   - Method calls updated (`AddField` ? `AddColumn`)

### ?? Remaining Work (Breaking Changes Required)

#### 4. Update CraftCardGrid Implementation
**Files to Modify:**
- `Components/CardGrid/CraftCardGrid.razor`
- `Components/CardGrid/CraftCardGrid.razor.cs`

**Changes Needed:**
```csharp
// In CraftCardGrid.razor.cs
// Change property name
public List<ICraftCardGridColumn<TEntity>> Columns { get; } = [];  // Was: Fields

// Update methods
public void AddColumn(ICraftCardGridColumn<TEntity> column)  // Was: AddField
{
    ArgumentNullException.ThrowIfNull(column);
    Columns.Add(column);
    StateHasChanged();
}

public void RemoveColumn(ICraftCardGridColumn<TEntity> column)  // Was: RemoveField
{
    ArgumentNullException.ThrowIfNull(column);
    Columns.Remove(column);
    StateHasChanged();
}

// Update all references from Fields ? Columns throughout the file
// Update all references from field ? column in variable names and comments
```

```razor
<!-- In CraftCardGrid.razor -->
<!-- Change cascading value name -->
<CascadingValue Value="(ICraftCardGrid<TEntity>)this" Name="CardGrid" IsFixed="true">
    @ChildContent
</CascadingValue>

<!-- Update all loops -->
<Show When="@Columns.Any()">  <!-- Was: Fields.Any() -->
    @foreach (var column in Columns.Where(c => c.Visible))  <!-- Was: Fields -->
    {
        <!-- column instead of field everywhere -->
    }
</Show>

<!-- Update special field detection -->
@{
    var idColumn = Columns.FirstOrDefault(c => c.FieldType == CardFieldType.Id);
    var titleColumn = Columns.FirstOrDefault(c => c.FieldType == CardFieldType.Title);
    var subTitleColumn = Columns.FirstOrDefault(c => c.FieldType == CardFieldType.SubTitle);
    var regularColumns = Columns.Where(c => c.FieldType == CardFieldType.Field && c.Visible).ToList();
}
```

#### 5. Update CraftGrid Wrapper
**Files to Modify:**
- `Components/Grid/CraftGrid.razor`
- `Components/Grid/CraftGrid.razor.cs`

**Major Changes:**

**Remove old parameters:**
```csharp
// DELETE these parameters
[Parameter] public RenderFragment? Columns { get; set; }
[Parameter] public RenderFragment? Fields { get; set; }
```

**Add new unified parameter:**
```csharp
/// <summary>
/// Child content containing unified column definitions.
/// Columns automatically adapt to table or card view.
/// </summary>
[Parameter] public RenderFragment? ChildContent { get; set; }

/// <summary>
/// Internal list of columns registered by CraftGridColumn components.
/// </summary>
public List<CraftGridColumn<TEntity>> Columns { get; } = [];

/// <summary>
/// Adds a column to the grid.
/// Called by CraftGridColumn during initialization.
/// </summary>
public void AddColumn(CraftGridColumn<TEntity> column)
{
    ArgumentNullException.ThrowIfNull(column);
    Columns.Add(column);
}
```

**Update razor template:**
```razor
<CascadingValue Value="this" Name="Grid" IsFixed="true">
    @ChildContent
</CascadingValue>

<MudHidden Breakpoint="@SwitchBreakpoint" Invert="@(!ShowCardViewOnSmallScreens)">
    @* Table View (DataGrid) *@
    <CraftDataGrid TEntity="TEntity" HttpService="@HttpService" ...>
        @foreach (var column in Columns.Where(c => c.Visible))
        {
            <CraftDataGridColumn TEntity="TEntity"
                                 PropertyExpression="@column.PropertyExpression"
                                 Title="@column.Title"
                                 Sortable="@column.Sortable"
                                 Searchable="@column.Searchable"
                                 Format="@column.Format"
                                 Width="@column.Width"
                                 Alignment="@column.Alignment"
                                 Template="@column.Template"
                                 ... />
        }
    </CraftDataGrid>
</MudHidden>

<MudHidden Breakpoint="@SwitchBreakpoint" Invert="@ShowCardViewOnSmallScreens">
    @* Card View (CardGrid) *@
    <CraftCardGrid TEntity="TEntity" HttpService="@HttpService" ...>
        @foreach (var column in Columns.Where(c => c.IsVisibleInCardView))
        {
            <CraftCardGridColumn TEntity="TEntity"
                                 PropertyExpression="@column.PropertyExpression"
                                 Caption="@column.Title"
                                 FieldType="@column.ResolvedCardFieldType"
                                 Sortable="@column.Sortable"
                                 Searchable="@column.Searchable"
                                 Format="@column.Format"
                                 Template="@column.Template"
                                 ... />
        }
    </CraftCardGrid>
</MudHidden>
```

#### 6. Update Documentation
**Files to Update:**

1. **`Components/CardGrid/README.md`**
   - Replace all `CraftCardGridField` ? `CraftCardGridColumn`
   - Update examples to show new usage

2. **`Components/CardGrid/QUICKSTART.md`**
   - Update quick start to use `CraftCardGridColumn`

3. **`Components/CardGrid/Example.txt`**
   - Update all 8 examples

4. **`Components/Grid/README.md`**
   - Remove `Columns`/`Fields` sections
   - Add unified `CraftGridColumn` documentation
   - Show smart defaults behavior
   - Document `ShowInCardView` parameter

5. **`Components/Grid/Example.txt`**
   - Rewrite all examples to use unified columns
   - Show smart defaults in action
   - Demonstrate `ShowInCardView` usage

6. **Create `Components/Grid/MIGRATION.md`**
   - Document migration path from old to new approach
   - Show before/after examples
   - List breaking changes

#### 7. Build and Verify
**Steps:**
1. Run `dotnet build` on Craft.UiBuilders project
2. Fix any compilation errors
3. Update any missed references
4. Test with a sample page
5. Verify responsive behavior
6. Check smart defaults work correctly

---

## New Usage Example

```razor
@page "/products"
@inject IHttpService<Product> ProductService

<CraftGrid TEntity="Product" HttpService="@ProductService" Title="Products"
           AllowEdit="true" AllowDelete="true"
           OnEdit="@HandleEdit" OnDelete="@HandleDelete">
    
    @* Id column - auto-detected, always included but hidden in UI *@
    <CraftGridColumn PropertyExpression="@(p => p.Id)" />
    
    @* Title for card view *@
    <CraftGridColumn PropertyExpression="@(p => p.Name)" 
                     Title="Product Name"
                     CardFieldType="CardFieldType.Title"
                     Sortable="true"
                     Searchable="true" />
    
    @* Subtitle for card view *@
    <CraftGridColumn PropertyExpression="@(p => p.Category.Name)" 
                     Title="Category"
                     CardFieldType="CardFieldType.SubTitle"
                     Sortable="true" />
    
    @* Regular columns - automatically shown in first 5 *@
    <CraftGridColumn PropertyExpression="@(p => p.Price)" 
                     Title="Price"
                     Format="C2"
                     Sortable="true"
                     Alignment="Alignment.End" />
    
    <CraftGridColumn PropertyExpression="@(p => p.Stock)" 
                     Title="Stock"
                     Sortable="true"
                     Alignment="Alignment.Center" />
    
    @* Only show in table, not card *@
    <CraftGridColumn PropertyExpression="@(p => p.Supplier.Name)" 
                     Title="Supplier"
                     ShowInCardView="false" />
    
    <CraftGridColumn PropertyExpression="@(p => p.LastRestocked)" 
                     Title="Last Restocked"
                     Format="d"
                     ShowInCardView="false" />
</CraftGrid>
```

## Benefits Achieved

? **Reduced Code Duplication** - Define columns once, not twice
? **Smart Defaults** - First 5 columns automatically visible in cards
? **Auto-Detection** - Id field automatically detected
? **Explicit Control** - `ShowInCardView` when needed
? **Type-Safe** - Full compile-time checking
? **Consistent API** - Same patterns as DataGrid/CardGrid
? **Breaking Changes Accepted** - Clean break from old approach
? **Better DX** - Less code to write and maintain

## Breaking Changes Summary

1. **CraftGrid** parameter changes:
   - `Columns` parameter removed
   - `Fields` parameter removed
   - New `ChildContent` parameter for unified columns

2. **Component renamed**:
   - `CraftCardGridField` ? `CraftCardGridColumn`

3. **Interface renamed**:
   - `ICraftCardGridField` ? `ICraftCardGridColumn`

4. **CraftCardGrid** property changes:
   - `Fields` property ? `Columns` property
   - `AddField` method ? `AddColumn` method
   - `RemoveField` method ? `RemoveColumn` method

5. **New component**:
   - `CraftGridColumn` - unified column for CraftGrid wrapper

## Migration Guide

### Before (Old Way)
```razor
<CraftGrid TEntity="Product" HttpService="@ProductService">
    <Columns>
        <CraftDataGridColumn PropertyExpression="@(p => p.Name)" ... />
    </Columns>
    <Fields>
        <CraftCardGridField PropertyExpression="@(p => p.Id)" FieldType="CardFieldType.Id" />
        <CraftCardGridField PropertyExpression="@(p => p.Name)" FieldType="CardFieldType.Title" />
    </Fields>
</CraftGrid>
```

### After (New Way)
```razor
<CraftGrid TEntity="Product" HttpService="@ProductService">
    <CraftGridColumn PropertyExpression="@(p => p.Id)" />  @* Auto-detected as Id *@
    <CraftGridColumn PropertyExpression="@(p => p.Name)" CardFieldType="CardFieldType.Title" ... />
</CraftGrid>
```

## Testing Checklist

- [ ] CraftGrid compiles without errors
- [ ] CraftCardGrid compiles without errors  
- [ ] CraftDataGrid still works
- [ ] Smart defaults work (first 5 columns visible in cards)
- [ ] Id auto-detection works
- [ ] `ShowInCardView` parameter works
- [ ] `CardFieldType` parameter works
- [ ] Template rendering works in both views
- [ ] Sorting works
- [ ] Searching works
- [ ] CRUD operations work
- [ ] Responsive switching works
- [ ] All examples compile and run

---

**Note:** The core architecture is in place. The remaining work involves systematic find-replace operations to update references from Field?Column and connect the unified CraftGridColumn to both child grid components.
