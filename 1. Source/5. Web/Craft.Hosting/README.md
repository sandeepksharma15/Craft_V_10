# Craft.Hosting

Centralized dependency injection extensions for the Craft framework. This package provides a single entry point for registering Craft services in your application.

## Purpose

The `Craft.Hosting` project consolidates all DI extension methods from various Craft projects into one cohesive location. This approach provides:

- **Single Entry Point**: External projects only need to reference `Craft.Hosting`
- **Clear Organization**: Extensions grouped by functionality
- **No Circular Dependencies**: Clean dependency graph
- **Discoverability**: All registration methods in one place
- **Convenience Methods**: Simplified registration for common scenarios

## Installation

```bash
dotnet add package Craft.Hosting
```

## Project Structure

```
Craft.Hosting/
├── Extensions/
│   ├── HttpServiceExtensions.cs                 # HTTP service registration
│   └── HttpServiceConvenienceExtensions.cs      # Simplified registration methods
└── README.md
```

## Usage

### Basic HTTP Service Registration

```csharp
using Craft.Hosting.Extensions;

// Register with full control over interface registration
services.AddTransientHttpService<Product, ProductVM, ProductDTO>(
    provider => provider.GetRequiredService<ApiClient>().HttpClient,
    "https+http://api",
    "/api/Product",
    registerPrimaryInterface: false,
    registerWithKeyType: true,
    registerSimplified: true);
```

### Convenience Methods for Blazor

```csharp
// Optimized for Blazor applications (registers interfaces used by edit and list components)
services.AddHttpServiceForBlazor<Product, ProductVM, ProductDTO>(
    provider => provider.GetRequiredService<ApiClient>().HttpClient,
    "https+http://api",
    "/api/Product");

// Custom service for Blazor
services.AddCustomHttpServiceForBlazor<BorderOrg, BorderOrgVM, BorderOrgDTO, BorderOrgHttpService>(
    provider => provider.GetRequiredService<ApiClient>().HttpClient,
    "https+http://api",
    "/api/BorderOrg");
```

### Convenience Methods for API-Only

```csharp
// For API projects without UI components
services.AddHttpServiceForApi<Product, ProductVM, ProductDTO>(
    provider => provider.GetRequiredService<ApiClient>().HttpClient,
    "https+http://api",
    "/api/Product");
```

### List-Only Registration

```csharp
// For read-only scenarios
services.AddHttpServiceForList<Product>(
    provider => provider.GetRequiredService<ApiClient>().HttpClient,
    "https+http://api",
    "/api/Product");
```

## Interface Registration Options

### `registerPrimaryInterface`
Registers `IHttpService<T, TView, TDto>` (without KeyType parameter).

### `registerWithKeyType`
Registers `IHttpService<T, TView, TDto, KeyType>` (with explicit KeyType parameter).
Used by `BaseEditComponent` and other edit scenarios.

### `registerSimplified`
Registers `IHttpService<T>` (simplified interface).
Used by list components for read-only operations.

## Service Lifetimes

All registration methods support three lifetimes:

- **Transient** (`AddTransientHttpService`): New instance per injection (default, recommended for HTTP services)
- **Scoped** (`AddScopedHttpService`): One instance per scope/request
- **Singleton** (`AddSingletonHttpService`): Single instance for application lifetime (use with caution)

## Migration from Craft.QuerySpec.Extensions

If you were previously using `Craft.QuerySpec.Extensions`, update your using statement:

```csharp
// Old
using Craft.QuerySpec.Extensions;

// New
using Craft.Hosting.Extensions;
```

The API remains the same, so no code changes are required beyond the using statement.

## Design Decisions

### Why Craft.Hosting?

- **Separation of Concerns**: DI registrations are separate from implementations
- **No Circular Dependencies**: Extensions reference implementations, not vice versa
- **ASP.NET Core Convention**: Follows the pattern of `Microsoft.Extensions.Hosting`
- **Future-Proof**: Easy to add new extension categories without polluting implementation projects

### Why "Fan-In" Dependencies?

`Craft.Hosting` references multiple Craft projects (QuerySpec, Data, Security, etc.). This is acceptable because:
- Extensions are a composition layer, not a business logic layer
- External projects only need one reference instead of many
- No circular dependencies are introduced
- Clear separation between "what" (implementations) and "how to register" (extensions)

## Contributing

When adding new Craft services that need DI registration:

1. Add the service implementation in the appropriate Craft project
2. Add the registration extension in `Craft.Hosting/Extensions/`
3. Update this README with usage examples
4. Consider adding convenience methods for common scenarios

## Related Projects

- **Craft.QuerySpec**: HTTP service implementations
- **Craft.Core**: Core interfaces and abstractions
- **Craft.Domain**: Domain models and entities
