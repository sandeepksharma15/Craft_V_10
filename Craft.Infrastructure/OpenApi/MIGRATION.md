# OpenAPI Module - Migration Guide

## Overview

This guide helps you migrate from the old OpenAPI implementation to the new, enhanced version with improved configuration, security, and customization options.

---

## What's New in v2.0

### ? New Features

1. **Comprehensive Configuration Options**
   - Nested configuration structure with `Security`, `UI`, and `Documentation` sections
   - Validation with `IValidatableObject` and data annotations
   - `ValidateOnStart()` for early error detection

2. **Enhanced Security Schemes**
   - JWT Bearer (enhanced)
   - API Key authentication
   - OAuth2 support
   - Multiple schemes simultaneously

3. **UI Customization**
   - Custom CSS and JavaScript
   - Theme support
   - Configurable layout options
   - Deep linking and filtering

4. **Environment-Specific Behavior**
   - Production disabled by default
   - Per-environment configuration
   - `EnableInProduction` flag

5. **Documentation Enhancement**
   - XML documentation auto-discovery
   - Tag descriptions
   - Obsolete API filtering
   - Custom schema options

6. **Configuration Encryption Support**
   - Works with Craft configuration encryption module
   - Secure storage of sensitive values

---

## Breaking Changes

### 1. Namespace Change

**Old:**
```csharp
using Craft.Infrastructure.OpenApi;

services.AddOpenApiDocumentation(config);
```

**New:**
```csharp
using Microsoft.Extensions.DependencyInjection; // Extensions moved here

builder.Services.AddOpenApiDocumentation(builder.Configuration);
```

### 2. Configuration Structure

**Old Configuration:**
```json
{
  "SwaggerOptions": {
    "Title": "API",
    "Version": "v1",
    "Description": "TBD",
    "ContactEmail": "TBD",
    "ContactName": "TBD",
    "ContactUrl": "TBD",
    "Enable": true,
    "License": false,
    "LicenseName": "TBD",
    "LicenseUrl": "TBD"
  }
}
```

**New Configuration:**
```json
{
  "SwaggerOptions": {
    "Enable": true,
    "Title": "API",
    "Version": "v1",
    "Description": "API Description",
    "ContactEmail": "support@example.com",
    "ContactName": "API Team",
    "ContactUrl": "https://example.com/contact",
    "LicenseName": "MIT",
    "LicenseUrl": "https://opensource.org/licenses/MIT",
    "Security": {
      "EnableJwtBearer": true
    },
    "UI": {
      "DocumentTitle": "API Documentation"
    },
    "Documentation": {
      "IgnoreObsoleteActions": true
    }
  }
}
```

### 3. License Property Removed

**Old:**
```json
{
  "SwaggerOptions": {
    "License": false  // Boolean flag
  }
}
```

**New:**
```json
{
  "SwaggerOptions": {
    "LicenseName": "MIT",  // Just set the name, no boolean flag
    "LicenseUrl": "https://opensource.org/licenses/MIT"
  }
}
```

### 4. Production Behavior

**Old:**
- Enabled in Development and Staging only

**New:**
- Enabled in Development and Staging by default
- Production requires explicit `EnableInProduction: true`

```json
{
  "SwaggerOptions": {
    "Enable": true,
    "EnableInProduction": true  // Required for production
  }
}
```

### 5. SwaggerIgnoreFilter Removed

The old `SwaggerIgnoreFilter` that removed all schemas has been removed. Use proper attributes instead:

**Old (Don't use):**
```csharp
// Removed all schemas from document
options.DocumentFilter<SwaggerIgnoreFilter>();
```

**New (Use attributes):**
```csharp
[SwaggerSchema(Required = new[] { "id", "name" })]
public class MyModel
{
    [SwaggerSchema("Unique identifier")]
    public int Id { get; set; }
    
    [SwaggerSchema("Name of the entity", Nullable = false)]
    public string Name { get; set; }
}
```

---

## Step-by-Step Migration

### Step 1: Update Configuration File

Replace your existing `SwaggerOptions` configuration:

**Before:**
```json
{
  "SwaggerOptions": {
    "Title": "My API",
    "Version": "v1",
    "Description": "TBD",
    "ContactEmail": "TBD",
    "ContactName": "TBD",
    "ContactUrl": "TBD",
    "Enable": true,
    "License": false,
    "LicenseName": "TBD",
    "LicenseUrl": "TBD"
  }
}
```

**After:**
```json
{
  "SwaggerOptions": {
    "Enable": true,
    "Title": "My API",
    "Version": "v1",
    "Description": "My API Documentation",
    "ContactName": "API Team",
    "ContactEmail": "support@example.com",
    "ContactUrl": "https://example.com/contact",
    "LicenseName": "MIT",
    "LicenseUrl": "https://opensource.org/licenses/MIT",
    "Security": {
      "EnableJwtBearer": true
    }
  }
}
```

### Step 2: Update Program.cs

**Before:**
```csharp
using Craft.Infrastructure.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApiDocumentation(builder.Configuration);

var app = builder.Build();

app.UseOpenApiDocumentation();
```

**After:**
```csharp
// No need to explicitly import - extensions are in Microsoft.Extensions.DependencyInjection

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApiDocumentation(builder.Configuration);

var app = builder.Build();

app.UseOpenApiDocumentation();
```

### Step 3: Remove "TBD" Values

Replace all "TBD" placeholder values with actual information or remove the properties:

**Option 1: Provide actual values**
```json
{
  "SwaggerOptions": {
    "ContactEmail": "support@example.com",
    "ContactUrl": "https://example.com/contact"
  }
}
```

**Option 2: Remove if not needed**
```json
{
  "SwaggerOptions": {
    // Omit ContactEmail and ContactUrl if not needed
    "Title": "My API",
    "Version": "v1"
  }
}
```

### Step 4: Enable XML Documentation (Recommended)

1. **Update .csproj:**

```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

2. **Add XML comments to controllers:**

```csharp
/// <summary>
/// User management endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    /// <summary>
    /// Get all users
    /// </summary>
    /// <returns>List of users</returns>
    [HttpGet]
    public IActionResult GetAll()
    {
        // Implementation
    }
}
```

### Step 5: Configure for Production (if needed)

If you want Swagger in production:

**appsettings.Production.json:**
```json
{
  "SwaggerOptions": {
    "Enable": true,
    "EnableInProduction": true,
    "Title": "My API",
    "Version": "v1"
  }
}
```

### Step 6: Test and Verify

1. **Run the application:**
```bash
dotnet run
```

2. **Access Swagger UI:**
- Development: `https://localhost:5001/swagger`
- Check for validation errors on startup

3. **Verify all features:**
- [ ] Swagger UI loads correctly
- [ ] Authentication scheme appears
- [ ] XML documentation shows up
- [ ] All endpoints are documented

---

## Environment-Specific Migration

### Development Environment

**appsettings.Development.json:**
```json
{
  "SwaggerOptions": {
    "Enable": true,
    "Title": "My API [Development]",
    "Version": "v1",
    "Description": "Development environment API documentation",
    "Servers": [
      {
        "Url": "https://localhost:5001",
        "Description": "Local Development"
      }
    ],
    "UI": {
      "DocExpansion": "full",
      "EnableTryItOutByDefault": true
    }
  }
}
```

### Staging Environment

**appsettings.Staging.json:**
```json
{
  "SwaggerOptions": {
    "Enable": true,
    "Title": "My API [Staging]",
    "Version": "v1",
    "Servers": [
      {
        "Url": "https://api-staging.example.com",
        "Description": "Staging Server"
      }
    ]
  }
}
```

### Production Environment

**appsettings.Production.json:**
```json
{
  "SwaggerOptions": {
    "Enable": true,
    "EnableInProduction": true,
    "Title": "My API",
    "Version": "v1",
    "RoutePrefix": "api-docs",
    "Servers": [
      {
        "Url": "https://api.example.com",
        "Description": "Production Server"
      }
    ]
  }
}
```

---

## Feature Adoption Guide

### Adopt New Security Features

#### Add API Key Authentication

```json
{
  "SwaggerOptions": {
    "Security": {
      "EnableJwtBearer": true,
      "EnableApiKey": true,
      "ApiKeyHeaderName": "X-API-Key"
    }
  }
}
```

#### Add OAuth2 Authentication

```json
{
  "SwaggerOptions": {
    "Security": {
      "EnableOAuth2": true,
      "OAuth2": {
        "AuthorizationUrl": "https://auth.example.com/authorize",
        "TokenUrl": "https://auth.example.com/token",
        "Scopes": {
          "read": "Read access",
          "write": "Write access"
        }
      }
    }
  }
}
```

### Adopt UI Customization

#### Add Dark Theme

```json
{
  "SwaggerOptions": {
    "UI": {
      "CustomCssUrl": "https://cdn.jsdelivr.net/npm/swagger-ui-themes@3.0.1/themes/3.x/theme-monokai.css"
    }
  }
}
```

#### Customize Layout

```json
{
  "SwaggerOptions": {
    "UI": {
      "DocExpansion": "list",
      "EnableFilter": true,
      "DisplayRequestDuration": true,
      "PersistAuthorization": true
    }
  }
}
```

### Adopt Documentation Enhancement

#### Add Tag Descriptions

```json
{
  "SwaggerOptions": {
    "Documentation": {
      "TagDescriptions": {
        "Users": "User management and authentication endpoints",
        "Products": "Product catalog and inventory",
        "Orders": "Order processing and fulfillment"
      }
    }
  }
}
```

#### Configure Schema Options

```json
{
  "SwaggerOptions": {
    "Documentation": {
      "IgnoreObsoleteActions": true,
      "IgnoreObsoleteProperties": true,
      "ShowEnumDescriptions": true,
      "UseOneOfForPolymorphism": true
    }
  }
}
```

---

## Common Migration Issues

### Issue 1: Validation Errors on Startup

**Error:**
```
OptionsValidationException: DataAnnotation validation failed for 'SwaggerOptions'
```

**Solution:**
Ensure all required fields are provided:
```json
{
  "SwaggerOptions": {
    "Enable": true,
    "Title": "My API",  // Required
    "Version": "v1"     // Required
  }
}
```

### Issue 2: Invalid URL Format

**Error:**
```
ValidationResult: Contact URL must be a valid absolute URI
```

**Solution:**
Use complete URLs with protocol:
```json
{
  "SwaggerOptions": {
    "ContactUrl": "https://example.com/contact"  // Not "example.com/contact"
  }
}
```

### Issue 3: Swagger Not Showing in Production

**Symptom:**
404 error when accessing /swagger in production

**Solution:**
Enable production explicitly:
```json
{
  "SwaggerOptions": {
    "Enable": true,
    "EnableInProduction": true
  }
}
```

### Issue 4: XML Documentation Not Appearing

**Solution:**
1. Enable XML generation in .csproj
2. Rebuild project
3. Verify XML file exists in output folder
4. Check configuration:

```json
{
  "SwaggerOptions": {
    "IncludeXmlComments": true
  }
}
```

---

## Rollback Plan

If you need to rollback to the old implementation:

1. **Revert configuration file:**
```bash
git checkout HEAD -- appsettings.json
```

2. **Revert code files:**
```bash
git checkout HEAD -- Craft.Infrastructure/OpenApi/
```

3. **Rebuild:**
```bash
dotnet build
```

---

## Testing Checklist

After migration, verify:

- [ ] Application starts without validation errors
- [ ] Swagger UI is accessible at `/swagger`
- [ ] All endpoints are documented
- [ ] Authentication scheme works
- [ ] XML documentation appears
- [ ] Models are properly described
- [ ] Request/response examples show up
- [ ] "Try it out" functionality works
- [ ] Production environment behaves as expected

---

## Support

If you encounter issues during migration:

1. Check the [Troubleshooting](./README.md#troubleshooting) section
2. Review the [README](./README.md) for complete documentation
3. Check application logs for detailed error messages
4. Verify configuration syntax with a JSON validator

---

**Migration Guide Version:** 2.0  
**Last Updated:** January 2025  
**Target Framework:** .NET 10
