# CraftGrid - Responsive Grid Wrapper

A responsive wrapper component that automatically switches between `CraftDataGrid` (table view) and `CraftCardGrid` (card view) based on screen size. This component reduces code duplication and provides a seamless responsive experience.

## Features

- ?? **Automatic Responsive Switching**: Switches between table and card views based on breakpoint
- ?? **Single Component**: One component instead of managing two separately
- ?? **Configurable Breakpoint**: Choose when to switch views
- ?? **Full Feature Parity**: All DataGrid and CardGrid features available
- ?? **Customizable**: Separate styling for table and card views
- ?? **Bidirectional**: Can switch in either direction (cards on small or large screens)

## Basic Usage

```razor
@page "/products"
@inject IHttpService<Product> ProductService

<CraftGrid TEntity="Product" 
           HttpService="@ProductService"
           Title="Products"
           AllowEdit="true"
           AllowDelete="true"
           OnEdit="@HandleEdit"
           OnDelete="@HandleDelete">
    
    @* Define columns for DataGrid (table view) *@
    <Columns>
        <CraftDataGridColumn TEntity="Product" 
                             PropertyExpression="@(p => p.Name)" 
                             Title="Product Name"
                             Sortable="true"
                             Searchable="true" />
        
        <CraftDataGridColumn TEntity="Product" 
                             PropertyExpression="@(p => p.Price)" 
                             Title="Price"
                             Format="C2"
                             Sortable="true" />
        
        <CraftDataGridColumn TEntity="Product" 
                             PropertyExpression="@(p => p.Stock)" 
                             Title="Stock"
                             Sortable="true" />
    </Columns>
    
    @* Define fields for CardGrid (card view) *@
    <Fields>
        <CraftCardGridField TEntity="Product" 
                            PropertyExpression="@(p => p.Id)" 
                            FieldType="CardFieldType.Id" />
        
        <CraftCardGridField TEntity="Product" 
                            PropertyExpression="@(p => p.Name)" 
                            FieldType="CardFieldType.Title"
                            Sortable="true"
                            Searchable="true" />
        
        <CraftCardGridField TEntity="Product" 
                            PropertyExpression="@(p => p.Price)" 
                            Caption="Price"
                            Format="C2"
                            Sortable="true" />
        
        <CraftCardGridField TEntity="Product" 
                            PropertyExpression="@(p => p.Stock)" 
                            Caption="Stock"
                            Sortable="true" />
    </Fields>
</CraftGrid>

@code {
    private void HandleEdit(Product product)
    {
        NavigationManager.NavigateTo($"/products/edit/{product.Id}");
    }

    private async Task HandleDelete(Product product)
    {
        var query = new Query<Product>().Where(p => p.Id, product.Id);
        await ProductService.DeleteAsync(query);
    }
}
```

## How It Works

### Default Behavior
- **Small screens (xs, sm)**: Shows CardGrid (card view)
- **Larger screens (md, lg, xl)**: Shows DataGrid (table view)

This is controlled by:
- `SwitchBreakpoint="Breakpoint.Md"` (default)
- `ShowCardViewOnSmallScreens="true"` (default)

### Breakpoints

MudBlazor breakpoints:
- `Xs`: Extra small (< 600px)
- `Sm`: Small (600px - 959px)
- `Md`: Medium (960px - 1279px)
- `Lg`: Large (1280px - 1919px)
- `Xl`: Extra large (? 1920px)

## Configuration Examples

### Show Cards on Small Screens, Table on Larger (Default)
```razor
<CraftGrid TEntity="Product" 
           HttpService="@ProductService"
           SwitchBreakpoint="Breakpoint.Md"
           ShowCardViewOnSmallScreens="true">
    <Columns>...</Columns>
    <Fields>...</Fields>
</CraftGrid>
```
**Result**: Cards on xs/sm, Table on md/lg/xl

### Show Table on Small Screens, Cards on Larger
```razor
<CraftGrid TEntity="Product" 
           HttpService="@ProductService"
           SwitchBreakpoint="Breakpoint.Md"
           ShowCardViewOnSmallScreens="false">
    <Columns>...</Columns>
    <Fields>...</Fields>
</CraftGrid>
```
**Result**: Table on xs/sm, Cards on md/lg/xl

### Switch at Large Breakpoint
```razor
<CraftGrid TEntity="Product" 
           HttpService="@ProductService"
           SwitchBreakpoint="Breakpoint.Lg"
           ShowCardViewOnSmallScreens="true">
    <Columns>...</Columns>
    <Fields>...</Fields>
</CraftGrid>
```
**Result**: Cards on xs/sm/md, Table on lg/xl

### Switch at Small Breakpoint
```razor
<CraftGrid TEntity="Product" 
           HttpService="@ProductService"
           SwitchBreakpoint="Breakpoint.Sm"
           ShowCardViewOnSmallScreens="true">
    <Columns>...</Columns>
    <Fields>...</Fields>
</CraftGrid>
```
**Result**: Cards on xs, Table on sm/md/lg/xl

## Component Parameters

### Required Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `HttpService` | `IHttpService<TEntity>` | ? Yes | HTTP service for data loading |
| `Columns` or `Fields` | `RenderFragment` | ? One required | Column/field definitions |

### Responsive Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `SwitchBreakpoint` | `Breakpoint` | Md | Breakpoint at which to switch views |
| `ShowCardViewOnSmallScreens` | `bool` | true | Show cards on small screens (true) or large screens (false) |

### Shared Parameters

All parameters that are common to both DataGrid and CardGrid:

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Title` | `string` | null | Title in toolbar |
| `QueryBuilder` | `Func<Query<TEntity>, Query<TEntity>>` | null | Custom query builder |
| `EnablePagination` | `bool` | true | Enable pagination |
| `InitialPageSize` | `int` | 10 | Initial page size |
| `ShowActions` | `bool` | true | Show action buttons |
| `ShowSearch` | `bool` | true | Show search box |
| `AllowRefresh` | `bool` | true | Show refresh button |
| `AllowAdd` | `bool` | false | Enable add functionality |
| `AllowEdit` | `bool` | false | Enable edit functionality |
| `AllowDelete` | `bool` | false | Enable delete functionality |
| `AllowExport` | `bool` | false | Enable export functionality |

### DataGrid-Specific Parameters

Parameters that only apply to the table view:

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Hover` | `bool` | true | Row hover effect |
| `Striped` | `bool` | true | Striped rows |
| `Dense` | `bool` | false | Dense padding |
| `Bordered` | `bool` | false | Cell borders |
| `FixedHeader` | `bool` | false | Fixed header |
| `Height` | `string` | null | Table height |
| `PageSizeOptions` | `int[]` | [10, 25, 50, 100] | Page size options |

### CardGrid-Specific Parameters

Parameters that only apply to the card view:

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `CardElevation` | `int` | 2 | Card elevation (0-25) |
| `CardSpacing` | `int` | 3 | Spacing between cards |
| `CardXs` | `int` | 12 | Columns on XS screens |
| `CardSm` | `int` | 6 | Columns on SM screens |
| `CardMd` | `int` | 4 | Columns on MD screens |
| `CardLg` | `int` | 3 | Columns on LG screens |
| `CardXl` | `int` | 3 | Columns on XL screens |
| `CardClass` | `string` | null | CSS class for cards |
| `CardStyle` | `string` | null | Inline styles for cards |

### Event Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `OnAdd` | `EventCallback` | Add button clicked |
| `OnView` | `EventCallback<TEntity>` | View action invoked |
| `OnEdit` | `EventCallback<TEntity>` | Edit action invoked |
| `OnDelete` | `EventCallback<TEntity>` | Delete confirmed |
| `OnExport` | `EventCallback<ExportFormat>` | Export requested |
| `OnDataLoaded` | `EventCallback<PageResponse<TEntity>>` | Data loaded |

## Advanced Usage

### Different Column/Field Configurations

You can define different columns for table view and fields for card view to optimize each display:

```razor
<CraftGrid TEntity="Product" HttpService="@ProductService">
    <Columns>
        @* More columns for table view (more space available) *@
        <CraftDataGridColumn PropertyExpression="@(p => p.Name)" Sortable="true" />
        <CraftDataGridColumn PropertyExpression="@(p => p.Category.Name)" Sortable="true" />
        <CraftDataGridColumn PropertyExpression="@(p => p.Price)" Format="C2" />
        <CraftDataGridColumn PropertyExpression="@(p => p.Stock)" />
        <CraftDataGridColumn PropertyExpression="@(p => p.Supplier.Name)" />
        <CraftDataGridColumn PropertyExpression="@(p => p.LastRestocked)" Format="d" />
    </Columns>
    
    <Fields>
        @* Fewer fields for card view (limited space) *@
        <CraftCardGridField PropertyExpression="@(p => p.Id)" FieldType="CardFieldType.Id" />
        <CraftCardGridField PropertyExpression="@(p => p.Name)" FieldType="CardFieldType.Title" />
        <CraftCardGridField PropertyExpression="@(p => p.Category.Name)" FieldType="CardFieldType.SubTitle" />
        <CraftCardGridField PropertyExpression="@(p => p.Price)" Caption="Price" Format="C2" />
        <CraftCardGridField PropertyExpression="@(p => p.Stock)" Caption="Stock" />
    </Fields>
</CraftGrid>
```

### Custom Styling for Each View

```razor
<CraftGrid TEntity="Product" 
           HttpService="@ProductService"
           Class="my-grid"
           Striped="true"
           Dense="true"
           CardElevation="4"
           CardSpacing="4"
           CardClass="my-custom-card">
    <Columns>...</Columns>
    <Fields>...</Fields>
</CraftGrid>
```

### With Custom Query Builder

```razor
<CraftGrid TEntity="Product" 
           HttpService="@ProductService"
           QueryBuilder="@BuildQuery">
    <Columns>...</Columns>
    <Fields>...</Fields>
</CraftGrid>

@code {
    private Query<Product> BuildQuery(Query<Product> query)
    {
        return query
            .Where(p => p.IsActive, true)
            .OrderByDescending(p => p.CreatedDate);
    }
}
```

## Best Practices

1. **Define Both Columns and Fields**: Always provide both to ensure proper display in each view
2. **Optimize for Each View**: Table view can show more columns, card view should show fewer fields
3. **Use Appropriate Field Types**: Use Title, SubTitle, and Id field types in CardGrid for better UX
4. **Test Responsive Behavior**: Check both views at different screen sizes
5. **Consistent Actions**: Ensure CRUD operations work the same in both views
6. **Choose Appropriate Breakpoint**: Consider your content and design when choosing the switch breakpoint

## Comparison: Manual vs CraftGrid

### Without CraftGrid (Manual Approach)
```razor
@* Lots of duplicate parameters *@
<MudHidden Breakpoint="Breakpoint.Md" Invert="true">
    <CraftDataGrid TEntity="Product" HttpService="@ProductService" Title="Products" AllowEdit="true" ...>
        <CraftDataGridColumn ... />
        <CraftDataGridColumn ... />
    </CraftDataGrid>
</MudHidden>

<MudHidden Breakpoint="Breakpoint.Md">
    <CraftCardGrid TEntity="Product" HttpService="@ProductService" Title="Products" AllowEdit="true" ...>
        <CraftCardGridField ... />
        <CraftCardGridField ... />
    </CraftCardGrid>
</MudHidden>
```

### With CraftGrid (Recommended)
```razor
@* Single component, no duplication *@
<CraftGrid TEntity="Product" HttpService="@ProductService" Title="Products" AllowEdit="true" ...>
    <Columns>
        <CraftDataGridColumn ... />
    </Columns>
    <Fields>
        <CraftCardGridField ... />
    </Fields>
</CraftGrid>
```

## See Also

- [CraftDataGrid](../DataGrid/README.md) - Table-based data display
- [CraftCardGrid](../CardGrid/README.md) - Card-based data display
- [MudBlazor Breakpoints](https://mudblazor.com/features/breakpoints)
