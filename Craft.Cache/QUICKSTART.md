# Craft.Cache - Quick Reference Guide

> **Version:** 2.0+ | **Target Framework:** .NET 10

## ?? Table of Contents

1. [Quick Start](#-quick-start)
2. [Configuration](#-configuration)
3. [Cache Providers](#-cache-providers)
4. [Basic Operations](#-basic-operations)
5. [Advanced Operations](#-advanced-operations)
6. [Features](#-features)
7. [Migration from v1.x](#-migration-from-v1x)
8. [Best Practices](#-best-practices)

---

## ?? Quick Start

### Basic Setup

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add cache services with automatic validation
builder.Services.AddCacheServices(builder.Configuration);

var app = builder.Build();
app.Run();
```

### Configuration (appsettings.json)

```json
{
  "CacheOptions": {
    "Provider": "memory",
    "DefaultExpiration": "01:00:00",
    "DefaultSlidingExpiration": "00:30:00",
    "EnableStatistics": true,
    "KeyPrefix": "craft:",
    "Memory": {
      "SizeLimit": 1024,
      "CompactionPercentage": 0.25
    }
  }
}
```

### Use the Cache

```csharp
public class ProductService
{
    private readonly ICacheService _cache;

    public ProductService(ICacheService cache)
    {
        _cache = cache;
    }

    public async Task<Product?> GetProductAsync(int id)
    {
        return await _cache.GetOrSetAsync(
            $"product:{id}",
            async () => await _repository.GetByIdAsync(id));
    }
}
```

---

## ?? Configuration

### CacheOptions Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Provider` | string | "memory" | Cache provider ("memory", "redis", "hybrid", "null") |
| `DefaultExpiration` | TimeSpan | 1 hour | Default cache entry expiration |
| `DefaultSlidingExpiration` | TimeSpan? | 30 min | Default sliding expiration |
| `EnableStatistics` | bool | true | Enable cache statistics tracking |
| `KeyPrefix` | string | "craft:" | Prefix for all cache keys |

### Memory Cache Settings

```json
{
  "CacheOptions": {
    "Memory": {
      "SizeLimit": 1024,
      "CompactionPercentage": 0.25,
      "ExpirationScanFrequency": "00:01:00"
    }
  }
}
```

### Redis Cache Settings

```json
{
  "CacheOptions": {
    "Provider": "redis",
    "Redis": {
      "ConnectionString": "localhost:6379",
      "Database": 0,
      "InstanceName": "craft:",
      "ConnectTimeout": 5000,
      "SyncTimeout": 5000,
      "RetryCount": 3,
      "UseSsl": false
    }
  }
}
```

### Configuration Methods

#### Method 1: From appsettings.json (Recommended)

```csharp
builder.Services.AddCacheServices(builder.Configuration);
```

#### Method 2: Programmatic Configuration

```csharp
builder.Services.AddCacheServices(options =>
{
    options.Provider = "memory";
    options.DefaultExpiration = TimeSpan.FromMinutes(30);
    options.EnableStatistics = true;
    options.KeyPrefix = "myapp:";
});
```

---

## ?? Cache Providers

### Built-in Providers

#### 1. Memory Provider (Default)

Fast in-memory caching for single-instance applications.

```json
{
  "CacheOptions": {
    "Provider": "memory"
  }
}
```

#### 2. Redis Provider

Distributed caching for multi-instance applications.

```json
{
  "CacheOptions": {
    "Provider": "redis",
    "Redis": {
      "ConnectionString": "localhost:6379"
    }
  }
}
```

#### 3. Null Provider

No-op provider for testing or disabling cache.

```json
{
  "CacheOptions": {
    "Provider": "null"
  }
}
```

### Custom Providers

```csharp
public class CustomCacheProvider : ICacheProvider
{
    public string Name => "custom";
    
    public bool IsConfigured() => true;
    
    // Implement other methods...
}

// Register
builder.Services
    .AddCacheServices(builder.Configuration)
    .AddCacheProvider<CustomCacheProvider>();
```

---

## ?? Basic Operations

### Get from Cache

```csharp
var (hasValue, product) = _cache.TryGet<Product>("product:1");
if (hasValue)
{
    return product;
}

// Or async
var result = await _cache.GetAsync<Product>("product:1");
if (result.HasValue)
{
    return result.Value;
}
```

### Set to Cache

```csharp
// Simple set
_cache.Set("product:1", product);

// With custom expiration
_cache.Set("product:1", product, TimeSpan.FromMinutes(5));

// With custom options
var options = new CacheEntryOptions
{
    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
    SlidingExpiration = TimeSpan.FromMinutes(15),
    Priority = CacheItemPriority.High
};
_cache.Set("product:1", product, options);

// Async
await _cache.SetAsync("product:1", product, options);
```

### Get or Set

```csharp
var product = await _cache.GetOrSetAsync(
    "product:1",
    async () => await _repository.GetByIdAsync(1));

// With custom options
var product = await _cache.GetOrSetAsync(
    "product:1",
    async () => await _repository.GetByIdAsync(1),
    CacheEntryOptions.WithExpiration(TimeSpan.FromMinutes(5)));
```

### Remove from Cache

```csharp
_cache.Remove("product:1");

// Or async
await _cache.RemoveAsync("product:1");
```

---

## ?? Advanced Operations

### Bulk Operations

```csharp
// Get many
var keys = new[] { "product:1", "product:2", "product:3" };
var products = await _cache.GetManyAsync<Product>(keys);

// Set many
var items = new Dictionary<string, Product>
{
    ["product:1"] = product1,
    ["product:2"] = product2,
    ["product:3"] = product3
};
await _cache.SetManyAsync(items);
```

### Pattern-Based Removal

```csharp
// Remove all products
await _cache.RemoveByPatternAsync("product:*");

// Remove all user sessions
await _cache.RemoveByPatternAsync("session:user:*");
```

### Cache Statistics

```csharp
var stats = await _cache.GetStatsAsync();
Console.WriteLine($"Hit Ratio: {stats.HitRatio:P2}");
Console.WriteLine($"Total Hits: {stats.Hits}");
Console.WriteLine($"Total Misses: {stats.Misses}");
Console.WriteLine($"Total Entries: {stats.EntryCount}");
```

### Check Key Existence

```csharp
if (await _cache.ExistsAsync("product:1"))
{
    // Key exists
}
```

### Clear Cache

```csharp
await _cache.ClearAsync();
```

### Refresh Cache Entry

```csharp
// Reset expiration time
await _cache.RefreshAsync("product:1");
```

---

## ? Features

### ? Implemented Features

- ? **Multiple Cache Providers**: Memory, Redis, Null, extensible
- ? **Provider Factory Pattern**: Dynamic provider selection
- ? **Async-First API**: All operations support cancellation
- ? **Bulk Operations**: GetMany, SetMany for performance
- ? **Pattern Matching**: Remove entries by wildcard pattern
- ? **Cache Statistics**: Real-time monitoring and metrics
- ? **Flexible Expiration**: Absolute, sliding, per-entry
- ? **Configuration Validation**: Automatic validation at startup
- ? **Comprehensive Logging**: Detailed operation logging
- ? **Extension Methods**: Clean DI registration
- ? **Backward Compatible**: Legacy API still works
- ? **Type-Safe**: Strongly-typed cache operations
- ? **Key Prefixing**: Namespace isolation

---

## ?? Migration from v1.x

### Breaking Changes

The old `MemoryCacheService` and `RedisCacheService` are now **obsolete**:

```
Warning CS0618: 'MemoryCacheService' is obsolete: 'This class is deprecated. Use ICacheService with MemoryCacheProvider instead.'
```

### Migration Steps

#### Step 1: Update Configuration

**Old (v1.x):**
```csharp
services.AddSingleton<ICacheService, MemoryCacheService>();
```

**New (v2.0+):**
```csharp
services.AddCacheServices(configuration);
```

#### Step 2: No Code Changes Needed!

The `ICacheService` interface is backward compatible. Your existing code continues to work:

```csharp
public class ProductService
{
    private readonly ICacheService _cache;
    
    // Same constructor
    public ProductService(ICacheService cache)
    {
        _cache = cache;
    }
    
    // Same methods work
    public async Task<Product?> GetProductAsync(int id)
    {
        return await _cache.GetOrSetAsync(
            $"product:{id}",
            async () => await _repository.GetByIdAsync(id));
    }
}
```

#### Step 3: (Optional) Use New Features

Take advantage of new features when you're ready:

```csharp
// Use new async methods
var result = await _cache.GetAsync<Product>("product:1");
if (result.IsSuccess && result.HasValue)
{
    return result.Value;
}

// Use bulk operations
var products = await _cache.GetManyAsync<Product>(productIds);

// Use pattern removal
await _cache.RemoveByPatternAsync("product:*");

// Get statistics
var stats = await _cache.GetStatsAsync();
```

---

## ?? Best Practices

### 1. Use Consistent Key Naming

```csharp
// Good
"product:123"
"user:session:abc"
"order:pending:456"

// Bad
"Product123"
"user_abc"
"pending_order_456"
```

### 2. Set Appropriate Expiration Times

```csharp
// Frequently changing data
await _cache.SetAsync("stock:123", stock, 
    CacheEntryOptions.WithExpiration(TimeSpan.FromMinutes(1)));

// Rarely changing data
await _cache.SetAsync("category:5", category, 
    CacheEntryOptions.WithExpiration(TimeSpan.FromHours(24)));

// Session data
await _cache.SetAsync($"session:{userId}", session, 
    CacheEntryOptions.WithSlidingExpiration(TimeSpan.FromMinutes(30)));
```

### 3. Use GetOrSet for Simplicity

```csharp
// Good - simple and clean
var product = await _cache.GetOrSetAsync(
    $"product:{id}",
    async () => await _repository.GetByIdAsync(id));

// Avoid - more code, same result
var (hasValue, product) = _cache.TryGet<Product>($"product:{id}");
if (!hasValue)
{
    product = await _repository.GetByIdAsync(id);
    _cache.Set($"product:{id}", product);
}
```

### 4. Invalidate Cache on Updates

```csharp
public async Task UpdateProductAsync(Product product)
{
    await _repository.UpdateAsync(product);
    await _cache.RemoveAsync($"product:{product.Id}");
}

public async Task DeleteProductsByCategoryAsync(int categoryId)
{
    await _repository.DeleteByCategoryAsync(categoryId);
    await _cache.RemoveByPatternAsync($"product:category:{categoryId}:*");
}
```

### 5. Monitor Cache Performance

```csharp
// Log statistics periodically
var stats = await _cache.GetStatsAsync();
_logger.LogInformation(
    "Cache Stats - Hit Ratio: {HitRatio:P2}, Hits: {Hits}, Misses: {Misses}, Entries: {Entries}",
    stats.HitRatio, stats.Hits, stats.Misses, stats.EntryCount);
```

### 6. Use Null Provider for Testing

```csharp
// appsettings.Testing.json
{
  "CacheOptions": {
    "Provider": "null"
  }
}
```

### 7. Environment-Specific Configuration

```csharp
// appsettings.Development.json
{
  "CacheOptions": {
    "Provider": "memory"
  }
}

// appsettings.Production.json
{
  "CacheOptions": {
    "Provider": "redis",
    "Redis": {
      "ConnectionString": "production-redis:6379"
    }
  }
}
```

---

## ?? Common Patterns

### Repository Pattern with Caching

```csharp
public class CachedProductRepository : IProductRepository
{
    private readonly IProductRepository _innerRepository;
    private readonly ICacheService _cache;
    
    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _cache.GetOrSetAsync(
            $"product:{id}",
            async () => await _innerRepository.GetByIdAsync(id),
            CacheEntryOptions.WithExpiration(TimeSpan.FromMinutes(10)));
    }
    
    public async Task UpdateAsync(Product product)
    {
        await _innerRepository.UpdateAsync(product);
        await _cache.RemoveAsync($"product:{product.Id}");
    }
}
```

### Cache-Aside Pattern

```csharp
public async Task<Product?> GetProductAsync(int id)
{
    // Try cache first
    var result = await _cache.GetAsync<Product>($"product:{id}");
    if (result.HasValue)
        return result.Value;
    
    // Get from database
    var product = await _repository.GetByIdAsync(id);
    
    // Update cache
    if (product != null)
        await _cache.SetAsync($"product:{id}", product);
    
    return product;
}
```

### Write-Through Cache

```csharp
public async Task SaveProductAsync(Product product)
{
    // Update database
    await _repository.SaveAsync(product);
    
    // Update cache immediately
    await _cache.SetAsync($"product:{product.Id}", product);
}
```

---

**Last Updated:** January 2025  
**Version:** 2.0  
**Target Framework:** .NET 10  
**Status:** ? Production Ready

---

For more details, see the full [README.md](./README.md) with comprehensive documentation.
