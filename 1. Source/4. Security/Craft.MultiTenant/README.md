# Craft.MultiTenant

Craft.MultiTenant is a .NET 10 library providing a flexible, extensible, and robust framework for multi-tenant application development. It supports tenant resolution, context management, and tenant-aware data access using a variety of strategies and stores.

## Features
- **Tenant Abstractions**: Interfaces for tenants, multi-tenant entities, and tenant context management.
- **Lightweight Current Tenant Access**: Access current tenant information without direct dependency on Craft.MultiTenant via `ICurrentTenant` in Craft.Core.
- **Tenant Resolution Strategies**: Host, header, claim, route, delegate, and base path strategies for identifying tenants from HTTP requests.
- **Tenant Stores**: In-memory, configuration, database, cache, and remote API stores for tenant persistence and retrieval.
- **Context Accessors**: Access and manage the current tenant context throughout the application lifecycle.
- **Error Handling**: Custom exceptions for multi-tenant scenarios.
- **.NET 10 & C# 13**: Utilizes the latest language and framework features.

## Usage Example

### Basic Usage with Lightweight ICurrentTenant
```csharp
// In projects that only need basic tenant info (reference Craft.Core only)
using Craft.Core;

public class MyService
{
    private readonly ICurrentTenant _currentTenant;
    
    public MyService(ICurrentTenant currentTenant) 
        => _currentTenant = currentTenant;

    public string? GetTenantInfo() 
    {
        if (_currentTenant.IsAvailable)
        {
            return $"Tenant: {_currentTenant.Name} ({_currentTenant.Identifier})";
        }
        return "No tenant resolved";
    }
}
```

### Advanced Usage with Full Tenant Details
```csharp
// In projects that need full tenant entity (reference Craft.MultiTenant)
using Craft.MultiTenant;

public class TenantManagementService
{
    private readonly ITenantContextAccessor _accessor;
    
    public TenantManagementService(ITenantContextAccessor accessor) 
        => _accessor = accessor;

    public string? GetConnectionString() 
    {
        var tenant = _accessor.TenantContext?.Tenant;
        return tenant?.ConnectionString;
    }
}
```

### Registration
```csharp
// Register multi-tenant services
services.AddMultiTenant()
    .WithHostStrategy()
    .WithInMemoryStore();
```

## Key Components
- `ICurrentTenant` (in Craft.Core): Lightweight interface for accessing current tenant information across all projects.
- `ITenant`, `IMultiTenant`, `ITenantStore`, `ITenantStrategy`: Core abstractions for multi-tenancy.
- `CurrentTenant`: Service that implements `ICurrentTenant` by wrapping the tenant context accessor.
- `HostStrategy`, `HeaderStrategy`, `ClaimStrategy`, etc.: Strategies for tenant resolution.
- `InMemoryStore`, `ConfigurationStore`, `DbStore`, etc.: Tenant persistence mechanisms.
- `MultiTenantException`: Exception for multi-tenant errors.

## Architecture

The library follows a clean separation of concerns:

1. **Craft.Core**: Contains the lightweight `ICurrentTenant` interface for cross-cutting access to tenant info
2. **Craft.MultiTenant**: Contains the full multi-tenancy infrastructure including:
   - Full `ITenant` entity with all properties (connection strings, database strategy, etc.)
   - `CurrentTenant` service that implements `ICurrentTenant`
   - Tenant resolution strategies and stores
   - Context management

This design allows projects to:
- Reference only `Craft.Core` if they just need to know "who is the current tenant"
- Reference `Craft.MultiTenant` when they need full tenant management capabilities

## Integration
Craft.MultiTenant is designed to be referenced by ASP.NET Core and .NET projects that require tenant-aware logic, data isolation, and flexible tenant identification.

## License
This project is licensed under the MIT License. See the `LICENSE` file for details.

---
For more details, review the source code and XML documentation in the project.

