# Craft.Hosting

**Centralized dependency injection extensions for the Craft Framework**

[![.NET 10](https://img.shields.io/badge/.NET-10.0-blue)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

## 📖 Overview

`Craft.Hosting` is a centralized library that provides dependency injection (DI) extension methods for all Craft Framework projects. Instead of having DI registration extensions scattered across individual framework libraries, this package consolidates them into a single, cohesive API surface.

### Key Benefits

- **Single Namespace**: All DI extensions available through `Craft.Hosting.Extensions`
- **Simplified Registration**: Cleaner, more concise service registration code
- **Consistent Patterns**: Standardized approach across all Craft services
- **Better Discoverability**: All available services in one place
- **Reduced Boilerplate**: Up to 80% reduction in registration code

### Architecture

```
Craft.Hosting (DI Registration Layer)
├── Extensions/
│   ├── DataAccess/          # Database & HTTP services
│   ├── Infrastructure/      # Cache, Email, Files, Jobs
│   ├── Security/            # Authentication, Multi-tenancy, Encryption
│   └── Web/                 # Controllers, UI Builders
└── Project References to all Craft Framework libraries
```

## 🚀 Quick Start

### Installation

Add a project reference to `Craft.Hosting`:

```xml
<ProjectReference Include="path/to/Craft.Hosting/Craft.Hosting.csproj" />
```

### Basic Usage

```csharp
using Craft.Hosting.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Before: Multiple using statements for different Craft projects
// using Craft.QuerySpec.Extensions;
// using Craft.Data.Extensions;
// using Craft.Cache.Extensions;
// ... etc

// After: Single namespace for all Craft DI extensions
// Register services using consolidated extensions
builder.Services.AddCacheServices(builder.Configuration);
builder.Services.AddEmailServices(builder.Configuration);
builder.Services.AddFileUploadServices(builder.Configuration);
```

## 📚 Available Extensions

### Data Access Extensions

#### Database Configuration

```csharp
// Standard DbContext registration (Scoped)
services.AddCraftDbContext<MyDbContext>((sp, options) =>
{
    options.UseNpgsql(connectionString);
});

// Pooled DbContext registration (Recommended for production)
services.AddCraftDbContextPool<MyDbContext>((sp, options) =>
{
    options.UseNpgsql(connectionString);
}, poolSize: 1024);

// PostgreSQL with Aspire integration
services.AddCraftPostgreSql<MyDbContext>(
    configuration,
    connectionName: "DefaultConnection",
    enablePooling: true,
    poolSize: 1024);
```

#### HTTP Service Registration

Register HTTP services for consuming APIs with full entity/view model/DTO support:

```csharp
// Transient registration (creates new instance per request)
services.AddTransientHttpService<Product>(
    sp => sp.GetRequiredService<HttpClient>(),
    "https+http://api",
    "/api/Product");

// With separate View and DTO types
services.AddTransientHttpService<Product, ProductVM, ProductDTO>(
    sp => sp.GetRequiredService<HttpClient>(),
    "https+http://api",
    "/api/Product");

// Scoped registration (one instance per scope/request)
services.AddScopedHttpService<Product>(
    sp => sp.GetRequiredService<HttpClient>(),
    "https+http://api",
    "/api/Product");

// Singleton registration (single instance for app lifetime)
services.AddSingletonHttpService<Product>(
    sp => sp.GetRequiredService<HttpClient>(),
    "https+http://api",
    "/api/Product");

// Blazor-optimized convenience method
services.AddHttpServiceForBlazor<Product, ProductVM, ProductDTO>(
    sp => sp.GetRequiredService<HttpClient>(),
    "https+http://api",
    "/api/Product");
```

**Parameters:**
- `registerPrimaryInterface`: Registers `IHttpService<T, TView, TDto>` (default: true)
- `registerWithKeyType`: Registers `IHttpService<T, TView, TDto, KeyType>` (default: false)
- `registerSimplified`: Registers `IHttpService<T>` with simplified constructor (default: false)

#### Query Specification Extensions

```csharp
// Configure JSON serialization options for QuerySpec
services.AddQuerySpecJsonOptions(options =>
{
    options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.WriteIndented = true;
});
```

### Infrastructure Extensions

#### Cache Services

```csharp
// From configuration section
services.AddCacheServices(configuration);

// From specific configuration section
services.AddCacheServices(configuration.GetSection("Cache"));

// Programmatic configuration
services.AddCacheServices(options =>
{
    options.DefaultProvider = "Memory";
    options.DefaultExpirationMinutes = 60;
});

// Add custom cache provider
services.AddCacheProvider<MyCustomCacheProvider>();
```

**Supported Cache Providers:**
- Memory Cache (default)
- Redis Cache
- Null Cache (for testing)
- Custom providers via `ICacheProvider`

#### Email Services

```csharp
// From configuration
services.AddEmailServices(configuration);

// From specific configuration section
services.AddEmailServices(configuration.GetSection("Email"));

// Programmatic configuration
services.AddEmailServices(options =>
{
    options.SmtpHost = "smtp.gmail.com";
    options.SmtpPort = 587;
    options.EnableSsl = true;
    options.DefaultFromEmail = "noreply@example.com";
});

// Add custom email provider
services.AddEmailProvider<MyCustomEmailProvider>();

// Add email template provider
services.AddEmailTemplateProvider<MyTemplateProvider>();
```

**Features:**
- SMTP email sending
- Template-based emails
- Queue support for batch sending
- Multiple provider support (SMTP, SendGrid, custom)

#### File Upload Services

```csharp
// From configuration
services.AddFileUploadServices(configuration);

// From specific configuration section
services.AddFileUploadServices(configuration.GetSection("FileUpload"));

// Programmatic configuration
services.AddFileUploadServices(options =>
{
    options.MaxFileSizeInMB = 10;
    options.AllowedExtensions = [".jpg", ".png", ".pdf"];
    options.StoragePath = "./uploads";
    options.EnableVirusScanning = true;
});

// Add custom storage provider
services.AddFileStorageProvider<MyCustomStorageProvider>();
```

**Features:**
- File validation (size, extension, content type)
- Virus scanning integration
- Thumbnail generation
- Multiple storage providers (Local, Azure Blob, S3, custom)

#### Background Job Services (Hangfire)

```csharp
// From configuration
services.AddJobServices(configuration);

// From specific configuration section
services.AddJobServices(configuration.GetSection("Hangfire"));

// Programmatic configuration
services.AddJobServices(options =>
{
    options.UseInMemoryStorage = false;
    options.UseSqlServerStorage = true;
    options.ConnectionString = "...";
});

// Register job types
services.AddRecurringJob<MyRecurringJob>();
```

**Features:**
- Background job processing
- Recurring job scheduling
- Job persistence (SQL Server, PostgreSQL, Redis, in-memory)
- Dashboard for monitoring

### Security Extensions

#### Authentication & Authorization

```csharp
// JWT Authentication
services.AddCraftJwtAuthentication(configuration);

// Custom authentication
services.AddCraftAuthentication(options =>
{
    options.DefaultScheme = "Bearer";
    options.EnableJwt = true;
    options.JwtSecret = "your-secret-key";
});

// Role-based authorization policies
services.AddCraftAuthorizationPolicies();
```

#### Multi-Tenancy

```csharp
// From configuration
services.AddMultiTenantServices(configuration);

// Programmatic configuration
services.AddMultiTenantServices(options =>
{
    options.TenantResolutionStrategy = TenantResolutionStrategy.Header;
    options.TenantHeaderName = "X-Tenant-Id";
});

// Add custom tenant resolver
services.AddTenantResolver<MyCustomTenantResolver>();
```

**Tenant Resolution Strategies:**
- HTTP Header
- Subdomain
- Route parameter
- Claim-based
- Custom resolver

#### Encryption Services

```csharp
// From configuration
services.AddCryptKeyServices(configuration);

// Programmatic configuration
services.AddCryptKeyServices(options =>
{
    options.EncryptionKey = "your-encryption-key";
    options.UseDataProtectionAPI = true;
});
```

### Web Extensions

#### Controller Extensions

```csharp
// Add database error handling middleware
services.AddDatabaseErrorHandling();

// Add API versioning
services.AddCraftApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
});
```

#### UI Builder Extensions

```csharp
// Register UI builder services
services.AddUiBuilderServices();

// Add custom UI components
services.AddUiComponent<MyCustomComponent>();
```

## 🎯 Migration from Individual Projects

### Before

```csharp
using Craft.QuerySpec.Extensions;
using Craft.Data.Extensions;
using Craft.Cache.Extensions;
using Craft.Emails.Extensions;
// ... many more using statements

services.AddTransientHttpService<Location, LocationVM, LocationDTO>(
    sp => sp.GetRequiredService<HttpClient>(),
    apiOptions.BaseAddress,
    "/api/Location",
    registerPrimaryInterface: false,
    registerWithKeyType: true,
    registerSimplified: true);

services.AddMemoryCacheProvider();
services.AddCacheService();
// ... repetitive registration code
```

### After

```csharp
using Craft.Hosting.Extensions;

services.AddHttpServiceForBlazor<Location, LocationVM, LocationDTO>(
    sp => sp.GetRequiredService<HttpClient>(),
    apiOptions.BaseAddress,
    "/api/Location");

services.AddCacheServices(configuration);
```

**Benefits:**
- 80% less registration code
- Single using statement
- Clear intent with optimized defaults
- Better maintainability

## ⚙️ Configuration

All Craft services support configuration via `appsettings.json`:

```json
{
  "Cache": {
    "DefaultProvider": "Memory",
    "DefaultExpirationMinutes": 60,
    "Redis": {
      "ConnectionString": "localhost:6379"
    }
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "EnableSsl": true,
    "DefaultFromEmail": "noreply@example.com"
  },
  "FileUpload": {
    "MaxFileSizeInMB": 10,
    "AllowedExtensions": [".jpg", ".png", ".pdf"],
    "StoragePath": "./uploads"
  },
  "Hangfire": {
    "UseInMemoryStorage": false,
    "UseSqlServerStorage": true,
    "ConnectionString": "..."
  },
  "MultiTenant": {
    "TenantResolutionStrategy": "Header",
    "TenantHeaderName": "X-Tenant-Id"
  }
}
```

## 🏗️ Project Structure

```
Craft.Hosting/
├── Extensions/
│   ├── HttpServiceExtensions.cs              # Root-level HTTP services
│   ├── HttpServiceConvenienceExtensions.cs   # Blazor-optimized helpers
│   ├── DataAccess/
│   │   ├── DatabaseExtensions.cs             # DbContext configuration
│   │   └── QuerySpecExtensions.cs            # Query specification
│   ├── Infrastructure/
│   │   ├── CacheExtensions.cs                # Cache services
│   │   ├── EmailExtensions.cs                # Email services
│   │   ├── FileUploadExtensions.cs           # File upload services
│   │   └── JobExtensions.cs                  # Background jobs
│   ├── Security/
│   │   ├── SecurityExtensions.cs             # Authentication/Authorization
│   │   ├── MultiTenantExtensions.cs          # Multi-tenancy
│   │   └── CryptKeyExtensions.cs             # Encryption
│   └── Web/
│       ├── ControllerExtensions.cs           # Controller utilities
│       └── UiBuilderExtensions.cs            # UI components
└── Craft.Hosting.csproj
```

## 🔧 Advanced Usage

### Lifetime Management

All HTTP service registrations support three lifetimes:

```csharp
// Transient (new instance per injection)
services.AddTransientHttpService<Product>(...);

// Scoped (one instance per scope/request)
services.AddScopedHttpService<Product>(...);

// Singleton (single instance for entire application)
services.AddSingletonHttpService<Product>(...);
```

### Custom Providers

Add custom implementations for any Craft service:

```csharp
// Custom cache provider
services.AddCacheProvider<MyCustomCacheProvider>();

// Custom email provider
services.AddEmailProvider<MyCustomEmailProvider>();

// Custom storage provider
services.AddFileStorageProvider<MyCustomStorageProvider>();

// Custom tenant resolver
services.AddTenantResolver<MyCustomTenantResolver>();
```

### Validation

All configuration options support validation via Data Annotations:

```csharp
services.AddCacheServices(configuration)
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

## 🧪 Testing

For testing scenarios, use the Null pattern implementations:

```csharp
services.AddCacheServices(options =>
{
    options.DefaultProvider = "Null"; // No-op cache for testing
});

// Or register services directly
services.AddSingleton<ICacheService, NullCacheService>();
services.AddSingleton<IEmailService, NullEmailService>();
```

## 📦 Dependencies

### .NET 10 Packages
- Microsoft.EntityFrameworkCore (10.0.3)
- Microsoft.Extensions.Configuration.Abstractions (10.0.3)
- Microsoft.Extensions.DependencyInjection.Abstractions (10.0.3)
- Microsoft.Extensions.Http (10.0.3)
- Microsoft.Extensions.Logging.Abstractions (10.0.3)
- Microsoft.Extensions.Options (10.0.3)
- Microsoft.Extensions.Options.ConfigurationExtensions (10.0.3)

### Craft Framework References
- **Core**: Craft.Core, Craft.Domain
- **Data Access**: Craft.Data, Craft.QuerySpec
- **Infrastructure**: Craft.Cache, Craft.Emails, Craft.Files, Craft.Jobs
- **Security**: Craft.CryptKey, Craft.MultiTenant, Craft.Security
- **Web**: Craft.Controllers
- **UI**: Craft.UiBuilders

## 🤝 Contributing

When adding new extension methods:

1. Place them in the appropriate category folder
2. Use namespace `Craft.Hosting.Extensions`
3. Add XML documentation comments
4. Follow existing patterns for consistency
5. Include configuration validation
6. Support both IConfiguration and programmatic setup
7. Update this README with examples

## 📄 License

This project is part of the Craft Framework by Sandeep SHARMA.

## 🔗 Related Projects

- [Craft.Core](../../../1.%20Core/Craft.Core) - Core abstractions and utilities
- [Craft.Domain](../../../1.%20Core/Craft.Domain) - Domain entities and value objects
- [Craft.Data](../../../2.%20Data%20Access/Craft.Data) - Data access layer
- [Craft.Cache](../../../3.%20Infrastructure/Craft.Cache) - Caching infrastructure
- [Craft.Security](../../../4.%20Security/Craft.Security) - Authentication & authorization

## 📞 Support

For issues, questions, or contributions, please open an issue on the GitHub repository.

---

**Version**: 1.0.0  
**Target Framework**: .NET 10.0  
**Author**: Sandeep SHARMA
