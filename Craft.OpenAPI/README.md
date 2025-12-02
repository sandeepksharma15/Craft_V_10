# OpenAPI/Swagger Documentation Module

> **Version:** 2.0+ | **Target Framework:** .NET 10

## ?? Table of Contents

1. [Quick Start](#-quick-start)
2. [Configuration](#-configuration)
3. [Security Schemes](#-security-schemes)
4. [UI Customization](#-ui-customization)
5. [Documentation Enhancement](#-documentation-enhancement)
6. [Environment-Specific Behavior](#-environment-specific-behavior)
7. [XML Documentation](#-xml-documentation)
8. [Advanced Scenarios](#-advanced-scenarios)
9. [Migration Guide](#-migration-guide)
10. [Troubleshooting](#-troubleshooting)

---

## ?? Quick Start

### Minimal Setup

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add OpenAPI documentation services
builder.Services.AddOpenApiDocumentation(builder.Configuration);

builder.Services.AddControllers();

var app = builder.Build();

// Use OpenAPI documentation (automatically enables based on environment)
app.UseOpenApiDocumentation();

app.MapControllers();
app.Run();
```

### Configuration (appsettings.json)

```json
{
  "SwaggerOptions": {
    "Enable": true,
    "Title": "My API",
    "Version": "v1",
    "Description": "My API Documentation",
    "ContactName": "API Team",
    "ContactEmail": "api@example.com",
    "ContactUrl": "https://example.com/contact"
  }
}
```

### Access Swagger UI

- **Development**: `https://localhost:5001/swagger`
- **Production**: Disabled by default (can be enabled with `EnableInProduction`)

---

## ?? Configuration

### Complete Configuration Example

```json
{
  "SwaggerOptions": {
    "Enable": true,
    "Title": "My API",
    "Version": "v1",
    "Description": "Comprehensive API documentation with examples and detailed descriptions",
    
    "ContactName": "API Support Team",
    "ContactEmail": "support@example.com",
    "ContactUrl": "https://example.com/support",
    
    "LicenseName": "MIT",
    "LicenseUrl": "https://opensource.org/licenses/MIT",
    "TermsOfService": "https://example.com/terms",
    
    "RoutePrefix": "swagger",
    "EnableInProduction": false,
    "IncludeXmlComments": true,
    "XmlDocumentationFiles": ["Api.xml", "Models.xml"],
    
    "Servers": [
      {
        "Url": "https://api.example.com",
        "Description": "Production Server"
      },
      {
        "Url": "https://api-staging.example.com",
        "Description": "Staging Server"
      }
    ],
    
    "Security": {
      "EnableJwtBearer": true,
      "JwtBearerSchemeName": "Bearer",
      "JwtBearerDescription": "Enter JWT Bearer token",
      
      "EnableApiKey": true,
      "ApiKeySchemeName": "ApiKey",
      "ApiKeyHeaderName": "X-API-Key",
      "ApiKeyDescription": "Enter your API Key",
      
      "EnableOAuth2": false,
      "OAuth2": {
        "AuthorizationUrl": "https://auth.example.com/authorize",
        "TokenUrl": "https://auth.example.com/token",
        "Scopes": {
          "read": "Read access",
          "write": "Write access"
        }
      }
    },
    
    "UI": {
      "DocumentTitle": "My API Documentation",
      "EnableDeepLinking": true,
      "DisplayOperationId": false,
      "DisplayRequestDuration": true,
      "DefaultModelsExpandDepth": 1,
      "EnableFilter": true,
      "EnableTryItOutByDefault": false,
      "PersistAuthorization": true,
      "DocExpansion": "none",
      "DefaultModelRendering": "example",
      "CustomCssUrl": "https://cdn.example.com/swagger-dark.css"
    },
    
    "Documentation": {
      "IgnoreObsoleteActions": true,
      "IgnoreObsoleteProperties": true,
      "ShowEnumDescriptions": true,
      "ShowDefaultValues": true,
      "UseAllOfForRequired": false,
      "UseOneOfForPolymorphism": true,
      "UseInlineDefinitionsForEnums": false,
      "TagDescriptions": {
        "Users": "User management and authentication endpoints",
        "Products": "Product catalog and inventory management",
        "Orders": "Order processing and fulfillment"
      }
    }
  }
}
```

### SwaggerOptions Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Enable` | bool | true | Enable/disable Swagger documentation |
| `Title` | string | "API Documentation" | API title |
| `Version` | string | "v1" | API version |
| `Description` | string | "API Documentation" | API description |
| `ContactName` | string? | null | Contact name |
| `ContactEmail` | string? | null | Contact email (validated) |
| `ContactUrl` | string? | null | Contact URL (validated) |
| `LicenseName` | string? | null | License name |
| `LicenseUrl` | string? | null | License URL (validated) |
| `TermsOfService` | string? | null | Terms of service URL (validated) |
| `RoutePrefix` | string | "swagger" | Swagger UI route prefix |
| `EnableInProduction` | bool | false | Enable in production environment |
| `IncludeXmlComments` | bool | true | Include XML documentation |
| `XmlDocumentationFiles` | List\<string\> | [] | XML documentation file paths |
| `Servers` | List\<ServerUrl\> | [] | Server URLs for different environments |
| `Security` | SecurityOptions | new() | Security scheme configuration |
| `UI` | SwaggerUIOptions | new() | UI customization options |
| `Documentation` | DocumentationOptions | new() | Documentation enhancement options |

---

## ?? Security Schemes

### JWT Bearer Authentication

```json
{
  "SwaggerOptions": {
    "Security": {
      "EnableJwtBearer": true,
      "JwtBearerSchemeName": "Bearer",
      "JwtBearerDescription": "Enter JWT token (without 'Bearer' prefix)"
    }
  }
}
```

**Usage in Swagger UI:**
1. Click "Authorize" button
2. Enter JWT token: `eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`
3. Click "Authorize"

### API Key Authentication

```json
{
  "SwaggerOptions": {
    "Security": {
      "EnableApiKey": true,
      "ApiKeySchemeName": "ApiKey",
      "ApiKeyHeaderName": "X-API-Key",
      "ApiKeyDescription": "Enter your API Key from the developer portal"
    }
  }
}
```

**API Key in Request:**
```http
GET /api/users
X-API-Key: your-api-key-here
```

### OAuth2 Authentication

```json
{
  "SwaggerOptions": {
    "Security": {
      "EnableOAuth2": true,
      "OAuth2": {
        "AuthorizationUrl": "https://auth.example.com/oauth/authorize",
        "TokenUrl": "https://auth.example.com/oauth/token",
        "Scopes": {
          "api.read": "Read access to API",
          "api.write": "Write access to API",
          "api.admin": "Administrative access"
        },
        "ClientId": "swagger-ui",
        "ClientSecret": "ENC:encrypted-secret-here"
      }
    }
  }
}
```

### Multiple Security Schemes

Enable all three schemes simultaneously:

```json
{
  "SwaggerOptions": {
    "Security": {
      "EnableJwtBearer": true,
      "EnableApiKey": true,
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

---

## ?? UI Customization

### Basic UI Customization

```json
{
  "SwaggerOptions": {
    "UI": {
      "DocumentTitle": "My API Documentation",
      "DocExpansion": "none",
      "DefaultModelRendering": "example",
      "EnableFilter": true,
      "PersistAuthorization": true
    }
  }
}
```

### Dark Theme

```json
{
  "SwaggerOptions": {
    "UI": {
      "CustomCssUrl": "https://cdn.jsdelivr.net/npm/swagger-ui-themes@3.0.1/themes/3.x/theme-monokai.css"
    }
  }
}
```

### Inline Custom CSS

```json
{
  "SwaggerOptions": {
    "UI": {
      "InlineCustomCss": ".swagger-ui .topbar { display: none; } .swagger-ui .info { margin-top: 20px; }"
    }
  }
}
```

### Custom JavaScript

```json
{
  "SwaggerOptions": {
    "UI": {
      "CustomJavaScriptUrl": "https://example.com/swagger-custom.js"
    }
  }
}
```

### UI Options Reference

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `DocumentTitle` | string | "API Documentation" | Browser tab title |
| `HeadContent` | string? | null | Custom HTML in \<head\> |
| `EnableDeepLinking` | bool | true | Enable deep linking to operations |
| `DisplayOperationId` | bool | false | Show operation IDs |
| `DisplayRequestDuration` | bool | true | Show request duration |
| `DefaultModelsExpandDepth` | int | 1 | Model expand depth (-1 to 10) |
| `EnableFilter` | bool | true | Enable operation filtering |
| `EnableTryItOutByDefault` | bool | false | Enable "Try it out" by default |
| `PersistAuthorization` | bool | true | Persist auth across browser refresh |
| `DocExpansion` | string | "none" | "none", "list", or "full" |
| `DefaultModelRendering` | string | "example" | "example" or "model" |
| `CustomCssUrl` | string? | null | External CSS URL |
| `InlineCustomCss` | string? | null | Inline CSS |
| `CustomJavaScriptUrl` | string? | null | External JavaScript URL |

---

## ?? Documentation Enhancement

### XML Documentation Comments

**Enable XML documentation:**

```json
{
  "SwaggerOptions": {
    "IncludeXmlComments": true
  }
}
```

**Controller with XML comments:**

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
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/users
    /// 
    /// </remarks>
    /// <returns>List of users</returns>
    /// <response code="200">Returns the list of users</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetAll()
    {
        // Implementation
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User details</returns>
    /// <response code="200">Returns the user</response>
    /// <response code="404">User not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(int id)
    {
        // Implementation
    }
}
```

**Enable XML generation in .csproj:**

```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

### Multiple XML Files

```json
{
  "SwaggerOptions": {
    "IncludeXmlComments": true,
    "XmlDocumentationFiles": [
      "MyApi.xml",
      "MyApi.Models.xml",
      "MyApi.Contracts.xml"
    ]
  }
}
```

### Tag Descriptions

```json
{
  "SwaggerOptions": {
    "Documentation": {
      "TagDescriptions": {
        "Users": "User management, authentication, and profile operations",
        "Products": "Product catalog, inventory, and pricing",
        "Orders": "Order processing, fulfillment, and tracking",
        "Reports": "Analytics and reporting endpoints"
      }
    }
  }
}
```

### Ignore Obsolete APIs

```json
{
  "SwaggerOptions": {
    "Documentation": {
      "IgnoreObsoleteActions": true,
      "IgnoreObsoleteProperties": true
    }
  }
}
```

**Mark obsolete:**

```csharp
[Obsolete("Use GetUsersV2 instead")]
[HttpGet("v1/users")]
public IActionResult GetUsersV1()
{
    // Old implementation
}

[HttpGet("v2/users")]
public IActionResult GetUsersV2()
{
    // New implementation
}
```

---

## ?? Environment-Specific Behavior

### Development & Staging (Enabled by Default)

```csharp
// appsettings.Development.json
{
  "SwaggerOptions": {
    "Enable": true,
    "Title": "My API [Development]",
    "Servers": [
      {
        "Url": "https://localhost:5001",
        "Description": "Local Development"
      }
    ]
  }
}
```

### Production (Disabled by Default)

```csharp
// appsettings.Production.json
{
  "SwaggerOptions": {
    "Enable": true,
    "EnableInProduction": true,  // Must explicitly enable
    "Title": "My API",
    "Servers": [
      {
        "Url": "https://api.example.com",
        "Description": "Production"
      }
    ]
  }
}
```

### Conditional Enabling

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApiDocumentation(options =>
{
    options.Enable = !builder.Environment.IsProduction();
    options.Title = $"My API [{builder.Environment.EnvironmentName}]";
    options.Version = "v1";
});

var app = builder.Build();
app.UseOpenApiDocumentation();
```

### Multiple Environments

```json
// appsettings.Development.json
{
  "SwaggerOptions": {
    "RoutePrefix": "swagger",
    "Servers": [
      { "Url": "https://localhost:5001", "Description": "Local" }
    ]
  }
}

// appsettings.Staging.json
{
  "SwaggerOptions": {
    "RoutePrefix": "swagger",
    "Servers": [
      { "Url": "https://api-staging.example.com", "Description": "Staging" }
    ]
  }
}

// appsettings.Production.json
{
  "SwaggerOptions": {
    "RoutePrefix": "api-docs",
    "EnableInProduction": true,
    "Servers": [
      { "Url": "https://api.example.com", "Description": "Production" }
    ]
  }
}
```

---

## ?? XML Documentation

### Enable XML Comments

1. **Update .csproj:**

```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

2. **Configure Swagger:**

```json
{
  "SwaggerOptions": {
    "IncludeXmlComments": true
  }
}
```

### XML Documentation Examples

**Controller:**

```csharp
/// <summary>
/// Product management API
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Products")]
public class ProductsController : ControllerBase
{
    /// <summary>
    /// Create a new product
    /// </summary>
    /// <param name="request">Product creation request</param>
    /// <returns>Created product</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/products
    ///     {
    ///         "name": "Product Name",
    ///         "price": 99.99,
    ///         "description": "Product description"
    ///     }
    /// 
    /// </remarks>
    /// <response code="201">Product created successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        // Implementation
    }
}
```

**Model:**

```csharp
/// <summary>
/// Product data transfer object
/// </summary>
public class ProductDto
{
    /// <summary>
    /// Product unique identifier
    /// </summary>
    /// <example>123</example>
    public int Id { get; set; }

    /// <summary>
    /// Product name
    /// </summary>
    /// <example>Laptop</example>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Product price in USD
    /// </summary>
    /// <example>999.99</example>
    [Range(0.01, 999999.99)]
    public decimal Price { get; set; }

    /// <summary>
    /// Product description
    /// </summary>
    /// <example>High-performance laptop with 16GB RAM</example>
    public string? Description { get; set; }
}
```

---

## ?? Advanced Scenarios

### Programmatic Configuration

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApiDocumentation(options =>
{
    options.Enable = true;
    options.Title = "My Advanced API";
    options.Version = "v1";
    options.Description = "Comprehensive API with advanced features";
    
    options.ContactName = "API Team";
    options.ContactEmail = "api@example.com";
    options.ContactUrl = "https://example.com/support";
    
    options.LicenseName = "MIT";
    options.LicenseUrl = "https://opensource.org/licenses/MIT";
    
    options.Servers.Add(new ServerUrl
    {
        Url = "https://api.example.com",
        Description = "Production Server"
    });
    
    options.Security.EnableJwtBearer = true;
    options.Security.EnableApiKey = true;
    
    options.UI.DocumentTitle = "My API Docs";
    options.UI.DocExpansion = "list";
    options.UI.EnableFilter = true;
    
    options.Documentation.TagDescriptions = new Dictionary<string, string>
    {
        ["Users"] = "User management endpoints",
        ["Products"] = "Product catalog"
    };
});
```

### Multiple API Versions

```csharp
// Program.cs
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

builder.Services.AddOpenApiDocumentation(options =>
{
    options.Title = "My API";
    options.Version = "v1";
});

// Controller
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class UsersV1Controller : ControllerBase
{
    // V1 endpoints
}

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("2.0")]
public class UsersV2Controller : ControllerBase
{
    // V2 endpoints
}
```

### Custom Route Prefix

```csharp
// Access at /api-documentation instead of /swagger
{
  "SwaggerOptions": {
    "RoutePrefix": "api-documentation"
  }
}
```

### Encrypted Configuration

```json
{
  "SwaggerOptions": {
    "Security": {
      "OAuth2": {
        "ClientSecret": "ENC:aB3cD4eF5gH6iJ7kL8mN9oP0qR1sT2uV3wX4yZ5=="
      }
    }
  }
}
```

### Integration with Configuration Encryption

The module automatically supports the Craft configuration encryption pattern:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Enable configuration decryption
builder.Configuration.AddDecryption();

// Sensitive values in SwaggerOptions will be decrypted automatically
builder.Services.AddOpenApiDocumentation(builder.Configuration);
```

---

## ?? Migration Guide

### From Old to New Implementation

**Old (Deprecated):**

```csharp
// Program.cs
services.AddOpenApiDocumentation(config);

app.UseOpenApiDocumentation();
```

**New (Current):**

```csharp
// Program.cs
builder.Services.AddOpenApiDocumentation(builder.Configuration);

app.UseOpenApiDocumentation();
```

### Configuration Migration

**Old Configuration:**

```json
{
  "SwaggerOptions": {
    "Title": "API",
    "Version": "v1",
    "ContactEmail": "TBD",
    "ContactUrl": "TBD"
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
    "ContactEmail": "support@example.com",
    "ContactUrl": "https://example.com/contact",
    "Security": {
      "EnableJwtBearer": true
    }
  }
}
```

### Breaking Changes

1. **Namespace Changed**: Extensions moved from `Craft.Infrastructure.OpenApi` to `Microsoft.Extensions.DependencyInjection`
2. **Configuration Validation**: Now validates on startup with `ValidateOnStart()`
3. **Environment Behavior**: Production disabled by default (set `EnableInProduction: true` to enable)
4. **Security Schemes**: More granular control with `SecurityOptions`

---

## ?? Troubleshooting

### Issue: Swagger UI Not Accessible

**Symptoms:**
- 404 error when accessing `/swagger`

**Solutions:**

1. **Check if enabled:**
```json
{
  "SwaggerOptions": {
    "Enable": true
  }
}
```

2. **Check environment:**
```json
{
  "SwaggerOptions": {
    "EnableInProduction": true  // Required for production
  }
}
```

3. **Check route prefix:**
```json
{
  "SwaggerOptions": {
    "RoutePrefix": "swagger"  // Access at /swagger
  }
}
```

---

### Issue: "One or more validation errors occurred"

**Symptoms:**
- Application fails to start with validation exception

**Solutions:**

1. **Check required fields:**
```json
{
  "SwaggerOptions": {
    "Enable": true,
    "Title": "My API",  // Required
    "Version": "v1"     // Required
  }
}
```

2. **Validate URLs:**
```json
{
  "SwaggerOptions": {
    "ContactUrl": "https://example.com",  // Must be valid absolute URL
    "LicenseUrl": "https://example.com"   // Must be valid absolute URL
  }
}
```

---

### Issue: XML Documentation Not Showing

**Symptoms:**
- No descriptions in Swagger UI

**Solutions:**

1. **Enable XML generation:**
```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>
```

2. **Check file exists:**
```bash
# Check bin folder for XML file
ls bin/Debug/net10.0/*.xml
```

3. **Verify configuration:**
```json
{
  "SwaggerOptions": {
    "IncludeXmlComments": true
  }
}
```

---

### Issue: Security Scheme Not Working

**Symptoms:**
- "Authorize" button missing or not working

**Solutions:**

1. **Enable security scheme:**
```json
{
  "SwaggerOptions": {
    "Security": {
      "EnableJwtBearer": true
    }
  }
}
```

2. **Check controller attributes:**
```csharp
[Authorize]  // Required for endpoints
[HttpGet]
public IActionResult Get() { }
```

---

### Issue: Custom CSS Not Loading

**Symptoms:**
- Custom theme not applied

**Solutions:**

1. **Verify URL is accessible:**
```bash
curl https://cdn.example.com/custom.css
```

2. **Check CORS settings:**
```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

---

## ?? Best Practices

### ? Do's

1. **Use XML documentation** for comprehensive API docs
2. **Enable validation** with `ValidateOnStart()`
3. **Configure environment-specific** settings
4. **Use security schemes** appropriate for your API
5. **Provide meaningful descriptions** for tags and operations
6. **Version your API** and document breaking changes
7. **Test Swagger UI** in all environments
8. **Use semantic versioning** (v1, v2, etc.)
9. **Document response types** with `ProducesResponseType`
10. **Provide example values** in XML comments

### ? Don'ts

1. **Don't enable in production** without security
2. **Don't use placeholder values** like "TBD"
3. **Don't ignore validation errors**
4. **Don't expose sensitive information** in descriptions
5. **Don't forget to update** XML comments
6. **Don't use generic error messages**
7. **Don't skip response documentation**
8. **Don't leave obsolete endpoints** undocumented
9. **Don't use complex authentication** without clear instructions
10. **Don't forget CORS** for external CSS/JS

---

## ?? Features Summary

### ? Implemented

- ? **Multiple Security Schemes**: JWT Bearer, API Key, OAuth2
- ? **UI Customization**: Themes, custom CSS/JS, layout options
- ? **XML Documentation**: Automatic inclusion with validation
- ? **Environment-Specific**: Different configs per environment
- ? **Configuration Validation**: Startup validation with detailed errors
- ? **Server URLs**: Multiple server configurations
- ? **Tag Descriptions**: Custom descriptions for endpoint groups
- ? **Document Filters**: Custom document modifications
- ? **Deep Linking**: Direct links to operations
- ? **Authorization Persistence**: Remember auth across sessions
- ? **Operation Filtering**: Search/filter operations
- ? **Configuration Encryption**: Supports encrypted values
- ? **Programmatic Configuration**: Code-based setup option
- ? **Comprehensive Logging**: Detailed operation logging

### ?? Future Enhancements

- ?? Multiple API version support (v1, v2 simultaneously)
- ?? Custom schema filters
- ?? Request/response examples from attributes
- ?? Auto-generate client SDKs
- ?? Export to OpenAPI YAML
- ?? Markdown support in descriptions

---

## ?? Related Documentation

- [Swashbuckle Documentation](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)
- [OpenAPI Specification](https://swagger.io/specification/)
- [.NET 10 Documentation](https://learn.microsoft.com/en-us/dotnet/)

---

**Last Updated:** January 2025  
**Version:** 2.0  
**Target Framework:** .NET 10  
**Status:** ? Production Ready

---

For more information or support, please check the troubleshooting section or contact the development team.
