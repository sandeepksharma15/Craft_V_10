# Craft.MultiTenant

A comprehensive multi-tenancy library for .NET 10 applications that provides flexible tenant identification strategies, data isolation, and tenant-specific configuration management.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Installation](#installation)
- [Core Concepts](#core-concepts)
- [Quick Start](#quick-start)
- [Configuration](#configuration)
- [Strategies](#strategies)
- [Stores](#stores)
- [Advanced Usage](#advanced-usage)
- [Best Practices](#best-practices)
- [Architecture](#architecture)
- [API Reference](#api-reference)

## Overview

Craft.MultiTenant enables SaaS applications to support multiple tenants with isolated data and resources. It provides a pluggable architecture for tenant identification (strategies) and tenant data storage (stores).

### Key Benefits

- **Flexible Tenant Resolution**: Multiple built-in strategies for identifying tenants
- **Pluggable Architecture**: Easy to extend with custom strategies and stores
- **Data Isolation**: Support for shared, per-tenant, and hybrid database models
- **Performance**: Built-in caching support
- **Type-Safe**: Full generic type support
- **Integration**: Seamless integration with ASP.NET Core and Entity Framework Core

## Features

- ? 10+ built-in tenant identification strategies
- ? Multiple tenant store implementations (In-Memory, EF Core, Configuration, Cache, Remote API)
- ? Generic and non-generic API support
- ? Tenant context per HTTP request
- ? Tenant-based data filtering
- ? Event-driven architecture for tenant resolution
- ? Support for multiple database types (Shared, Per-Tenant, Hybrid)
- ? Comprehensive test coverage (177 tests)

## Installation

### Package Reference

```xml
<PackageReference Include="Craft.MultiTenant" Version="1.0.0" />
```

### Dependencies

- .NET 10.0
- Microsoft.AspNetCore.App
- Microsoft.EntityFrameworkCore (10.0.1)
- Craft.Core
- Craft.Domain
- Craft.Utilities
- Craft.Cache

## Core Concepts

### Tenant

A tenant represents an isolated customer/organization in your application. Each tenant has:

- **Id**: Unique numeric identifier
- **Identifier**: Unique string identifier (for URL/API usage)
- **Name**: Display name
- **ConnectionString**: Database connection (for per-tenant databases)
- **DbProvider**: Database provider type
- **DbType**: Database isolation model (Shared/PerTenant/Hybrid)
- **Type**: Tenant type (Tenant/Host/Both)
- **IsActive**: Active status
- **ValidUpTo**: Subscription expiry date

### Strategy

A strategy determines how to identify the current tenant from an HTTP request. Strategies can use:
- Host/subdomain
- URL path
- HTTP headers
- Route parameters
- Query strings
- Session
- Claims
- Custom logic

### Store

A store provides access to tenant data. Stores can retrieve tenant information from:
- In-memory collections
- EF Core databases
- Configuration files
- Cache
- Remote APIs
- Custom sources

## Quick Start

### 1. Basic Setup

```csharp
// Program.cs or Startup.cs
using Craft.MultiTenant;

// Register multi-tenant services
builder.Services
    .AddMultiTenant<Tenant>()
    .WithHostStrategy()
    .WithInMemoryStore(options =>
    {
        options.Tenants.Add(new Tenant
        {
            Id = 1,
            Identifier = "tenant1",
            Name = "Tenant One",
            IsActive = true
        });
        options.Tenants.Add(new Tenant
        {
            Id = 2,
            Identifier = "tenant2",
            Name = "Tenant Two",
            IsActive = true
        });
    });

// Add middleware
var app = builder.Build();
app.UseMultiTenant();
```

### 2. Access Current Tenant

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
        }
    }
}
```

### 3. Access Full Tenant Details

```csharp
using Craft.MultiTenant;

public class TenantService
{
    private readonly ITenantContextAccessor _accessor;
    
    public TenantService(ITenantContextAccessor accessor)
    {
        _accessor = accessor;
    }
    
    public string? GetConnectionString()
    {
        var tenant = _accessor.TenantContext?.Tenant;
        return tenant?.ConnectionString;
    }
}
```

## Configuration

### App Settings Configuration

```json
{
  "MultiTenancy": {
    "ConfigurationStore": {
      "Defaults": {
        "ConnectionString": "Server=localhost;Database=DefaultDb;"
      },
      "Tenants": [
        {
          "Id": "1",
          "Identifier": "tenant1",
          "Name": "Tenant One",
          "ConnectionString": "Server=localhost;Database=Tenant1Db;"
        },
        {
          "Id": "2",
          "Identifier": "tenant2",
          "Name": "Tenant Two",
          "ConnectionString": "Server=localhost;Database=Tenant2Db;"
        }
      ]
    }
  }
}
```

### Using Configuration Store

```csharp
builder.Services
    .AddMultiTenant<Tenant>()
    .WithHostStrategy()
    .WithConfigurationStore();
```

### Multi-Tenant Options

```csharp
builder.Services
    .AddMultiTenant<Tenant>(options =>
    {
        options.IgnoredIdentifiers.Add("localhost");
        options.IgnoredIdentifiers.Add("admin");
        
        options.Events.OnTenantResolved = async context =>
        {
            var tenant = context.Tenant;
            await Task.CompletedTask;
        };
        
        options.Events.OnTenantNotResolved = async context =>
        {
            var identifier = context.Identifier;
            await Task.CompletedTask;
        };
    })
    .WithHostStrategy()
    .WithInMemoryStore();
```

## Strategies

### Host Strategy

Extracts tenant identifier from the host/domain name.

```csharp
builder.Services
    .AddMultiTenant<Tenant>()
    .WithHostStrategy("__TENANT__.example.com")
    .WithInMemoryStore();
```

**Templates:**
- `__TENANT__` - Entire host as identifier
- `__TENANT__.*` - First segment (subdomain)
- `*.__TENANT__.?` - Second-to-last segment
- `__TENANT__.example.com` - Subdomain with specific domain

**Examples:**
- `tenant1.example.com` ? `tenant1`
- `api.tenant1.example.com` ? `tenant1` (with `*.__TENANT__.?` template)

### Subdomain Strategy

Extracts tenant from subdomain (simplified version of HostStrategy).

```csharp
builder.Services
    .AddMultiTenant<Tenant>()
    .WithSubDomainStrategy()
    .WithInMemoryStore();
```

**Examples:**
- `tenant1.example.com` ? `tenant1`
- `example.com` ? null

### Base Path Strategy

Extracts tenant from the first segment of the URL path.

```csharp
builder.Services
    .AddMultiTenant<Tenant>()
    .WithBasePathStrategy()
    .WithInMemoryStore();
```

**Examples:**
- `/tenant1/api/orders` ? `tenant1`
- `/tenant2/products` ? `tenant2`

### Route Strategy

Extracts tenant from route parameters.

```csharp
builder.Services
    .AddMultiTenant<Tenant>()
    .WithRouteStrategy("tenantId")
    .WithInMemoryStore();
```

**Route Definition:**
```csharp
app.MapGet("/{tenantId}/api/orders", (string tenantId) => { });
```

### Header Strategy

Extracts tenant from HTTP request header.

```csharp
builder.Services
    .AddMultiTenant<Tenant>()
    .WithHeaderStrategy("X-Tenant-Id")
    .WithInMemoryStore();
```

**Request Header:**
```
X-Tenant-Id: tenant1
```

### Query String Strategy

Extracts tenant from query string parameter.

```csharp
builder.Services
    .AddMultiTenant<Tenant>()
    .WithQueryStringStrategy("tenant")
    .WithInMemoryStore();
```

**Examples:**
- `/api/orders?tenant=tenant1` ? `tenant1`

### Session Strategy

Extracts tenant from session state.

```csharp
builder.Services.AddSession();

builder.Services
    .AddMultiTenant<Tenant>()
    .WithSessionStrategy("TenantId")
    .WithInMemoryStore();

app.UseSession();
app.UseMultiTenant();
```

### Claim Strategy

Extracts tenant from user claims after authentication.

```csharp
builder.Services
    .AddMultiTenant<Tenant>()
    .WithClaimStrategy("tenant_id", "Bearer")
    .WithInMemoryStore();
```

### Static Strategy

Always returns a fixed tenant identifier (useful for testing).

```csharp
builder.Services
    .AddMultiTenant<Tenant>()
    .WithStaticStrategy("tenant1")
    .WithInMemoryStore();
```

### Delegate Strategy

Custom tenant resolution logic.

```csharp
builder.Services
    .AddMultiTenant<Tenant>()
    .WithDelegateStrategy(context =>
    {
        if (context is HttpContext httpContext)
        {
            var customHeader = httpContext.Request.Headers["X-Custom-Tenant"];
            return Task.FromResult(customHeader.ToString());
        }
        return Task.FromResult<string?>(null);
    })
    .WithInMemoryStore();
```

### Multiple Strategies

Strategies are evaluated in priority order until a tenant is found.

```csharp
builder.Services
    .AddMultiTenant<Tenant>()
    .WithHeaderStrategy()
    .WithRouteStrategy()
    .WithHostStrategy()
    .WithStaticStrategy("default")
    .WithInMemoryStore();
```

## Stores

### In-Memory Store

Simple in-memory tenant storage.

```csharp
builder.Services
    .AddMultiTenant<Tenant>()
    .WithHostStrategy()
    .WithInMemoryStore(options =>
    {
        options.IsCaseSensitive = false;
        options.Tenants.Add(new Tenant
        {
            Id = 1,
            Identifier = "tenant1",
            Name = "Tenant One"
        });
    });
```

### Configuration Store

Loads tenants from appsettings.json with hot-reload support.

```csharp
builder.Services
    .AddMultiTenant<Tenant>()
    .WithHostStrategy()
    .WithConfigurationStore();
```

### EF Core Store

Stores tenants in a database using Entity Framework Core.

**Step 1: Create DbContext**

```csharp
public class TenantDbContext : DbContext, ITenantDbContext<Tenant>
{
    public DbSet<Tenant> Tenants { get; set; }
    
    public TenantDbContext(DbContextOptions<TenantDbContext> options)
        : base(options)
    {
    }
}
```

**Step 2: Register Store**

```csharp
builder.Services
    .AddMultiTenant<Tenant>()
    .WithHostStrategy()
    .WithEfCoreStore<TenantDbContext>();
```

### Cache Store

Wraps another store with caching for better performance.

```csharp
builder.Services
    .AddMultiTenant<Tenant>()
    .WithHostStrategy()
    .WithEfCoreStore<TenantDbContext>()
    .WithCacheStore();
```

### Remote API Store

Fetches tenant data from a remote HTTP API.

```csharp
builder.Services
    .AddMultiTenant<Tenant>()
    .WithHostStrategy()
    .WithRemoteApiStore("https://api.example.com/tenants/{__TENANT__}",
        client => client.AddHttpMessageHandler<AuthHandler>());
```

### Multiple Stores

Stores are checked in order until a tenant is found.

```csharp
builder.Services
    .AddMultiTenant<Tenant>()
    .WithHostStrategy()
    .WithCacheStore()
    .WithInMemoryStore()
    .WithEfCoreStore<TenantDbContext>();
```

## Advanced Usage

### Custom Tenant Model

```csharp
public class CustomTenant : Tenant
{
    public string CustomProperty { get; set; }
    public int MaxUsers { get; set; }
}

builder.Services
    .AddMultiTenant<CustomTenant>()
    .WithHostStrategy()
    .WithInMemoryStore();
```

### Manual Tenant Setting

```csharp
public class TenantController : ControllerBase
{
    [HttpPost("switch/{tenantId}")]
    public IActionResult SwitchTenant(string tenantId)
    {
        var tenant = new Tenant
        {
            Id = 1,
            Identifier = tenantId,
            Name = "Switched Tenant"
        };
        
        HttpContext.SetTenant(tenant, resetServiceProviderScope: true);
        
        return Ok();
    }
}
```

### Access Tenant Context

```csharp
public class OrderController : ControllerBase
{
    [HttpGet]
    public IActionResult GetOrders()
    {
        var tenantContext = HttpContext.GetTenantContext();
        
        var tenant = tenantContext.Tenant;
        var strategy = tenantContext.Strategy;
        var store = tenantContext.Store;
        
        return Ok(new
        {
            TenantId = tenant?.Id,
            TenantName = tenant?.Name,
            ResolvedBy = strategy?.GetType().Name
        });
    }
}
```

### Tenant-Specific Database Context

```csharp
public class AppDbContext : DbContext
{
    private readonly ITenantContextAccessor _tenantAccessor;
    
    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ITenantContextAccessor tenantAccessor)
        : base(options)
    {
        _tenantAccessor = tenantAccessor;
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var tenant = _tenantAccessor.TenantContext?.Tenant;
        
        if (tenant?.DbType == TenantDbType.PerTenant)
        {
            optionsBuilder.UseSqlServer(tenant.ConnectionString);
        }
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var tenant = _tenantAccessor.TenantContext?.Tenant;
        
        if (tenant?.DbType == TenantDbType.Shared)
        {
            modelBuilder.Entity<Order>().HasQueryFilter(
                o => o.TenantId == tenant.Id);
        }
    }
}
```

### Tenant-Specific Services

```csharp
builder.Services.AddScoped(sp =>
{
    var tenantAccessor = sp.GetRequiredService<ITenantContextAccessor>();
    var tenant = tenantAccessor.TenantContext?.Tenant;
    
    return new EmailService(tenant?.EmailSettings);
});
```

## Best Practices

### 1. Use ICurrentTenant for Lightweight Access

Reference only `Craft.Core` when you only need basic tenant information:

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
        if (_currentTenant.IsAvailable)
        {
            var tenantId = _currentTenant.Id;
        }
    }
}
```

### 2. Use ITenantContextAccessor for Full Access

Reference `Craft.MultiTenant` when you need full tenant details:

```csharp
using Craft.MultiTenant;

public class TenantService
{
    private readonly ITenantContextAccessor _accessor;
    
    public TenantService(ITenantContextAccessor accessor)
    {
        _accessor = accessor;
    }
    
    public string? GetConnectionString()
    {
        return _accessor.TenantContext?.Tenant?.ConnectionString;
    }
}
```

### 3. Combine Both When Needed

```csharp
using Craft.Core;
using Craft.MultiTenant;

public class DataService
{
    private readonly ICurrentTenant _currentTenant;
    private readonly ITenantContextAccessor _accessor;
    
    public DataService(
        ICurrentTenant currentTenant,
        ITenantContextAccessor accessor)
    {
        _currentTenant = currentTenant;
        _accessor = accessor;
    }
    
    public DbContext CreateDbContext()
    {
        var tenantId = _currentTenant.Id;
        
        var fullTenant = _accessor.TenantContext?.Tenant;
        var connectionString = fullTenant?.ConnectionString;
        
        return new AppDbContext(connectionString);
    }
}
```

### 4. Handle Missing Tenants

```csharp
public class OrderService
{
    private readonly ICurrentTenant _currentTenant;
    
    public void CreateOrder()
    {
        if (!_currentTenant.IsAvailable)
        {
            throw new InvalidOperationException("No tenant context available");
        }
        
        if (!_currentTenant.IsActive)
        {
            throw new InvalidOperationException("Tenant is not active");
        }
    }
}
```

### 5. Ignore Specific Identifiers

```csharp
builder.Services
    .AddMultiTenant<Tenant>(options =>
    {
        options.IgnoredIdentifiers.Add("localhost");
        options.IgnoredIdentifiers.Add("admin");
        options.IgnoredIdentifiers.Add("www");
    })
    .WithHostStrategy()
    .WithInMemoryStore();
```

### 6. Use Events for Custom Logic

```csharp
builder.Services
    .AddMultiTenant<Tenant>(options =>
    {
        options.Events.OnTenantResolved = async context =>
        {
            var logger = context.Context.RequestServices
                .GetRequiredService<ILogger<Program>>();
            
            logger.LogInformation(
                "Tenant {TenantId} resolved using {Strategy}",
                context.Tenant?.Identifier,
                context.StrategyType?.Name);
            
            await Task.CompletedTask;
        };
    })
    .WithHostStrategy()
    .WithInMemoryStore();
```

## Architecture

### Component Overview

```
???????????????????????????????????????????????????????????????
?                      HTTP Request                            ?
???????????????????????????????????????????????????????????????
                     ?
                     ?
???????????????????????????????????????????????????????????????
?                  TenantMiddleware                            ?
?  - Calls TenantResolver for each request                    ?
?  - Stores result in TenantContextAccessor                   ?
???????????????????????????????????????????????????????????????
                     ?
                     ?
???????????????????????????????????????????????????????????????
?                  TenantResolver                              ?
?  - Iterates through registered Strategies                   ?
?  - Gets identifier from first successful Strategy           ?
?  - Queries Stores with identifier                           ?
?  - Returns TenantContext with Tenant data                   ?
???????????????????????????????????????????????????????????????
        ?                                          ?
        ?                                          ?
????????????????????                    ????????????????????????
?   Strategies     ?                    ?      Stores          ?
????????????????????                    ????????????????????????
? - HostStrategy   ?                    ? - InMemoryStore      ?
? - RouteStrategy  ?                    ? - EfCoreStore        ?
? - HeaderStrategy ?                    ? - ConfigStore        ?
? - ClaimStrategy  ?                    ? - CacheStore         ?
? - SessionStrategy?                    ? - RemoteApiStore     ?
? - Custom...      ?                    ? - Custom...          ?
????????????????????                    ????????????????????????
```

### Tenant Resolution Flow

1. **Request arrives** at TenantMiddleware
2. **Strategies are evaluated** in priority order
3. **First strategy** that returns an identifier wins
4. **Stores are queried** with the identifier
5. **First store** that finds the tenant wins
6. **TenantContext is created** with Tenant, Strategy, and Store
7. **Context is stored** in TenantContextAccessor (AsyncLocal)
8. **Services can access** tenant via ICurrentTenant or ITenantContextAccessor

### Separation of Concerns

- **Craft.Core**: Contains lightweight `ICurrentTenant` interface
- **Craft.MultiTenant**: Contains full multi-tenancy infrastructure
- **Application Code**: References only what it needs

## API Reference

### Interfaces

#### ICurrentTenant

Lightweight interface for accessing basic tenant information (defined in Craft.Core).

```csharp
public interface ICurrentTenant<TKey>
{
    TKey Id { get; }
    string? Identifier { get; }
    string? Name { get; }
    bool IsAvailable { get; }
    bool IsActive { get; }
    TKey GetId();
}
```

#### ITenant

Full tenant entity interface.

```csharp
public interface ITenant<TKey>
{
    TKey Id { get; set; }
    string Identifier { get; set; }
    string Name { get; set; }
    string AdminEmail { get; set; }
    string LogoUri { get; set; }
    string ConnectionString { get; set; }
    string DbProvider { get; set; }
    TenantType Type { get; set; }
    TenantDbType DbType { get; set; }
    DateTime ValidUpTo { get; set; }
    bool IsActive { get; set; }
    bool IsDeleted { get; set; }
}
```

#### ITenantContext

Context containing resolved tenant information.

```csharp
public interface ITenantContext<T>
{
    T? Tenant { get; set; }
    ITenantStore<T>? Store { get; set; }
    ITenantStrategy? Strategy { get; set; }
    bool HasResolvedTenant { get; }
}
```

#### ITenantContextAccessor

Accessor for tenant context (AsyncLocal storage).

```csharp
public interface ITenantContextAccessor<T>
{
    ITenantContext<T>? TenantContext { get; set; }
}
```

#### ITenantStrategy

Interface for tenant identification strategies.

```csharp
public interface ITenantStrategy
{
    int Priority { get; }
    Task<string?> GetIdentifierAsync(HttpContext context);
}
```

#### ITenantStore

Interface for tenant data stores.

```csharp
public interface ITenantStore<T>
{
    Task<T?> GetByIdentifierAsync(string identifier, bool includeDetails = false, CancellationToken cancellationToken = default);
    Task<T?> GetAsync(long id, bool includeDetails = false, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(bool includeDetails = false, CancellationToken cancellationToken = default);
    Task<T?> GetHostAsync(bool includeDetails = false, CancellationToken cancellationToken = default);
    Task<long> GetCountAsync(CancellationToken cancellationToken = default);
    Task<T?> AddAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default);
    Task<T> UpdateAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default);
    Task<T?> DeleteAsync(T entity, bool autoSave = true, CancellationToken cancellationToken = default);
}
```

### Enums

#### TenantType

```csharp
[Flags]
public enum TenantType : byte
{
    None = 0,
    Tenant = 1,
    Host = 2,
    Both = Tenant | Host
}
```

#### TenantDbType

```csharp
[Flags]
public enum TenantDbType
{
    None = 0,
    Shared = 1,
    PerTenant = 2,
    Hybrid = Shared | PerTenant
}
```

### Extension Methods

#### ServiceCollectionExtensions

```csharp
services.AddMultiTenant<Tenant>();
services.AddMultiTenant<Tenant>(options => { });
```

#### ApplicationBuilderExtensions

```csharp
app.UseMultiTenant();
```

#### HttpContextExtensions

```csharp
var context = httpContext.GetTenantContext();
var context = httpContext.GetTenantContext<Tenant>();
httpContext.SetTenant(tenant, resetServiceProviderScope: true);
```

### TenantBuilder Methods

```csharp
builder.WithHostStrategy();
builder.WithSubDomainStrategy();
builder.WithBasePathStrategy();
builder.WithRouteStrategy("paramName");
builder.WithHeaderStrategy("headerName");
builder.WithQueryStringStrategy("queryParam");
builder.WithSessionStrategy("sessionKey");
builder.WithClaimStrategy("claimType", "authScheme");
builder.WithStaticStrategy("identifier");
builder.WithDelegateStrategy(func);

builder.WithInMemoryStore();
builder.WithConfigurationStore();
builder.WithEfCoreStore<TDbContext>();
builder.WithCacheStore();
builder.WithRemoteApiStore("endpoint");
```

## License

Copyright © 2025 Sandeep SHARMA

---

**Version**: 1.0.0  
**Target Framework**: .NET 10.0  
**Last Updated**: 2025-01-21
