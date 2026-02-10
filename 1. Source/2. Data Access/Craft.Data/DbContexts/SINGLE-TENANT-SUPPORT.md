# Single-Tenant / Non-Multi-Tenant DbContext Support

## Overview
Craft.Data now provides simplified constructors for `BaseDbContext` and `BaseIdentityDbContext` that automatically handle non-multi-tenant scenarios. You no longer need to create and pass `NullTenant` instances manually.

## Feature: Automatic NullTenant Handling

### What Changed

**Before:**
```csharp
public class AppDbContext : BaseIdentityDbContext<AppDbContext, AppUser, AppRole, KeyType>
{
    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUser currentUser)
        : base(options, new NullTenant(), currentUser)  // ❌ Manual NullTenant creation
    {
        // Configure features...
    }
}
```

**After:**
```csharp
public class AppDbContext : BaseIdentityDbContext<AppDbContext, AppUser, AppRole, KeyType>
{
    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUser currentUser)
        : base(options, currentUser)  // ✅ No NullTenant needed!
    {
        // Configure features...
    }
}
```

### Implementation Details

#### 1. NullTenant Singleton
A singleton `NullTenant` instance is now available in Craft.Data:

```csharp
// Craft.Data.NullTenant
public sealed class NullTenant : ITenant
{
    /// <summary>
    /// Singleton instance. Use this instead of creating new instances.
    /// </summary>
    public static readonly NullTenant Instance = new();
    
    private NullTenant() { }  // Private constructor
    
    // Default implementations...
}
```

#### 2. Constructor Overloads

Both `BaseDbContext<TContext>` and `BaseIdentityDbContext<TContext, TUser, TRole, TKey>` now have two constructors:

**For Single-Tenant Applications:**
```csharp
protected BaseDbContext(DbContextOptions<TContext> options, ICurrentUser currentUser)
    : this(options, NullTenant.Instance, currentUser)
{
}
```

**For Multi-Tenant Applications:**
```csharp
protected BaseDbContext(
    DbContextOptions<TContext> options, 
    ITenant currentTenant,  // Explicit tenant parameter
    ICurrentUser currentUser)
    : base(options)
{
    _currentTenant = currentTenant;
    _currentUser = currentUser;
}
```

## Usage Guide

### Single-Tenant Application (No Multi-Tenancy)

Simply omit the `ITenant` parameter:

```csharp
public class MyDbContext : BaseDbContext<MyDbContext>
{
    public MyDbContext(DbContextOptions<MyDbContext> options, ICurrentUser currentUser)
        : base(options, currentUser)  // ✅ Clean and simple
    {
        // Configure features
        Features
            .AddSoftDelete()
            .AddAuditTrail()
            .AddVersionTracking();
    }
}
```

### Multi-Tenant Application

Pass the tenant explicitly:

```csharp
public class TenantDbContext : BaseDbContext<TenantDbContext>
{
    public TenantDbContext(
        DbContextOptions<TenantDbContext> options,
        ITenant currentTenant,  // Tenant is required
        ICurrentUser currentUser)
        : base(options, currentTenant, currentUser)
    {
        // Configure features
        Features
            .AddSoftDelete()
            .AddAuditTrail()
            .AddMultiTenancy(currentTenant);  // Enable multi-tenancy
    }
}
```

### Identity DbContext (Single-Tenant)

Same simplification for Identity-based contexts:

```csharp
public class AppDbContext : BaseIdentityDbContext<AppDbContext, AppUser, AppRole, long>
{
    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUser currentUser)
        : base(options, currentUser)  // ✅ No NullTenant needed
    {
        Features
            .AddSoftDelete()
            .AddAuditTrail()
            .AddIdentity<AppUser, AppRole, long>();
    }
}
```

## Benefits

### 1. **Cleaner Code**
- No need to create `NullTenant` instances
- Reduced boilerplate in constructors
- More intuitive API for single-tenant apps

### 2. **Better Performance**
- Singleton pattern avoids repeated allocations
- No GC pressure from temporary `NullTenant` objects

### 3. **Clearer Intent**
- Constructor signature clearly indicates single-tenant vs multi-tenant
- No ambiguity about tenant handling

### 4. **Backward Compatibility**
- Existing multi-tenant code continues to work
- Migration is opt-in and straightforward

## Migration Guide

### Step 1: Remove NullTenant Creation

**Before:**
```csharp
public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUser currentUser)
    : base(options, new NullTenant(), currentUser)
```

**After:**
```csharp
public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUser currentUser)
    : base(options, currentUser)
```

### Step 2: Remove Custom NullTenant Class (Optional)

If you created a custom `NullTenant` class in your application, you can now delete it:

```csharp
// ❌ Delete this if you no longer need it
public class NullTenant : ITenant
{
    // ...
}
```

### Step 3: Update Using Statements (If Needed)

If you had `using` statements for your custom `NullTenant`, you can remove them:

```csharp
// ❌ Remove if no longer needed
using GccPT.Shared.Models;  // Where your NullTenant was
```

## Advanced Usage

### Accessing the NullTenant Instance

If you ever need to reference the `NullTenant` instance directly:

```csharp
using Craft.Data;

var nullTenant = NullTenant.Instance;
Console.WriteLine(nullTenant.Identifier);  // "default"
Console.WriteLine(nullTenant.Name);        // "Default Tenant"
```

### Conditional Multi-Tenancy

You can conditionally enable multi-tenancy based on configuration:

```csharp
public class ConfigurableDbContext : BaseDbContext<ConfigurableDbContext>
{
    public ConfigurableDbContext(
        DbContextOptions<ConfigurableDbContext> options,
        ICurrentUser currentUser,
        ITenant? tenant = null)  // Optional tenant
        : base(options, tenant ?? NullTenant.Instance, currentUser)
    {
        if (tenant != null && tenant != NullTenant.Instance)
        {
            Features.AddMultiTenancy(tenant);
        }
    }
}
```

## Troubleshooting

### Issue: Ambiguous Reference Error

**Error:**
```
CS0104: 'NullTenant' is an ambiguous reference between 'Craft.Data.NullTenant' and 'YourApp.NullTenant'
```

**Solution:**
Either:
1. Remove your custom `NullTenant` class (recommended)
2. Or fully qualify the reference: `new Craft.Data.NullTenant()`
3. Or use the simpler constructor that doesn't need `NullTenant` at all

### Issue: Multi-Tenancy Not Working

**Problem:** Multi-tenant features aren't isolating data by tenant

**Solution:** Make sure you're:
1. Using the multi-tenant constructor with an `ITenant` parameter
2. Calling `Features.AddMultiTenancy(tenant)` in your constructor
3. Implementing `IHasTenant` on your entities

## Technical Details

### NullTenant Properties

```csharp
Property             | Value
---------------------|-------------------
Id                   | default(KeyType)
Identifier           | "default"
Name                 | "Default Tenant"
AdminEmail           | ""
LogoUri              | ""
ConnectionString     | ""
DbProvider           | ""
Type                 | TenantType.Host
DbType               | TenantDbType.Shared
ValidUpTo            | DateTime.MaxValue
IsDeleted            | false
ConcurrencyStamp     | ""
IsActive             | true
```

### Constructor Resolution

The C# compiler automatically selects the appropriate constructor based on parameters:

```csharp
// Two parameters → Single-tenant constructor
: base(options, currentUser)

// Three parameters → Multi-tenant constructor
: base(options, currentTenant, currentUser)
```

## Related Features

- **Multi-Tenancy Feature**: `Features.AddMultiTenancy(tenant)`
- **Tenant Context**: `ITenant` and `ICurrentTenant`
- **Tenant Isolation**: `IHasTenant` interface for entities

## Version History

- **v1.0.33.0** - Added `NullTenant` singleton and simplified constructors
- **v1.0.32.9** - Original implementation requiring manual `NullTenant` creation

## See Also

- [Multi-Tenancy Documentation](../Craft.MultiTenant/README.md)
- [BaseDbContext Documentation](./DbContexts/README.md)
- [Feature Configuration Guide](./DbContextFeatures/README.md)
