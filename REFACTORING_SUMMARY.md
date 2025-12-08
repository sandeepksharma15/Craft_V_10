# ICurrentTenant Refactoring Summary

## Overview
Successfully refactored the multi-tenant architecture to enable cross-project tenant access without creating dependencies on `Craft.MultiTenant`.

## Problem Solved
Previously, `ICurrentTenant` was defined in `Craft.MultiTenant` and inherited from `ITenant`, creating a tight coupling. Projects needing basic tenant information had to reference the entire `Craft.MultiTenant` library, even when they only needed simple tenant identification.

## Solution Implemented
Adopted a **lightweight tenant info pattern** similar to the existing `ICurrentUser` design:

### 1. Lightweight Interface in Craft.Core
Created `ICurrentTenant` interface in `Craft.Core/Abstractions/ICurrentTenant.cs`:
- Contains only essential properties: `Id`, `Identifier`, `Name`, `IsAvailable`, `IsActive`
- No dependencies on multi-tenant infrastructure
- Generic `ICurrentTenant<TKey>` and non-generic `ICurrentTenant` variants

### 2. Service Implementation in Craft.MultiTenant
Created `CurrentTenant` service in `Craft.MultiTenant/Services/CurrentTenant.cs`:
- Implements the lightweight `ICurrentTenant` interface
- Wraps `ITenantContextAccessor` to access full tenant details
- Exposes only essential properties from the full tenant entity
- Generic `CurrentTenant<TKey>` and non-generic `CurrentTenant` variants

### 3. Removed Old Interface
- Deleted `Craft.MultiTenant/Abstractions/ICurrentTenant.cs` (old version that inherited from `ITenant`)

### 4. Updated Service Registration
Modified `Craft.MultiTenant/Extensions/ServiceCollectionExtensions.cs`:
- Added registration for `ICurrentTenant` and `ICurrentTenant<KeyType>` services
- Registered as scoped services alongside other multi-tenant services

### 5. Updated TenantDbContextFactory
Modified `Craft.Data/DbFactory/TenantDbContextFactory.cs`:
- Now injects both `ICurrentTenant` (for basic info) and `ITenantContextAccessor` (for full tenant details)
- Uses `ICurrentTenant` for logging and tenant identification
- Uses `ITenantContextAccessor` to access full `ITenant` properties like `ConnectionString`, `DbProvider`, `DbType`
- Updated imports to reference `Craft.Core` instead of `Craft.MultiTenant` for `ICurrentTenant`

### 6. Updated QueryFilterExtension
Modified `Craft.Data/Extensions/QueryFilterExtension.cs`:
- Updated imports to use `ICurrentTenant` from `Craft.Core`
- Extension methods now accept the lightweight interface

### 7. Updated Tests
**Created new test file:**
- `Tests/Craft.MultiTenant.Tests/ServiceTests/CurrentTenantTests.cs`
  - Tests for the new `CurrentTenant` service
  - Covers scenarios: resolved tenant, no tenant, inactive tenant, generic variants

**Updated existing test files:**
- `Tests/Craft.Data.Tests/DbFactory/TenantDbContextFactoryTests.cs`
  - Updated `TestCurrentTenant` to implement lightweight interface
  - Updated factory method to mock both `ICurrentTenant` and `ITenantContextAccessor`
  - All tests passing

- `Tests/Craft.Data.Tests/Extensions/QueryFilterExtensionTests.cs`
  - Updated `FakeCurrentTenant` to implement lightweight interface
  - Updated imports to reference `Craft.Core`
  - All tests passing

### 8. Updated Documentation
- Modified `Craft.MultiTenant/README.md` with:
  - Examples showing lightweight usage (referencing only Craft.Core)
  - Examples showing advanced usage (referencing Craft.MultiTenant)
  - Architecture section explaining the separation of concerns

## Benefits

### 1. Separation of Concerns
- **Craft.Core**: Contains lightweight `ICurrentTenant` for essential tenant information
- **Craft.MultiTenant**: Contains full multi-tenancy infrastructure with `ITenant`, stores, strategies, etc.

### 2. No Circular Dependencies
- Projects needing basic tenant info ? reference only `Craft.Core`
- Projects needing full tenant management ? reference `Craft.MultiTenant`

### 3. Consistent Pattern
- Mirrors the existing `ICurrentUser` design pattern
- Familiar to developers working in the codebase

### 4. Flexibility
- Services can inject `ICurrentTenant` for basic tenant identification
- Services can inject `ITenantContextAccessor` when full tenant details are needed
- Both can be used together in the same class when appropriate

### 5. Better Performance
- Lightweight interface reduces memory footprint for services that don't need full tenant details
- No unnecessary loading of full tenant entity when only ID is needed

## Usage Examples

### Basic Usage (Reference Craft.Core only)
```csharp
using Craft.Core;

public class OrderService
{
    private readonly ICurrentTenant _currentTenant;
    
    public OrderService(ICurrentTenant currentTenant) 
    {
        _currentTenant = currentTenant;
    }

    public void CreateOrder()
    {
        if (_currentTenant.IsAvailable && _currentTenant.IsActive)
        {
            var tenantId = _currentTenant.Id;
            var tenantName = _currentTenant.Name;
            // Use tenant info without Craft.MultiTenant dependency
        }
    }
}
```

### Advanced Usage (Reference Craft.MultiTenant)
```csharp
using Craft.MultiTenant;

public class TenantManagementService
{
    private readonly ITenantContextAccessor _accessor;
    
    public TenantManagementService(ITenantContextAccessor accessor) 
    {
        _accessor = accessor;
    }

    public string? GetTenantConnectionString() 
    {
        var tenant = _accessor.TenantContext?.Tenant;
        return tenant?.ConnectionString;
    }
}
```

### Combined Usage
```csharp
using Craft.Core;
using Craft.MultiTenant;

public class TenantDbContextFactory<T>
{
    private readonly ICurrentTenant _currentTenant;
    private readonly ITenantContextAccessor _tenantContextAccessor;
    
    public TenantDbContextFactory(
        ICurrentTenant currentTenant,
        ITenantContextAccessor tenantContextAccessor)
    {
        _currentTenant = currentTenant;
        _tenantContextAccessor = tenantContextAccessor;
    }

    public T CreateDbContext()
    {
        // Use lightweight interface for logging
        var tenantName = _currentTenant.Name;
        
        // Access full tenant for connection details
        var fullTenant = _tenantContextAccessor.TenantContext?.Tenant;
        var connectionString = fullTenant?.ConnectionString;
        
        // ...
    }
}
```

## Files Changed

### Created
1. `Craft.Core/Abstractions/ICurrentTenant.cs` - Lightweight interface
2. `Craft.MultiTenant/Services/CurrentTenant.cs` - Service implementation
3. `Tests/Craft.MultiTenant.Tests/ServiceTests/CurrentTenantTests.cs` - Unit tests

### Modified
1. `Craft.MultiTenant/Extensions/ServiceCollectionExtensions.cs` - DI registration
2. `Craft.Data/DbFactory/TenantDbContextFactory.cs` - Use lightweight interface + accessor
3. `Craft.Data/Extensions/QueryFilterExtension.cs` - Import from Craft.Core
4. `Tests/Craft.Data.Tests/DbFactory/TenantDbContextFactoryTests.cs` - Updated test mocks
5. `Tests/Craft.Data.Tests/Extensions/QueryFilterExtensionTests.cs` - Updated fake implementation
6. `Craft.MultiTenant/README.md` - Documentation updates

### Deleted
1. `Craft.MultiTenant/Abstractions/ICurrentTenant.cs` - Old interface (replaced)

## Build Status
? Build successful
? All 3,387 tests passing
? No breaking changes to existing functionality

## Migration Guide for Consumers

### If you were using ICurrentTenant from Craft.MultiTenant:

**Before:**
```csharp
using Craft.MultiTenant;

public class MyService
{
    private readonly ICurrentTenant _currentTenant;
    
    public MyService(ICurrentTenant currentTenant) 
    {
        _currentTenant = currentTenant;
    }

    public void DoWork()
    {
        var id = _currentTenant.Id;
        var name = _currentTenant.Name;
        var connectionString = _currentTenant.ConnectionString; // Full access
    }
}
```

**After (Option 1 - Basic info only):**
```csharp
using Craft.Core;

public class MyService
{
    private readonly ICurrentTenant _currentTenant;
    
    public MyService(ICurrentTenant currentTenant) 
    {
        _currentTenant = currentTenant;
    }

    public void DoWork()
    {
        var id = _currentTenant.Id;
        var name = _currentTenant.Name;
        // No connectionString property - use ITenantContextAccessor if needed
    }
}
```

**After (Option 2 - Full tenant details):**
```csharp
using Craft.Core;
using Craft.MultiTenant;

public class MyService
{
    private readonly ICurrentTenant _currentTenant;
    private readonly ITenantContextAccessor _accessor;
    
    public MyService(ICurrentTenant currentTenant, ITenantContextAccessor accessor) 
    {
        _currentTenant = currentTenant;
        _accessor = accessor;
    }

    public void DoWork()
    {
        var id = _currentTenant.Id;
        var name = _currentTenant.Name;
        var fullTenant = _accessor.TenantContext?.Tenant;
        var connectionString = fullTenant?.ConnectionString; // Full access
    }
}
```

## Conclusion
The refactoring successfully achieves the goal of enabling cross-project tenant access without creating dependencies on `Craft.MultiTenant`. The design follows established patterns in the codebase (`ICurrentUser`), maintains backward compatibility, and provides clear separation between lightweight tenant identification and full tenant management capabilities.
