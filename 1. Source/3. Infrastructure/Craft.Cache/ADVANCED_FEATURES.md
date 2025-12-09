# Craft.Cache - Advanced Features Guide

## Table of Contents
- [Cache Key Generation](#cache-key-generation)
- [Cache Attributes](#cache-attributes)
- [Cache Invalidation](#cache-invalidation)
- [Invalidation Strategies](#invalidation-strategies)
- [Real-World Examples](#real-world-examples)

---

## Cache Key Generation

The `ICacheKeyGenerator` provides consistent, collision-resistant cache key generation.

### Basic Usage

```csharp
public class ProductService
{
    private readonly ICacheService _cache;
    private readonly ICacheKeyGenerator _keyGenerator;

    public ProductService(ICacheService cache, ICacheKeyGenerator keyGenerator)
    {
        _cache = cache;
        _keyGenerator = keyGenerator;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        // Generate entity key: "craft:Product:123"
        var key = _keyGenerator.GenerateEntityKey<Product>(id);
        
        return await _cache.GetOrSetAsync(
            key,
            async () => await _repository.GetByIdAsync(id));
    }
}
```

### Key Generation Methods

```csharp
// Entity key generation
var key = _keyGenerator.GenerateEntityKey<Product>(123);
// Result: "craft:Product:123"

// Method key generation with parameters
var key = _keyGenerator.GenerateMethodKey("ProductService", "Search", "electronics", 10);
// Result: "craft:ProductService:Search:a3f5b8c4d2e1f9a7"

// Pattern generation for bulk operations
var pattern = _keyGenerator.GenerateEntityPattern<Product>();
// Result: "craft:Product:*"

var pattern = _keyGenerator.GenerateEntityPattern<Product>("category:*");
// Result: "craft:Product:category:*"

// Type-based key generation
var key = _keyGenerator.Generate(typeof(Product), "active", "category:electronics");
// Result: "craft:Product:b8c4d2e1f9a7a3f5"
```

---

## Cache Attributes

Declarative caching using attributes (requires AOP interceptor - coming in future update).

### [Cacheable] Attribute

```csharp
public class ProductService
{
    // Cache for 1 hour
    [Cacheable(ExpirationSeconds = 3600)]
    public async Task<Product?> GetProductAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    // Cache with sliding expiration
    [Cacheable(
        ExpirationSeconds = 3600,
        SlidingExpirationSeconds = 900)] // Refresh on access
    public async Task<List<Product>> GetActiveProductsAsync()
    {
        return await _repository.GetActiveAsync();
    }

    // Custom cache key prefix
    [Cacheable(
        KeyPrefix = "product-search",
        ExpirationSeconds = 600)]
    public async Task<List<Product>> SearchAsync(string term, int categoryId)
    {
        return await _repository.SearchAsync(term, categoryId);
    }

    // Include only specific arguments in cache key
    [Cacheable(
        KeyArguments = ["userId", "categoryId"],
        ExpirationSeconds = 1800)]
    public async Task<List<Product>> GetRecommendedAsync(
        int userId,
        int categoryId,
        [CacheKeyIgnore] bool includeDetails) // Ignored in key
    {
        return await _repository.GetRecommendedAsync(userId, categoryId, includeDetails);
    }

    // Conditional caching
    [Cacheable(
        Condition = "IsSuccessful", // Only cache if result.IsSuccessful == true
        ExpirationSeconds = 600)]
    public async Task<OperationResult<Product>> CreateProductAsync(Product product)
    {
        return await _repository.CreateAsync(product);
    }
}
```

### [CacheInvalidate] Attribute

```csharp
public class ProductService
{
    // Invalidate specific keys after update
    [CacheInvalidate(
        Keys = ["product:{id}"],
        Timing = CacheInvalidationTiming.After)]
    public async Task UpdateProductAsync(int id, Product product)
    {
        await _repository.UpdateAsync(product);
    }

    // Invalidate by pattern
    [CacheInvalidate(
        Keys = ["product:category:*"],
        IsPattern = true)]
    public async Task UpdateCategoryAsync(int categoryId, Category category)
    {
        await _repository.UpdateAsync(category);
    }

    // Invalidate entire entity type
    [CacheInvalidate(
        EntityType = typeof(Product),
        Timing = CacheInvalidationTiming.Both)]
    public async Task ImportProductsAsync(List<Product> products)
    {
        await _repository.BulkInsertAsync(products);
    }

    // Invalidate only on success
    [CacheInvalidate(
        Keys = ["product:{id}"],
        OnlyOnSuccess = true)]
    public async Task DeleteProductAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }

    // Multiple invalidations
    [CacheInvalidate(Keys = ["product:{productId}"])]
    [CacheInvalidate(Keys = ["product:category:*"], IsPattern = true)]
    public async Task ChangeProductCategoryAsync(int productId, int newCategoryId)
    {
        await _repository.UpdateCategoryAsync(productId, newCategoryId);
    }
}
```

---

## Cache Invalidation

The `ICacheInvalidator` provides powerful invalidation capabilities.

### Basic Invalidation

```csharp
public class ProductService
{
    private readonly ICacheInvalidator _invalidator;

    // Invalidate single entity
    public async Task UpdateProductAsync(Product product)
    {
        await _repository.UpdateAsync(product);
        await _invalidator.InvalidateEntityAsync<Product>(product.Id);
    }

    // Invalidate entire entity type
    public async Task ImportProductsAsync(List<Product> products)
    {
        await _repository.BulkInsertAsync(products);
        await _invalidator.InvalidateEntityTypeAsync<Product>();
    }

    // Invalidate specific keys
    public async Task UpdateMultipleAsync(List<int> ids)
    {
        await _repository.UpdateRangeAsync(ids);
        var keys = ids.Select(id => $"product:{id}");
        await _invalidator.InvalidateKeysAsync(keys);
    }

    // Invalidate by pattern
    public async Task UpdateCategoryAsync(int categoryId)
    {
        await _repository.UpdateCategoryAsync(categoryId);
        await _invalidator.InvalidatePatternAsync($"product:category:{categoryId}:*");
    }
}
```

---

## Invalidation Strategies

Use strategies for complex invalidation scenarios.

### EntityTypeInvalidationStrategy

```csharp
// Invalidate all products
var strategy = new EntityTypeInvalidationStrategy<Product>(_keyGenerator);
var count = await _invalidator.InvalidateAsync(strategy);
// Invalidates: "craft:Product:*"
```

### EntityIdInvalidationStrategy

```csharp
// Invalidate specific products
var strategy = new EntityIdInvalidationStrategy<Product>(_keyGenerator, 1, 2, 3);
var count = await _invalidator.InvalidateAsync(strategy);
// Invalidates: "craft:Product:1", "craft:Product:2", "craft:Product:3"
```

### PatternInvalidationStrategy

```csharp
// Invalidate multiple patterns
var strategy = new PatternInvalidationStrategy(
    "product:*",
    "category:*",
    "user:cart:*");
var count = await _invalidator.InvalidateAsync(strategy);
```

### DependentEntityInvalidationStrategy

```csharp
// Invalidate related entities
var strategy = new DependentEntityInvalidationStrategy(_keyGenerator)
    .AddDependency<Product>(1, 2, 3)        // Specific products
    .AddDependency<Order>(orderId)           // Specific order
    .AddTypeDependency<Category>();          // All categories

var count = await _invalidator.InvalidateAsync(strategy);
```

---

## Real-World Examples

### E-Commerce Product Service

```csharp
public class ProductService
{
    private readonly IRepository<Product> _repository;
    private readonly ICacheService _cache;
    private readonly ICacheKeyGenerator _keyGenerator;
    private readonly ICacheInvalidator _invalidator;

    public ProductService(
        IRepository<Product> repository,
        ICacheService cache,
        ICacheKeyGenerator keyGenerator,
        ICacheInvalidator invalidator)
    {
        _repository = repository;
        _cache = cache;
        _keyGenerator = keyGenerator;
        _invalidator = invalidator;
    }

    // Get product with caching
    public async Task<Product?> GetByIdAsync(int id)
    {
        var key = _keyGenerator.GenerateEntityKey<Product>(id);
        
        return await _cache.GetOrSetAsync(
            key,
            async () => await _repository.GetByIdAsync(id),
            CacheEntryOptions.WithExpiration(TimeSpan.FromHours(1)));
    }

    // Get products by category with caching
    public async Task<List<Product>> GetByCategoryAsync(int categoryId)
    {
        var key = _keyGenerator.GenerateMethodKey(
            nameof(ProductService),
            nameof(GetByCategoryAsync),
            categoryId);

        return await _cache.GetOrSetAsync(
            key,
            async () => await _repository.GetByCategoryAsync(categoryId),
            CacheEntryOptions.WithSlidingExpiration(TimeSpan.FromMinutes(30)));
    }

    // Update product and invalidate related caches
    public async Task UpdateAsync(Product product)
    {
        await _repository.UpdateAsync(product);

        // Invalidate product cache
        await _invalidator.InvalidateEntityAsync<Product>(product.Id);

        // Invalidate category listings
        await _invalidator.InvalidatePatternAsync(
            $"*{nameof(GetByCategoryAsync)}*");
    }

    // Change category and invalidate dependent caches
    public async Task ChangeCategoryAsync(int productId, int oldCategoryId, int newCategoryId)
    {
        await _repository.UpdateCategoryAsync(productId, newCategoryId);

        // Use strategy for complex invalidation
        var strategy = new DependentEntityInvalidationStrategy(_keyGenerator)
            .AddDependency<Product>(productId)
            .AddDependency<Category>(oldCategoryId, newCategoryId);

        await _invalidator.InvalidateAsync(strategy);
    }

    // Bulk import with full cache clear
    public async Task BulkImportAsync(List<Product> products)
    {
        await _repository.BulkInsertAsync(products);
        
        // Clear all product-related caches
        await _invalidator.InvalidateEntityTypeAsync<Product>();
    }
}
```

### User Service with Session Caching

```csharp
public class UserService
{
    private readonly IUserRepository _repository;
    private readonly ICacheService _cache;
    private readonly ICacheKeyGenerator _keyGenerator;
    private readonly ICacheInvalidator _invalidator;

    // Get user profile (cached for 30 min)
    public async Task<UserProfile?> GetProfileAsync(int userId)
    {
        var key = _keyGenerator.GenerateEntityKey<UserProfile>(userId);
        
        return await _cache.GetOrSetAsync(
            key,
            async () => await _repository.GetProfileAsync(userId),
            CacheEntryOptions.WithSlidingExpiration(TimeSpan.FromMinutes(30)));
    }

    // Get user permissions (cached for 5 min)
    public async Task<List<string>> GetPermissionsAsync(int userId)
    {
        var key = _keyGenerator.GenerateMethodKey(
            nameof(UserService),
            nameof(GetPermissionsAsync),
            userId);

        return await _cache.GetOrSetAsync(
            key,
            async () => await _repository.GetPermissionsAsync(userId),
            CacheEntryOptions.WithExpiration(TimeSpan.FromMinutes(5)));
    }

    // Update user and invalidate all related caches
    public async Task UpdateUserAsync(User user)
    {
        await _repository.UpdateAsync(user);

        // Invalidate user-specific caches
        var strategy = new PatternInvalidationStrategy(
            _keyGenerator.GenerateEntityPattern<UserProfile>($"{user.Id}"),
            $"*{nameof(GetPermissionsAsync)}*{user.Id}*");

        await _invalidator.InvalidateAsync(strategy);
    }

    // Update permissions and invalidate permission cache
    public async Task UpdatePermissionsAsync(int userId, List<string> permissions)
    {
        await _repository.UpdatePermissionsAsync(userId, permissions);
        
        // Invalidate permission cache specifically
        var key = _keyGenerator.GenerateMethodKey(
            nameof(UserService),
            nameof(GetPermissionsAsync),
            userId);

        await _cache.RemoveAsync(key);
    }
}
```

### Multi-Level Cache with Strategies

```csharp
public class OrderService
{
    private readonly IOrderRepository _repository;
    private readonly ICacheService _cache;
    private readonly ICacheKeyGenerator _keyGenerator;
    private readonly ICacheInvalidator _invalidator;

    // Get order with related entities
    public async Task<OrderDetails?> GetOrderDetailsAsync(int orderId)
    {
        var key = _keyGenerator.GenerateEntityKey<OrderDetails>(orderId);
        
        return await _cache.GetOrSetAsync(
            key,
            async () => await _repository.GetOrderDetailsAsync(orderId),
            CacheEntryOptions.WithExpiration(TimeSpan.FromHours(1)));
    }

    // Update order and invalidate related caches
    public async Task UpdateOrderAsync(Order order)
    {
        await _repository.UpdateAsync(order);

        // Invalidate order and all related entities
        var strategy = new DependentEntityInvalidationStrategy(_keyGenerator)
            .AddDependency<OrderDetails>(order.Id)
            .AddDependency<Product>(order.ProductId)
            .AddDependency<Customer>(order.CustomerId);

        var invalidatedCount = await _invalidator.InvalidateAsync(strategy);
        _logger.LogInformation("Invalidated {Count} cache entries for order {OrderId}", 
            invalidatedCount, order.Id);
    }

    // Cancel order and clear customer-specific caches
    public async Task CancelOrderAsync(int orderId)
    {
        var order = await _repository.GetByIdAsync(orderId);
        await _repository.CancelAsync(orderId);

        // Clear order and customer order history
        var strategy = new PatternInvalidationStrategy(
            _keyGenerator.GenerateEntityKey<OrderDetails>(orderId),
            $"*CustomerOrderHistory*{order.CustomerId}*");

        await _invalidator.InvalidateAsync(strategy);
    }
}
```

---

## Best Practices

### 1. Use KeyGenerator for Consistency

```csharp
// ? Good - Consistent, collision-resistant
var key = _keyGenerator.GenerateEntityKey<Product>(id);

// ? Bad - Manual string construction
var key = $"product:{id}";
```

### 2. Choose Appropriate Expiration

```csharp
// Frequently changing data - short expiration
var key = _keyGenerator.GenerateEntityKey<StockLevel>(productId);
await _cache.SetAsync(key, stock, 
    CacheEntryOptions.WithExpiration(TimeSpan.FromMinutes(1)));

// Rarely changing data - long expiration
var key = _keyGenerator.GenerateEntityKey<Category>(categoryId);
await _cache.SetAsync(key, category, 
    CacheEntryOptions.WithExpiration(TimeSpan.FromHours(24)));

// Session data - sliding expiration
var key = _keyGenerator.GenerateEntityKey<UserSession>(sessionId);
await _cache.SetAsync(key, session, 
    CacheEntryOptions.WithSlidingExpiration(TimeSpan.FromMinutes(30)));
```

### 3. Use Strategies for Complex Invalidation

```csharp
// ? Good - Clean, maintainable
var strategy = new DependentEntityInvalidationStrategy(_keyGenerator)
    .AddDependency<Product>(productId)
    .AddDependency<Category>(categoryId)
    .AddTypeDependency<ProductReview>();

await _invalidator.InvalidateAsync(strategy);

// ? Bad - Manual, error-prone
await _cache.RemoveAsync($"product:{productId}");
await _cache.RemoveAsync($"category:{categoryId}");
await _cache.RemoveByPatternAsync("product-review:*");
```

### 4. Monitor Invalidation

```csharp
public async Task UpdateProductAsync(Product product)
{
    await _repository.UpdateAsync(product);

    var strategy = new EntityIdInvalidationStrategy<Product>(_keyGenerator, product.Id);
    var count = await _invalidator.InvalidateAsync(strategy);

    _logger.LogInformation(
        "Product {ProductId} updated. Invalidated {Count} cache entries",
        product.Id, count);
}
```

---

## Configuration

These services are automatically registered when you call `AddCacheServices`:

```csharp
// Program.cs
builder.Services.AddCacheServices(builder.Configuration);

// Services registered:
// - ICacheService
// - ICacheKeyGenerator
// - ICacheInvalidator
// - ICacheProviderFactory
// - ICacheProvider implementations
```

---

**Last Updated:** January 2025  
**Version:** 3.0  
**Target Framework:** .NET 10
