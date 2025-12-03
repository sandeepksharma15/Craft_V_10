# Craft.Cache - Legacy Method Removal Migration Guide

## Overview

As of version 3.0, the legacy synchronous cache methods have been **completely removed** from Craft.Cache. This guide will help you migrate your code to use the modern async methods.

## Removed Methods

The following methods have been removed from `ICacheService`:

| Removed Method | Replacement | Notes |
|---------------|-------------|-------|
| `void Remove(string key)` | `Task<CacheResult> RemoveAsync(string key, CancellationToken ct = default)` | Async with result |
| `T? Set<T>(string key, T? value)` | `Task<CacheResult> SetAsync<T>(string key, T? value, CacheEntryOptions? options = null, CancellationToken ct = default)` | Async with options |
| `(bool, T?) TryGet<T>(string key)` | `Task<CacheResult<T>> GetAsync<T>(string key, CancellationToken ct = default)` | Async with CacheResult |

## Migration Examples

### 1. Removing Items

**Before (Removed):**
```csharp
_cache.Remove("product:123");
```

**After (Async):**
```csharp
await _cache.RemoveAsync("product:123");

// With error handling
var result = await _cache.RemoveAsync("product:123");
if (!result.IsSuccess)
{
    _logger.LogError("Failed to remove cache entry: {Error}", result.ErrorMessage);
}
```

### 2. Setting Items

**Before (Removed):**
```csharp
_cache.Set("product:123", product);
```

**After (Async):**
```csharp
await _cache.SetAsync("product:123", product);

// With options
await _cache.SetAsync("product:123", product, 
    CacheEntryOptions.WithExpiration(TimeSpan.FromMinutes(10)));

// With error handling
var result = await _cache.SetAsync("product:123", product);
if (!result.IsSuccess)
{
    _logger.LogError("Failed to set cache entry: {Error}", result.ErrorMessage);
}
```

### 3. Getting Items

**Before (Removed):**
```csharp
var (hasValue, product) = _cache.TryGet<Product>("product:123");
if (hasValue)
{
    return product;
}
```

**After (Async):**
```csharp
var result = await _cache.GetAsync<Product>("product:123");
if (result.HasValue)
{
    return result.Value;
}

// With error handling
var result = await _cache.GetAsync<Product>("product:123");
if (result.IsSuccess && result.HasValue)
{
    return result.Value;
}
else if (!result.IsSuccess)
{
    _logger.LogError("Cache error: {Error}", result.ErrorMessage);
}
```

### 4. Get or Set Pattern

**Before (Multiple Operations):**
```csharp
var (hasValue, product) = _cache.TryGet<Product>("product:123");
if (!hasValue)
{
    product = await _repository.GetByIdAsync(123);
    _cache.Set("product:123", product);
}
return product;
```

**After (Single Operation):**
```csharp
return await _cache.GetOrSetAsync(
    "product:123",
    async () => await _repository.GetByIdAsync(123),
    CacheEntryOptions.WithExpiration(TimeSpan.FromMinutes(10)));
```

## Common Migration Patterns

### Pattern 1: Service Method with Cache

**Before:**
```csharp
public Product? GetProduct(int id)
{
    var (hasValue, product) = _cache.TryGet<Product>($"product:{id}");
    if (hasValue)
        return product;
    
    product = _repository.GetById(id);
    _cache.Set($"product:{id}", product);
    return product;
}
```

**After:**
```csharp
public async Task<Product?> GetProductAsync(int id)
{
    return await _cache.GetOrSetAsync(
        $"product:{id}",
        async () => await _repository.GetByIdAsync(id));
}
```

### Pattern 2: Update with Cache Invalidation

**Before:**
```csharp
public void UpdateProduct(Product product)
{
    _repository.Update(product);
    _cache.Remove($"product:{product.Id}");
}
```

**After:**
```csharp
public async Task UpdateProductAsync(Product product)
{
    await _repository.UpdateAsync(product);
    await _cache.RemoveAsync($"product:{product.Id}");
}
```

### Pattern 3: Conditional Caching

**Before:**
```csharp
public Product? GetProduct(int id, bool useCache)
{
    Product? product = null;
    
    if (useCache)
    {
        var (hasValue, cached) = _cache.TryGet<Product>($"product:{id}");
        if (hasValue)
            return cached;
    }
    
    product = _repository.GetById(id);
    
    if (useCache)
        _cache.Set($"product:{id}", product);
    
    return product;
}
```

**After:**
```csharp
public async Task<Product?> GetProductAsync(int id, bool useCache)
{
    if (useCache)
    {
        return await _cache.GetOrSetAsync(
            $"product:{id}",
            async () => await _repository.GetByIdAsync(id));
    }
    
    return await _repository.GetByIdAsync(id);
}
```

## Breaking Changes Summary

### ?? Removed

- `void Remove(string cacheKey)`
- `T? Set<T>(string cacheKey, T? value)`  
- `(bool, T?) TryGet<T>(string cacheKey)`

### ? Use Instead

- `Task<CacheResult> RemoveAsync(string cacheKey, CancellationToken ct = default)`
- `Task<CacheResult> SetAsync<T>(string cacheKey, T? value, CacheEntryOptions? options = null, CancellationToken ct = default)`
- `Task<CacheResult<T>> GetAsync<T>(string cacheKey, CancellationToken ct = default)`
- `Task<T?> GetOrSetAsync<T>(string cacheKey, Func<Task<T>> valueFactory, CacheEntryOptions? options = null, CancellationToken ct = default)`

## Benefits of Migration

### 1. **Proper Async/Await**
- No more blocking calls with `.GetAwaiter().GetResult()`
- Better scalability and performance
- Proper cancellation token support

### 2. **Better Error Handling**
- `CacheResult` and `CacheResult<T>` provide detailed error information
- Distinguish between cache misses and errors
- Access to exception details when needed

### 3. **Modern API Design**
- Consistent async patterns throughout
- Better integration with modern .NET applications
- Follows .NET async best practices

### 4. **Enhanced Features**
- Flexible cache entry options
- Support for cancellation tokens
- Bulk operations (GetManyAsync, SetManyAsync)
- Pattern-based removal
- Cache statistics

## Finding Legacy Usage

To find legacy method usage in your codebase, search for:

```bash
# PowerShell
Get-ChildItem -Recurse -Include *.cs | Select-String "\.TryGet<|\.Set\(|\.Remove\(" | Where-Object { $_.Line -like "*_cache*" -or $_.Line -like "*cacheService*" }

# Or use your IDE's find feature to search for:
# - .TryGet<
# - _cache.Set(
# - _cache.Remove(
# - cacheService.Set(
# - cacheService.Remove(
```

## Need Help?

If you encounter issues during migration:

1. Check the [QUICKSTART.md](./QUICKSTART.md) for examples
2. Review the [README.md](./README.md) for comprehensive documentation
3. Look at the integration tests in `Tests/Craft.Cache.Tests/CacheServiceIntegrationTests.cs`
4. File an issue on the project repository

---

**Migration Status:** ? All legacy methods removed in v3.0  
**Last Updated:** January 2025  
**Target Framework:** .NET 10
