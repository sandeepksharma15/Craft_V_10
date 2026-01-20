# Craft.Hosting - Comprehensive Extension Migration Plan

## Overview

This document outlines the plan to move ALL DI extension methods from 12 Craft projects into `Craft.Hosting`.

## Projects with DI Extensions

### 1. **Craft.QuerySpec** (Data Access)
- `HttpServiceExtensions.cs` - ✅ **DONE** (moved to Craft.Hosting)
- `QuerySpecServiceExtensions.cs` - Adds QuerySpec JSON serialization
  - `AddQuerySpecJsonOptions(IMvcBuilder)` 

### 2. **Craft.Data** (Data Access)
- `DatabaseExtensions.cs` - Database configuration
  - `ConfigureDatabases(IServiceCollection, IConfiguration)`
  - `AddCraftPostgreSql<TContext>(...)` 
  - `AddCraftSqlServer<TContext>(...)`
- `DbContextOptionsBuilderExtensions.cs` - DbContext configuration helpers

### 3. **Craft.Cache** (Infrastructure)
- `CacheServiceExtensions.cs` - Cache service registration
  - `AddCacheServices(IServiceCollection, IConfiguration)`
  - `AddCacheServices(IServiceCollection, IConfigurationSection)`
  - `AddCacheServices(IServiceCollection, Action<CacheOptions>)`
  - `AddCacheProvider<TProvider>(IServiceCollection)`

### 4. **Craft.Emails** (Infrastructure)
- `EmailServiceExtensions.cs` - Email service registration  
  - `AddEmailServices(IServiceCollection, IConfiguration)`
  - `AddEmailServices(IServiceCollection, IConfigurationSection)`
  - `AddEmailServices(IServiceCollection, Action<EmailOptions>)`
  - `AddEmailProvider<TProvider>(IServiceCollection)`
  - `AddEmailQueue<TQueue>(IServiceCollection)`
  - `AddEmailTemplateRenderer<TRenderer>(IServiceCollection)`

### 5. **Craft.Files** (Infrastructure)
- `FileUploadServiceExtensions.cs` - File upload service registration
  - `AddFileUploadServices(IServiceCollection, IConfiguration)`
  - `AddFileUploadServices(IServiceCollection, IConfigurationSection)`
  - `AddFileUploadServices(IServiceCollection, Action<FileUploadOptions>)`
  - `AddFileStorageProvider<TProvider>(IServiceCollection)`
  - `AddVirusScanner<TScanner>(IServiceCollection)`
  - `AddThumbnailGenerator<TGenerator>(IServiceCollection)`

### 6. **Craft.Jobs** (Infrastructure)
- `JobServiceExtensions.cs` - Background job service registration
  - `AddJobServices(IServiceCollection, IConfiguration)`
  - `AddJobServices(IServiceCollection, IConfigurationSection)`
  - `AddJobServices(IServiceCollection, Action<JobOptions>)`
  - `AddJobStore<TStore>(IServiceCollection)`
  - `AddJobExecutor<TExecutor>(IServiceCollection)`

### 7. **Craft.Logging** (Infrastructure)
- Typically ApplicationBuilderExtensions (not ServiceCollection)
- May not need to move

### 8. **Craft.Security** (Security)
- `ServiceCollectionExtensions.cs` - Security service registration
  - Authentication/Authorization setup
  - JWT configuration
  - Identity configuration

### 9. **Craft.MultiTenant** (Security)
- `ServiceCollectionExtensions.cs` - Multi-tenancy registration
  - `AddMultiTenancy(IServiceCollection, IConfiguration)`
  - `AddTenantResolver<TResolver>(IServiceCollection)`
  - `AddTenantStore<TStore>(IServiceCollection)`

### 10. **Craft.CryptKey** (Security)
- `ServiceCollectionExtensions.cs` - Encryption key services
  - `AddCryptKeyServices(IServiceCollection, IConfiguration)`
  - `AddKeyProvider<TProvider>(IServiceCollection)`

### 11. **Craft.Controllers** (Web)
- `ApiVersioningExtensions.cs` - API versioning
  - `AddCraftApiVersioning(IServiceCollection)`
- `DatabaseErrorHandlingExtensions.cs` - Error handling
  - `AddDatabaseErrorHandling(IServiceCollection)`

### 12. **Craft.UiBuilders** (UI)
- `ServiceCollectionExtensions.cs` - UI component services
  - Various component registration methods

## Proposed Craft.Hosting Structure

```
Craft.Hosting/
├── Craft.Hosting.csproj
├── Usings.cs
├── README.md
├── MIGRATION.md
├── Extensions/
│   ├── DataAccess/
│   │   ├── HttpServiceExtensions.cs (✅ Done)
│   │   ├── HttpServiceConvenienceExtensions.cs (✅ Done)
│   │   ├── QuerySpecExtensions.cs
│   │   └── DatabaseExtensions.cs
│   ├── Infrastructure/
│   │   ├── CacheExtensions.cs
│   │   ├── EmailExtensions.cs
│   │   ├── FileUploadExtensions.cs
│   │   └── JobExtensions.cs
│   ├── Security/
│   │   ├── SecurityExtensions.cs
│   │   ├── MultiTenantExtensions.cs
│   │   └── CryptKeyExtensions.cs
│   └── Web/
│       ├── ControllerExtensions.cs
│       └── UiBuilderExtensions.cs
```

## Implementation Strategy

### Option A: Full Migration (Recommended Long-Term)
**Copy** all extensions to Craft.Hosting, mark originals as `[Obsolete]` with migration guidance.

**Pros:**
- Clean, centralized architecture
- Single reference for external projects
- Easier to maintain and discover
- Best practice long-term

**Cons:**
- Large migration effort
- Need to update all internal Craft projects
- Need to handle project references carefully

### Option B: Selective Migration (Pragmatic)
**Move** only most commonly used extensions:
- HttpServices ✅
- Database configuration
- Cache services  
- Email services
- Security (Authentication/Authorization)

**Keep** in original projects:
- Highly specialized extensions
- Extensions with complex dependencies
- Extensions rarely used externally

**Pros:**
- Smaller scope
- Less disruption
- Quick wins

**Cons:**
- Still need multiple references
- Partial solution
- May need to revisit later

### Option C: Facade Pattern (Minimal Changes)
**Create** wrapper/convenience methods in Craft.Hosting that call original methods.

**Pros:**
- Minimal code duplication
- No breaking changes
- Single reference point

**Cons:**
- Extra layer of indirection
- Still need transitive references
- Doesn't truly consolidate

## Recommended Approach

**Phase 1** (Immediate - 2-4 hours):
- ✅ HttpServices (Done)
- Database Extensions (AddCraftPostgreSql, AddCraftSqlServer)
- QuerySpec JSON options
- Cache Services
- Email Services

**Phase 2** (Next iteration - 2-3 hours):
- File Upload Services
- Job Services
- Basic Security Extensions

**Phase 3** (Future - 2-3 hours):
- MultiTenancy
- Controller Extensions
- UI Builder Extensions

## Estimated Effort

- **Full Migration (Option A)**: 8-12 hours
- **Selective Migration (Option B - Phase 1)**: 2-4 hours
- **Facade Pattern (Option C)**: 1-2 hours

## Dependencies to Add to Craft.Hosting.csproj

Will need to reference:
- Craft.Cache
- Craft.Emails
- Craft.Files
- Craft.Jobs
- Craft.Security
- Craft.MultiTenant
- Craft.CryptKey
- Craft.Controllers
- Craft.UiBuilders
- Craft.Data (partially)

## Questions for Decision

1. **Scope**: Full migration or phased approach?
2. **Breaking Changes**: Mark originals as Obsolete or keep both?
3. **Timeline**: Do all now or spread across multiple sessions?
4. **Priority**: Which extensions are most critical for GccPT project?

## My Recommendation

Start with **Phase 1** (immediate, high-value items) and complete it fully including:
- Database configuration extensions
- QuerySpec JSON options
- Cache services
- Email services
- File upload services

This gives you the most commonly used DI extensions in one place (~80% of use cases) while being achievable in one session.

Then evaluate if Phases 2 & 3 are needed based on actual usage patterns.

**Would you like me to proceed with Phase 1?**
