# Craft.Core

## Overview

**Craft.Core** is the foundational library for the Craft ecosystem, providing core abstractions, request/response types, and standardized interfaces for service registration and dependency injection. It is designed for .NET 10 and supports modern Blazor applications, following best practices for maintainability, discoverability, and extensibility.

## Features

- **Service Abstractions:** Marker interfaces (`IService`, `IServiceResult`) for standardized DI and result handling.
- **Request Types:** Base classes and enums for API request modeling (`APIRequest`, `ApiGetRequest`, `ApiRequestType`).
- **XML Documentation:** All public types, methods, and properties are documented for API discoverability.
- **.NET 10 Compatibility:** Uses the latest language features and conventions.
- **Extensible:** Easily integrate with other Craft modules and Blazor projects.

## Getting Started

### Installation

Add a reference to `Craft.Core` in your `.csproj`:

```xml
<ItemGroup>
  <ProjectReference Include="..\Craft.Core\Craft.Core.csproj" />
</ItemGroup>
```

### Usage

#### Service Registration

Implement the `IService` marker interface on any class intended for DI registration:

```csharp
public class MyService : IService
{
    // Implementation
}
```

Register your services in the DI container (example for Blazor):

```csharp
builder.Services.AddScoped<IService, MyService>();
```

#### API Request Modeling

Use the provided base request types for standardized API contracts:

```csharp
public class GetCustomerRequest : APIRequest<Guid>
{
    public GetCustomerRequest() : base(ApiRequestType.Get) { }
}
```

#### Service Results

Return standardized results from your services:

```csharp
public class CustomerService : IService
{
    public IServiceResult GetCustomer(Guid id)
    {
        // Implementation
    }
}
```

## Documentation

All public types, methods, and properties include XML documentation. Use IntelliSense or reference the source for details.

## Contributing

1. Fork the repository and create a feature branch.
2. Follow .NET 10 and repo coding standards.
3. Add XML documentation to all public members.
4. Run `dotnet build` and ensure all tests pass.
5. Submit a pull request with a clear description.

## License

This project is licensed under the MIT License.

## Contact

For questions or support, open an issue or contact the maintainer.
