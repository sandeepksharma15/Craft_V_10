# OpenAPI Module - Quick Reference Card

> Quick reference for the most common tasks with the OpenAPI/Swagger module

---

## ?? Quick Setup (5 minutes)

### 1. Add to Program.cs
```csharp
builder.Services.AddOpenApiDocumentation(builder.Configuration);
app.UseOpenApiDocumentation();
```

### 2. Configure appsettings.json
```json
{
  "SwaggerOptions": {
    "Enable": true,
    "Title": "My API",
    "Version": "v1",
    "Description": "My API Documentation"
  }
}
```

### 3. Run and Access
```bash
dotnet run
# Navigate to: https://localhost:5001/swagger
```

---

## ?? Common Configurations

### Minimal Setup
```json
{
  "SwaggerOptions": {
    "Enable": true,
    "Title": "My API",
    "Version": "v1"
  }
}
```

### With Contact Info
```json
{
  "SwaggerOptions": {
    "Enable": true,
    "Title": "My API",
    "Version": "v1",
    "ContactEmail": "support@example.com",
    "ContactUrl": "https://example.com/support"
  }
}
```

### With JWT Authentication
```json
{
  "SwaggerOptions": {
    "Enable": true,
    "Title": "My API",
    "Version": "v1",
    "Security": {
      "EnableJwtBearer": true
    }
  }
}
```

### With API Key Authentication
```json
{
  "SwaggerOptions": {
    "Enable": true,
    "Title": "My API",
    "Version": "v1",
    "Security": {
      "EnableApiKey": true,
      "ApiKeyHeaderName": "X-API-Key"
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

### Production Setup
```json
{
  "SwaggerOptions": {
    "Enable": true,
    "EnableInProduction": true,
    "RoutePrefix": "api-docs"
  }
}
```

---

## ?? Security Configurations

### JWT Only
```json
{
  "Security": {
    "EnableJwtBearer": true
  }
}
```

### API Key Only
```json
{
  "Security": {
    "EnableApiKey": true,
    "ApiKeyHeaderName": "X-API-Key"
  }
}
```

### JWT + API Key
```json
{
  "Security": {
    "EnableJwtBearer": true,
    "EnableApiKey": true,
    "ApiKeyHeaderName": "X-API-Key"
  }
}
```

### OAuth2
```json
{
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
```

---

## ?? UI Customizations

### Expand All Operations
```json
{
  "UI": {
    "DocExpansion": "full"
  }
}
```

### Enable Filtering
```json
{
  "UI": {
    "EnableFilter": true
  }
}
```

### Show Operation IDs
```json
{
  "UI": {
    "DisplayOperationId": true
  }
}
```

### Dark Theme
```json
{
  "UI": {
    "CustomCssUrl": "https://cdn.jsdelivr.net/npm/swagger-ui-themes@3.0.1/themes/3.x/theme-monokai.css"
  }
}
```

### Custom Document Title
```json
{
  "UI": {
    "DocumentTitle": "My API Documentation"
  }
}
```

---

## ?? XML Documentation

### Enable in .csproj
```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

### Controller Example
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
    public IActionResult GetAll() { }
}
```

### Model Example
```csharp
/// <summary>
/// User data transfer object
/// </summary>
public class UserDto
{
    /// <summary>
    /// User unique identifier
    /// </summary>
    [Required]
    public int Id { get; set; }

    /// <summary>
    /// User full name
    /// </summary>
    [Required]
    public string Name { get; set; }
}
```

---

## ?? Environment-Specific

### Development
```json
{
  "SwaggerOptions": {
    "Enable": true,
    "Title": "My API [Dev]",
    "UI": {
      "DocExpansion": "full"
    }
  }
}
```

### Staging
```json
{
  "SwaggerOptions": {
    "Enable": true,
    "Title": "My API [Staging]",
    "Servers": [
      {
        "Url": "https://api-staging.example.com"
      }
    ]
  }
}
```

### Production
```json
{
  "SwaggerOptions": {
    "Enable": true,
    "EnableInProduction": true,
    "RoutePrefix": "api-docs",
    "Servers": [
      {
        "Url": "https://api.example.com"
      }
    ]
  }
}
```

---

## ?? Programmatic Configuration

### Basic
```csharp
builder.Services.AddOpenApiDocumentation(options =>
{
    options.Enable = true;
    options.Title = "My API";
    options.Version = "v1";
});
```

### With Security
```csharp
builder.Services.AddOpenApiDocumentation(options =>
{
    options.Enable = true;
    options.Title = "My API";
    options.Version = "v1";
    options.Security.EnableJwtBearer = true;
});
```

### Environment-Based
```csharp
builder.Services.AddOpenApiDocumentation(options =>
{
    options.Enable = !builder.Environment.IsProduction();
    options.Title = $"My API [{builder.Environment.EnvironmentName}]";
    options.Version = "v1";
});
```

---

## ?? Troubleshooting

### Swagger Not Showing

**Check 1: Enabled?**
```json
{ "SwaggerOptions": { "Enable": true } }
```

**Check 2: Production?**
```json
{ "SwaggerOptions": { "EnableInProduction": true } }
```

**Check 3: Route?**
```
Default: https://localhost:5001/swagger
Custom: https://localhost:5001/{RoutePrefix}
```

### Validation Errors

**Ensure Required Fields:**
```json
{
  "SwaggerOptions": {
    "Enable": true,
    "Title": "Required",
    "Version": "Required"
  }
}
```

### XML Comments Not Showing

**Step 1: Enable in .csproj**
```xml
<GenerateDocumentationFile>true</GenerateDocumentationFile>
```

**Step 2: Rebuild**
```bash
dotnet build
```

**Step 3: Verify Config**
```json
{ "SwaggerOptions": { "IncludeXmlComments": true } }
```

---

## ?? Property Reference

### Essential Properties

| Property | Default | Description |
|----------|---------|-------------|
| `Enable` | true | Enable/disable Swagger |
| `Title` | "API Documentation" | API title |
| `Version` | "v1" | API version |
| `RoutePrefix` | "swagger" | Swagger UI route |
| `EnableInProduction` | false | Enable in production |

### Security Properties

| Property | Default | Description |
|----------|---------|-------------|
| `Security.EnableJwtBearer` | true | JWT Bearer auth |
| `Security.EnableApiKey` | false | API Key auth |
| `Security.ApiKeyHeaderName` | "X-API-Key" | Header name |
| `Security.EnableOAuth2` | false | OAuth2 auth |

### UI Properties

| Property | Default | Description |
|----------|---------|-------------|
| `UI.DocExpansion` | "none" | "none", "list", "full" |
| `UI.EnableFilter` | true | Enable filtering |
| `UI.PersistAuthorization` | true | Remember auth |
| `UI.CustomCssUrl` | null | Theme URL |

---

## ?? Quick Links

- **Documentation**: `Craft.Infrastructure/OpenApi/README.md`
- **Migration Guide**: `Craft.Infrastructure/OpenApi/MIGRATION.md`
- **Sample Config**: `Craft.Infrastructure/OpenApi/appsettings.swagger.json`
- **Tests**: `Tests/Craft.Infrastructure.Tests/OpenApi/`

---

## ?? Tips

1. **Always use XML documentation** for better API docs
2. **Enable filtering** for better UX: `"EnableFilter": true`
3. **Persist authorization** for convenience: `"PersistAuthorization": true`
4. **Use dark theme** for better visibility
5. **Disable in production** unless necessary
6. **Use meaningful descriptions** in XML comments
7. **Document response codes** with `[ProducesResponseType]`
8. **Test in all environments** before deployment

---

**Quick Reference Version:** 2.0  
**Last Updated:** January 2025  
**Target:** .NET 10
