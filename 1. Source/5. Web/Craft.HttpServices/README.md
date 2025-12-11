# Craft.HttpServices

**Craft.HttpServices** is a .NET 10 library providing robust, reusable, and extensible HTTP client service abstractions for consuming REST APIs. It standardizes CRUD operations, error handling, and result wrapping for strongly-typed, testable, and maintainable client-side data access.

## Table of Contents

- [Features](#features)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Core Components](#core-components)
  - [HttpServiceBase](#httpservicebase)
  - [HttpReadService](#httpreadservice)
  - [HttpChangeService](#httpchangeservice)
  - [HttpServiceResult](#httpserviceresult)
- [Usage Examples](#usage-examples)
  - [Basic Read Operations](#basic-read-operations)
  - [CRUD Operations](#crud-operations)
  - [Dependency Injection Setup](#dependency-injection-setup)
  - [Error Handling](#error-handling)
  - [Cancellation Token Support](#cancellation-token-support)
- [Advanced Scenarios](#advanced-scenarios)
  - [Custom Key Types](#custom-key-types)
  - [Using Different View and DTO Models](#using-different-view-and-dto-models)
  - [Pagination](#pagination)
- [API Reference](#api-reference)
- [Testing](#testing)
- [License](#license)

## Features

- **Generic HTTP Services**: Strongly-typed, generic services for read and change (CRUD) operations over HTTP
- **Service Result Wrapping**: All operations return `HttpServiceResult<T>`, encapsulating data, success status, errors, and HTTP status codes
- **Abstractions for Testability**: Interfaces for all service types to enable easy mocking and testing
- **Async & Paginated Operations**: Full support for asynchronous and paginated API calls
- **Consistent Error Handling**: Unified error handling and propagation from HTTP responses, supporting both JSON and plain text error messages
- **Automatic DTO Mapping**: Uses Mapster for automatic mapping between view models and DTOs
- **Logging Support**: Integrated logging with Microsoft.Extensions.Logging
- **Cancellation Token Support**: All async methods support cancellation tokens for graceful operation cancellation
- **.NET 10 & C# 13**: Utilizes the latest language and framework features

## Installation

Add a project reference to `Craft.HttpServices`:

```xml
<ItemGroup>
  <ProjectReference Include="path\to\Craft.HttpServices\Craft.HttpServices.csproj" />
</ItemGroup>
```

Or install via NuGet (if published):

```bash
dotnet add package Craft.HttpServices
```

## Quick Start

```csharp
// 1. Define your entity
public class Product : IEntity<int>, IModel<int>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

// 2. Register HttpClient and service in DI
services.AddHttpClient();
services.AddTransient<IHttpReadService<Product, int>>(sp => 
    new HttpReadService<Product, int>(
        new Uri("https://api.example.com/products"),
        sp.GetRequiredService<HttpClient>(),
        sp.GetRequiredService<ILogger<HttpReadService<Product, int>>>()));

// 3. Inject and use in your application
public class ProductService
{
    private readonly IHttpReadService<Product, int> _httpService;

    public ProductService(IHttpReadService<Product, int> httpService)
    {
        _httpService = httpService;
    }

    public async Task<IReadOnlyList<Product>?> GetAllProductsAsync()
    {
        var result = await _httpService.GetAllAsync();
        
        if (result.Success)
            return result.Data;
        
        // Handle errors
        foreach (var error in result.Errors ?? [])
            Console.WriteLine($"Error: {error}");
        
        return null;
    }
}
```

## Core Components

### HttpServiceBase

Abstract base class providing core HTTP operation handling and error management.

**Key Features:**
- Centralized error handling with automatic HTTP status code capture
- Support for both JSON and plain text error responses
- Cancellation token propagation
- Defensive null checking

**Protected Methods:**
- `GetAndParseAsync<TResult>`: Execute GET requests and parse responses
- `SendAndParseAsync<TResult>`: Execute POST/PUT requests and parse responses
- `SendAndParseNoContentAsync`: Execute requests expecting no content response
- `GetAllFromPagedAsync<TItem, TPaged>`: Helper to flatten paged results

### HttpReadService

Provides HTTP-based read operations for entities.

**Interface:** `IHttpReadService<T, TKey>`

**Methods:**
- `GetAllAsync(bool includeDetails, CancellationToken)`: Get all entities
- `GetAsync(TKey id, bool includeDetails, CancellationToken)`: Get entity by ID
- `GetCountAsync(CancellationToken)`: Get total entity count
- `GetPagedListAsync(int page, int pageSize, bool includeDetails, CancellationToken)`: Get paginated entities

**Expected API Endpoints:**
- `GET {apiURL}/{includeDetails}` - Get all entities
- `GET {apiURL}/{id}/{includeDetails}` - Get entity by ID
- `GET {apiURL}/count` - Get count
- `GET {apiURL}/items?page={page}&pageSize={pageSize}&includeDetails={includeDetails}` - Get paged list

### HttpChangeService

Extends `HttpReadService` with create, update, and delete operations.

**Interface:** `IHttpChangeService<T, ViewT, DataTransferT, TKey>`

**Additional Methods:**
- `AddAsync(ViewT model, CancellationToken)`: Add single entity
- `AddRangeAsync(IReadOnlyCollection<ViewT> models, CancellationToken)`: Add multiple entities
- `UpdateAsync(ViewT model, CancellationToken)`: Update single entity
- `UpdateRangeAsync(IReadOnlyCollection<ViewT> models, CancellationToken)`: Update multiple entities
- `DeleteAsync(TKey id, CancellationToken)`: Delete entity by ID
- `DeleteRangeAsync(IReadOnlyCollection<ViewT> models, CancellationToken)`: Delete multiple entities

**Expected API Endpoints:**
- `POST {apiURL}` - Add entity
- `POST {apiURL}/addrange` - Add multiple entities
- `PUT {apiURL}` - Update entity
- `PUT {apiURL}/UpdateRange` - Update multiple entities
- `DELETE {apiURL}/{id}` - Delete entity
- `PUT {apiURL}/RemoveRange` - Delete multiple entities

### HttpServiceResult

Result wrapper for all HTTP service operations.

```csharp
public class HttpServiceResult<T>
{
    public T? Data { get; set; }           // The result data
    public bool Success { get; set; }      // Operation success indicator
    public List<string>? Errors { get; set; }  // List of error messages
    public int? StatusCode { get; set; }   // HTTP status code
}
```

## Usage Examples

### Basic Read Operations

```csharp
// Get all entities
var allResult = await httpReadService.GetAllAsync(includeDetails: true);
if (allResult.Success)
{
    foreach (var item in allResult.Data!)
    {
        Console.WriteLine($"Item: {item.Id}");
    }
}

// Get single entity
var getResult = await httpReadService.GetAsync(42);
if (getResult.Success && getResult.Data != null)
{
    Console.WriteLine($"Found: {getResult.Data.Name}");
}

// Get count
var countResult = await httpReadService.GetCountAsync();
Console.WriteLine($"Total items: {countResult.Data}");

// Get paginated list
var pagedResult = await httpReadService.GetPagedListAsync(page: 1, pageSize: 10);
if (pagedResult.Success && pagedResult.Data != null)
{
    Console.WriteLine($"Page 1 of {pagedResult.Data.TotalPages}");
    Console.WriteLine($"Total items: {pagedResult.Data.TotalCount}");
}
```

### CRUD Operations

```csharp
// Create
var newProduct = new ProductView { Name = "Widget", Price = 9.99m };
var addResult = await httpChangeService.AddAsync(newProduct);
if (addResult.Success)
{
    Console.WriteLine($"Created with ID: {addResult.Data!.Id}");
}

// Update
existingProduct.Price = 12.99m;
var updateResult = await httpChangeService.UpdateAsync(existingProduct);
if (updateResult.Success)
{
    Console.WriteLine("Updated successfully");
}

// Delete
var deleteResult = await httpChangeService.DeleteAsync(42);
if (deleteResult.Success)
{
    Console.WriteLine("Deleted successfully");
}

// Batch operations
var products = new List<ProductView> { product1, product2, product3 };
var batchResult = await httpChangeService.AddRangeAsync(products);
Console.WriteLine($"Added {batchResult.Data?.Count ?? 0} items");
```

### Dependency Injection Setup

```csharp
// In Startup.cs or Program.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddHttpClient();
    
    // Simple registration for read-only service
    services.AddScoped<IHttpReadService<Product, int>>(sp =>
        new HttpReadService<Product, int>(
            new Uri("https://api.example.com/products"),
            sp.GetRequiredService<HttpClient>(),
            sp.GetRequiredService<ILogger<HttpReadService<Product, int>>>()));
    
    // Registration for full CRUD service
    services.AddScoped<IHttpChangeService<Product, ProductView, ProductDto, int>>(sp =>
        new HttpChangeService<Product, ProductView, ProductDto, int>(
            new Uri("https://api.example.com/products"),
            sp.GetRequiredService<HttpClient>(),
            sp.GetRequiredService<ILogger<HttpChangeService<Product, ProductView, ProductDto, int>>>()));
}
```

### Error Handling

```csharp
var result = await httpService.GetAsync(999);

if (!result.Success)
{
    // Check HTTP status code
    if (result.StatusCode == 404)
    {
        Console.WriteLine("Item not found");
    }
    else if (result.StatusCode >= 500)
    {
        Console.WriteLine("Server error occurred");
    }
    
    // Log all errors
    if (result.Errors != null)
    {
        foreach (var error in result.Errors)
        {
            logger.LogError($"Error: {error}");
        }
    }
}
```

### Cancellation Token Support

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

try
{
    var result = await httpService.GetAllAsync(
        includeDetails: true,
        cancellationToken: cts.Token);
    
    if (result.Success)
    {
        ProcessData(result.Data);
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Operation was cancelled");
}
```

## Advanced Scenarios

### Custom Key Types

By default, services use `long` as the key type (defined as `KeyType` in `Usings.cs`). To use custom key types:

```csharp
// Using Guid as key
public class Order : IEntity<Guid>, IModel<Guid>
{
    public Guid Id { get; set; }
    // ... other properties
}

var service = new HttpReadService<Order, Guid>(apiUrl, httpClient, logger);
```

### Using Different View and DTO Models

```csharp
// Entity (database model)
public class Product : IEntity<int>, IModel<int>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
}

// View Model (for UI binding)
public class ProductView : IModel<int>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

// DTO (for API transfer)
public class ProductDto : IModel<int>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

// Service automatically maps between types using Mapster
var service = new HttpChangeService<Product, ProductView, ProductDto, int>(
    apiUrl, httpClient, logger);

// ViewT ? DataTransferT ? sent to API
var result = await service.AddAsync(productView);

// API response ? T (Entity)
Product createdProduct = result.Data;
```

### Pagination

```csharp
public async Task<List<Product>> GetAllProductsPaginatedAsync()
{
    var allProducts = new List<Product>();
    int page = 1;
    int pageSize = 100;
    
    while (true)
    {
        var result = await httpService.GetPagedListAsync(page, pageSize);
        
        if (!result.Success || result.Data == null || !result.Data.Items.Any())
            break;
        
        allProducts.AddRange(result.Data.Items);
        
        if (page >= result.Data.TotalPages)
            break;
        
        page++;
    }
    
    return allProducts;
}
```

## API Reference

### IHttpReadService<T, TKey>

| Method | Parameters | Returns | Description |
|--------|-----------|---------|-------------|
| GetAllAsync | includeDetails, cancellationToken | `HttpServiceResult<IReadOnlyList<T>?>` | Gets all entities |
| GetAsync | id, includeDetails, cancellationToken | `HttpServiceResult<T?>` | Gets entity by ID |
| GetCountAsync | cancellationToken | `HttpServiceResult<long>` | Gets total count |
| GetPagedListAsync | page, pageSize, includeDetails, cancellationToken | `HttpServiceResult<PageResponse<T>?>` | Gets paginated list |

### IHttpChangeService<T, ViewT, DataTransferT, TKey>

Extends `IHttpReadService<T, TKey>` with:

| Method | Parameters | Returns | Description |
|--------|-----------|---------|-------------|
| AddAsync | model, cancellationToken | `HttpServiceResult<T?>` | Adds single entity |
| AddRangeAsync | models, cancellationToken | `HttpServiceResult<List<T>?>` | Adds multiple entities |
| UpdateAsync | model, cancellationToken | `HttpServiceResult<T?>` | Updates single entity |
| UpdateRangeAsync | models, cancellationToken | `HttpServiceResult<List<T>?>` | Updates multiple entities |
| DeleteAsync | id, cancellationToken | `HttpServiceResult<bool>` | Deletes entity by ID |
| DeleteRangeAsync | models, cancellationToken | `HttpServiceResult<bool>` | Deletes multiple entities |

### Simplified Interfaces

For services using default key type (`long`):

- `IHttpReadService<T>` - Equivalent to `IHttpReadService<T, KeyType>`
- `IHttpChangeService<T, ViewT, DataTransferT>` - Equivalent to `IHttpChangeService<T, ViewT, DataTransferT, KeyType>`

## Testing

The library includes comprehensive unit tests covering:

- ? Successful operations with various response types
- ? Error handling (HTTP errors, network errors, malformed responses)
- ? Null argument validation
- ? Cancellation token support
- ? Status code propagation
- ? Edge cases (empty lists, null responses, etc.)
- ? Logging verification

Example test:

```csharp
[Fact]
public async Task GetAsync_ReturnsEntity_WhenResponseIsValid()
{
    // Arrange
    var entity = new TestEntity { Id = 42 };
    var response = new HttpResponseMessage(HttpStatusCode.OK)
    {
        Content = JsonContent.Create(entity)
    };
    var httpClient = CreateMockHttpClient(response);
    var service = new HttpReadService<TestEntity, int>(apiUrl, httpClient, logger);

    // Act
    var result = await service.GetAsync(42);

    // Assert
    Assert.True(result.Success);
    Assert.NotNull(result.Data);
    Assert.Equal(42, result.Data.Id);
}
```

## License

This project is licensed under the MIT License. See the LICENSE file for details.

---

## Related Projects

- **Craft.Core** - Core domain models and utilities
- **Craft.Domain** - Entity and model interfaces
- **Craft.QuerySpec** - Advanced query specifications with HTTP service extensions
- **Craft.Extensions** - Extension methods including HTTP response helpers

## Support

For issues, questions, or contributions, please refer to the main Craft framework repository.
