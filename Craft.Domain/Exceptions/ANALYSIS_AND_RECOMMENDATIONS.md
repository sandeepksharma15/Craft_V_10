# Craft.Exceptions - Comprehensive Analysis & Recommendations

**Analysis Date**: December 2, 2025  
**Analyzer**: GitHub Copilot  
**Current Exception Count**: 20 exception classes

---

## Executive Summary

The Craft.Exceptions library is **well-structured and comprehensive** with good HTTP status code coverage. However, there are opportunities for:
1. **Naming inconsistency**: `EntityNotFoundException` should be renamed to `NotFoundException`
2. **Minor redundancy**: `ConflictException` and `ConcurrencyException` overlap (? ACCEPTABLE)
3. **Missing exceptions**: 5 commonly needed exceptions identified
4. **ExceptionFactory**: ? **RECOMMENDED** - Would significantly improve usability and consistency

**Overall Rating**: 8.5/10 (Excellent with room for minor improvements)

---

## Current Exception Inventory

### ? Base Exception (1)
| Exception | Status | Location |
|-----------|--------|----------|
| `CraftException` | ? Perfect | `Base/` |

### ? Domain Exceptions (6)
| Exception | Status | HTTP Code | Recommendation |
|-----------|--------|-----------|----------------|
| `AlreadyExistsException` | ? Good | 422 | Keep |
| `BadRequestException` | ? Good | 400 | Keep |
| `ConcurrencyException` | ? Good | 409 | Keep (specialized) |
| `ConflictException` | ? Good | 409 | Keep (general) |
| `EntityNotFoundException` | ?? **RENAME** | 404 | Rename to `NotFoundException` |
| `ModelValidationException` | ? Excellent | 400 | Keep (unique property) |

### ? Security Exceptions (3)
| Exception | Status | HTTP Code | Recommendation |
|-----------|--------|-----------|----------------|
| `ForbiddenException` | ? Good | 403 | Keep |
| `InvalidCredentialsException` | ? Good | 401 | Keep |
| `UnauthorizedException` | ? Good | 401 | Keep |

### ? Server Exceptions (4)
| Exception | Status | HTTP Code | Recommendation |
|-----------|--------|-----------|----------------|
| `BadGatewayException` | ? Good | 502 | Keep |
| `GatewayTimeoutException` | ? Good | 504 | Keep |
| `InternalServerException` | ? Good | 500 | Keep |
| `ServiceUnavailableException` | ? Good | 503 | Keep |

### ? Client Exceptions (3)
| Exception | Status | HTTP Code | Recommendation |
|-----------|--------|-----------|----------------|
| `FeatureNotImplementedException` | ? Good | 501 | Keep |
| `TooManyRequestsException` | ? Good | 429 | Keep |
| `UnsupportedMediaTypeException` | ? Good | 415 | Keep |

### ? Infrastructure Exceptions (3)
| Exception | Status | HTTP Code | Recommendation |
|-----------|--------|-----------|----------------|
| `ConfigurationException` | ? Good | 500 | Keep |
| `DatabaseException` | ? Good | 500 | Keep |
| `ExternalServiceException` | ? Good | 502 | Keep |

---

## Issues Identified

### ?? Critical Issue: Naming Inconsistency

**Problem**: `EntityNotFoundException` vs standard .NET naming convention

```csharp
// Current (inconsistent with README)
public class EntityNotFoundException : CraftException { }

// README says "NotFoundException" but class is "EntityNotFoundException"
```

**Impact**:
- ? Confusion for developers
- ? Inconsistent with documentation
- ? "Entity" prefix is redundant (all domain exceptions are entity-related)
- ? Tests file is named `NotFoundExceptionTests.cs` but tests `EntityNotFoundException`

**Recommendation**: 
```csharp
// RENAME: EntityNotFoundException ? NotFoundException
public class NotFoundException : CraftException 
{
    public NotFoundException(string name, object key)
        : base($"Entity \"{name}\" ({key}) was not found.", null, HttpStatusCode.NotFound) { }
    // ... other constructors
}
```

**Migration Path**:
1. Create `NotFoundException.cs` with same implementation
2. Mark `EntityNotFoundException` as `[Obsolete("Use NotFoundException instead")]`
3. Update all references
4. Remove `EntityNotFoundException` in next major version

---

### ?? Minor Issue: ConflictException vs ConcurrencyException Overlap

**Analysis**: Both return 409 Conflict status code

```csharp
// ConflictException - General conflict
throw new ConflictException("Order", "Cannot delete order with active shipments");

// ConcurrencyException - Optimistic concurrency conflict
throw new ConcurrencyException("Order", 123, "v1", "v2");
```

**Verdict**: ? **KEEP BOTH** - They serve different purposes:
- `ConflictException`: Business rule conflicts
- `ConcurrencyException`: Optimistic concurrency/version conflicts

**Rationale**:
1. Different use cases with clear semantic distinction
2. `ConcurrencyException` has specialized constructors for version tracking
3. Helps developers understand the type of conflict
4. Common pattern in enterprise applications (EF Core concurrency)

---

## Missing Exceptions

### ? Missing Critical Exceptions (5)

#### 1. **PayloadTooLargeException** (413 Payload Too Large)

**Use Case**: File uploads, request size validation

```csharp
public class PayloadTooLargeException : CraftException
{
    public PayloadTooLargeException()
        : base("The request payload is too large", [], (HttpStatusCode)413) { }

    public PayloadTooLargeException(string message)
        : base(message, [], (HttpStatusCode)413) { }

    public PayloadTooLargeException(string message, Exception innerException)
        : base(message, innerException, (HttpStatusCode)413) { }

    public PayloadTooLargeException(long actualSize, long maxSize)
        : base($"Payload size {actualSize} bytes exceeds maximum allowed size of {maxSize} bytes", 
               [], (HttpStatusCode)413) { }

    public PayloadTooLargeException(string resourceType, long actualSize, long maxSize)
        : base($"{resourceType} size {actualSize} bytes exceeds maximum {maxSize} bytes", 
               [], (HttpStatusCode)413) { }
}

// Usage
if (fileSize > maxFileSize)
    throw new PayloadTooLargeException("File", fileSize, maxFileSize);
```

**Category**: Client  
**Priority**: ?? High  
**Justification**: Common in file upload scenarios, important for security

---

#### 2. **GoneException** (410 Gone)

**Use Case**: Soft-deleted resources, permanently removed endpoints

```csharp
public class GoneException : CraftException
{
    public GoneException()
        : base("The requested resource is no longer available", [], (HttpStatusCode)410) { }

    public GoneException(string message)
        : base(message, [], (HttpStatusCode)410) { }

    public GoneException(string message, Exception innerException)
        : base(message, innerException, (HttpStatusCode)410) { }

    public GoneException(string entityName, object key, DateTime? deletedAt = null)
        : base(deletedAt.HasValue 
            ? $"Entity \"{entityName}\" ({key}) was permanently deleted on {deletedAt:yyyy-MM-dd}"
            : $"Entity \"{entityName}\" ({key}) has been permanently deleted", 
               [], (HttpStatusCode)410) { }
}

// Usage
if (entity.IsDeleted && entity.DeletedAt < DateTime.UtcNow.AddDays(-30))
    throw new GoneException("Order", orderId, entity.DeletedAt);
```

**Category**: Domain  
**Priority**: ?? Medium  
**Justification**: Important for REST API compliance, soft delete scenarios

---

#### 3. **PreconditionFailedException** (412 Precondition Failed)

**Use Case**: ETag validation, conditional requests

```csharp
public class PreconditionFailedException : CraftException
{
    public PreconditionFailedException()
        : base("Precondition failed for the request", [], (HttpStatusCode)412) { }

    public PreconditionFailedException(string message)
        : base(message, [], (HttpStatusCode)412) { }

    public PreconditionFailedException(string message, Exception innerException)
        : base(message, innerException, (HttpStatusCode)412) { }

    public PreconditionFailedException(string condition, string expectedValue, string actualValue)
        : base($"Precondition '{condition}' failed. Expected: {expectedValue}, Actual: {actualValue}", 
               [], (HttpStatusCode)412) { }
}

// Usage
if (request.Headers["If-Match"] != currentETag)
    throw new PreconditionFailedException("If-Match", request.Headers["If-Match"], currentETag);
```

**Category**: Domain  
**Priority**: ?? Medium  
**Justification**: REST API best practices for conditional requests

---

#### 4. **BusinessRuleViolationException** (400 Bad Request or 422 Unprocessable Entity)

**Use Case**: Domain-specific business rule violations

```csharp
public class BusinessRuleViolationException : CraftException
{
    public BusinessRuleViolationException()
        : base("A business rule was violated", [], HttpStatusCode.UnprocessableEntity) { }

    public BusinessRuleViolationException(string message)
        : base(message, [], HttpStatusCode.UnprocessableEntity) { }

    public BusinessRuleViolationException(string message, Exception innerException)
        : base(message, innerException, HttpStatusCode.UnprocessableEntity) { }

    public BusinessRuleViolationException(string message, List<string>? errors = default)
        : base(message, errors, HttpStatusCode.UnprocessableEntity) { }

    public BusinessRuleViolationException(string ruleName, string reason)
        : base($"Business rule '{ruleName}' violated: {reason}", 
               [], HttpStatusCode.UnprocessableEntity) { }
}

// Usage
if (order.Status == OrderStatus.Shipped && order.Items.Count == 0)
    throw new BusinessRuleViolationException(
        "CannotShipEmptyOrder", 
        "Orders must have at least one item before shipping");
```

**Category**: Domain  
**Priority**: ?? Low  
**Justification**: Semantic clarity for business rule failures vs validation failures

---

#### 5. **ResourceLockedException** (423 Locked)

**Use Case**: Resource is locked by another process/user

```csharp
public class ResourceLockedException : CraftException
{
    public ResourceLockedException()
        : base("The resource is currently locked", [], (HttpStatusCode)423) { }

    public ResourceLockedException(string message)
        : base(message, [], (HttpStatusCode)423) { }

    public ResourceLockedException(string message, Exception innerException)
        : base(message, innerException, (HttpStatusCode)423) { }

    public ResourceLockedException(string entityName, object key, string lockedBy)
        : base($"Entity \"{entityName}\" ({key}) is currently locked by {lockedBy}", 
               [], (HttpStatusCode)423) { }

    public ResourceLockedException(string entityName, object key, string lockedBy, DateTime lockExpires)
        : base($"Entity \"{entityName}\" ({key}) is locked by {lockedBy} until {lockExpires:HH:mm:ss}", 
               [], (HttpStatusCode)423) { }
}

// Usage
if (document.LockedBy != null && document.LockedBy != currentUser)
    throw new ResourceLockedException("Document", documentId, document.LockedBy, document.LockExpires);
```

**Category**: Domain  
**Priority**: ?? Low  
**Justification**: Useful for collaborative editing, document management systems

---

## ExceptionFactory - STRONGLY RECOMMENDED ?

### Why ExceptionFactory is Beneficial

#### 1. **Consistency Enforcement**
```csharp
// Without Factory (inconsistent)
throw new NotFoundException("User", userId);
throw new EntityNotFoundException($"User {userId} was not found"); // Oops, wrong format

// With Factory (consistent)
throw CraftExceptionFactory.NotFound("User", userId);
```

#### 2. **Centralized Business Logic**
```csharp
public static class CraftExceptionFactory
{
    public static NotFoundException NotFound(string entityName, object key)
    {
        // Log to telemetry
        _telemetry.TrackEvent("EntityNotFound", new { entityName, key });
        
        // Return exception
        return new NotFoundException(entityName, key);
    }
}
```

#### 3. **Easier Testing**
```csharp
// Test becomes simpler
var exception = CraftExceptionFactory.NotFound("User", 123);
Assert.Equal("Entity \"User\" (123) was not found.", exception.Message);
```

#### 4. **Discoverability**
- IntelliSense shows all available exception creation methods
- Developers don't need to remember constructor signatures
- Reduces cognitive load

### Recommended ExceptionFactory Implementation

**Location**: `Craft.Domain/Exceptions/Factories/CraftExceptionFactory.cs`

```csharp
namespace Craft.Exceptions;

/// <summary>
/// Factory for creating Craft exceptions with consistent formatting and optional telemetry integration.
/// </summary>
public static class CraftExceptionFactory
{
    // Domain Exceptions
    
    public static NotFoundException NotFound(string entityName, object key)
        => new(entityName, key);

    public static NotFoundException NotFound(string message)
        => new(message);

    public static AlreadyExistsException AlreadyExists(string entityName, object key)
        => new(entityName, key);

    public static AlreadyExistsException AlreadyExists(string message)
        => new(message);

    public static ConcurrencyException Concurrency(string entityName, object key)
        => new(entityName, key);

    public static ConcurrencyException Concurrency(string entityName, object key, 
        string expectedVersion, string actualVersion)
        => new(entityName, key, expectedVersion, actualVersion);

    public static ConflictException Conflict(string resourceName, string reason)
        => new(resourceName, reason);

    public static BadRequestException BadRequest(string message, List<string>? errors = null)
        => new(message, errors ?? []);

    public static ModelValidationException ValidationFailed(
        IDictionary<string, string[]> validationErrors)
        => new("One or more validation failures have occurred.", validationErrors);

    public static ModelValidationException ValidationFailed(string message, 
        IDictionary<string, string[]> validationErrors)
        => new(message, validationErrors);

    // Security Exceptions
    
    public static UnauthorizedException Unauthorized(string? message = null)
        => message == null ? new() : new(message);

    public static ForbiddenException Forbidden(string? message = null, List<string>? errors = null)
        => message == null ? new() : new(message, errors ?? []);

    public static InvalidCredentialsException InvalidCredentials(string? message = null)
        => message == null ? new() : new(message);

    // Infrastructure Exceptions
    
    public static ConfigurationException Configuration(string configKey, string reason)
        => new(configKey, reason);

    public static DatabaseException Database(string operation, string details)
        => new(operation, details);

    public static ExternalServiceException ExternalService(string serviceName, 
        int statusCode, string errorDetails)
        => new(serviceName, statusCode, errorDetails);

    public static ExternalServiceException ExternalService(string serviceName, string errorDetails)
        => new(serviceName, errorDetails);

    // Server Exceptions
    
    public static InternalServerException InternalServer(string? message = null, Exception? innerException = null)
        => message == null ? new() : innerException == null ? new(message) : new(message, innerException);

    public static BadGatewayException BadGateway(string serviceName, string details)
        => new(serviceName, details);

    public static GatewayTimeoutException GatewayTimeout(string serviceName, int timeoutSeconds)
        => new(serviceName, timeoutSeconds);

    public static ServiceUnavailableException ServiceUnavailable(string? message = null)
        => message == null ? new() : new(message);

    // Client Exceptions
    
    public static FeatureNotImplementedException NotImplemented(string featureName, string details)
        => new(featureName, details);

    public static TooManyRequestsException TooManyRequests(int limit, string period)
        => new(limit, period);

    public static TooManyRequestsException TooManyRequests(int retryAfterSeconds)
        => new(retryAfterSeconds);

    public static UnsupportedMediaTypeException UnsupportedMediaType(string mediaType, 
        string[] supportedTypes)
        => new(mediaType, supportedTypes);

    // Utility methods for wrapping exceptions
    
    public static CraftException FromStandardException(Exception exception)
    {
        return exception switch
        {
            ArgumentNullException ex => BadRequest($"Argument '{ex.ParamName}' cannot be null"),
            ArgumentException ex => BadRequest(ex.Message),
            InvalidOperationException ex => BadRequest(ex.Message),
            UnauthorizedAccessException => Unauthorized("Access denied"),
            KeyNotFoundException ex => NotFound(ex.Message),
            NotImplementedException ex => NotImplemented("Feature", ex.Message),
            TimeoutException => GatewayTimeout("Upstream service", 30),
            _ => InternalServer("An unexpected error occurred", exception)
        };
    }
}
```

### Usage Examples with Factory

```csharp
// Domain Layer
public class UserService
{
    public async Task<User> GetUserAsync(int id)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user == null)
            throw CraftExceptionFactory.NotFound("User", id); // ? Clean & consistent
        
        return user;
    }

    public async Task<User> CreateUserAsync(CreateUserDto dto)
    {
        if (await _repository.ExistsAsync(u => u.Email == dto.Email))
            throw CraftExceptionFactory.AlreadyExists("User", dto.Email); // ? Clear intent
        
        var validationErrors = ValidateUser(dto);
        if (validationErrors.Any())
            throw CraftExceptionFactory.ValidationFailed(validationErrors); // ? Simplified
        
        // ... create user
    }
}

// Infrastructure Layer
public class PaymentGatewayClient
{
    public async Task<Payment> ProcessAsync(PaymentRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsync("/api/payments", request);
            if (!response.IsSuccessStatusCode)
                throw CraftExceptionFactory.ExternalService(
                    "PaymentGateway", 
                    (int)response.StatusCode, 
                    await response.Content.ReadAsStringAsync()
                ); // ? Consistent formatting
            
            return await response.Content.ReadFromJsonAsync<Payment>();
        }
        catch (HttpRequestException ex)
        {
            throw CraftExceptionFactory.ExternalService("PaymentGateway", ex.Message);
        }
    }
}

// Configuration Layer
public class AppSettings
{
    public void Validate()
    {
        if (string.IsNullOrEmpty(ConnectionString))
            throw CraftExceptionFactory.Configuration(
                "Database:ConnectionString", 
                "Connection string is required"
            ); // ? Clear configuration error
    }
}

// Wrapping standard exceptions
try
{
    var config = await LoadConfigAsync();
}
catch (Exception ex)
{
    throw CraftExceptionFactory.FromStandardException(ex); // ? Automatic conversion
}
```

---

## Recommendations Summary

### ?? High Priority (Do Immediately)

1. **Rename `EntityNotFoundException` ? `NotFoundException`**
   - Create new class
   - Mark old class as `[Obsolete]`
   - Update all references
   - Update tests and documentation

2. **Create `CraftExceptionFactory`**
   - Implement factory class with static methods
   - Add comprehensive XML documentation
   - Create usage examples
   - Add to README

3. **Add `PayloadTooLargeException`** (413)
   - Common for file uploads
   - Security boundary

### ?? Medium Priority (Next Sprint)

4. **Add `GoneException`** (410)
   - REST API compliance
   - Soft delete scenarios

5. **Add `PreconditionFailedException`** (412)
   - ETag support
   - Conditional requests

6. **Update README.md**
   - Add ExceptionFactory section
   - Update code examples to use factory
   - Add migration guide for EntityNotFoundException

### ?? Low Priority (Future)

7. **Add `BusinessRuleViolationException`** (422)
   - Semantic clarity
   - Optional if ConflictException sufficient

8. **Add `ResourceLockedException`** (423)
   - Collaborative editing
   - Document management systems

9. **Add Telemetry Integration** (Optional)
   - Application Insights
   - OpenTelemetry
   - Factory is perfect injection point

---

## Migration Plan

### Phase 1: Immediate (Week 1)

**Step 1**: Create `NotFoundException.cs`
```csharp
// New file: Craft.Domain/Exceptions/Domain/NotFoundException.cs
public class NotFoundException : CraftException
{
    // Same implementation as EntityNotFoundException
}
```

**Step 2**: Mark old exception as obsolete
```csharp
// Update: Craft.Domain/Exceptions/Domain/EntityNotFoundException.cs
[Obsolete("Use NotFoundException instead. This class will be removed in v2.0.0")]
public class EntityNotFoundException : NotFoundException
{
    // Inherit from NotFoundException for backward compatibility
}
```

**Step 3**: Update internal references
- Update all Craft library code to use `NotFoundException`
- Update tests
- Update middleware

**Step 4**: Create `CraftExceptionFactory.cs`
- Implement factory methods
- Add comprehensive unit tests
- Update documentation

### Phase 2: Documentation (Week 1-2)

**Step 1**: Update README.md
- Add factory section
- Update all code examples
- Add migration guide

**Step 2**: Create factory usage examples
- Service layer examples
- Repository layer examples
- Controller examples

### Phase 3: New Exceptions (Week 2-3)

**Step 1**: Add `PayloadTooLargeException`
- Implement exception
- Add unit tests
- Update factory
- Update documentation

**Step 2**: Add `GoneException` and `PreconditionFailedException`
- Follow same pattern
- Update middleware handling

### Phase 4: Cleanup (v2.0.0 - Future)

**Step 1**: Remove `EntityNotFoundException` entirely
- Breaking change
- Requires major version bump

---

## Final Recommendations

### ? KEEP AS-IS (18 exceptions)
- All current exceptions are well-designed
- Good HTTP status code coverage
- Consistent constructor patterns
- Comprehensive test coverage

### ?? RENAME (1 exception)
- `EntityNotFoundException` ? `NotFoundException`

### ? ADD (2-5 new exceptions)
- **Must Add (2)**: `PayloadTooLargeException`, `CraftExceptionFactory`
- **Should Add (2)**: `GoneException`, `PreconditionFailedException`
- **Nice to Have (1)**: `BusinessRuleViolationException`, `ResourceLockedException`

### ?? STRONGLY RECOMMEND
- **ExceptionFactory**: Creates significant value with minimal cost
- Improves consistency, discoverability, and testability
- Common pattern in enterprise frameworks

---

## Conclusion

Your Craft.Exceptions library is **excellent** with a few opportunities for improvement:

**Strengths**:
- ? Comprehensive HTTP status code coverage
- ? Consistent constructor patterns
- ? Good categorization (Domain, Security, Server, Client, Infrastructure)
- ? Excellent test coverage
- ? Well-documented

**Improvements**:
1. Rename `EntityNotFoundException` for consistency
2. Add `CraftExceptionFactory` for better DX (Developer Experience)
3. Add 2-3 missing common HTTP exceptions

**Estimated Effort**:
- Phase 1 (Rename + Factory): 8-12 hours
- Phase 2 (Documentation): 4-6 hours
- Phase 3 (New Exceptions): 6-8 hours
- **Total**: 18-26 hours for complete implementation

**ROI**: High - These improvements will significantly enhance the library's usability and maintainability.

---

**Next Steps**: Proceed with Phase 1 implementation?
