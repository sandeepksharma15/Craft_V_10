# CraftExceptionFactory - Quick Reference Guide

## Overview
`CraftExceptionFactory` provides static factory methods for creating all Craft exceptions with consistent formatting and simplified usage.

**Namespace**: `Craft.Exceptions`

---

## Quick Usage

```csharp
using Craft.Exceptions;

// Instead of this:
throw new NotFoundException("User", userId);

// Use this:
throw CraftExceptionFactory.NotFound("User", userId);
```

---

## All Factory Methods

### ?? Domain Exceptions

```csharp
// NotFoundException (404)
CraftExceptionFactory.NotFound("User", 123);
CraftExceptionFactory.NotFound("Resource not found");
CraftExceptionFactory.NotFound(message, errors);

// AlreadyExistsException (422)
CraftExceptionFactory.AlreadyExists("Product", "SKU-123");
CraftExceptionFactory.AlreadyExists("User already exists");

// ConcurrencyException (409)
CraftExceptionFactory.Concurrency("Order", 456);
CraftExceptionFactory.Concurrency("Order", 456, "v1", "v2");

// ConflictException (409)
CraftExceptionFactory.Conflict("Order", "Cannot delete with active shipments");

// BadRequestException (400)
CraftExceptionFactory.BadRequest("Invalid input", errors);

// ModelValidationException (400)
CraftExceptionFactory.ValidationFailed(validationErrors);
CraftExceptionFactory.ValidationFailed("Validation failed", validationErrors);

// GoneException (410)
CraftExceptionFactory.Gone("Document", 789);
CraftExceptionFactory.Gone("Document", 789, deletedAt);

// PreconditionFailedException (412)
CraftExceptionFactory.PreconditionFailed("If-Match", "expectedETag", "actualETag");
```

### ?? Security Exceptions

```csharp
// UnauthorizedException (401)
CraftExceptionFactory.Unauthorized();
CraftExceptionFactory.Unauthorized("Token expired");

// ForbiddenException (403)
CraftExceptionFactory.Forbidden("Access denied");
CraftExceptionFactory.Forbidden("Insufficient permissions", errors);

// InvalidCredentialsException (401)
CraftExceptionFactory.InvalidCredentials();
CraftExceptionFactory.InvalidCredentials("Wrong password");
```

### ?? Infrastructure Exceptions

```csharp
// ConfigurationException (500)
CraftExceptionFactory.Configuration("Database:ConnectionString", "Missing");

// DatabaseException (500)
CraftExceptionFactory.Database("INSERT", "Unique constraint violation");

// ExternalServiceException (502)
CraftExceptionFactory.ExternalService("PaymentAPI", 503, "Service unavailable");
CraftExceptionFactory.ExternalService("ShippingAPI", "Connection timeout");
```

### ??? Server Exceptions

```csharp
// InternalServerException (500)
CraftExceptionFactory.InternalServer();
CraftExceptionFactory.InternalServer("Critical error", exception);

// BadGatewayException (502)
CraftExceptionFactory.BadGateway("ExternalAPI", "Invalid response");

// GatewayTimeoutException (504)
CraftExceptionFactory.GatewayTimeout("SlowAPI", 30);

// ServiceUnavailableException (503)
CraftExceptionFactory.ServiceUnavailable("Maintenance in progress");
```

### ?? Client Exceptions

```csharp
// FeatureNotImplementedException (501)
CraftExceptionFactory.NotImplemented("PDF Export", "Coming in v2.0");

// TooManyRequestsException (429)
CraftExceptionFactory.TooManyRequests(100, "minute");
CraftExceptionFactory.TooManyRequests(60); // retry after seconds

// UnsupportedMediaTypeException (415)
CraftExceptionFactory.UnsupportedMediaType("text/csv", supportedTypes);

// PayloadTooLargeException (413)
CraftExceptionFactory.PayloadTooLarge(15000000, 10000000);
CraftExceptionFactory.PayloadTooLarge("Image", 5000000, 2000000);
```

---

## Utility Methods

### Convert Standard .NET Exceptions

```csharp
try
{
    // Some operation
}
catch (Exception ex)
{
    throw CraftExceptionFactory.FromStandardException(ex);
}

// Converts:
// - ArgumentNullException ? BadRequestException
// - UnauthorizedAccessException ? UnauthorizedException
// - KeyNotFoundException ? NotFoundException
// - NotImplementedException ? FeatureNotImplementedException
// - TimeoutException ? GatewayTimeoutException
// - Unknown ? InternalServerException
```

### Create from HTTP Status Code

```csharp
var statusCode = 404;
var message = "Resource not found";
var errors = new List<string> { "Additional info" };

throw CraftExceptionFactory.FromStatusCode(statusCode, message, errors);

// Automatically creates the right exception based on status code
```

---

## Common Patterns

### Repository Layer

```csharp
public class UserRepository
{
    public async Task<User> GetByIdAsync(int id)
    {
        var user = await _dbContext.Users.FindAsync(id);
        if (user == null)
            throw CraftExceptionFactory.NotFound("User", id);
        
        return user;
    }
}
```

### Service Layer

```csharp
public class OrderService
{
    public async Task<Order> CreateOrderAsync(CreateOrderDto dto)
    {
        // Validation
        var validationErrors = ValidateOrder(dto);
        if (validationErrors.Any())
            throw CraftExceptionFactory.ValidationFailed(validationErrors);
        
        // Business rule check
        if (await _repository.ExistsAsync(o => o.OrderNumber == dto.OrderNumber))
            throw CraftExceptionFactory.AlreadyExists("Order", dto.OrderNumber);
        
        // Create order
        var order = _mapper.Map<Order>(dto);
        return await _repository.AddAsync(order);
    }
    
    public async Task UpdateOrderAsync(int id, UpdateOrderDto dto, string etag)
    {
        var order = await _repository.GetByIdAsync(id);
        
        // Check ETag
        var currentETag = GenerateETag(order);
        if (currentETag != etag)
            throw CraftExceptionFactory.PreconditionFailed("If-Match", etag, currentETag);
        
        // Check if order is locked
        if (order.Status == OrderStatus.Processing)
            throw CraftExceptionFactory.Conflict("Order", "Order is being processed");
        
        // Update...
    }
}
```

### API Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        // Exception automatically caught by GlobalExceptionHandler
        var user = await _service.GetUserAsync(id);
        return Ok(user);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserDto dto)
    {
        var user = await _service.CreateUserAsync(dto);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }
}
```

### External API Client

```csharp
public class PaymentGatewayClient
{
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

### File Upload Handler

```csharp
public class FileUploadService
{
    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB
    
    public async Task<string> UploadFileAsync(IFormFile file)
    {
        if (file.Length > MaxFileSize)
            throw CraftExceptionFactory.PayloadTooLarge("File", file.Length, MaxFileSize);
        
        if (!IsValidFileType(file.ContentType))
        {
            var supportedTypes = new[] { "image/jpeg", "image/png", "application/pdf" };
            throw CraftExceptionFactory.UnsupportedMediaType(file.ContentType, supportedTypes);
        }
        
        // Upload file...
    }
}
```

### Configuration Validation

```csharp
public class AppSettings
{
    public string ConnectionString { get; set; }
    public string ApiKey { get; set; }
    
    public void Validate()
    {
        if (string.IsNullOrEmpty(ConnectionString))
            throw CraftExceptionFactory.Configuration(
                "Database:ConnectionString",
                "Connection string is required"
            );
        
        if (string.IsNullOrEmpty(ApiKey))
            throw CraftExceptionFactory.Configuration(
                "ExternalAPI:ApiKey",
                "API key is required for external service integration"
            );
    }
}
```

---

## Benefits

? **Consistency**: All exceptions created the same way  
? **Discoverability**: IntelliSense guides you  
? **Maintainability**: Single point to update  
? **Type Safety**: Compile-time checking  
? **Documentation**: Self-documenting code

---

## IntelliSense Support

Start typing `CraftExceptionFactory.` and IntelliSense will show you:
- All available factory methods
- Parameter requirements
- Return types
- XML documentation

---

## Migration from Direct Construction

### Find and Replace Pattern

```csharp
// OLD
new NotFoundException("User", userId)

// NEW
CraftExceptionFactory.NotFound("User", userId)
```

### Gradual Adoption

1. Start using factory for new code
2. Update existing code as you touch it
3. No breaking changes - both ways work

---

## See Also

- [IMPLEMENTATION_SUMMARY.md](./IMPLEMENTATION_SUMMARY.md) - Complete implementation details
- [ANALYSIS_AND_RECOMMENDATIONS.md](./ANALYSIS_AND_RECOMMENDATIONS.md) - Design decisions
- [README.md](./README.md) - Full exception documentation

---

**Version**: 1.1.0  
**Last Updated**: December 2, 2025
