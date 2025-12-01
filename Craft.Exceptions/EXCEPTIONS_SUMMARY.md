# Craft.Exceptions Summary

## Overview
This document provides a comprehensive overview of all exception classes in the Craft.Exceptions library.

## Exception Categories

### Base Exception
- **CraftException** - Abstract base class for all custom exceptions
  - Properties: `List<string> Errors`, `HttpStatusCode StatusCode`
  - Location: `Craft.Exceptions/Base/`

---

## Domain Exceptions
Location: `Craft.Exceptions/Domain/`

### 1. AlreadyExistsException (422 - Unprocessable Entity)
Thrown when attempting to create a resource that already exists.

**Constructors:**
- `AlreadyExistsException()`
- `AlreadyExistsException(string message)`
- `AlreadyExistsException(string message, Exception innerException)`
- `AlreadyExistsException(string message, List<string> errors, HttpStatusCode statusCode)`
- `AlreadyExistsException(string name, object key)` - Formatted: `"Entity "{name}" ({key}) already exists"`

### 2. BadRequestException (400 - Bad Request) ? NEW
Thrown when a request is malformed or contains invalid data that doesn't fit validation scenarios.

**Constructors:**
- `BadRequestException()` - Default: "The request is invalid"
- `BadRequestException(string message)`
- `BadRequestException(string message, Exception innerException)`
- `BadRequestException(string message, List<string>? errors)`

### 3. ConflictException (409 - Conflict) ? NEW
Thrown when a request conflicts with the current state of a resource.

**Constructors:**
- `ConflictException()` - Default: "A conflict occurred with the current state of the resource"
- `ConflictException(string message)`
- `ConflictException(string message, Exception innerException)`
- `ConflictException(string message, List<string>? errors)`
- `ConflictException(string resourceName, string reason)` - Formatted: `"Conflict with resource "{resourceName}": {reason}"`

### 4. ModelValidationException (400 - Bad Request)
Thrown when model validation fails.

**Properties:**
- `IDictionary<string, string[]> ValidationErrors`

**Constructors:**
- `ModelValidationException()` - Default: "One or more validation failures have occurred."
- `ModelValidationException(string message)`
- `ModelValidationException(string message, Exception innerException)`
- `ModelValidationException(string message, IDictionary<string, string[]> validationErrors)`

### 5. NotFoundException (404 - Not Found)
Thrown when a requested resource cannot be found.

**Constructors:**
- `NotFoundException()`
- `NotFoundException(string message)`
- `NotFoundException(string message, Exception innerException)`
- `NotFoundException(string message, List<string> errors, HttpStatusCode statusCode)`
- `NotFoundException(string name, object key)` - Formatted: `"Entity "{name}" ({key}) was not found."`

---

## Security Exceptions
Location: `Craft.Exceptions/Security/`

### 1. ForbiddenException (403 - Forbidden)
Thrown when the user is authenticated but lacks permission to access a resource.

**Constructors:**
- `ForbiddenException()`
- `ForbiddenException(string message)`
- `ForbiddenException(string message, Exception innerException)`
- `ForbiddenException(string message, List<string> errors, HttpStatusCode statusCode)`

### 2. InvalidCredentialsException (401 - Unauthorized)
Thrown when authentication fails due to invalid credentials.

**Constructors:**
- `InvalidCredentialsException()` - Default: "Invalid Credentials: Please check your credentials"
- `InvalidCredentialsException(string message)`
- `InvalidCredentialsException(string message, Exception innerException)`
- `InvalidCredentialsException(string message, List<string> errors, HttpStatusCode statusCode)`

### 3. UnauthorizedException (401 - Unauthorized)
Thrown when authentication is required but not provided.

**Constructors:**
- `UnauthorizedException()`
- `UnauthorizedException(string message)`
- `UnauthorizedException(string message, Exception innerException)`
- `UnauthorizedException(string message, List<string> errors, HttpStatusCode statusCode)`

---

## Server Exceptions
Location: `Craft.Exceptions/Server/`

### 1. BadGatewayException (502 - Bad Gateway) ? NEW
Thrown when the server received an invalid response from an upstream server.

**Constructors:**
- `BadGatewayException()` - Default: "Bad gateway - invalid response from upstream server"
- `BadGatewayException(string message)`
- `BadGatewayException(string message, Exception innerException)`
- `BadGatewayException(string message, List<string>? errors)`
- `BadGatewayException(string upstreamService, string reason)` - Formatted: `"Bad gateway from "{upstreamService}": {reason}"`

### 2. GatewayTimeoutException (504 - Gateway Timeout) ? NEW
Thrown when the server did not receive a timely response from an upstream server.

**Constructors:**
- `GatewayTimeoutException()` - Default: "Gateway timeout - no response from upstream server"
- `GatewayTimeoutException(string message)`
- `GatewayTimeoutException(string message, Exception innerException)`
- `GatewayTimeoutException(string message, List<string>? errors)`
- `GatewayTimeoutException(string upstreamService, int timeoutSeconds)` - Formatted: `"Gateway timeout from "{upstreamService}" after {timeoutSeconds} seconds"`

### 3. InternalServerException (500 - Internal Server Error)
Thrown when an internal server error occurs.

**Constructors:**
- `InternalServerException()` - Default: "An internal server error occurred"
- `InternalServerException(string message)`
- `InternalServerException(string message, Exception innerException)`
- `InternalServerException(string message, List<string>? errors)`
- `InternalServerException(string message, List<string> errors, HttpStatusCode statusCode)`

### 4. ServiceUnavailableException (503 - Service Unavailable) ? NEW
Thrown when the service is temporarily unavailable.

**Constructors:**
- `ServiceUnavailableException()` - Default: "The service is temporarily unavailable"
- `ServiceUnavailableException(string message)`
- `ServiceUnavailableException(string message, Exception innerException)`
- `ServiceUnavailableException(string message, List<string>? errors)`

---

## Client Exceptions
Location: `Craft.Exceptions/Client/` ? NEW CATEGORY

### 1. FeatureNotImplementedException (501 - Not Implemented) ? NEW
Thrown when a feature or endpoint is not yet implemented.

**Constructors:**
- `FeatureNotImplementedException()` - Default: "This feature is not implemented"
- `FeatureNotImplementedException(string message)`
- `FeatureNotImplementedException(string message, Exception innerException)`
- `FeatureNotImplementedException(string message, List<string>? errors)`
- `FeatureNotImplementedException(string featureName, string details)` - Formatted: `"Feature "{featureName}" is not implemented: {details}"`

### 2. TooManyRequestsException (429 - Too Many Requests) ? NEW
Thrown when the client has sent too many requests in a given time period.

**Constructors:**
- `TooManyRequestsException()` - Default: "Too many requests - rate limit exceeded"
- `TooManyRequestsException(string message)`
- `TooManyRequestsException(string message, Exception innerException)`
- `TooManyRequestsException(string message, List<string>? errors)`
- `TooManyRequestsException(int limit, string period)` - Formatted: `"Rate limit exceeded: {limit} requests per {period}"`
- `TooManyRequestsException(int retryAfterSeconds)` - Formatted: `"Too many requests. Retry after {retryAfterSeconds} seconds"`

### 3. UnsupportedMediaTypeException (415 - Unsupported Media Type) ? NEW
Thrown when the request content type is not supported.

**Constructors:**
- `UnsupportedMediaTypeException()` - Default: "The media type is not supported"
- `UnsupportedMediaTypeException(string message)`
- `UnsupportedMediaTypeException(string message, Exception innerException)`
- `UnsupportedMediaTypeException(string message, List<string>? errors)`
- `UnsupportedMediaTypeException(string mediaType, string[] supportedTypes)` - Formatted: `"Media type "{mediaType}" is not supported. Supported types: {joined list}"`

---

## HTTP Status Code Reference

| Status Code | Exception Class |
|-------------|----------------|
| 400 | BadRequestException, ModelValidationException |
| 401 | UnauthorizedException, InvalidCredentialsException |
| 403 | ForbiddenException |
| 404 | NotFoundException |
| 409 | ConflictException |
| 415 | UnsupportedMediaTypeException |
| 422 | AlreadyExistsException |
| 429 | TooManyRequestsException |
| 500 | InternalServerException |
| 501 | FeatureNotImplementedException |
| 502 | BadGatewayException |
| 503 | ServiceUnavailableException |
| 504 | GatewayTimeoutException |

---

## Usage Examples

### Domain Exceptions

```csharp
// BadRequestException
throw new BadRequestException("Invalid input format");
throw new BadRequestException("Missing required fields", new List<string> { "Email", "Password" });

// ConflictException
throw new ConflictException("Order", "Cannot delete order with active shipments");

// NotFoundException
throw new NotFoundException("User", userId);
```

### Security Exceptions

```csharp
// UnauthorizedException
throw new UnauthorizedException("Authentication token is missing or invalid");

// ForbiddenException
throw new ForbiddenException("You do not have permission to access this resource");

// InvalidCredentialsException
throw new InvalidCredentialsException();
```

### Server Exceptions

```csharp
// ServiceUnavailableException
throw new ServiceUnavailableException("Database maintenance in progress");

// BadGatewayException
throw new BadGatewayException("PaymentAPI", "Received malformed JSON response");

// GatewayTimeoutException
throw new GatewayTimeoutException("ExternalAPI", 30);
```

### Client Exceptions

```csharp
// FeatureNotImplementedException
throw new FeatureNotImplementedException("Export to PDF", "Planned for Q2 2024");

// TooManyRequestsException
throw new TooManyRequestsException(100, "minute");
throw new TooManyRequestsException(60); // retry after 60 seconds

// UnsupportedMediaTypeException
throw new UnsupportedMediaTypeException("text/csv", new[] { "application/json", "application/xml" });
```

---

## Test Coverage

All exception classes have comprehensive unit test coverage including:
- Default constructor behavior
- Message-only constructor
- Message with inner exception
- Message with errors list
- Special formatted constructors (where applicable)
- Null error list handling
- Status code verification

**Total Tests: 87** ? All Passing

---

## Design Patterns

### Constructor Overloads
All exceptions follow a consistent constructor pattern:
1. Default constructor (with sensible default message)
2. Message-only constructor
3. Message + InnerException constructor
4. Message + Errors + StatusCode constructor
5. Specialized constructors (entity-specific formatting)

### Error Lists
All exceptions inherit an `Errors` property from `CraftException` that can store multiple error messages for detailed error reporting.

### HTTP Status Codes
Each exception class is associated with an appropriate HTTP status code, making them suitable for web API error responses.

---

*Last Updated: [Current Date]*
*Version: 1.0*
