# ConcurrencyException

## Overview

`ConcurrencyException` is thrown when a concurrency conflict occurs during data modification. This typically happens when optimistic concurrency checks fail (e.g., row version mismatch in Entity Framework Core).

**HTTP Status Code**: `409 Conflict`

**Namespace**: `Craft.Exceptions`

## Constructors

### 1. Default Constructor
```csharp
throw new ConcurrencyException();
// Output: A concurrency conflict occurred. The record has been modified by another user.
// Status: 409 Conflict
```

### 2. Custom Message
```csharp
throw new ConcurrencyException("The order has been updated by another user");
// Status: 409 Conflict
```

### 3. With Inner Exception
```csharp
try
{
    await dbContext.SaveChangesAsync();
}
catch (DbUpdateConcurrencyException ex)
{
    throw new ConcurrencyException("Failed to save changes due to concurrency conflict", ex);
}
// Status: 409 Conflict
// Inner exception preserved for logging
```

### 4. With Error List
```csharp
var errors = new List<string> 
{ 
    "Row version mismatch",
    "Record modified by user@example.com at 10:30 AM",
    "Please refresh and try again"
};
throw new ConcurrencyException("Concurrency conflict detected", errors);
// Status: 409 Conflict
// Errors array included in response
```

### 5. Entity-Specific (Entity Name + Key)
```csharp
throw new ConcurrencyException("Order", 12345);
// Output: Concurrency conflict for entity "Order" (12345). The record has been modified by another user.
// Status: 409 Conflict

// Works with different key types
throw new ConcurrencyException("Product", "SKU-ABC-123");
throw new ConcurrencyException("User", Guid.Parse("550e8400-e29b-41d4-a716-446655440000"));
```

### 6. Version-Specific (Entity + Key + Expected/Actual Version)
```csharp
throw new ConcurrencyException("Order", 12345, "v1", "v2");
// Output: Concurrency conflict for entity "Order" (12345). Expected version: v1, Actual version: v2.
// Status: 409 Conflict

// With EF Core row version
var expectedVersion = Convert.ToBase64String(originalRowVersion);
var actualVersion = Convert.ToBase64String(currentRowVersion);
throw new ConcurrencyException("Invoice", invoiceId, expectedVersion, actualVersion);
```

## Usage Examples

### Entity Framework Core Integration

```csharp
public class OrderRepository
{
    private readonly AppDbContext _context;

    public async Task<Result> UpdateOrderAsync(Order order)
    {
        try
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Get the conflicting entity
            var entry = ex.Entries.FirstOrDefault();
            if (entry?.Entity is Order conflictingOrder)
            {
                var originalVersion = entry.OriginalValues.GetValue<byte[]>("RowVersion");
                var currentVersion = entry.CurrentValues.GetValue<byte[]>("RowVersion");

                throw new ConcurrencyException(
                    "Order",
                    conflictingOrder.Id,
                    Convert.ToBase64String(originalVersion),
                    Convert.ToBase64String(currentVersion)
                );
            }

            throw new ConcurrencyException("Failed to update order due to concurrency conflict", ex);
        }
    }
}
```

### Service Layer Implementation

```csharp
public class OrderService
{
    private readonly IOrderRepository _repository;

    public async Task<Result> ProcessOrderAsync(int orderId, string expectedVersion)
    {
        var order = await _repository.GetByIdAsync(orderId);
        if (order == null)
            throw new EntityNotFoundException("Order", orderId);

        // Check version match
        var currentVersion = Convert.ToBase64String(order.RowVersion);
        if (currentVersion != expectedVersion)
        {
            throw new ConcurrencyException(
                "Order", 
                orderId, 
                expectedVersion, 
                currentVersion
            );
        }

        // Process order...
        order.Status = OrderStatus.Processing;
        
        try
        {
            await _repository.UpdateAsync(order);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var errors = new List<string>
            {
                "Another user has modified this order",
                $"Last modified: {order.ModifiedAt:yyyy-MM-dd HH:mm:ss}",
                "Please refresh the page and try again"
            };
            throw new ConcurrencyException(
                "Concurrency conflict while processing order",
                errors
            );
        }
    }
}
```

### API Controller Usage

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrder(
        int id, 
        [FromBody] UpdateOrderDto dto,
        [FromHeader(Name = "If-Match")] string? etag)
    {
        // ConcurrencyException will be automatically caught by GlobalExceptionHandler
        // and returned as 409 Conflict with proper ProblemDetails format
        await _orderService.UpdateOrderAsync(id, dto, etag);
        return NoContent();
    }
}
```

## HTTP Response Format

When caught by `GlobalExceptionHandler`, the exception returns a standardized ProblemDetails response:

### Basic Response
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.10",
  "title": "Conflict",
  "status": 409,
  "detail": "Concurrency conflict for entity \"Order\" (12345). The record has been modified by another user.",
  "instance": "/api/orders/12345",
  "errorId": "550e8400-e29b-41d4-a716-446655440000",
  "correlationId": "abc-123-def",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### With Version Details
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.10",
  "title": "Conflict",
  "status": 409,
  "detail": "Concurrency conflict for entity \"Order\" (12345). Expected version: v1, Actual version: v2.",
  "instance": "/api/orders/12345",
  "errorId": "550e8400-e29b-41d4-a716-446655440000",
  "correlationId": "abc-123-def",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### With Error List
```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.10",
  "title": "Conflict",
  "status": 409,
  "detail": "Concurrency conflict detected",
  "instance": "/api/orders/12345",
  "errors": [
    "Row version mismatch",
    "Record modified by user@example.com at 10:30 AM",
    "Please refresh and try again"
  ],
  "errorId": "550e8400-e29b-41d4-a716-446655440000",
  "correlationId": "abc-123-def",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

## Best Practices

### 1. Use Entity-Specific Constructor
```csharp
// ? Good - Clear entity and key
throw new ConcurrencyException("Order", orderId);

// ? Bad - Generic message
throw new ConcurrencyException("Conflict occurred");
```

### 2. Include Version Information When Available
```csharp
// ? Good - Helpful for debugging
throw new ConcurrencyException("Order", orderId, expectedVersion, actualVersion);

// ?? Acceptable but less informative
throw new ConcurrencyException("Order", orderId);
```

### 3. Provide Detailed Errors for User Actions
```csharp
// ? Good - Actionable information
var errors = new List<string>
{
    "Another user modified this record",
    "Last modified: 2024-01-15 10:30 AM by john.doe@example.com",
    "Please refresh the page and retry your changes"
};
throw new ConcurrencyException("Concurrency conflict", errors);
```

### 4. Wrap EF Core Exceptions
```csharp
// ? Good - Preserves original exception for logging
catch (DbUpdateConcurrencyException ex)
{
    throw new ConcurrencyException("Failed to save changes", ex);
}
```

## Entity Framework Core Setup

To use optimistic concurrency with EF Core:

```csharp
public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; }
    
    [Timestamp]
    public byte[] RowVersion { get; set; }
}

// Or in Fluent API
modelBuilder.Entity<Order>()
    .Property(o => o.RowVersion)
    .IsRowVersion();
```

## Testing

```csharp
[Fact]
public async Task UpdateOrder_WithOutdatedVersion_ThrowsConcurrencyException()
{
    // Arrange
    var order = await _repository.GetByIdAsync(123);
    var originalVersion = Convert.ToBase64String(order.RowVersion);
    
    // Simulate another user updating the order
    await _repository.UpdateOrderAsync(order);
    
    // Act & Assert
    var exception = await Assert.ThrowsAsync<ConcurrencyException>(
        () => _service.UpdateOrderAsync(123, originalVersion)
    );
    
    Assert.Equal(409, (int)exception.StatusCode);
    Assert.Contains("Order", exception.Message);
    Assert.Contains("123", exception.Message);
}
```

## Related Exceptions

- `ConflictException` - General conflict (409)
- `EntityNotFoundException` - Entity not found (404)
- `BadRequestException` - Invalid request (400)

## See Also

- [RFC 9110 Section 15.5.10 - 409 Conflict](https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.10)
- [Optimistic Concurrency in EF Core](https://learn.microsoft.com/en-us/ef/core/saving/concurrency)
