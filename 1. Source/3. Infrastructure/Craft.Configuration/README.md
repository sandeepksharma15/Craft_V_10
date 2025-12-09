# Craft.Configuration

A production-ready, scalable configuration management library for .NET 10 applications. Provides encrypted configuration values, type-safe configuration classes, automatic reload on change, multi-environment support, and extensible cloud provider integration.

## Features

- ? **Encrypted Configuration Values** - AES-256 encryption for sensitive data
- ? **Type-Safe Configuration** - Strongly-typed configuration classes with validation
- ? **Automatic Reload** - Hot-reload support when configuration files change
- ? **Multi-Environment Support** - Automatic environment detection and cascading configurations
- ? **Validation on Startup** - Fail-fast validation using Data Annotations
- ? **Mixed Encrypted/Plain Text** - Support for both encrypted and plain text values in the same source
- ? **Extensible Providers** - Pluggable support for Azure Key Vault and AWS Secrets Manager
- ? **Comprehensive Logging** - Built-in logging for debugging and monitoring

## Table of Contents

- [Installation](#installation)
- [Quick Start](#quick-start)
- [Configuration](#configuration)
  - [Basic Usage](#basic-usage)
  - [Encrypted Configuration](#encrypted-configuration)
  - [Multi-Environment Support](#multi-environment-support)
  - [Configuration Validation](#configuration-validation)
- [Advanced Usage](#advanced-usage)
  - [Custom Encryption Providers](#custom-encryption-providers)
  - [Cloud Provider Integration](#cloud-provider-integration)
  - [Configuration Service](#configuration-service)
- [Best Practices](#best-practices)
- [Testing](#testing)
- [API Reference](#api-reference)

## Installation

Add the project reference to your application:

```xml
<ProjectReference Include="..\Craft.Configuration\Craft.Configuration.csproj" />
```

### Required Dependencies

The library requires the following environment variables for encryption:

```bash
AES_ENCRYPTION_KEY=<Base64-encoded 32-byte key>
AES_ENCRYPTION_IV=<Base64-encoded 16-byte IV>
```

Generate keys using the `Craft.KeySafe` utility:

```bash
dotnet run --project Apps/Craft.KeySafe -- generate
```

## Quick Start

### 1. Basic Configuration Setup

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Craft Configuration
builder.Services.AddCraftConfiguration(builder.Configuration);

var app = builder.Build();
```

### 2. Using Encrypted Values in appsettings.json

```json
{
  "Database": {
    "Server": "localhost",
    "Password": "ENC:8y7kH3mR9tY2wQ5vP1xN4cV6bZ0aS=="
  },
  "ApiKeys": {
    "PaymentGateway": "ENC:2wQ5vP1xN4cV6bZ0aS8y7kH3mR9tY==",
    "EmailService": "plain-text-api-key"
  }
}
```

### 3. Type-Safe Configuration Classes

```csharp
public class DatabaseSettings
{
    public string Server { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
}

// Register with validation
builder.Services.ConfigureAndValidate<DatabaseSettings>(
    builder.Configuration, 
    "Database");
```

## Configuration

### Basic Usage

#### Using IConfiguration Service

```csharp
public class MyService
{
    private readonly IConfigurationService _configService;

    public MyService(IConfigurationService configService)
    {
        _configService = configService;
    }

    public void DoWork()
    {
        // Get string value
        var serverName = _configService.Get("Database:Server");

        // Get typed value
        var maxRetries = _configService.Get<int>("Database:MaxRetries");

        // Get section as object
        var dbSettings = _configService.GetSection<DatabaseSettings>("Database");

        // Check if key exists
        if (_configService.Exists("OptionalFeature"))
        {
            // Feature is configured
        }
    }
}
```

#### Using IOptions Pattern

```csharp
public class MyService
{
    private readonly DatabaseSettings _settings;

    public MyService(IOptions<DatabaseSettings> options)
    {
        _settings = options.Value;
    }

    public void Connect()
    {
        // Password is automatically decrypted
        Connect(_settings.Server, _settings.Password);
    }
}
```

### Encrypted Configuration

#### Encrypting Configuration Values

Use the `Craft.KeySafe` utility to encrypt sensitive values:

```bash
# Encrypt a value
dotnet run --project Apps/Craft.KeySafe -- encrypt "MySecretPassword123"

# Output: 8y7kH3mR9tY2wQ5vP1xN4cV6bZ0aS==
```

Add the encrypted value to your configuration with the `ENC:` prefix:

```json
{
  "Database": {
    "Password": "ENC:8y7kH3mR9tY2wQ5vP1xN4cV6bZ0aS=="
  }
}
```

#### Using Different Encryption Prefixes

```csharp
var envConfig = new EnvironmentConfiguration
{
    UseEncryption = true,
    EncryptionPrefix = "ENCRYPTED:"
};

builder.Services.AddCraftConfiguration(builder.Configuration, envConfig);
```

#### Backward Compatible API

For existing code, you can use the extension methods directly:

```csharp
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddDecryption("ENC:")
    .Build();
```

### Multi-Environment Support

#### Automatic Environment Detection

```csharp
var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureCraftConfiguration((context, config) =>
{
    var envConfig = new EnvironmentConfiguration
    {
        Environment = context.HostingEnvironment.EnvironmentName,
        UseEnvironmentSpecificFiles = true,
        JsonFiles = ["appsettings.json", "secrets.json"],
        ReloadOnChange = true
    };

    config.ConfigureEnvironment(envConfig, context.HostingEnvironment.ContentRootPath);
});
```

This will load configurations in the following order:
1. `appsettings.json`
2. `appsettings.{Environment}.json` (e.g., `appsettings.Development.json`)
3. `secrets.json`
4. `secrets.{Environment}.json`
5. Environment variables
6. User secrets (in Development only)

#### Environment-Specific Configuration

```json
// appsettings.json (base configuration)
{
  "Database": {
    "Server": "localhost",
    "MaxRetries": 3
  }
}

// appsettings.Production.json (production overrides)
{
  "Database": {
    "Server": "production-server.database.windows.net",
    "Password": "ENC:productionPasswordHash=="
  }
}
```

### Configuration Validation

#### Using Data Annotations

```csharp
public class ApiSettings
{
    [Required]
    [Url]
    public string BaseUrl { get; set; } = string.Empty;

    [Required]
    [MinLength(32)]
    public string ApiKey { get; set; } = string.Empty;

    [Range(1, 10)]
    public int MaxRetries { get; set; } = 3;

    [Range(1000, 60000)]
    public int TimeoutMs { get; set; } = 30000;
}

// Register with validation
builder.Services.ConfigureAndValidate<ApiSettings>(builder.Configuration, "Api");
```

#### Validation on Startup

```csharp
var envConfig = new EnvironmentConfiguration
{
    ValidateOnStartup = true
};

builder.Services.AddCraftConfiguration(builder.Configuration, envConfig);

// Optionally validate explicitly
builder.Services.ValidateConfigurationOnStartup();
```

If validation fails, the application will throw an exception on startup with detailed error messages.

#### Manual Validation

```csharp
public class StartupValidator
{
    private readonly IConfigurationValidator _validator;

    public StartupValidator(IConfigurationValidator validator)
    {
        _validator = validator;
    }

    public void ValidateAll()
    {
        var result = _validator.Validate();
        
        if (!result.IsValid)
        {
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"Configuration Error: {error}");
            }
            
            throw new InvalidOperationException("Configuration validation failed");
        }
    }

    public void ValidateSpecific()
    {
        var result = _validator.Validate<ApiSettings>();
        
        if (!result.IsValid)
        {
            // Handle validation errors
        }
    }
}
```

## Advanced Usage

### Custom Encryption Providers

#### Using ASP.NET Core Data Protection

```csharp
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"C:\keys"))
    .SetApplicationName("MyApp");

builder.Services.AddSingleton<IConfigurationEncryption>(sp =>
{
    var dataProtectionProvider = sp.GetRequiredService<IDataProtectionProvider>();
    var logger = sp.GetService<ILogger<DataProtectionEncryptor>>();
    
    return new DataProtectionEncryptor(
        dataProtectionProvider, 
        "ENC:", 
        logger);
});
```

#### Implementing Custom Encryption

```csharp
public class CustomEncryptor : IConfigurationEncryption
{
    public string EncryptionPrefix => "CUSTOM:";

    public string Encrypt(string plainText)
    {
        // Your custom encryption logic
        return Convert.ToBase64String(YourEncryptionMethod(plainText));
    }

    public string Decrypt(string encryptedText)
    {
        // Your custom decryption logic
        return YourDecryptionMethod(Convert.FromBase64String(encryptedText));
    }

    public bool IsEncrypted(string? value)
    {
        return value?.StartsWith(EncryptionPrefix) ?? false;
    }
}

// Register custom encryptor
builder.Services.AddSingleton<IConfigurationEncryption, CustomEncryptor>();
```

### Cloud Provider Integration

#### Azure Key Vault (Placeholder)

```csharp
// Install: Azure.Extensions.AspNetCore.Configuration.Secrets

builder.Configuration.AddAzureKeyVault(
    new Uri("https://your-vault.vault.azure.net/"),
    new DefaultAzureCredential());

// The configuration service will automatically work with Key Vault values
builder.Services.AddCraftConfiguration(builder.Configuration);
```

#### AWS Secrets Manager (Placeholder)

```csharp
// Install: Amazon.Extensions.Configuration.SystemsManager

builder.Configuration.AddSystemsManager(config =>
{
    config.Path = "/myapp/";
    config.ReloadAfter = TimeSpan.FromMinutes(5);
});

builder.Services.AddCraftConfiguration(builder.Configuration);
```

### Configuration Service

#### Programmatic Configuration Management

```csharp
public class ConfigurationManager
{
    private readonly IConfigurationService _configService;

    public ConfigurationManager(IConfigurationService configService)
    {
        _configService = configService;
    }

    public void UpdateConfiguration()
    {
        // Set a value
        _configService.Set("FeatureFlags:NewFeature", "true");

        // Reload from sources
        _configService.Reload();
    }

    public void ListAllKeys()
    {
        foreach (var key in _configService.GetAllKeys())
        {
            Console.WriteLine($"{key} = {_configService.Get(key)}");
        }
    }
}
```

## Best Practices

### 1. Environment Variables for Encryption Keys

**Never** commit encryption keys to source control. Use environment variables:

```bash
# Development
export AES_ENCRYPTION_KEY="your-dev-key"
export AES_ENCRYPTION_IV="your-dev-iv"

# Production (use secure secret management)
# Azure App Service: Application Settings
# AWS: Parameter Store or Secrets Manager
# Kubernetes: Secrets
```

### 2. Layered Configuration

Use configuration layering for flexibility:

```
appsettings.json              (defaults)
  ?
appsettings.{Environment}.json (environment overrides)
  ?
Environment Variables          (runtime overrides)
  ?
User Secrets (Development)    (local development)
  ?
Cloud Secrets (Production)    (Azure Key Vault / AWS Secrets)
```

### 3. Encrypt Only Sensitive Data

Only encrypt truly sensitive data (passwords, API keys, connection strings). Plain text is faster and easier to debug.

```json
{
  "Database": {
    "Server": "localhost",                              // Plain text
    "Port": 5432,                                       // Plain text
    "Database": "myapp",                                // Plain text
    "Username": "myuser",                               // Plain text (not sensitive)
    "Password": "ENC:8y7kH3mR9tY2wQ5vP1xN4cV6bZ0aS=="  // Encrypted
  }
}
```

### 4. Validate Configuration Early

Always validate configuration on startup to catch errors early:

```csharp
builder.Services.ConfigureAndValidate<DatabaseSettings>(builder.Configuration);
builder.Services.ValidateConfigurationOnStartup();
```

### 5. Use Type-Safe Configuration

Prefer strongly-typed configuration classes over string keys:

```csharp
// ? Avoid
var connectionString = configuration["Database:ConnectionString"];

// ? Prefer
var dbSettings = configService.GetSection<DatabaseSettings>("Database");
var connectionString = dbSettings.ConnectionString;
```

### 6. Reload Configuration Safely

When using `ReloadOnChange`, be aware of threading implications:

```csharp
public class ConfigurationAwareService
{
    private readonly IOptionsMonitor<MySettings> _options;

    public ConfigurationAwareService(IOptionsMonitor<MySettings> options)
    {
        _options = options;
        
        // Subscribe to changes
        _options.OnChange(newSettings =>
        {
            // React to configuration changes
            UpdateBehavior(newSettings);
        });
    }

    public void DoWork()
    {
        // Always get current value
        var settings = _options.CurrentValue;
    }
}
```

## Testing

### Unit Testing Configuration

```csharp
public class ConfigurationTests
{
    [Fact]
    public void ConfigurationService_DecryptsEncryptedValues()
    {
        // Arrange
        SetupTestEncryptionKeys();
        var keySafeService = new KeySafeService();
        var plainText = "MySecret";
        var encrypted = keySafeService.Encrypt(plainText);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Secret"] = $"ENC:{encrypted}"
            })
            .Build();

        var encryption = new AesConfigurationEncryptor(keySafeService);
        var configService = new JsonConfigurationProvider(config, encryption);

        // Act
        var value = configService.Get("Secret");

        // Assert
        Assert.Equal(plainText, value);
    }

    private void SetupTestEncryptionKeys()
    {
        var key = "wXzE5vL9kN2fR8tY6mQ3pW7jH4nA1cV0";
        var iv = "xP9mK3nR7tY2wQ5v";
        
        Environment.SetEnvironmentVariable("AES_ENCRYPTION_KEY", 
            Convert.ToBase64String(Encoding.UTF8.GetBytes(key)));
        Environment.SetEnvironmentVariable("AES_ENCRYPTION_IV", 
            Convert.ToBase64String(Encoding.UTF8.GetBytes(iv)));
    }
}
```

### Integration Testing

```csharp
public class ConfigurationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ConfigurationIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Application_LoadsConfigurationCorrectly()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/health");

        // Assert
        response.EnsureSuccessStatusCode();
    }
}
```

## API Reference

### Interfaces

#### IConfigurationService

```csharp
public interface IConfigurationService
{
    string? Get(string key);
    T? Get<T>(string key);
    T? GetSection<T>(string sectionKey) where T : class, new();
    void Set(string key, string? value);
    void Reload();
    IEnumerable<string> GetAllKeys();
    bool Exists(string key);
}
```

#### IConfigurationEncryption

```csharp
public interface IConfigurationEncryption
{
    string Encrypt(string plainText);
    string Decrypt(string encryptedText);
    bool IsEncrypted(string? value);
    string EncryptionPrefix { get; }
}
```

#### IConfigurationValidator

```csharp
public interface IConfigurationValidator
{
    ConfigurationValidationResult Validate();
    ConfigurationValidationResult Validate<T>() where T : class;
}
```

### Extension Methods

```csharp
// Service registration
IServiceCollection AddCraftConfiguration(IConfiguration, EnvironmentConfiguration?)

// Configuration builder
IConfigurationBuilder ConfigureEnvironment(EnvironmentConfiguration, string?)
IConfigurationBuilder AddConfigurationEncryption(string encryptionPrefix = "ENC:")
IConfigurationBuilder AddDecryption(string encryptionPrefix = "ENC:")

// Validation
IServiceCollection ValidateConfigurationOnStartup()
IServiceCollection ConfigureAndValidate<TOptions>(IConfiguration, string?)

// Backward compatibility
IConfigurationBuilder AddDecryption(ILoggerFactory, string)
IServiceCollection AddOptionsDecryption<TOptions>(string)
IConfigurationRoot DecryptConfiguration(string)
```

### Configuration Classes

#### EnvironmentConfiguration

```csharp
public class EnvironmentConfiguration
{
    public string Environment { get; set; } = "Development";
    public bool UseEncryption { get; set; }
    public string EncryptionPrefix { get; set; } = "ENC:";
    public bool ValidateOnStartup { get; set; } = true;
    public bool ReloadOnChange { get; set; } = true;
    public List<string> JsonFiles { get; set; } = ["appsettings.json"];
    public bool UseEnvironmentSpecificFiles { get; set; } = true;
    public bool UseUserSecrets { get; set; } = true;
    public string? UserSecretsId { get; set; }
}
```

## Troubleshooting

### Common Issues

#### 1. Encryption keys not found

**Error**: `InvalidOperationException: Encryption Key not found`

**Solution**: Set environment variables:
```bash
export AES_ENCRYPTION_KEY="<your-base64-key>"
export AES_ENCRYPTION_IV="<your-base64-iv>"
```

#### 2. Configuration validation fails on startup

**Error**: `InvalidOperationException: Configuration validation failed`

**Solution**: Check your configuration values match the validation attributes. Enable detailed logging to see specific errors.

#### 3. Encrypted values not decrypting

**Error**: Values still show as `ENC:...`

**Solution**: Ensure you're using the correct encryption service and the keys match what was used to encrypt the values.

## License

This project is part of the Craft framework. See the main repository for license information.

## Contributing

Contributions are welcome! Please follow the coding standards defined in `.editorconfig` and ensure all tests pass.

## Support

For issues and questions:
- GitHub Issues: https://github.com/sandeepksharma15/Craft_V_10/issues
- Documentation: See inline XML documentation in the code

---

**Version**: 1.0.0 (.NET 10)
**Last Updated**: 2024
