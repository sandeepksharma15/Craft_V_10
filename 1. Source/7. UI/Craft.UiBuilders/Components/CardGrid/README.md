# CraftCardGrid Component

A modern, production-ready card grid component for Blazor applications that displays data in card format and integrates seamlessly with the Craft framework's `IHttpService` infrastructure.

## Features

- ?? **Card-Based Layout**: Display data in attractive card format perfect for mobile and responsive designs
- ?? **Automatic Data Loading**: Integrates with `IHttpService<T>` for seamless API data fetching
- ?? **Pagination**: Built-in server-side pagination with customizable page sizes
- ?? **Sorting**: Field-level sorting support with visual indicators
- ?? **Searching**: Global search across searchable fields
- ?? **CRUD Operations**: Built-in support for View, Edit, and Delete actions
- ?? **Export**: CSV, Excel, and PDF export capabilities
- ?? **Responsive Design**: Built with MudBlazor for modern, responsive UI
- ??? **Special Fields**: Support for Id, Title, SubTitle, and regular field designations
- ?? **Type-Safe**: Fully generic with strong typing
- ? **Async/Await**: Proper async patterns throughout
- ?? **Cancellation Support**: Automatic cancellation of pending requests
- ?? **Error Handling**: Comprehensive error handling with user feedback
- ??? **Customizable**: Extensive customization options for appearance and behavior

## Basic Usage

```razor
@page "/products"
@inject IHttpService<Product> ProductService

<CraftCardGrid TEntity="Product" 
               HttpService="@ProductService"
               Title="Products"
               AllowAdd="true"
               AllowEdit="true"
               AllowDelete="true"
               OnAdd="@HandleAdd"
               OnEdit="@HandleEdit"
               OnDelete="@HandleDelete">
    
    @* Id Field - Required for CRUD operations *@
    <CraftCardGridField TEntity="Product" 
                        PropertyExpression="@(p => p.Id)" 
                        FieldType="CardFieldType.Id" />
    
    @* Title Field - Displayed prominently at top *@
    <CraftCardGridField TEntity="Product" 
                        PropertyExpression="@(p => p.Name)" 
                        FieldType="CardFieldType.Title"
                        Sortable="true"
                        Searchable="true" />
    
    @* SubTitle Field - Displayed below title *@
    <CraftCardGridField TEntity="Product" 
                        PropertyExpression="@(p => p.Category.Name)" 
                        Caption="Category"
                        FieldType="CardFieldType.SubTitle"
                        Searchable="true" />
    
    @* Regular Fields - Displayed in card body *@
    <CraftCardGridField TEntity="Product" 
                        PropertyExpression="@(p => p.Price)" 
                        Caption="Price"
                        Format="C2"
                        Sortable="true" />
    
    <CraftCardGridField TEntity="Product" 
                        PropertyExpression="@(p => p.Stock)" 
                        Caption="In Stock"
                        Sortable="true" />
    
    <CraftCardGridField TEntity="Product" 
                        PropertyExpression="@(p => p.CreatedDate)" 
                        Caption="Created"
                        Format="d"
                        Sortable="true" />
</CraftCardGrid>

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

## Field Types

The `CraftCardGrid` supports four types of fields:

### 1. Id Field (CardFieldType.Id)
- Used internally for CRUD operations
- Not displayed in the card (hidden)
- Required if you use Edit or Delete actions

```razor
<CraftCardGridField TEntity="Product" 
                    PropertyExpression="@(p => p.Id)" 
                    FieldType="CardFieldType.Id" />
```

### 2. Title Field (CardFieldType.Title)
- Displayed prominently at the top of the card
- Larger font size (Typo.h6)
- Truncated if too long
- Only one Title field recommended

```razor
<CraftCardGridField TEntity="Product" 
                    PropertyExpression="@(p => p.Name)" 
                    FieldType="CardFieldType.Title"
                    Sortable="true"
                    Searchable="true" />
```

### 3. SubTitle Field (CardFieldType.SubTitle)
- Displayed below the title
- Secondary color
- Truncated if too long
- Only one SubTitle field recommended

```razor
<CraftCardGridField TEntity="Product" 
                    PropertyExpression="@(p => p.Category.Name)" 
                    Caption="Category"
                    FieldType="CardFieldType.SubTitle" />
```

### 4. Regular Field (CardFieldType.Field)
- Default field type
- Displayed in card body with caption and value
- Multiple fields supported

```razor
<CraftCardGridField TEntity="Product" 
                    PropertyExpression="@(p => p.Price)" 
                    Caption="Price"
                    Format="C2" />
```

## Advanced Usage

### Custom Query Builder

```razor
<CraftCardGrid TEntity="Product" 
               HttpService="@ProductService"
               QueryBuilder="@BuildCustomQuery">
    @* Fields *@
</CraftCardGrid>

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

### Custom Field Templates

```razor
<CraftCardGridField TEntity="Product" 
                    PropertyExpression="@(p => p.Status)">
    <Template Context="product">
        <MudChip Color="@GetStatusColor(product.Status)" Size="Size.Small">
            @product.Status.ToString()
        </MudChip>
    </Template>
</CraftCardGridField>

<CraftCardGridField TEntity="Product" 
                    PropertyExpression="@(p => p.Rating)">
    <Template Context="product">
        <MudRating ReadOnly="true" SelectedValue="@product.Rating" />
    </Template>
</CraftCardGridField>
```

### Card Layout Customization

```razor
<CraftCardGrid TEntity="Product" 
               HttpService="@ProductService"
               CardElevation="4"
               CardSpacing="4"
               CardXs="12"
               CardSm="6"
               CardMd="4"
               CardLg="3"
               CardXl="2"
               CardClass="custom-card"
               CardStyle="border-radius: 12px;">
    @* Fields *@
</CraftCardGrid>
```

### Export Functionality

```razor
<CraftCardGrid TEntity="Product" 
               HttpService="@ProductService"
               AllowExport="true"
               OnExport="@HandleExport">
    @* Fields *@
</CraftCardGrid>

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
<CraftCardGrid TEntity="Product" 
               HttpService="@ProductService"
               OnDataLoaded="@HandleDataLoaded">
    @* Fields *@
</CraftCardGrid>

@code {
    private void HandleDataLoaded(PageResponse<Product> pageResponse)
    {
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
| `ChildContent` | `RenderFragment` | No | null | Field definitions |

### Appearance Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Title` | `string` | null | Title displayed in toolbar |
| `Class` | `string` | null | CSS class for container |
| `CardClass` | `string` | null | CSS class for each card |
| `CardStyle` | `string` | null | Inline styles for each card |
| `CardGridClass` | `string` | null | CSS class for card grid |
| `CardElevation` | `int` | 2 | Card elevation (0-25) |
| `CardSpacing` | `int` | 3 | Spacing between cards (0-16) |
| `CardXs` | `int` | 12 | Columns on XS screens |
| `CardSm` | `int` | 6 | Columns on SM screens |
| `CardMd` | `int` | 4 | Columns on MD screens |
| `CardLg` | `int` | 3 | Columns on LG screens |
| `CardXl` | `int` | 3 | Columns on XL screens |

### Pagination Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `EnablePagination` | `bool` | true | Enable/disable pagination |
| `InitialPageSize` | `int` | 12 | Initial page size |
| `PaginationInfoFormat` | `string` | "Showing..." | Pagination info format |

### Feature Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ShowActions` | `bool` | true | Show action buttons in cards |
| `ShowSearch` | `bool` | true | Show search box |
| `SearchPlaceholder` | `string` | "Search..." | Search box placeholder |
| `AllowRefresh` | `bool` | true | Show refresh button |
| `AllowAdd` | `bool` | false | Enable add functionality |
| `AddButtonText` | `string` | "Add New" | Add button text |
| `AddIcon` | `string` | Icons.Material.Filled.Add | Add button icon |
| `AllowView` | `bool` | false | Enable view action |
| `AllowEdit` | `bool` | false | Enable edit action |
| `AllowDelete` | `bool` | false | Enable delete action |
| `AllowExport` | `bool` | false | Enable export functionality |

### Message Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `NoRecordsMessage` | `string` | "No records to display" | No records message |
| `LoadingMessage` | `string` | "Loading..." | Loading message |
| `DeleteConfirmationMessage` | `string` | "Are you sure..." | Delete confirmation message |

### Event Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `OnAdd` | `EventCallback` | Add button clicked |
| `OnView` | `EventCallback<TEntity>` | View button clicked |
| `OnEdit` | `EventCallback<TEntity>` | Edit button clicked |
| `OnDelete` | `EventCallback<TEntity>` | Delete confirmed |
| `OnExport` | `EventCallback<ExportFormat>` | Export requested |
| `OnDataLoaded` | `EventCallback<PageResponse<TEntity>>` | Data loaded successfully |

## Field Parameters

### CraftCardGridField Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `PropertyExpression` | `Expression<Func<TEntity, object>>` | null | Property to display |
| `Caption` | `string` | Auto-derived | Field caption/label |
| `Template` | `RenderFragment<TEntity>` | null | Custom template |
| `FieldType` | `CardFieldType` | Field | Type of field (Id, Title, SubTitle, Field) |
| `Visible` | `bool` | true | Field visibility |
| `Sortable` | `bool` | false | Enable sorting |
| `Searchable` | `bool` | false | Enable searching |
| `DefaultSort` | `GridSortDirection?` | null | Default sort direction |
| `SortOrder` | `int` | 0 | Sort priority |
| `Format` | `string` | null | Value format string |

## Tips and Best Practices

1. **Always define an Id field** when using CRUD operations
2. **Use only one Title and one SubTitle** field for best visual results
3. **Keep regular fields to 3-5** for optimal card size
4. **Use templates** for complex rendering (images, chips, ratings, etc.)
5. **Set appropriate card column breakpoints** for your layout
6. **Use searchable on key fields** (Title, SubTitle) for better UX
7. **Enable sorting on important fields** for user convenience
8. **Customize card spacing and elevation** to match your design system

## Comparison with DataGrid

| Feature | DataGrid | CardGrid |
|---------|----------|----------|
| Layout | Table rows | Cards in grid |
| Best For | Desktop, many columns | Mobile, fewer details |
| Field Types | Columns | Id, Title, SubTitle, Field |
| Sorting | Column headers | Sort menu |
| Actions | Row buttons | Card buttons |
| Responsive | Horizontal scroll | Stacks vertically |

## See Also

- [CraftDataGrid](../DataGrid/README.md) - Table-based data display
- [CraftGrid](../Grid/README.md) - Responsive wrapper (auto-switches between DataGrid and CardGrid)
- [Quick Start Guide](QUICKSTART.md)
- [Example Usage](Example.txt)
