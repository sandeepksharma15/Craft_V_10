# Craft.Core

**Version**: 1.0.0  
**Target Framework**: .NET 10  
**Language**: C# 14

Craft.Core is the foundational library of the Craft ecosystem, providing core abstractions, common types, and infrastructure components essential for building robust .NET 10 applications. It serves as the base upon which other Craft libraries are built.

---

## ?? Table of Contents

- [Features](#-features)
- [Installation](#-installation)
- [Core Components](#-core-components)
  - [Result Pattern](#result-pattern)
  - [Pagination](#pagination)
  - [Abstractions](#abstractions)
  - [Dependency Injection Markers](#dependency-injection-markers)
  - [API Requests](#api-requests)
- [Usage Examples](#-usage-examples)
- [Best Practices](#-best-practices)
- [Integration](#-integration-with-other-craft-libraries)
- [API Reference](#-api-reference)

---

## ? Features

### Core Functionality
- **Result Pattern**: Type-safe operation outcomes with `Result<T>` and functional programming support
- **Pagination Support**: Complete pagination infrastructure with `PageInfo` and `PageResponse<T>`
- **Server Responses**: Standardized API response structures with `ServerResponse<T>`
- **Current User/Tenant**: Abstractions for multi-tenant and user context management
- **Dependency Injection Markers**: Interface markers for automatic DI registration
- **API Request Models**: Standardized request types for RESTful APIs

### Design Principles
- **Type-Safe**: Strong typing throughout with nullable reference types
- **Functional Programming**: Support for Map and Bind operations on Result types
- **Performance**: Optimized for .NET 10 with minimal allocations
- **Extensibility**: Open for extension, closed for modification

---

## ?? Installation

```bash
dotnet add reference Craft.Core
```

Or add to your `.csproj`:

```xml
<ProjectReference Include="..\Craft.Core\Craft.Core.csproj" />
```

---

## ?? Core Components

### Result Pattern

The `Result<T>` pattern provides a type-safe way to handle operation outcomes without throwing exceptions.

#### Why Use Result Pattern?

? **Explicit Error Handling** - Success and failure states are explicit in the type system  
? **Performance** - Avoid exception overhead for expected failures  
? **Composability** - Chain operations using `Map` and `Bind`  
? **Type Safety** - Compiler enforces error handling  
? **Testability** - Easy to test both success and failure paths

#### Basic Usage

```csharp
using Craft.Core;

// Non-generic Result (for operations without return values)
public Result DeleteUser(int userId)
{
    if (!UserExists(userId))
        return Result.Failure("User not found");
    
    _repository.Delete(userId);
    return Result.Success();
}

// Generic Result<T> (for operations that return values)
public Result<User> GetUser(int userId)
{
    var user = _repository.FindById(userId);
    
    return user != null
        ? Result<User>.Success(user)
        : Result<User>.Failure("User not found");
}
```

#### Working with Results

```csharp
// Consuming a Result
var result = userService.GetUser(123);

if (result.IsSuccess)
{
    Console.WriteLine($"Found user: {result.Value.Name}");
}
else
{
    Console.WriteLine($"Error: {result.ErrorMessage}");
}

// Multiple errors
var validationResult = ValidateUser(userData);
if (validationResult.IsFailure && validationResult.Errors != null)
{
    foreach (var error in validationResult.Errors)
        Console.WriteLine($"- {error}");
}
```

#### Functional Programming with Result

##### Map - Transform Success Values

```csharp
// Transform the value inside a successful result
var userResult = GetUser(123);
var nameResult = userResult.Map(user => user.Name);
// nameResult is Result<string>

// Chain multiple transformations
var displayResult = GetUser(123)
    .Map(user => user.Name)
    .Map(name => name.ToUpper())
    .Map(name => $"Hello, {name}!");
```

##### Bind - Chain Operations That Return Results

```csharp
// Chain operations where each step can fail
public Result<Order> ProcessOrder(int orderId)
{
    return GetOrder(orderId)
        .Bind(order => ValidateOrder(order))
        .Bind(order => CalculateTotal(order))
        .Bind(order => ApplyDiscount(order))
        .Bind(order => SaveOrder(order));
}

// Each method returns Result<Order>, failures short-circuit
private Result<Order> ValidateOrder(Order order)
{
    if (order.Items.Count == 0)
        return Result<Order>.Failure("Order must have items");
    
    return Result<Order>.Success(order);
}
```

##### Combining Map and Bind

```csharp
public Result<OrderDto> CreateOrderDto(int orderId)
{
    return GetOrder(orderId)                           // Result<Order>
        .Bind(order => ValidateOrder(order))            // Result<Order>
        .Map(order => CalculateTotal(order))            // Result<decimal>
        .Bind(total => ApplyBusinessRules(total))       // Result<decimal>
        .Map(total => new OrderDto { Total = total });  // Result<OrderDto>
}
```

#### Advanced Result Patterns

```csharp
// Repository Pattern
public class UserRepository
{
    public async Task<Result<User>> GetByIdAsync(int id)
    {
        try
        {
            var user = await _dbContext.Users.FindAsync(id);
            return user != null
                ? Result<User>.Success(user)
                : Result<User>.Failure($"User {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user {UserId}", id);
            return Result<User>.Failure($"Database error: {ex.Message}");
        }
    }
}

// Service Layer with Validation
public class OrderService
{
    public async Task<Result<OrderDto>> CreateOrderAsync(CreateOrderRequest request)
    {
        // Validate input
        var validationErrors = ValidateRequest(request);
        if (validationErrors.Any())
            return Result<OrderDto>.Failure(validationErrors);
        
        // Create order
        var order = _mapper.Map<Order>(request);
        var saveResult = await _repository.AddAsync(order);
        
        return saveResult.IsSuccess
            ? Result<OrderDto>.Success(_mapper.Map<OrderDto>(saveResult.Value))
            : Result<OrderDto>.Failure(saveResult.ErrorMessage!);
    }
}

// Controller Integration
[HttpGet("{id}")]
public async Task<IActionResult> GetUser(int id)
{
    var result = await _userService.GetByIdAsync(id);
    
    return result.IsSuccess
        ? Ok(result.Value)
        : NotFound(new { error = result.ErrorMessage });
}

[HttpPost]
public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
{
    var result = await _orderService.CreateOrderAsync(request);
    
    if (result.IsSuccess)
        return CreatedAtAction(nameof(GetOrder), new { id = result.Value!.Id }, result.Value);
    
    return result.ErrorMessage?.Contains("validation", StringComparison.OrdinalIgnoreCase) == true
        ? BadRequest(new { errors = result.Errors })
        : StatusCode(500, new { error = result.ErrorMessage });
}
```

---

### Pagination

Complete pagination support with helper classes for paged data.

#### PageInfo

```csharp
public class PageInfo
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    
    // Computed properties
    public int TotalPages { get; }      // Automatically calculated
    public int From { get; }            // Starting record number
    public int To { get; }              // Ending record number
    public bool HasNextPage { get; }
    public bool HasPreviousPage { get; }
}
```

#### PageResponse<T>

```csharp
public class PageResponse<T>
{
    public IEnumerable<T> Data { get; set; }
    public PageInfo PageInfo { get; set; }
    public IDictionary<string, object>? MetaData { get; set; }
}
```

#### Usage Example

```csharp
// Service layer
public async Task<PageResponse<UserDto>> GetUsersAsync(int page = 1, int pageSize = 10)
{
    var totalCount = await _repository.CountAsync();
    var users = await _repository
        .GetAll()
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
    
    return new PageResponse<UserDto>
    {
        Data = _mapper.Map<List<UserDto>>(users),
        PageInfo = new PageInfo
        {
            CurrentPage = page,
            PageSize = pageSize,
            TotalCount = totalCount
        }
    };
}

// Controller
[HttpGet]
public async Task<ActionResult<PageResponse<UserDto>>> GetUsers(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
{
    var result = await _userService.GetUsersAsync(page, pageSize);
    return Ok(result);
}

// Response example:
{
  "data": [
    { "id": 1, "name": "John Doe" },
    { "id": 2, "name": "Jane Smith" }
  ],
  "pageInfo": {
    "currentPage": 1,
    "pageSize": 10,
    "totalCount": 100,
    "totalPages": 10,
    "from": 1,
    "to": 10,
    "hasNextPage": true,
    "hasPreviousPage": false
  }
}
```

---

### Abstractions

Core interfaces for application infrastructure.

#### ICurrentUser

```csharp
public interface ICurrentUser
{
    string? UserId { get; }
    string? UserName { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    IEnumerable<string> Roles { get; }
    bool IsInRole(string role);
}

// Implementation example
public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public string? UserId => _httpContextAccessor.HttpContext?.User
        .FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User
        .Identity?.IsAuthenticated ?? false;
    
    // ... other implementations
}
```

#### ICurrentTenant

```csharp
public interface ICurrentTenant
{
    string? TenantId { get; }
    string? TenantName { get; }
    bool IsAvailable { get; }
}
```

#### IDbContext

```csharp
public interface IDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    void BeginTransaction();
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
```

---

### Dependency Injection Markers

Interface markers for automatic service registration.

```csharp
// Mark services for automatic DI registration
public interface ITransientDependency { }
public interface IScopedDependency { }
public interface ISingletonDependency { }

// Usage
public class EmailService : IEmailService, ITransientDependency
{
    // Automatically registered as transient
}

public class CacheService : ICacheService, ISingletonDependency
{
    // Automatically registered as singleton
}

// Registration (in Startup or Program.cs)
services.AddServicesWithLifetime(Assembly.GetExecutingAssembly());
```

---

### API Requests

Standardized request models for RESTful APIs.

```csharp
// Base request
public abstract class ApiRequest
{
    public string? ApiType { get; set; }
    public string? Url { get; set; }
}

// GET request
public class GetRequest : ApiRequest
{
    public GetRequest()
    {
        ApiType = "GET";
    }
}

// Paged request
public class GetPagedRequest : GetRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

// Usage
var request = new GetPagedRequest
{
    Url = "/api/users",
    PageNumber = 2,
    PageSize = 20
};
```

---

## ?? Usage Examples

### Complete CRUD Example with Result Pattern

```csharp
// Entity
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

// Repository
public class ProductRepository : IProductRepository
{
    private readonly IDbContext _context;
    
    public async Task<Result<Product>> GetByIdAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        return product != null
            ? Result<Product>.Success(product)
            : Result<Product>.Failure($"Product {id} not found");
    }
    
    public async Task<Result<Product>> CreateAsync(Product product)
    {
        try
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return Result<Product>.Success(product);
        }
        catch (Exception ex)
        {
            return Result<Product>.Failure($"Failed to create product: {ex.Message}");
        }
    }
    
    public async Task<PageResponse<Product>> GetPagedAsync(int page, int pageSize)
    {
        var totalCount = await _context.Products.CountAsync();
        var products = await _context.Products
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return new PageResponse<Product>
        {
            Data = products,
            PageInfo = new PageInfo
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount
            }
        };
    }
}

// Service
public class ProductService : IProductService, IScopedDependency
{
    private readonly IProductRepository _repository;
    private readonly ICurrentUser _currentUser;
    
    public async Task<Result<ProductDto>> GetProductAsync(int id)
    {
        var result = await _repository.GetByIdAsync(id);
        return result.Map(product => new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price
        });
    }
    
    public async Task<Result<ProductDto>> CreateProductAsync(CreateProductRequest request)
    {
        // Validate
        var validationErrors = ValidateProduct(request);
        if (validationErrors.Any())
            return Result<ProductDto>.Failure(validationErrors);
        
        // Check permissions
        if (!_currentUser.IsInRole("ProductManager"))
            return Result<ProductDto>.Failure("Insufficient permissions");
        
        // Create
        var product = new Product
        {
            Name = request.Name,
            Price = request.Price
        };
        
        var createResult = await _repository.CreateAsync(product);
        return createResult.Map(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price
        });
    }
    
    private List<string> ValidateProduct(CreateProductRequest request)
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(request.Name))
            errors.Add("Name is required");
        
        if (request.Price <= 0)
            errors.Add("Price must be greater than zero");
        
        return errors;
    }
}

// Controller
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        var result = await _service.GetProductAsync(id);
        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(new { error = result.ErrorMessage });
    }
    
    [HttpGet]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetProductsAsync(page, pageSize);
        return Ok(result);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        var result = await _service.CreateProductAsync(request);
        
        if (!result.IsSuccess)
        {
            return result.Errors?.Any() == true
                ? BadRequest(new { errors = result.Errors })
                : BadRequest(new { error = result.ErrorMessage });
        }
        
        return CreatedAtAction(
            nameof(GetProduct),
            new { id = result.Value!.Id },
            result.Value);
    }
}
```

---

## ?? Best Practices

### Result Pattern

1. **Use Results for Expected Failures**
   ```csharp
   // ? Good - Expected failure (user not found)
   public Result<User> GetUser(int id)
   {
       var user = _repository.Find(id);
       return user != null
           ? Result<User>.Success(user)
           : Result<User>.Failure("User not found");
   }
   
   // ? Avoid - Don't use for unexpected exceptions
   public Result<User> GetUser(int id)
   {
       try
       {
           var user = _repository.Find(id);
           return Result<User>.Success(user);
       }
       catch (OutOfMemoryException ex)
       {
           return Result<User>.Failure(ex.Message); // Let it throw!
       }
   }
   ```

2. **Provide Meaningful Error Messages**
   ```csharp
   // ? Good
   return Result<Order>.Failure($"Order {orderId} cannot be cancelled: already shipped");
   
   // ? Avoid
   return Result<Order>.Failure("Error");
   ```

3. **Use Error Lists for Multiple Errors**
   ```csharp
   // ? Good
   var errors = new List<string>
   {
       "Name is required",
       "Email format is invalid",
       "Age must be 18 or older"
   };
   return Result<User>.Failure(errors);
   ```

4. **Leverage Map and Bind**
   ```csharp
   // ? Good - Functional composition
   return GetUser(id)
       .Bind(user => ValidateUser(user))
       .Map(user => MapToDto(user));
   
   // ? Avoid - Nested if statements
   var getUserResult = GetUser(id);
   if (getUserResult.IsSuccess)
   {
       var validateResult = ValidateUser(getUserResult.Value);
       if (validateResult.IsSuccess)
       {
           return MapToDto(validateResult.Value);
       }
   }
   ```

### Pagination

1. **Always Validate Page Parameters**
   ```csharp
   public async Task<PageResponse<T>> GetPagedAsync(int page, int pageSize)
   {
       page = Math.Max(1, page);
       pageSize = Math.Clamp(pageSize, 1, 100);
       // ... rest of implementation
   }
   ```

2. **Include Metadata When Helpful**
   ```csharp
   return new PageResponse<UserDto>
   {
       Data = users,
       PageInfo = pageInfo,
       MetaData = new Dictionary<string, object>
       {
           ["activeCount"] = activeUsers,
           ["filterApplied"] = !string.IsNullOrEmpty(filter)
       }
   };
   ```

### Dependency Injection

1. **Use Appropriate Lifetimes**
   ```csharp
   // Stateless services ? Transient or Scoped
   public class EmailService : IEmailService, ITransientDependency { }
   
   // Services with state/caching ? Singleton
   public class ConfigurationService : IConfigService, ISingletonDependency { }
   
   // Services needing per-request state ? Scoped
   public class CurrentUser : ICurrentUser, IScopedDependency { }
   ```

---

## ?? Integration with Other Craft Libraries

Craft.Core serves as the foundation for other Craft libraries:

- **Craft.Domain** - Uses Result pattern and abstractions
- **Craft.Repositories** - Implements pagination and Result pattern
- **Craft.Security** - Implements `ICurrentUser` and `ICurrentTenant`
- **Craft.Middleware** - Uses Result pattern for error handling
- **Craft.Controllers** - Standardizes API responses with PageResponse

---

## ?? API Reference

### Result

| Method/Property | Description |
|----------------|-------------|
| `Success()` | Creates a successful result |
| `Failure(string)` | Creates a failed result with error message |
| `Failure(List<string>)` | Creates a failed result with multiple errors |
| `IsSuccess` | True if operation succeeded |
| `IsFailure` | True if operation failed |
| `ErrorMessage` | Single error message (if any) |
| `Errors` | List of error messages (if any) |

### Result<T>

| Method/Property | Description |
|----------------|-------------|
| `Success(T)` | Creates a successful result with value |
| `Failure(string)` | Creates a failed result with error message |
| `Failure(List<string>)` | Creates a failed result with multiple errors |
| `Value` | The result value (if successful) |
| `Map<TNew>(Func<T, TNew>)` | Transform the value if successful |
| `Bind<TNew>(Func<T, Result<TNew>>)` | Chain operations that return results |

### PageInfo

| Property | Description |
|----------|-------------|
| `CurrentPage` | Current page number (1-based) |
| `PageSize` | Number of items per page |
| `TotalCount` | Total number of items |
| `TotalPages` | Total number of pages (computed) |
| `From` | Starting item number (computed) |
| `To` | Ending item number (computed) |
| `HasNextPage` | True if there's a next page (computed) |
| `HasPreviousPage` | True if there's a previous page (computed) |

---

## ?? Testing

Craft.Core includes comprehensive unit tests. See `Tests/Craft.Core.Tests/` for examples.

```bash
# Run tests
dotnet test Tests/Craft.Core.Tests/Craft.Core.Tests.csproj
```

---

## ?? License

This project is licensed under the MIT License. See the `LICENSE` file for details.

---

## ?? Contributing

Contributions are welcome! Please ensure:
- All public members have XML documentation
- Unit tests cover new functionality
- Code follows existing patterns and conventions

---

**Target Framework**: .NET 10  
**Language Version**: C# 14  
**Maintained By**: Sandeep K Sharma  
**Repository**: https://github.com/sandeepksharma15/Craft_V_10
