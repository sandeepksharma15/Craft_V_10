# Craft.Domain

**Version**: 1.1.0  
**Target Framework**: .NET 10  
**Language**: C# 14

Craft.Domain is a comprehensive .NET 10 library providing foundational components for domain-driven design (DDD) and robust exception handling in modern applications. It includes base classes, abstractions, helpers, and a complete standardized exception framework with HTTP status code integration.

---

## ?? Features

### Core Domain Features
- **Base Entity & Model Classes**: Abstract common entity and model behaviors, including identity, concurrency, and soft deletion
- **Domain Abstractions**: Interfaces for entities, models, tenants, users, concurrency, and versioning
- **Equality & Helper Methods**: Utilities for entity equality, default ID checks, and multi-tenancy detection
- **Constants & Validation**: Centralized domain constants and regular expressions for validation

### Exception Framework
- **23 Specialized Exceptions**: Comprehensive HTTP status code coverage (400-504)
- **Exception Factory**: Centralized, consistent exception creation with 50+ factory methods
- **Automatic Middleware Integration**: Works seamlessly with `GlobalExceptionHandler`
- **Validation Support**: Special handling for field-level validation errors
- **Full Test Coverage**: 371 unit tests ensuring reliability

---

## ?? Quick Start

### Installation

```bash
dotnet add reference Craft.Domain
```

### Basic Entity Usage

```csharp
using Craft.Domain;

// Define a domain entity
public class Customer : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

// Equality check
var areEqual = customer1.EntityEquals(customer2);

// Check if ID is default
var isNew = customer.IsDefaultId();
```

### Exception Usage (Traditional)

```csharp
using Craft.Exceptions;

// Throw exceptions directly
throw new NotFoundException("User", userId);
throw new AlreadyExistsException("Product", productCode);
throw new ConcurrencyException("Order", orderId, "v1", "v2");
```

### Exception Usage (Factory Pattern - Recommended)

```csharp
using Craft.Exceptions;

// Use factory for consistency
throw CraftExceptionFactory.NotFound("User", userId);
throw CraftExceptionFactory.AlreadyExists("Product", productCode);
throw CraftExceptionFactory.Concurrency("Order", orderId, "v1", "v2");
```

---

## ?? Core Components

### Base Classes

#### `BaseEntity<TKey>` / `BaseEntity`
Abstract base classes for domain entities with identity management.

```csharp
public class Product : BaseEntity<Guid>
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}

// With default int ID
public class Category : BaseEntity
{
    public string Name { get; set; }
}
```

#### `BaseModel<TKey>` / `BaseModel`
Base classes for DTOs and models.

```csharp
public class ProductDto : BaseModel<Guid>
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

### Interfaces

| Interface | Purpose |
|-----------|---------|
| `IEntity<TKey>` | Defines entity with identity |
| `IModel<TKey>` | Defines model/DTO with identity |
| `IHasTenant` | Multi-tenancy support |
| `IHasUser<TKey>` | User tracking |
| `IHasConcurrency` | Optimistic concurrency |
| `ISoftDelete` | Soft deletion support |
| `IHasVersion` | Versioning support |

### Helpers

#### `EntityHelper`
Static helper methods for entity operations.

```csharp
// Check if ID is default
bool isNew = EntityHelper.IsDefaultId(entity.Id);

// Check entity equality
bool areEqual = EntityHelper.EntityEquals(entity1, entity2);

// Check if entity has tenant
bool hasTenant = EntityHelper.HasTenant(entity);
```

---

## ?? Exception Framework

### Exception Categories

#### ?? Domain Exceptions
Handle domain-specific business logic errors.

| Exception | HTTP Code | Use Case |
|-----------|-----------|----------|
| `NotFoundException` | 404 | Resource not found |
| `AlreadyExistsException` | 422 | Duplicate resource |
| `BadRequestException` | 400 | Invalid request data |
| `ModelValidationException` | 400 | Validation failures |
| `ConflictException` | 409 | Business rule conflict |
| `ConcurrencyException` | 409 | Optimistic concurrency failure |
| `GoneException` | 410 | Permanently deleted resource |
| `PreconditionFailedException` | 412 | ETag/conditional request failure |

#### ?? Security Exceptions
Handle authentication and authorization errors.

| Exception | HTTP Code | Use Case |
|-----------|-----------|----------|
| `UnauthorizedException` | 401 | Missing/invalid authentication |
| `InvalidCredentialsException` | 401 | Wrong username/password |
| `ForbiddenException` | 403 | Insufficient permissions |

#### ??? Server Exceptions
Handle server-side errors.

| Exception | HTTP Code | Use Case |
|-----------|-----------|----------|
| `InternalServerException` | 500 | Internal server error |
| `BadGatewayException` | 502 | Invalid upstream response |
| `ServiceUnavailableException` | 503 | Service temporarily down |
| `GatewayTimeoutException` | 504 | Upstream timeout |

#### ?? Client Exceptions
Handle client-side errors.

| Exception | HTTP Code | Use Case |
|-----------|-----------|----------|
| `FeatureNotImplementedException` | 501 | Feature not yet implemented |
| `TooManyRequestsException` | 429 | Rate limit exceeded |
| `UnsupportedMediaTypeException` | 415 | Invalid content type |
| `PayloadTooLargeException` | 413 | Request payload too large |

#### ?? Infrastructure Exceptions
Handle infrastructure-related errors.

| Exception | HTTP Code | Use Case |
|-----------|-----------|----------|
| `ConfigurationException` | 500 | Configuration errors |
| `DatabaseException` | 500 | Database operation failures |
| `ExternalServiceException` | 502 | External API failures |

---

## ?? Exception Factory

The `CraftExceptionFactory` provides a centralized, consistent way to create exceptions.

### Benefits

? **Consistency** - All exceptions created the same way  
? **Discoverability** - IntelliSense guides you to the right exception  
? **Maintainability** - Single point to update exception creation  
? **Type Safety** - Compile-time checking

### Quick Reference

```csharp
// Domain Exceptions
CraftExceptionFactory.NotFound("User", userId);
CraftExceptionFactory.AlreadyExists("Product", productId);
CraftExceptionFactory.Concurrency("Order", orderId, "v1", "v2");
CraftExceptionFactory.Conflict("Order", "Cannot delete with active shipments");
CraftExceptionFactory.BadRequest("Invalid input", errors);
CraftExceptionFactory.ValidationFailed(validationErrors);
CraftExceptionFactory.Gone("Document", documentId, deletedAt);
CraftExceptionFactory.PreconditionFailed("If-Match", expectedETag, actualETag);

// Security Exceptions
CraftExceptionFactory.Unauthorized("Token expired");
CraftExceptionFactory.Forbidden("Insufficient permissions");
CraftExceptionFactory.InvalidCredentials("Wrong password");

// Infrastructure Exceptions
CraftExceptionFactory.Configuration("ConnectionString", "Value missing");
CraftExceptionFactory.Database("INSERT", "Unique constraint violation");
CraftExceptionFactory.ExternalService("PaymentAPI", 503, "Service unavailable");

// Server Exceptions
CraftExceptionFactory.InternalServer("Critical error");
CraftExceptionFactory.BadGateway("ExternalAPI", "Invalid response");
CraftExceptionFactory.GatewayTimeout("SlowAPI", 30);
CraftExceptionFactory.ServiceUnavailable("Maintenance in progress");

// Client Exceptions
CraftExceptionFactory.NotImplemented("PDF Export", "Coming in v2.0");
CraftExceptionFactory.TooManyRequests(100, "minute");
CraftExceptionFactory.UnsupportedMediaType("text/csv", supportedTypes);
CraftExceptionFactory.PayloadTooLarge(actualSize, maxSize);

// Utility Methods
CraftExceptionFactory.FromStandardException(exception);
CraftExceptionFactory.FromStatusCode(404, "Not found");
```

---

## ?? Usage Examples

### Repository Layer

```csharp
public class UserRepository
{
    public async Task<User> GetByIdAsync(int id)
    {
        var user = await _dbContext.Users.FindAsync(id);
        if (user == null)
            throw CraftExceptionFactory.NotFound("User", id);
        
        if (user.IsDeleted && user.DeletedAt < DateTime.UtcNow.AddDays(-30))
            throw CraftExceptionFactory.Gone("User", id, user.DeletedAt);
        
        return user;
    }

    public async Task<User> CreateAsync(User user)
    {
        try
        {
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
            return user;
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx)
        {
            if (sqlEx.Number == 2627) // Unique constraint
                throw CraftExceptionFactory.AlreadyExists("User", user.Email);
            
            throw CraftExceptionFactory.Database("INSERT", sqlEx.Message);
        }
    }

    public async Task UpdateAsync(User user)
    {
        try
        {
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw CraftExceptionFactory.Concurrency("User", user.Id);
        }
    }
}
```

### Service Layer

```csharp
public class OrderService
{
    public async Task<Order> CreateOrderAsync(CreateOrderDto dto)
    {
        // Validate input
        var validationErrors = ValidateOrder(dto);
        if (validationErrors.Any())
            throw CraftExceptionFactory.ValidationFailed(validationErrors);
        
        // Check for duplicates
        if (await _repository.ExistsAsync(o => o.OrderNumber == dto.OrderNumber))
            throw CraftExceptionFactory.AlreadyExists("Order", dto.OrderNumber);
        
        // Create order
        var order = _mapper.Map<Order>(dto);
        return await _repository.AddAsync(order);
    }
    
    public async Task UpdateOrderAsync(int id, UpdateOrderDto dto, string etag)
    {
        var order = await _repository.GetByIdAsync(id);
        
        // Check ETag for conditional updates
        var currentETag = GenerateETag(order);
        if (currentETag != etag)
            throw CraftExceptionFactory.PreconditionFailed("If-Match", etag, currentETag);
        
        // Check business rules
        if (order.Status == OrderStatus.Shipped)
            throw CraftExceptionFactory.Conflict("Order", "Cannot modify shipped orders");
        
        // Update order
        _mapper.Map(dto, order);
        await _repository.UpdateAsync(order);
    }
}
```

### API Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        // Exceptions automatically handled by GlobalExceptionHandler
        var user = await _service.GetUserAsync(id);
        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserDto dto)
    {
        var user = await _service.CreateUserAsync(dto);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(
        int id, 
        [FromBody] UpdateUserDto dto,
        [FromHeader(Name = "If-Match")] string? etag)
    {
        await _service.UpdateUserAsync(id, dto, etag);
        return NoContent();
    }
}
```

### External Service Integration

```csharp
public class PaymentGatewayClient
{
    private readonly HttpClient _httpClient;

    public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/payments", request);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw CraftExceptionFactory.ExternalService(
                    "PaymentGateway",
                    (int)response.StatusCode,
                    error
                );
            }
            
            return await response.Content.ReadFromJsonAsync<PaymentResult>();
        }
        catch (HttpRequestException ex)
        {
            throw CraftExceptionFactory.ExternalService("PaymentGateway", ex.Message);
        }
        catch (TaskCanceledException)
        {
            throw CraftExceptionFactory.GatewayTimeout("PaymentGateway", 30);
        }
    }
}
```

### File Upload with Validation

```csharp
public class FileUploadService
{
    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

    public async Task<string> UploadFileAsync(IFormFile file)
    {
        // Check file size
        if (file.Length > MaxFileSize)
            throw CraftExceptionFactory.PayloadTooLarge("File", file.Length, MaxFileSize);
        
        // Check content type
        if (!IsValidContentType(file.ContentType))
        {
            var supportedTypes = new[] { "image/jpeg", "image/png", "application/pdf" };
            throw CraftExceptionFactory.UnsupportedMediaType(file.ContentType, supportedTypes);
        }
        
        // Upload file...
        return await UploadToStorageAsync(file);
    }
}
```

---

## ?? Middleware Integration

Exceptions are automatically handled by the `GlobalExceptionHandler` middleware (from `Craft.Middleware`), which converts them to standardized RFC 7807 ProblemDetails responses.

### Example Response

```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.5",
  "title": "Resource not found",
  "status": 404,
  "detail": "Entity \"User\" (123) was not found.",
  "instance": "/api/users/123",
  "errorId": "550e8400-e29b-41d4-a716-446655440000",
  "correlationId": "abc-123-def",
  "timestamp": "2024-12-02T10:30:00Z"
}
```

### Validation Error Response

```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred",
  "status": 400,
  "detail": "Validation failed",
  "instance": "/api/users",
  "errors": {
    "Email": ["Email is required", "Invalid email format"],
    "Age": ["Must be 18 or older"]
  },
  "errorId": "550e8400-e29b-41d4-a716-446655440000"
}
```

---

## ?? Best Practices

### 1. Use the Factory Pattern

```csharp
// ? Good - Consistent and discoverable
throw CraftExceptionFactory.NotFound("User", userId);

// ? Avoid - Direct construction (still works but less consistent)
throw new NotFoundException("User", userId);
```

### 2. Provide Context with Error Lists

```csharp
// ? Good - Detailed, actionable errors
var errors = new List<string>
{
    "Order has 3 active shipments",
    "Complete or cancel shipments first",
    "Last shipment: #12345"
};
throw CraftExceptionFactory.Conflict("Cannot delete order", errors);

// ? Avoid - Vague messages
throw CraftExceptionFactory.Conflict("Cannot delete");
```

### 3. Wrap Underlying Exceptions

```csharp
// ? Good - Preserves stack trace and context
try
{
    await externalService.CallAsync();
}
catch (HttpRequestException ex)
{
    throw CraftExceptionFactory.ExternalService("PaymentAPI", ex.Message);
}

// ? Avoid - Loses original exception
throw CraftExceptionFactory.ExternalService("PaymentAPI", "Error occurred");
```

### 4. Use Specialized Constructors

```csharp
// ? Good - Consistent formatting
throw CraftExceptionFactory.NotFound("User", userId);
// Output: Entity "User" (123) was not found.

// ? Avoid - Manual formatting
throw CraftExceptionFactory.NotFound($"User {userId} not found");
```

---

## ?? Testing

All components include comprehensive unit test coverage.

### Test Statistics

- **Total Tests**: 371
- **Exception Tests**: 281
- **Factory Tests**: 69
- **Domain Tests**: 21
- **Pass Rate**: 100% ?

### Example Test

```csharp
[Fact]
public void NotFound_WithEntityAndKey_CreatesCorrectException()
{
    // Arrange & Act
    var ex = CraftExceptionFactory.NotFound("User", 123);

    // Assert
    Assert.IsType<NotFoundException>(ex);
    Assert.Equal("Entity \"User\" (123) was not found.", ex.Message);
    Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
}
```

---

## ?? Project Structure

```
Craft.Domain/
??? Abstractions/         # Domain interfaces
??? Base/                 # Base entity and model classes
??? Enums/                # Domain enumerations
??? Exceptions/           # Complete exception framework
?   ??? Base/            # Base exception class
?   ??? Domain/          # Domain exceptions
?   ??? Security/        # Security exceptions
?   ??? Server/          # Server exceptions
?   ??? Client/          # Client exceptions
?   ??? Infrastructure/  # Infrastructure exceptions
?   ??? Factories/       # Exception factory
??? Extensions/          # Extension methods
??? Helpers/             # Helper utilities
```

---

## ?? Integration with Other Craft Libraries

- **Craft.Middleware**: Automatic exception handling with `GlobalExceptionHandler`
- **Craft.Repositories**: Use exceptions in repository implementations
- **Craft.Data**: Database-specific exception handling
- **Craft.Security**: Authentication and authorization exceptions
- **Craft.HttpServices**: External service exception handling

---

## ?? Additional Resources

- **XML Documentation**: IntelliSense provides inline documentation for all public members
- **Unit Tests**: Review test files in `Tests/Craft.Domain.Tests/` for usage examples
- **Source Code**: Browse the repository for implementation details

---

## ?? License

This project is licensed under the MIT License. See the `LICENSE` file for details.

---

## ?? Version History

**v1.1.0** (December 2, 2025)
- Added `CraftExceptionFactory` with 50+ factory methods
- Added `PayloadTooLargeException` (413)
- Added `GoneException` (410)
- Added `PreconditionFailedException` (412)
- Added `NotFoundException` (renamed from `EntityNotFoundException`)
- Deprecated `EntityNotFoundException`
- Fixed `UnauthorizedException` default constructor
- 118 new unit tests (371 total)

**v1.0.0**
- Initial release with core domain components
- 20 exception types
- Base entity and model classes
- Domain abstractions and helpers

---

**Target Framework**: .NET 10  
**Language Version**: C# 14  
**Maintained By**: Sandeep K Sharma
