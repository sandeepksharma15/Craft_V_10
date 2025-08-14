# Craft.MultiTenant

Craft.MultiTenant is a .NET 10 library providing a flexible, extensible, and robust framework for multi-tenant application development. It supports tenant resolution, context management, and tenant-aware data access using a variety of strategies and stores.

## Features
- **Tenant Abstractions**: Interfaces for tenants, multi-tenant entities, and tenant context management.
- **Tenant Resolution Strategies**: Host, header, claim, route, delegate, and base path strategies for identifying tenants from HTTP requests.
- **Tenant Stores**: In-memory, configuration, database, cache, and remote API stores for tenant persistence and retrieval.
- **Context Accessors**: Access and manage the current tenant context throughout the application lifecycle.
- **Error Handling**: Custom exceptions for multi-tenant scenarios.
- **.NET 10 & C# 13**: Utilizes the latest language and framework features.

## Usage Example
```csharp
// Register a tenant store and strategy in DI
services.AddSingleton<ITenantStore, InMemoryStore>();
services.AddSingleton<ITenantStrategy, HostStrategy>();

// Access current tenant in your service
public class MyService
{
    private readonly ICurrentTenant _currentTenant;
    public MyService(ICurrentTenant currentTenant) => _currentTenant = currentTenant;

    public string? GetTenantIdentifier() => _currentTenant.Identifier;
}
```

## Key Components
- `ITenant`, `IMultiTenant`, `ITenantStore`, `ITenantStrategy`, `ICurrentTenant`, etc.: Core abstractions for multi-tenancy.
- `HostStrategy`, `HeaderStrategy`, `ClaimStrategy`, etc.: Strategies for tenant resolution.
- `InMemoryStore`, `ConfigurationStore`, `DbStore`, etc.: Tenant persistence mechanisms.
- `MultiTenantException`: Exception for multi-tenant errors.

## Integration
Craft.MultiTenant is designed to be referenced by ASP.NET Core and .NET projects that require tenant-aware logic, data isolation, and flexible tenant identification.

## License
This project is licensed under the MIT License. See the `LICENSE` file for details.

---
For more details, review the source code and XML documentation in the project.
