# CraftCardGrid Quick Start Guide

Get up and running with CraftCardGrid in 5 minutes!

## Step 1: Inject Your HTTP Service

```razor
@page "/products"
@inject IHttpService<Product> ProductService
@inject NavigationManager NavigationManager
```

## Step 2: Add the Component

```razor
<CraftCardGrid TEntity="Product" 
               HttpService="@ProductService"
               Title="Products"
               AllowAdd="true"
               AllowEdit="true"
               AllowDelete="true"
               OnAdd="@HandleAdd"
               OnEdit="@HandleEdit"
               OnDelete="@HandleDelete">
    
</CraftCardGrid>
```

## Step 3: Define Your Fields

### Required: Id Field (for CRUD operations)
```razor
<CraftCardGridField TEntity="Product" 
                    PropertyExpression="@(p => p.Id)" 
                    FieldType="CardFieldType.Id" />
```

### Recommended: Title Field
```razor
<CraftCardGridField TEntity="Product" 
                    PropertyExpression="@(p => p.Name)" 
                    FieldType="CardFieldType.Title"
                    Sortable="true"
                    Searchable="true" />
```

### Optional: SubTitle Field
```razor
<CraftCardGridField TEntity="Product" 
                    PropertyExpression="@(p => p.Category.Name)" 
                    Caption="Category"
                    FieldType="CardFieldType.SubTitle" />
```

### Regular Fields
```razor
<CraftCardGridField TEntity="Product" 
                    PropertyExpression="@(p => p.Price)" 
                    Caption="Price"
                    Format="C2"
                    Sortable="true" />

<CraftCardGridField TEntity="Product" 
                    PropertyExpression="@(p => p.Stock)" 
                    Caption="In Stock" />
```

## Step 4: Implement Event Handlers

```razor
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

## Complete Example

```razor
@page "/products"
@inject IHttpService<Product> ProductService
@inject NavigationManager NavigationManager

<CraftCardGrid TEntity="Product" 
               HttpService="@ProductService"
               Title="Products"
               AllowAdd="true"
               AllowEdit="true"
               AllowDelete="true"
               OnAdd="@HandleAdd"
               OnEdit="@HandleEdit"
               OnDelete="@HandleDelete">
    
    <CraftCardGridField TEntity="Product" 
                        PropertyExpression="@(p => p.Id)" 
                        FieldType="CardFieldType.Id" />
    
    <CraftCardGridField TEntity="Product" 
                        PropertyExpression="@(p => p.Name)" 
                        FieldType="CardFieldType.Title"
                        Sortable="true"
                        Searchable="true" />
    
    <CraftCardGridField TEntity="Product" 
                        PropertyExpression="@(p => p.Category.Name)" 
                        Caption="Category"
                        FieldType="CardFieldType.SubTitle"
                        Searchable="true" />
    
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
                        Format="d" />
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

## Next Steps

- Customize card appearance with `CardElevation`, `CardSpacing`, and responsive column settings
- Add custom templates for rich field rendering
- Implement export functionality
- Add custom filters with `QueryBuilder`
- Read the [full documentation](README.md) for all features

## Common Customizations

### Change Card Layout
```razor
<CraftCardGrid CardXs="12" CardSm="6" CardMd="4" CardLg="3" CardXl="2">
```

### Add Custom Styling
```razor
<CraftCardGrid CardElevation="4" 
               CardSpacing="4"
               CardClass="my-custom-card"
               CardStyle="border-radius: 12px;">
```

### Customize Pagination
```razor
<CraftCardGrid InitialPageSize="16"
               PaginationInfoFormat="Page {first_item}-{last_item} of {all_items}">
```

### Custom Field Template
```razor
<CraftCardGridField TEntity="Product" 
                    PropertyExpression="@(p => p.Status)">
    <Template Context="product">
        <MudChip Color="@GetStatusColor(product.Status)" Size="Size.Small">
            @product.Status
        </MudChip>
    </Template>
</CraftCardGridField>
```

That's it! You now have a fully functional card grid with search, sort, pagination, and CRUD operations. ??
