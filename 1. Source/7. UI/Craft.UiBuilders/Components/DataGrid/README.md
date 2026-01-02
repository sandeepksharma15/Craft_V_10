# CraftDataGrid Component

A modern, production-ready data grid component for Blazor applications that integrates seamlessly with the Craft framework's `IHttpService` infrastructure.

## Features

- ? **Automatic Data Loading**: Integrates with `IHttpService<T>` for seamless API data fetching
- ? **Pagination**: Built-in server-side pagination with customizable page sizes
- ? **Sorting**: Column-level sorting support (single column)
- ? **Searching**: Global search across searchable columns
- ? **CRUD Operations**: Built-in support for View, Edit, and Delete actions
- ? **Export**: CSV, Excel, and PDF export capabilities
- ? **Responsive Design**: Built with MudBlazor for modern, responsive UI
- ? **Type-Safe**: Fully generic with strong typing
- ? **Async/Await**: Proper async patterns throughout
- ? **Cancellation Support**: Automatic cancellation of pending requests
- ? **Error Handling**: Comprehensive error handling with user feedback
- ? **Customizable**: Extensive customization options for appearance and behavior

## Basic Usage

```razor
@page "/products"
@inject IHttpService<Product> ProductService

<CraftDataGrid TEntity="Product" 
               HttpService="@ProductService"
               Title="Products"
               AllowAdd="true"
               AllowEdit="true"
               AllowDelete="true"
               OnAdd="@HandleAdd"
               OnEdit="@HandleEdit"
               OnDelete="@HandleDelete">
    
    <CraftDataGridColumn TEntity="Product" 
                         PropertyExpression="@(p => p.Id)" 
                         Title="ID" 
                         Width="80px" />
    
    <CraftDataGridColumn TEntity="Product" 
                         PropertyExpression="@(p => p.Name)" 
                         Title="Product Name"
                         Sortable="true"
                         Searchable="true" />
    
    <CraftDataGridColumn TEntity="Product" 
                         PropertyExpression="@(p => p.Price)" 
                         Title="Price"
                         Format="C2"
                         Sortable="true"
                         Alignment="Alignment.End" />
    
    <CraftDataGridColumn TEntity="Product" 
                         PropertyExpression="@(p => p.CreatedDate)" 
                         Title="Created"
                         Format="d"
                         Sortable="true" />
    
    <CraftDataGridColumn TEntity="Product" 
                         PropertyExpression="@(p => p.IsActive)" 
                         Title="Active"
                         Alignment="Alignment.Center" />
</CraftDataGrid>

@code {
    private void HandleAdd()
    {
        NavigationManager.NavigateTo("/products/new");
    }

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

## Advanced Usage

### Custom Query Builder

```razor
<CraftDataGrid TEntity="Product" 
               HttpService="@ProductService"
               QueryBuilder="@BuildCustomQuery">
    @* Columns *@
</CraftDataGrid>

@code {
    private Query<Product> BuildCustomQuery(Query<Product> query)
    {
        // Add custom filters
        query.Where(p => p.IsActive, true)
             .Where(p => p.Price, ComparisonType.GreaterThan, 0);
        
        // Add default sorting
        query.OrderByDescending(p => p.CreatedDate);
        
        return query;
    }
}
```

### Custom Column Templates

```razor
<CraftDataGridColumn TEntity="Product" 
                     PropertyExpression="@(p => p.Category)">
    <Template Context="product">
        <MudChip Color="Color.Primary" Size="Size.Small">
            @product.Category?.Name
        </MudChip>
    </Template>
</CraftDataGridColumn>

<CraftDataGridColumn TEntity="Product" 
                     PropertyExpression="@(p => p.Status)">
    <Template Context="product">
        <MudIcon Icon="@GetStatusIcon(product.Status)" 
                 Color="@GetStatusColor(product.Status)" />
        @product.Status.ToString()
    </Template>
</CraftDataGridColumn>
```

### Export Functionality

```razor
<CraftDataGrid TEntity="Product" 
               HttpService="@ProductService"
               AllowExport="true"
               OnExport="@HandleExport">
    @* Columns *@
</CraftDataGrid>

@code {
    private async Task HandleExport(ExportFormat format)
    {
        // Get all data without pagination
        var query = new Query<Product>();
        var result = await ProductService.GetAllAsync(query);
        
        if (result.IsSuccess && result.Data != null)
        {
            switch (format)
            {
                case ExportFormat.Csv:
                    await ExportToCsv(result.Data);
                    break;
                case ExportFormat.Excel:
                    await ExportToExcel(result.Data);
                    break;
                case ExportFormat.Pdf:
                    await ExportToPdf(result.Data);
                    break;
            }
        }
    }
}
```

### Data Loaded Callback

```razor
<CraftDataGrid TEntity="Product" 
               HttpService="@ProductService"
               OnDataLoaded="@HandleDataLoaded">
    @* Columns *@
</CraftDataGrid>

@code {
    private void HandleDataLoaded(PageResponse<Product> pageResponse)
    {
        // Perform actions after data is loaded
        Console.WriteLine($"Loaded {pageResponse.Items.Count()} items");
        Console.WriteLine($"Total records: {pageResponse.TotalCount}");
    }
}
```

## Component Parameters

### Data Source Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `HttpService` | `IHttpService<TEntity>` | ? Yes | - | HTTP service for loading data |
| `QueryBuilder` | `Func<Query<TEntity>, Query<TEntity>>` | No | null | Custom query builder function |
| `ChildContent` | `RenderFragment` | No | null | Column definitions |

### Appearance Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Title` | `string` | null | Title displayed in toolbar |
| `Class` | `string` | null | Custom CSS class |
| `Style` | `string` | null | Custom inline styles |
| `Hover` | `bool` | true | Enable row hover effect |
| `Striped` | `bool` | true | Enable striped rows |
| `Dense` | `bool` | false | Enable dense padding |
| `Bordered` | `bool` | false | Enable cell borders |
| `FixedHeader` | `bool` | false | Fix header while scrolling |
| `Height` | `string` | null | Container height (e.g., "400px") |
| `LoadingProgressColor` | `Color` | Primary | Loading bar color |

### Pagination Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `EnablePagination` | `bool` | true | Enable pagination |
| `InitialPageSize` | `int` | 10 | Initial page size |
| `PageSizeOptions` | `int[]` | [10, 25, 50, 100] | Available page sizes |
| `RowsPerPageString` | `string` | "Rows per page:" | Page size label |
| `InfoFormat` | `string` | "{first_item}-{last_item} of {all_items}" | Pagination info format |

### Feature Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ShowActions` | `bool` | true | Show action column |
| `ShowSearch` | `bool` | true | Show search box |
| `SearchPlaceholder` | `string` | "Search..." | Search box placeholder |
| `AllowRefresh` | `bool` | true | Show refresh button |
| `AllowAdd` | `bool` | false | Enable add button |
| `AddButtonText` | `string` | "Add New" | Add button text |
| `AddIcon` | `string` | Add icon | Add button icon |
| `AllowView` | `bool` | false | Enable view action |
| `AllowEdit` | `bool` | false | Enable edit action |
| `AllowDelete` | `bool` | false | Enable delete action |
| `AllowExport` | `bool` | false | Enable export |

### Message Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `NoRecordsMessage` | `string` | "No records to display" | Empty state message |
| `LoadingMessage` | `string` | "Loading..." | Loading message |
| `DeleteConfirmationMessage` | `string` | "Are you sure..." | Delete confirmation |

### Event Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `OnAdd` | `EventCallback` | Invoked when add button is clicked |
| `OnView` | `EventCallback<TEntity>` | Invoked when view action is clicked |
| `OnEdit` | `EventCallback<TEntity>` | Invoked when edit action is clicked |
| `OnDelete` | `EventCallback<TEntity>` | Invoked when delete is confirmed |
| `OnExport` | `EventCallback<ExportFormat>` | Invoked when export is requested |
| `OnDataLoaded` | `EventCallback<PageResponse<TEntity>>` | Invoked after data loads |

## Column Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `Title` | `string` | No | Auto-derived | Column header title |
| `PropertyExpression` | `Expression<Func<TEntity, object>>` | No* | null | Property accessor |
| `Template` | `RenderFragment<TEntity>` | No | null | Custom cell template |
| `Visible` | `bool` | No | true | Column visibility |
| `Sortable` | `bool` | No | false | Enable sorting |
| `Searchable` | `bool` | No | false | Include in search |
| `DefaultSort` | `SortDirection?` | No | null | Default sort direction |
| `SortOrder` | `int` | No | 0 | Sort order for multi-column |
| `Width` | `string` | No | null | Column width |
| `MinWidth` | `string` | No | null | Minimum width |
| `MaxWidth` | `string` | No | null | Maximum width |
| `Alignment` | `Alignment` | No | Start | Text alignment |
| `Format` | `string` | No | null | Format string |

*Required if `Sortable` or `Searchable` is true.

## Public Methods

```csharp
// Refresh the grid data
await grid.RefreshAsync();

// Navigate to a specific page
await grid.GoToPageAsync(3);

// Change page size
await grid.ChangePageSizeAsync(50);
```

## Best Practices

1. **Always provide HttpService**: The component requires `IHttpService<TEntity>` to function.

2. **Use PropertyExpression for sortable/searchable columns**: Columns marked as sortable or searchable must have a `PropertyExpression` defined.

3. **Implement proper error handling**: Use try-catch blocks in event handlers to provide good user feedback.

4. **Optimize search**: Only mark columns as searchable that make sense for text search.

5. **Use custom templates sparingly**: Custom templates can impact performance with large datasets.

6. **Leverage QueryBuilder for complex scenarios**: Use the `QueryBuilder` parameter for complex filtering logic instead of modifying the service.

7. **Handle disposal properly**: The component implements `IAsyncDisposable` and handles cleanup automatically.

## Migration from CDataGrid

If you're migrating from the old `CDataGrid` component:

1. **Change component name**: `<CDataGrid>` ? `<CraftDataGrid>`
2. **Update column syntax**: `<Column>` ? `<CraftDataGridColumn>`
3. **Replace PagedListMethod**: Use `HttpService` parameter instead
4. **Update event handlers**: Use new event callback pattern
5. **Remove delegate-based approach**: Use direct `IHttpService` integration
6. **Update styling**: Replace `Cb*` components with MudBlazor equivalents

## Requirements

- .NET 10
- MudBlazor 9.0.0 or higher
- Craft.QuerySpec
- Craft.HttpServices
- Craft.UiComponents

## License

This component is part of the Craft framework.
