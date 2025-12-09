# Craft.Security

Craft.Security is a .NET 10 library providing robust, extensible, and modern security abstractions and utilities for authentication, authorization, and user context management in C# applications. It is designed to support JWT-based authentication, claims-based identity, and secure user/session handling for APIs and web apps.

## Features
- **Current User Abstractions**: Interfaces and services for accessing the current user's identity, claims, and roles.
- **JWT Authentication**: Models and helpers for issuing, validating, and refreshing JWT tokens.
- **Claims Extensions**: Utilities for extracting and managing claims from `ClaimsPrincipal`.
- **Bearer Token Handler**: Delegating handler for propagating bearer tokens in outgoing HTTP requests.
- **Role & Permission Support**: Abstractions for roles, permissions, and tenant-aware security.
- **Password Management**: Models for password change, reset, and forgot flows.
- **Google Auth Options**: Support for Google authentication configuration.
- **Dependency Injection Ready**: Extension methods for registering security services in DI.
- **.NET 10 & C# 13**: Utilizes the latest language and framework features.

## Getting Started

### Installation
Add a project reference to `Craft.Security` in your .NET 10 solution:

```
dotnet add reference ../Craft.Security/Craft.Security.csproj
```

### Usage Example
```csharp
using Craft.Security;

// Register current user services in DI
services.AddCurrentApiUser(); // For API scenarios
// or
services.AddCurrentUiUser(); // For UI scenarios

// Access current user in your service
public class MyService
{
    private readonly ICurrentUser _currentUser;
    public MyService(ICurrentUser currentUser) => _currentUser = currentUser;

    public string? GetUserEmail() => _currentUser.GetEmail();
}
```

## Key Components
- `ICurrentUser`, `ICurrentUserProvider`: Abstractions for accessing current user context.
- `JwtAuthResponse`, `RefreshTokenRequest`: Models for JWT authentication and refresh flows.
- `BearerTokenHandler`: HTTP handler for bearer token propagation.
- `ClaimsPrincipalExtensions`: Helpers for claims extraction and manipulation.
- `ICraftUser`, `ICraftRole`: Interfaces for user and role models.
- `PasswordChangeRequest`, `PasswordForgotRequest`, `PasswordResetRequest`: Models for password management.
- `GoogleAuthOptions`: Options for Google authentication integration.

## Integration
Craft.Security is designed to be referenced by .NET projects that require secure, claims-based authentication and user context management, including APIs, web apps, and Blazor applications.

## License
This project is licensed under the MIT License. See the `LICENSE` file for details.

---
For more details, review the source code and XML documentation in the project.
