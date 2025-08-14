# Craft.Exceptions

Craft.Exceptions is a .NET 10 library providing a robust, extensible framework for custom exception handling in modern applications. It standardizes error reporting, status codes, and validation errors for domain, security, and API scenarios.

## Features
- **Base Exception Abstraction**: `CraftException` provides a foundation for all custom exceptions, supporting error lists and HTTP status codes.
- **Domain Exceptions**: Includes `AlreadyExistsException`, `NotFoundException`, and `ModelValidationException` for common domain error scenarios.
- **Security Exceptions**: Includes `ForbiddenException`, `UnauthorizedException`, and `InvalidCredentialsException` for access and authentication errors.
- **Validation Support**: `ModelValidationException` supports detailed validation error reporting.
- **HTTP Status Integration**: All exceptions can carry appropriate HTTP status codes for API responses.
- **.NET 10 & C# 13**: Utilizes the latest language and framework features.

## Usage Example
```csharp
if (!user.HasPermission)
    throw new ForbiddenException("You do not have access to this resource.");

if (entity == null)
    throw new NotFoundException("Entity not found.");

throw new ModelValidationException("Validation failed", validationErrors);
```

## Key Components
- `CraftException`: Abstract base for all custom exceptions.
- `AlreadyExistsException`, `NotFoundException`, `ModelValidationException`: Domain error types.
- `ForbiddenException`, `UnauthorizedException`, `InvalidCredentialsException`: Security error types.

## Integration
Craft.Exceptions is designed to be referenced by domain, API, and service projects to ensure consistent and expressive error handling.

## License
This project is licensed under the MIT License. See the `LICENSE` file for details.

---
For more details, review the source code and XML documentation in the project.
