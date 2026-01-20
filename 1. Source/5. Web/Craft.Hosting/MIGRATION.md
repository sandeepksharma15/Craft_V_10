# Migration Guide: Craft.QuerySpec.Extensions → Craft.Hosting

## Overview

All dependency injection extension methods have been moved from `Craft.QuerySpec.Extensions` to the new `Craft.Hosting` package to provide a centralized, well-organized location for service registration.

## Why This Change?

**Problems Solved:**
- ✅ **Single Entry Point**: External projects only need to reference `Craft.Hosting`
- ✅ **Clear Separation**: DI registrations are separate from implementation concerns
- ✅ **No Circular Dependencies**: Clean dependency graph
- ✅ **Better Discoverability**: All extensions in one place
- ✅ **Convenience Methods**: Simplified registration for common scenarios

## Migration Steps

### 1. Update Project Reference

**Remove:**
```xml
<ItemGroup>
  <ProjectReference Include="..\..\Craft_V_10\1. Source\2. Data Access\Craft.QuerySpec\Craft.QuerySpec.csproj" />
</ItemGroup>
```

**Add:**
```xml
<ItemGroup>
  <ProjectReference Include="..\..\Craft_V_10\1. Source\5. Web\Craft.Hosting\Craft.Hosting.csproj" />
</ItemGroup>
```

### 2. Update Using Statements

**Before:**
```csharp
using Craft.QuerySpec.Extensions;
```

**After:**
```csharp
using Craft.Hosting.Extensions;
```

### 3. Update Service Registrations (Optional - Use Convenience Methods)

**Before (Explicit):**
```csharp
services.AddTransientHttpService<Location, LocationVM, LocationDTO>(
    httpClientFactory,
    apiClientOptions.BaseAddress,
    "/api/Location",
    registerPrimaryInterface: false,
    registerWithKeyType: true,
    registerSimplified: true);
```

**After (Simplified with Convenience Method):**
```csharp
services.AddHttpServiceForBlazor<Location, LocationVM, LocationDTO>(
    httpClientFactory,
    apiClientOptions.BaseAddress,
    "/api/Location");
```

## New Convenience Methods

### For Blazor Applications

Registers interfaces optimized for Blazor (IHttpService<T, TView, TDto, KeyType> + IHttpService<T>):

```csharp
// Standard service
services.AddHttpServiceForBlazor<Product, ProductVM, ProductDTO>(
    httpClientFactory, baseAddress, "/api/Product");

// Custom service implementation
services.AddCustomHttpServiceForBlazor<BorderOrg, BorderOrgVM, BorderOrgDTO, BorderOrgHttpService>(
    httpClientFactory, baseAddress, "/api/BorderOrg");
```

### For API Projects

Registers only IHttpService<T, TView, TDto, KeyType>:

```csharp
services.AddHttpServiceForApi<Product, ProductVM, ProductDTO>(
    httpClientFactory, baseAddress, "/api/Product");
```

### For List-Only Components

Registers only IHttpService<T>:

```csharp
services.AddHttpServiceForList<Product>(
    httpClientFactory, baseAddress, "/api/Product");
```

## Breaking Changes

### None! 

The API remains **100% backward compatible**. If you don't want to use convenience methods:

1. Change `using Craft.QuerySpec.Extensions;` to `using Craft.Hosting.Extensions;`
2. Keep all your existing registration code as-is

## Benefits of Using Convenience Methods

### Before (Manual Configuration)
```csharp
// 10 lines of registration with explicit parameters
services.AddTransientHttpService<Location, LocationVM, LocationDTO>(
    httpClientFactory,
    apiClientOptions.BaseAddress,
    "/api/Location",
    registerPrimaryInterface: false,    // Easy to get wrong
    registerWithKeyType: true,
    registerSimplified: true);

services.AddTransientHttpService<Product, ProductVM, ProductDTO>(
    httpClientFactory,
    apiClientOptions.BaseAddress,
    "/api/Product",
    registerPrimaryInterface: false,    // Repetitive
    registerWithKeyType: true,
    registerSimplified: true);
```

### After (Convenience Methods)
```csharp
// 2 lines - clear intent, optimized configuration
services.AddHttpServiceForBlazor<Location, LocationVM, LocationDTO>(
    httpClientFactory, apiClientOptions.BaseAddress, "/api/Location");

services.AddHttpServiceForBlazor<Product, ProductVM, ProductDTO>(
    httpClientFactory, apiClientOptions.BaseAddress, "/api/Product");
```

**Benefits:**
- ✅ 80% less code
- ✅ Clear intent (ForBlazor, ForApi, ForList)
- ✅ No chance of misconfiguration
- ✅ Optimized interface registration for your use case
- ✅ Easier to read and maintain

## Example Migration

### GccPT.Web Before

```csharp
using Craft.QuerySpec.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHttpServices(this IServiceCollection services, IConfiguration config)
    {
        var apiClientOptions = config.GetSection(ApiClientOptions.SectionName).Get<ApiClientOptions>()
            ?? new ApiClientOptions();

        Func<IServiceProvider, HttpClient> httpClientFactory = provider =>
            provider.GetRequiredService<ApiClient>().HttpClient;

        services.AddTransientHttpService<Location, LocationVM, LocationDTO>(
            httpClientFactory,
            apiClientOptions.BaseAddress,
            "/api/Location",
            registerPrimaryInterface: false,
            registerWithKeyType: true,
            registerSimplified: true);

        // ... 9 more similar registrations
        
        return services;
    }
}
```

### GccPT.Web After

```csharp
using Craft.Hosting.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHttpServices(this IServiceCollection services, IConfiguration config)
    {
        var apiClientOptions = config.GetSection(ApiClientOptions.SectionName).Get<ApiClientOptions>()
            ?? new ApiClientOptions();

        Func<IServiceProvider, HttpClient> httpClientFactory = provider =>
            provider.GetRequiredService<ApiClient>().HttpClient;

        services.AddHttpServiceForBlazor<Location, LocationVM, LocationDTO>(
            httpClientFactory, apiClientOptions.BaseAddress, "/api/Location");

        services.AddCustomHttpServiceForBlazor<BorderOrg, BorderOrgVM, BorderOrgDTO, BorderOrgHttpService>(
            httpClientFactory, apiClientOptions.BaseAddress, "/api/BorderOrg");

        services.AddHttpServiceForBlazor<BudgetHead, BudgetHeadVM, BudgetHeadDTO>(
            httpClientFactory, apiClientOptions.BaseAddress, "/api/BudgetHead");

        // ... 7 more clean, concise registrations
        
        return services;
    }
}
```

## Troubleshooting

### Build Error: "Unable to find project information for Craft.Hosting.csproj"

**Solution:**
```bash
dotnet restore "F:\Projects\Craft_V_10\1. Source\5. Web\Craft.Hosting\Craft.Hosting.csproj"
dotnet restore "F:\Projects\GccPT\GccPT.Web\GccPT.Web.csproj"
```

### Build Error: "The type or namespace name 'Hosting' does not exist in the namespace 'Craft'"

**Solution:**
Ensure your project has the correct reference:
```xml
<ProjectReference Include="..\..\Craft_V_10\1. Source\5. Web\Craft.Hosting\Craft.Hosting.csproj" />
```

### Build Error: "AddHttpServiceForBlazor' does not exist"

**Solution:**
Add the using statement:
```csharp
using Craft.Hosting.Extensions;
```

## Future Additions

The `Craft.Hosting` project is designed to grow with more DI extensions:

- ✨ Database service extensions
- ✨ Caching service extensions
- ✨ Email service extensions
- ✨ File upload service extensions
- ✨ Authentication/Authorization extensions
- ✨ Multi-tenancy extensions

All will be available from a single package reference!

## Questions?

See `Craft.Hosting/README.md` for complete documentation.
