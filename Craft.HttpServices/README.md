# Craft.HttpServices

Craft.HttpServices is a .NET 10 library providing robust, reusable, and extensible HTTP client service abstractions for consuming REST APIs. It standardizes CRUD operations, error handling, and result wrapping for strongly-typed, testable, and maintainable client-side data access.

## Features
- **Generic HTTP Services**: Strongly-typed, generic services for read and change (CRUD) operations over HTTP.
- **Service Result Wrapping**: All operations return `HttpServiceResult<T>`, encapsulating data, errors, and status codes.
- **Abstractions for Testability**: Interfaces for all service types to enable easy mocking and testing.
- **Async & Paginated Operations**: Full support for asynchronous and paginated API calls.
- **Error Handling**: Consistent error handling and propagation from HTTP responses.
- **.NET 10 & C# 13**: Utilizes the latest language and framework features.

## Usage Example
```csharp
// Register HttpClient and service in DI
services.AddHttpClient();
services.AddTransient<IHttpReadService<Product>, HttpReadService<Product>>();

// Inject and use in your application
public class ProductClient
{
    private readonly IHttpReadService<Product> _service;
    public ProductClient(IHttpReadService<Product> service) => _service = service;

    public async Task<IReadOnlyList<Product>?> GetAllProductsAsync()
    {
        var result = await _service.GetAllAsync();
        return result.Data;
    }
}
```

## Key Components
- `HttpServiceBase`: Abstract base for HTTP helpers and result handling.
- `HttpReadService<T, TKey>`, `HttpChangeService<T, ViewT, DataTransferT, TKey>`: Generic HTTP services for CRUD.
- `IHttpService`, `IHttpReadService`, `IHttpChangeService`: Service abstractions for DI and testing.
- `HttpServiceResult<T>`: Standardized result wrapper for all HTTP operations.

## Integration
Craft.HttpServices is designed to be referenced by client applications, Blazor apps, and other .NET projects that consume REST APIs.

## License
This project is licensed under the MIT License. See the `LICENSE` file for details.

---
For more details, review the source code and XML documentation in the project.
