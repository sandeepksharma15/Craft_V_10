# Craft.Exceptions

A comprehensive .NET 10 library providing a robust, standardized exception framework for modern applications. It offers consistent error handling with HTTP status code integration, detailed error reporting, and validation support across domain, security, server, and client scenarios.

## ?? Overview

Craft.Exceptions standardizes error handling across your entire application stack with:
- **HTTP Status Code Integration**: Each exception maps to appropriate HTTP status codes
- **Detailed Error Lists**: Support for multiple error messages per exception
- **Consistent Constructor Patterns**: Predictable, flexible exception creation
- **Comprehensive Coverage**: 15 exception types covering common scenarios
- **Full Test Coverage**: 87 unit tests ensuring reliability
- **.NET 10 & C# 14**: Leverages the latest language features

---

## ?? Installation

```bash
dotnet add reference Craft.Exceptions
```

---

## ??? Architecture

### Base Exception

**`CraftException`** - Abstract base class for all custom exceptions

**Properties:**
- `List<string> Errors` - Collection of error messages
- `HttpStatusCode StatusCode` - HTTP status code for API responses

**Location:** `Craft.Exceptions/Base/`

---

## ?? Exception Categories

### ?? Domain Exceptions
**Location:** `Craft.Exceptions/Domain/`

#### 1. AlreadyExistsException (422)
Thrown when attempting to create a resource that already exists.

```csharp
// Basic usage
throw new AlreadyExistsException();
throw new AlreadyExistsException("User with this email already exists");

// Formatted with entity name and key
throw new AlreadyExistsException("User", userId);
// Output: Entity "User" (123) already exists

// With error details
throw new AlreadyExistsException("Duplicate entry", new List<string> { "Email already registered" });
```

#### 2. BadRequestException (400)
Thrown when a request is malformed or contains invalid data.

```csharp
// Basic usage
throw new BadRequestException();
throw new BadRequestException("Invalid input format");

// With multiple errors
throw new BadRequestException("Missing required fields", 
    new List<string> { "Email is required", "Password is required" });

// With inner exception for debugging
throw new BadRequestException("Invalid JSON", jsonException);
```

#### 3. ConflictException (409)
Thrown when a request conflicts with the current state of a resource.

```csharp
// Basic usage
throw new ConflictException();
throw new ConflictException("Resource version mismatch");

// Formatted with resource name and reason
throw new ConflictException("Order", "Cannot delete order with active shipments");
// Output: Conflict with resource "Order": Cannot delete order with active shipments

// With error list
throw new ConflictException("State conflict", new List<string> { "Order is locked", "Payment pending" });
```

#### 4. ModelValidationException (400)
Thrown when model validation fails. Provides detailed field-level validation errors.

**Additional Property:**
- `IDictionary<string, string[]> ValidationErrors` - Field-specific validation errors

```csharp
// Basic usage
throw new ModelValidationException();
// Output: One or more validation failures have occurred.

// With validation dictionary
var validationErrors = new Dictionary<string, string[]>
{
    { "Email", new[] { "Email is required", "Invalid email format" } },
    { "Password", new[] { "Password must be at least 8 characters" } }
};
throw new ModelValidationException("Validation failed", validationErrors);
```

#### 5. NotFoundException (404)
Thrown when a requested resource cannot be found.

```csharp
// Basic usage
throw new NotFoundException("Resource not found");

// Formatted with entity name and key
throw new NotFoundException("User", userId);
// Output: Entity "User" (123) was not found.

// With error list
throw new NotFoundException("User not found", new List<string> { "User may have been deleted" });
```

---

### ?? Security Exceptions
**Location:** `Craft.Exceptions/Security/`

#### 1. ForbiddenException (403)
Thrown when the user is authenticated but lacks permission to access a resource.

```csharp
// Basic usage
throw new ForbiddenException();
throw new ForbiddenException("You do not have permission to access this resource");

// With detailed permissions
throw new ForbiddenException("Access denied", 
    new List<string> { "Requires Admin role", "Current role: User" });
```

#### 2. InvalidCredentialsException (401)
Thrown when authentication fails due to invalid credentials.

```csharp
// Basic usage
throw new InvalidCredentialsException();
// Output: Invalid Credentials: Please check your credentials

// Custom message
throw new InvalidCredentialsException("Username or password is incorrect");

// With error details
throw new InvalidCredentialsException("Login failed", 
    new List<string> { "Account may be locked", "Too many failed attempts" });
```

#### 3. UnauthorizedException (401)
Thrown when authentication is required but not provided or invalid.

```csharp
// Basic usage
throw new UnauthorizedException();
throw new UnauthorizedException("Authentication token is missing or invalid");

// With error details
throw new UnauthorizedException("Unauthorized access", 
    new List<string> { "Token expired", "Please login again" });
```

---

### ??? Server Exceptions
**Location:** `Craft.Exceptions/Server/`

#### 1. BadGatewayException (502)
Thrown when the server received an invalid response from an upstream server.

```csharp
// Basic usage
throw new BadGatewayException();
// Output: Bad gateway - invalid response from upstream server

// Formatted with upstream service details
throw new BadGatewayException("PaymentAPI", "Received malformed JSON response");
// Output: Bad gateway from "PaymentAPI": Received malformed JSON response

// With inner exception
throw new BadGatewayException("Invalid response from external service", httpException);
```

#### 2. GatewayTimeoutException (504)
Thrown when the server did not receive a timely response from an upstream server.

```csharp
// Basic usage
throw new GatewayTimeoutException();
// Output: Gateway timeout - no response from upstream server

// Formatted with service and timeout duration
throw new GatewayTimeoutException("ExternalAPI", 30);
// Output: Gateway timeout from "ExternalAPI" after 30 seconds

// With error details
throw new GatewayTimeoutException("Upstream timeout", 
    new List<string> { "Service may be down", "Retry after 60 seconds" });
```

#### 3. InternalServerException (500)
Thrown when an internal server error occurs.

```csharp
// Basic usage
throw new InternalServerException();
// Output: An internal server error occurred

// With custom message
throw new InternalServerException("Database connection failed");

// With inner exception for logging
throw new InternalServerException("Unexpected error", databaseException);

// With error list
throw new InternalServerException("Critical error", 
    new List<string> { "Contact administrator", "Error logged" });
```

#### 4. ServiceUnavailableException (503)
Thrown when the service is temporarily unavailable.

```csharp
// Basic usage
throw new ServiceUnavailableException();
// Output: The service is temporarily unavailable

// Custom message
throw new ServiceUnavailableException("Database maintenance in progress");

// With error details
throw new ServiceUnavailableException("Service temporarily offline", 
    new List<string> { "Scheduled maintenance", "Expected completion: 2 hours" });
```

---

### ?? Client Exceptions
**Location:** `Craft.Exceptions/Client/`

#### 1. FeatureNotImplementedException (501)
Thrown when a feature or endpoint is not yet implemented.

```csharp
// Basic usage
throw new FeatureNotImplementedException();
// Output: This feature is not implemented

// Formatted with feature details
throw new FeatureNotImplementedException("Export to PDF", "Planned for Q2 2024");
// Output: Feature "Export to PDF" is not implemented: Planned for Q2 2024

// With error details
throw new FeatureNotImplementedException("Advanced search", 
    new List<string> { "Currently in development", "Use basic search instead" });
```

#### 2. TooManyRequestsException (429)
Thrown when the client has sent too many requests (rate limiting).

```csharp
// Basic usage
throw new TooManyRequestsException();
// Output: Too many requests - rate limit exceeded

// With rate limit details
throw new TooManyRequestsException(100, "minute");
// Output: Rate limit exceeded: 100 requests per minute

// With retry-after information
throw new TooManyRequestsException(60);
// Output: Too many requests. Retry after 60 seconds

// Custom message with errors
throw new TooManyRequestsException("Rate limit hit", 
    new List<string> { "Limit: 1000/hour", "Current: 1000/hour" });
```

#### 3. UnsupportedMediaTypeException (415)
Thrown when the request content type is not supported.

```csharp
// Basic usage
throw new UnsupportedMediaTypeException();
// Output: The media type is not supported

// Formatted with supported types
var supportedTypes = new[] { "application/json", "application/xml" };
throw new UnsupportedMediaTypeException("text/csv", supportedTypes);
// Output: Media type "text/csv" is not supported. Supported types: application/json, application/xml

// With error details
throw new UnsupportedMediaTypeException("Invalid content type", 
    new List<string> { "Only JSON is supported", "Set Content-Type header" });
```

---

## ?? HTTP Status Code Reference

| Status Code | Exception Class | Category |
|-------------|-----------------|----------|
| 400 | BadRequestException | Domain |
| 400 | ModelValidationException | Domain |
| 401 | UnauthorizedException | Security |
| 401 | InvalidCredentialsException | Security |
| 403 | ForbiddenException | Security |
| 404 | NotFoundException | Domain |
| 409 | ConflictException | Domain |
| 415 | UnsupportedMediaTypeException | Client |
| 422 | AlreadyExistsException | Domain |
| 429 | TooManyRequestsException | Client |
| 500 | InternalServerException | Server |
| 501 | FeatureNotImplementedException | Client |
| 502 | BadGatewayException | Server |
| 503 | ServiceUnavailableException | Server |
| 504 | GatewayTimeoutException | Server |

---

## ?? Design Patterns

### Consistent Constructor Overloads

All exceptions follow a standardized constructor pattern:

1. **Default Constructor** - Provides sensible default message
2. **Message Constructor** - Custom message
3. **Message + InnerException** - For wrapping underlying exceptions
4. **Message + Errors** - For multiple error details
5. **Specialized Constructors** - Entity-specific formatting (varies by exception)

```csharp
// Pattern examples
throw new NotFoundException();                              // 1. Default
throw new NotFoundException("User not found");              // 2. Message
throw new NotFoundException("Error", innerEx);              // 3. Inner exception
throw new NotFoundException("Error", errorList);            // 4. Error list
throw new NotFoundException("User", userId);                // 5. Specialized
```

### Error Lists

All exceptions inherit an `Errors` property from `CraftException`:

```csharp
var errors = new List<string> 
{ 
    "Primary error message",
    "Secondary error detail",
    "Additional context"
};
throw new BadRequestException("Request validation failed", errors);

// Access errors
try { /* ... */ }
catch (CraftException ex)
{
    Console.WriteLine($"Message: {ex.Message}");
    Console.WriteLine($"Status: {ex.StatusCode}");
    foreach (var error in ex.Errors)
        Console.WriteLine($"  - {error}");
}
```

---

## ?? Integration Examples

### ASP.NET Core Middleware

```csharp
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        
        if (exception is CraftException craftEx)
        {
            context.Response.StatusCode = (int)craftEx.StatusCode;
            await context.Response.WriteAsJsonAsync(new
            {
                message = craftEx.Message,
                errors = craftEx.Errors,
                statusCode = craftEx.StatusCode
            });
        }
        else
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new
            {
                message = "An unexpected error occurred"
            });
        }
    });
});
```

### Domain Layer Usage

```csharp
public class UserService
{
    public async Task<User> CreateUserAsync(CreateUserDto dto)
    {
        // Check if user exists
        if (await _userRepository.ExistsAsync(u => u.Email == dto.Email))
            throw new AlreadyExistsException("User", dto.Email);
        
        // Validate input
        var validationErrors = ValidateUser(dto);
        if (validationErrors.Any())
            throw new ModelValidationException("Invalid user data", validationErrors);
        
        // Create user
        var user = _mapper.Map<User>(dto);
        return await _userRepository.AddAsync(user);
    }
    
    public async Task<User> GetUserByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new NotFoundException("User", id);
        
        return user;
    }
}
```

### API Controller Usage

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        // NotFoundException (404) will be automatically handled by middleware
        var user = await _userService.GetUserByIdAsync(id);
        return Ok(user);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserDto dto)
    {
        // AlreadyExistsException (422) and ModelValidationException (400)
        // will be automatically handled by middleware
        var user = await _userService.CreateUserAsync(dto);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }
    
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        // ForbiddenException (403) will be thrown if user lacks Admin role
        if (!User.IsInRole("Admin"))
            throw new ForbiddenException("Only administrators can delete users");
        
        await _userService.DeleteUserAsync(id);
        return NoContent();
    }
}
```

---

## ? Test Coverage

All exception classes have comprehensive unit test coverage:

- Default constructor behavior
- Message-only constructor
- Message with inner exception
- Message with errors list
- Specialized formatted constructors
- Null error list handling
- Status code verification

**Total Tests: 87** ? **All Passing**

---

## ?? Best Practices

### 1. Choose the Right Exception

```csharp
// ? Good - Specific exception
throw new NotFoundException("User", userId);

// ? Bad - Generic exception
throw new Exception("User not found");
```

### 2. Provide Context with Error Lists

```csharp
// ? Good - Detailed errors
throw new ConflictException("Cannot delete order", new List<string>
{
    "Order has active shipments",
    "Complete or cancel shipments first"
});

// ? Bad - Vague message
throw new ConflictException("Cannot delete");
```

### 3. Wrap Underlying Exceptions

```csharp
// ? Good - Preserve stack trace
try
{
    await externalService.CallAsync();
}
catch (HttpRequestException ex)
{
    throw new BadGatewayException("External service error", ex);
}

// ? Bad - Lose original exception
throw new BadGatewayException("External service error");
```

### 4. Use Formatted Constructors

```csharp
// ? Good - Consistent formatting
throw new NotFoundException("User", userId);
// Output: Entity "User" (123) was not found.

// ? Bad - Manual formatting
throw new NotFoundException($"Entity User {userId} was not found.");
```

---

## ?? Additional Resources

- **Source Code**: Browse the repository for implementation details
- **XML Documentation**: IntelliSense provides inline documentation
- **Unit Tests**: Review test files for usage examples

---

## ?? License

This project is licensed under the MIT License. See the `LICENSE` file for details.

---

## ?? Contributing

Contributions are welcome! Please ensure:
- New exceptions follow existing patterns
- Comprehensive unit tests are included
- XML documentation is provided
- HTTP status codes are appropriate

---

**Version:** 1.0  
**Target Framework:** .NET 10  
**Language Version:** C# 14
