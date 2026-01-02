# CraftDataGrid - Quick Start Guide

## Installation

The component is part of `Craft.UiBuilders` project. Ensure your project references:
- `Craft.UiBuilders`
- `Craft.QuerySpec`
- `Craft.HttpServices`
- `MudBlazor` (9.0.0 or higher)

## Basic Setup

### 1. Register Services

In your `Program.cs`:

```csharp
builder.Services.AddMudServices();
builder.Services.AddCraftUiBuilders();
```

### 2. Add Using Statements

In your `_Imports.razor`:

```razor
@using Craft.UiBuilders.Components
@using Craft.QuerySpec
@using MudBlazor
```

### 3. Create Your First Grid

```razor
@page "/products"
@inject IHttpService<Product> ProductService

<CraftDataGrid TEntity="Product" HttpService="@ProductService">
    <CraftDataGridColumn TEntity="Product" 
                         PropertyExpression="@(p => p.Id)" 
                         Visible="false" />
    
    <CraftDataGridColumn TEntity="Product" 
                         PropertyExpression="@(p => p.Name)" 
                         Title="Product Name"
                         Sortable="true"
                         Searchable="true" />
    
    <CraftDataGridColumn TEntity="Product" 
                         PropertyExpression="@(p => p.Price)" 
                         Format="C2" />
</CraftDataGrid>
```

## Common Scenarios

### Enable CRUD Operations

```razor
<CraftDataGrid TEntity="Product" 
               HttpService="@ProductService"
               AllowAdd="true"
               AllowEdit="true"
               AllowDelete="true"
               OnAdd="@(() => Nav.NavigateTo("/products/new"))"
               OnEdit="@(p => Nav.NavigateTo($"/products/edit/{p.Id}"))"
               OnDelete="@DeleteProduct">
    @* Columns *@
</CraftDataGrid>

@code {
    [Inject] private NavigationManager Nav { get; set; }
    
    private async Task DeleteProduct(Product product)
    {
        var query = new Query<Product>().Where(p => p.Id, product.Id);
        await ProductService.DeleteAsync(query);
    }
}
```

### Custom Column Template

```razor
<CraftDataGridColumn TEntity="Product" 
                     PropertyExpression="@(p => p.Status)">
    <Template Context="product">
        @if (product.Status == Status.Active)
        {
            <MudChip Color="Color.Success" Size="Size.Small">Active</MudChip>
        }
        else
        {
            <MudChip Color="Color.Default" Size="Size.Small">Inactive</MudChip>
        }
    </Template>
</CraftDataGridColumn>
```

### Apply Default Filters

```razor
<CraftDataGrid TEntity="Product" 
               HttpService="@ProductService"
               QueryBuilder="@CustomQuery">
    @* Columns *@
</CraftDataGrid>

@code {
    private Query<Product> CustomQuery(Query<Product> query)
    {
        // Only show active products
        query.Where(p => p.IsActive, true);
        
        // Default sort by name
        query.OrderBy(p => p.Name);
        
        return query;
    }
}
```

### Export Functionality

```razor
<CraftDataGrid TEntity="Product" 
               HttpService="@ProductService"
               AllowExport="true"
               OnExport="@ExportData">
    @* Columns *@
</CraftDataGrid>

@code {
    private async Task ExportData(ExportFormat format)
    {
        var query = new Query<Product>();
        var result = await ProductService.GetAllAsync(query);
        
        if (result.Success && result.Data != null)
        {
            // Implement export logic based on format
            // Use libraries like CsvHelper, EPPlus, or QuestPDF
        }
    }
}
```

## Common Patterns

### Format Numbers and Dates

```razor
@* Currency *@
<CraftDataGridColumn PropertyExpression="@(p => p.Price)" Format="C2" />

@* Percentage *@
<CraftDataGridColumn PropertyExpression="@(p => p.TaxRate)" Format="P2" />

@* Short Date *@
<CraftDataGridColumn PropertyExpression="@(p => p.CreatedDate)" Format="d" />

@* Long Date *@
<CraftDataGridColumn PropertyExpression="@(p => p.CreatedDate)" Format="D" />

@* DateTime *@
<CraftDataGridColumn PropertyExpression="@(p => p.CreatedDate)" Format="g" />
```

### Align Columns

```razor
@* Right-aligned *@
<CraftDataGridColumn PropertyExpression="@(p => p.Price)" 
                     Alignment="Alignment.End" />

@* Centered *@
<CraftDataGridColumn PropertyExpression="@(p => p.Status)" 
                     Alignment="Alignment.Center" />
```

### Set Column Widths

```razor
@* Fixed width *@
<CraftDataGridColumn PropertyExpression="@(p => p.Id)" Width="80px" />

@* Min/Max width *@
<CraftDataGridColumn PropertyExpression="@(p => p.Description)" 
                     MinWidth="200px"
                     MaxWidth="500px" />
```

### Default Sorting

```razor
@* Single column default sort *@
<CraftDataGridColumn PropertyExpression="@(p => p.Name)" 
                     Sortable="true"
                     DefaultSort="GridSortDirection.Ascending" />

@* Multiple columns with order *@
<CraftDataGridColumn PropertyExpression="@(p => p.Category)" 
                     DefaultSort="GridSortDirection.Ascending"
                     SortOrder="0" />
<CraftDataGridColumn PropertyExpression="@(p => p.Name)" 
                     DefaultSort="GridSortDirection.Ascending"
                     SortOrder="1" />
```

## Troubleshooting

### Data Not Loading
- Verify `HttpService` is injected correctly
- Check if entity implements `IEntity` and `IModel`
- Ensure API endpoint is accessible
- Check browser console for errors

### Columns Not Displaying
- Verify `PropertyExpression` is correctly specified
- Check if column `Visible` property is set to true
- Ensure property exists on entity

### Sorting Not Working
- Set `Sortable="true"` on column
- Ensure `PropertyExpression` is provided
- Check if backend API supports sorting

### Search Not Working
- Set `Searchable="true"` on columns
- Ensure `ShowSearch="true"` (default)
- Verify backend API implements search

## Performance Tips

1. **Use pagination**: Don't disable it for large datasets
2. **Limit searchable columns**: Only mark relevant columns as searchable
3. **Optimize templates**: Avoid complex logic in templates
4. **Use QueryBuilder**: Apply filters at the database level
5. **Cancel pending requests**: Component handles this automatically

## Next Steps

- Read the [full documentation](README.md)
- Check the [example implementation](Example.razor.txt)
- Review the [implementation summary](IMPLEMENTATION_SUMMARY.md)
- Explore advanced features like custom query builders
- Implement export functionality for your use case

## Support

For issues or questions:
1. Check the documentation
2. Review the example code
3. Examine the component source code
4. Check Craft framework documentation
5. Review MudBlazor documentation

Happy coding! ??
