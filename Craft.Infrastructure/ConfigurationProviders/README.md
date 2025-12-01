# Configuration Encryption - Quick Reference Guide

> **Version:** 1.0+ | **Target Framework:** .NET 10

## ?? Table of Contents

1. [Quick Start](#-quick-start)
2. [Overview](#-overview)
3. [Three Approaches](#-three-approaches)
4. [Encrypting Configuration Values](#-encrypting-configuration-values)
5. [Configuration Examples](#-configuration-examples)
6. [Advanced Scenarios](#-advanced-scenarios)
7. [Best Practices](#-best-practices)
8. [Troubleshooting](#-troubleshooting)

---

## ?? Quick Start

### 1. Setup Encryption Keys

Generate encryption keys using the Craft.KeySafe CLI:

```bash
# Generate new keys
KeySafe.exe -g

# Output:
# Generated Key: wXzE5vL9kN2fR8tY6mQ3pW7jH4nA1cV0dG8kL2pQ9m==
# Generated IV: xP9mK3nR7tY2wQ5vJ8nM4pL7k==

# Set environment variables
KeySafe.exe --set-key "wXzE5vL9kN2fR8tY6mQ3pW7jH4nA1cV0dG8kL2pQ9m=="
KeySafe.exe --set-iv "xP9mK3nR7tY2wQ5vJ8nM4pL7k=="
```

### 2. Encrypt Sensitive Values

```bash
# Encrypt a password
KeySafe.exe -e "MySecretPassword123"

# Output:
# Encrypted: aB3cD4eF5gH6iJ7kL8mN9oP0qR==
```

### 3. Add to Configuration

**appsettings.json:**
```json
{
  "Database": {
    "Server": "localhost",
    "Password": "ENC:aB3cD4eF5gH6iJ7kL8mN9oP0qR==",
    "Database": "MyApp"
  },
  "ExternalServices": {
    "ApiKey": "ENC:xY9zA1bC2dE3fG4hI5jK6lM7nO8pQ==",
    "Endpoint": "https://api.example.com"
  }
}
```

### 4. Enable Decryption in Your App

**Program.cs:**
```csharp
var builder = WebApplication.CreateBuilder(args);

// Approach 1: Configuration Provider (Recommended)
builder.Configuration.AddDecryption();

// OR Approach 2: Post-Processing
// var config = builder.Configuration as IConfigurationRoot;
// config?.DecryptConfiguration();

// Use configuration normally
var app = builder.Build();
app.Run();
```

---

## ?? Overview

The Configuration Encryption feature provides automatic encryption/decryption of sensitive configuration values using AES-256 encryption. It supports:

? **Multiple Approaches** - Provider, Post-Processing, and IOptions  
? **Automatic Detection** - Values prefixed with `ENC:` are decrypted automatically  
? **Any Configuration Source** - JSON, Environment Variables, User Secrets, etc.  
? **Environment-Specific Keys** - Different keys per environment  
? **Mixed Values** - Encrypted and plain-text in the same file  
? **Type-Safe** - Works with IOptions pattern  
? **Comprehensive Logging** - Detailed diagnostics  

---

## ?? Three Approaches

### Approach 1: Custom Configuration Provider (Recommended)

**Best for:** New applications, cleanest approach

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add decryption to ALL configuration sources
builder.Configuration.AddDecryption();

var app = builder.Build();
app.Run();
```

**Features:**
- ? Decrypts during configuration loading
- ? Works with any configuration source
- ? Most transparent and performant
- ? No code changes needed to access values

**Usage:**
```csharp
// Values are automatically decrypted
var password = builder.Configuration["Database:Password"];
// Returns: "MySecretPassword123" (decrypted)
```

### Approach 2: Post-Processing

**Best for:** Legacy applications, explicit control

```csharp
var builder = WebApplication.CreateBuilder(args);

// Must cast to IConfigurationRoot
var config = builder.Configuration as IConfigurationRoot;
config?.DecryptConfiguration();

var app = builder.Build();
app.Run();
```

**Features:**
- ? Explicit and visible
- ? Works with existing configuration
- ? Can be called multiple times
- ?? Modifies configuration in place

**Usage:**
```csharp
// Values are decrypted after calling DecryptConfiguration()
var password = builder.Configuration["Database:Password"];
```

### Approach 3: IOptions Post-Configuration

**Best for:** Strongly-typed configuration classes

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register options
builder.Services.Configure<DatabaseOptions>(
    builder.Configuration.GetSection("Database"));

// Add decryption for this specific options class
builder.Services.AddOptionsDecryption<DatabaseOptions>();

var app = builder.Build();
app.Run();
```

**Features:**
- ? Works with IOptions pattern
- ? Type-safe
- ? Decrypts only string properties
- ? Can combine with other approaches

**Usage:**
```csharp
public class MyService
{
    private readonly DatabaseOptions _options;

    public MyService(IOptions<DatabaseOptions> options)
    {
        _options = options.Value;
        // _options.Password is decrypted
    }
}
```

---

## ?? Encrypting Configuration Values

### Using Craft.KeySafe CLI

#### Generate Keys
```bash
KeySafe.exe -g
```

#### Encrypt a Value
```bash
KeySafe.exe -e "YourSensitiveValue"
```

#### Decrypt a Value (for testing)
```bash
KeySafe.exe -d "aB3cD4eF5gH6iJ7kL8mN9oP0qR=="
```

#### Set Environment Variables
```bash
# For current process only
KeySafe.exe --set-key "your-base64-key"
KeySafe.exe --set-iv "your-base64-iv"
```

### Programmatically

```csharp
using Craft.Utilities.Services;

var keySafeService = new KeySafeService();
var encrypted = keySafeService.Encrypt("MySecret");
Console.WriteLine($"ENC:{encrypted}");
```

---

## ?? Configuration Examples

### Example 1: Database Connection Strings

**appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MyApp;User Id=sa;Password=ENC:xY9zA1bC2dE3=="
  }
}
```

**Program.cs:**
```csharp
builder.Configuration.AddDecryption();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Returns: "Server=localhost;Database=MyApp;User Id=sa;Password=MyActualPassword"
```

### Example 2: API Keys

**appsettings.json:**
```json
{
  "ExternalServices": {
    "Stripe": {
      "PublicKey": "pk_test_123456",
      "SecretKey": "ENC:aB3cD4eF5gH6iJ7kL8mN9oP0qR=="
    },
    "SendGrid": {
      "ApiKey": "ENC:xY9zA1bC2dE3fG4hI5jK6lM7nO8pQ=="
    }
  }
}
```

### Example 3: Strongly-Typed Options

**appsettings.json:**
```json
{
  "EmailOptions": {
    "From": "noreply@example.com",
    "SmtpServer": "smtp.example.com",
    "SmtpPassword": "ENC:aB3cD4eF5gH6iJ7kL8mN9oP0qR=="
  }
}
```

**Configuration Class:**
```csharp
public class EmailOptions
{
    public string From { get; set; } = string.Empty;
    public string SmtpServer { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
}
```

**Program.cs:**
```csharp
builder.Services.Configure<EmailOptions>(
    builder.Configuration.GetSection("EmailOptions"));

builder.Services.AddOptionsDecryption<EmailOptions>();
```

**Usage:**
```csharp
public class EmailService
{
    private readonly EmailOptions _options;

    public EmailService(IOptions<EmailOptions> options)
    {
        _options = options.Value;
        // _options.SmtpPassword is automatically decrypted
    }
}
```

### Example 4: Environment-Specific Encryption

**appsettings.Development.json:**
```json
{
  "Database": {
    "Password": "PlainTextPasswordForDev"
  }
}
```

**appsettings.Production.json:**
```json
{
  "Database": {
    "Password": "ENC:xY9zA1bC2dE3fG4hI5jK6lM7nO8pQ=="
  }
}
```

### Example 5: Environment Variables

```bash
# Set encrypted value in environment
export API_KEY="ENC:aB3cD4eF5gH6iJ7kL8mN9oP0qR=="
```

**Program.cs:**
```csharp
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddDecryption();

var apiKey = builder.Configuration["API_KEY"];
// Automatically decrypted
```

### Example 6: Mixed Encrypted and Plain Values

**appsettings.json:**
```json
{
  "App": {
    "Name": "My Application",
    "Version": "1.0.0",
    "Environment": "Production",
    "LicenseKey": "ENC:aB3cD4eF5gH6iJ7kL8mN9oP0qR==",
    "AdminEmail": "admin@example.com",
    "AdminPassword": "ENC:xY9zA1bC2dE3fG4hI5jK6lM7nO8pQ=="
  }
}
```

All plain values remain unchanged, only values with `ENC:` prefix are decrypted.

---

## ?? Advanced Scenarios

### Custom Encryption Prefix

```csharp
// Use a different prefix
builder.Configuration.AddDecryption("ENCRYPTED:");
```

**appsettings.json:**
```json
{
  "ApiKey": "ENCRYPTED:aB3cD4eF5gH6iJ7kL8mN9oP0qR=="
}
```

### Multiple Configuration Sources

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add all your configuration sources first
builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>()
    .AddCommandLine(args);

// Then add decryption - it wraps ALL sources
builder.Configuration.AddDecryption();
```

### With Logging

```csharp
// Configuration Provider with logging
var loggerFactory = LoggerFactory.Create(builder => 
    builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

builder.Configuration.AddDecryption(loggerFactory);

// Post-Processing with logging
var logger = app.Logger;
config?.DecryptConfiguration(logger);
```

### Combining All Three Approaches

```csharp
var builder = WebApplication.CreateBuilder(args);

// Approach 1: Provider-level decryption
builder.Configuration.AddDecryption();

// Approach 2: Post-processing (optional, redundant here)
// var config = builder.Configuration as IConfigurationRoot;
// config?.DecryptConfiguration();

// Approach 3: Options-level decryption
builder.Services.Configure<DatabaseOptions>(
    builder.Configuration.GetSection("Database"));
builder.Services.AddOptionsDecryption<DatabaseOptions>();

var app = builder.Build();
```

### Per-Environment Keys

**Development:**
```bash
# Use development keys
set AES_ENCRYPTION_KEY=dev-key-base64
set AES_ENCRYPTION_IV=dev-iv-base64
```

**Production:**
```bash
# Use production keys (stored securely in Azure Key Vault, AWS Secrets Manager, etc.)
# These should be injected at runtime, not stored in source control
```

### Azure Key Vault Integration

```csharp
var builder = WebApplication.CreateBuilder(args);

// 1. Load from Azure Key Vault
if (builder.Environment.IsProduction())
{
    var keyVaultUri = new Uri($"https://{builder.Configuration["KeyVaultName"]}.vault.azure.net/");
    builder.Configuration.AddAzureKeyVault(keyVaultUri, new DefaultAzureCredential());
}

// 2. Add decryption (after Key Vault)
builder.Configuration.AddDecryption();
```

### Selective Decryption

```csharp
// Decrypt only specific sections
var databaseSection = builder.Configuration.GetSection("Database");
var secretsSection = builder.Configuration.GetSection("Secrets");

// Use post-processing approach for selective decryption
if (databaseSection is IConfigurationRoot dbConfig)
    dbConfig.DecryptConfiguration();
```

---

## ? Best Practices

### 1. **Never Commit Keys to Source Control**

```bash
# .gitignore
*.key
*.enc
appsettings.Production.json  # If it contains keys
```

### 2. **Use Different Keys Per Environment**

- Development: Simple keys, can be in source control
- Staging: Moderate security, stored in CI/CD
- Production: High security, Azure Key Vault / AWS Secrets Manager

### 3. **Rotate Keys Regularly**

```bash
# Generate new keys
KeySafe.exe -g

# Re-encrypt all values
KeySafe.exe -e "YourValue"

# Update configuration files
# Update environment variables
```

### 4. **Document What's Encrypted**

Add comments in configuration files:

```json
{
  "Database": {
    "Server": "localhost",
    "Password": "ENC:aB3cD4eF5gH6iJ7k==",  // Encrypted with AES-256
    "Timeout": 30
  }
}
```

### 5. **Validate Keys at Startup**

```csharp
var builder = WebApplication.CreateBuilder(args);

// Validate encryption keys are set
var key = Environment.GetEnvironmentVariable("AES_ENCRYPTION_KEY");
var iv = Environment.GetEnvironmentVariable("AES_ENCRYPTION_IV");

if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(iv))
{
    throw new InvalidOperationException(
        "Encryption keys not configured. Set AES_ENCRYPTION_KEY and AES_ENCRYPTION_IV environment variables.");
}

builder.Configuration.AddDecryption();
```

### 6. **Use Approach 1 (Provider) for New Apps**

```csharp
// ? Recommended
builder.Configuration.AddDecryption();

// ?? Use only if you need explicit control
// config.DecryptConfiguration();
```

### 7. **Test Decryption in CI/CD**

```csharp
[Fact]
public void Configuration_DecryptsEncryptedValues()
{
    // Arrange
    Environment.SetEnvironmentVariable("AES_ENCRYPTION_KEY", "...");
    Environment.SetEnvironmentVariable("AES_ENCRYPTION_IV", "...");

    var config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.Test.json")
        .AddDecryption()
        .Build();

    // Act
    var password = config["Database:Password"];

    // Assert
    Assert.Equal("ExpectedPlainTextPassword", password);
}
```

### 8. **Monitor Decryption Failures**

Enable logging to catch decryption failures:

```json
{
  "Logging": {
    "LogLevel": {
      "Craft.Infrastructure.ConfigurationProviders": "Debug"
    }
  }
}
```

### 9. **Encrypt Only Sensitive Data**

Don't encrypt everything - only:
- Passwords
- API Keys
- Connection Strings
- Tokens
- License Keys

Leave non-sensitive data plain for easier debugging.

### 10. **Backup Encryption Keys**

Store keys in multiple secure locations:
- Azure Key Vault (primary)
- Encrypted backup file (offline)
- Password manager (team access)

---

## ?? Troubleshooting

### Issue: "Encryption Key not found"

**Symptom:**
```
InvalidOperationException: Encryption Key not found
```

**Solution:**
```bash
# Verify environment variables are set
echo %AES_ENCRYPTION_KEY%
echo %AES_ENCRYPTION_IV%

# Set them if missing
KeySafe.exe --set-key "your-key"
KeySafe.exe --set-iv "your-iv"
```

---

### Issue: "Decryption failed"

**Symptom:**
```
InvalidOperationException: Decryption failed. The provided key and IV may be incorrect.
```

**Solution:**
1. Verify the encryption keys match the ones used to encrypt
2. Check the encrypted value is valid Base64
3. Ensure the `ENC:` prefix is present
4. Test decryption manually:

```bash
KeySafe.exe -d "your-encrypted-value"
```

---

### Issue: "Value not decrypted"

**Symptom:** Configuration value still shows `ENC:...`

**Solution:**
1. Ensure `AddDecryption()` is called AFTER all configuration sources
2. Check the prefix matches (default is `ENC:`)
3. Verify environment variables are set

```csharp
// ? Wrong order
builder.Configuration.AddDecryption();
builder.Configuration.AddJsonFile("appsettings.json");

// ? Correct order
builder.Configuration.AddJsonFile("appsettings.json");
builder.Configuration.AddDecryption();
```

---

### Issue: "Works locally but not in production"

**Symptom:** Decryption works in development but fails in production

**Solution:**
1. Verify production environment variables are set
2. Check keys are different per environment
3. Ensure production keys are loaded from secure storage

```csharp
if (builder.Environment.IsProduction())
{
    // Load from Azure Key Vault
    var keyVaultUri = new Uri("https://your-vault.vault.azure.net/");
    builder.Configuration.AddAzureKeyVault(keyVaultUri, new DefaultAzureCredential());
}
```

---

### Issue: "Configuration changes not reflected"

**Symptom:** Changed encrypted values but they don't update

**Solution:**
1. Restart the application (environment variables are loaded at startup)
2. Clear any configuration caching
3. Verify the new encrypted value is correct

---

### Issue: "IOptions not decrypting"

**Symptom:** `IOptions<T>` values still encrypted

**Solution:**
Ensure you called `AddOptionsDecryption<T>()`:

```csharp
builder.Services.Configure<DatabaseOptions>(
    builder.Configuration.GetSection("Database"));

// ? Add this line
builder.Services.AddOptionsDecryption<DatabaseOptions>();
```

---

### Issue: "Multiple encryption prefixes"

**Symptom:** Need to support multiple prefixes

**Solution:**
Chain multiple decryption calls:

```csharp
// Not currently supported - use single prefix
// Workaround: Standardize on one prefix

// Convert all to ENC:
builder.Configuration.AddDecryption("ENC:");
```

---

### Common Error Messages

| Error | Cause | Solution |
|-------|-------|----------|
| `Encryption Key not found` | Environment variables not set | Set `AES_ENCRYPTION_KEY` and `AES_ENCRYPTION_IV` |
| `Decryption failed` | Wrong keys or corrupted value | Verify keys match encryption keys |
| `Invalid Base64` | Malformed encrypted value | Re-encrypt the value |
| `Key must be 32 bytes` | Invalid encryption key | Generate new key with `KeySafe.exe -g` |
| `IV must be 16 bytes` | Invalid IV | Generate new IV with `KeySafe.exe -g` |

---

## ?? Additional Resources

### Related Documentation
- [Craft.KeySafe CLI Guide](../../Apps/Craft.KeySafe/README.md)
- [KeySafeService Documentation](../../Craft.Utilities/Services/README.md)
- [Configuration in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration)

### Security Best Practices
- [OWASP Secrets Management](https://owasp.org/www-community/Secrets_Management_Cheat_Sheet)
- [Azure Key Vault](https://learn.microsoft.com/en-us/azure/key-vault/)
- [AWS Secrets Manager](https://aws.amazon.com/secrets-manager/)

### Sample Projects
- See `Tests/Craft.Infrastructure.Tests/ConfigurationProviders/` for comprehensive examples

---

## ?? Summary

### Quick Decision Guide

**Use Configuration Provider (Approach 1) if:**
- ? Starting a new application
- ? Want automatic, transparent decryption
- ? Need best performance

**Use Post-Processing (Approach 2) if:**
- ? Working with legacy code
- ? Need explicit control
- ? Can't modify configuration builder

**Use IOptions (Approach 3) if:**
- ? Using strongly-typed configuration
- ? Want type-safe decryption
- ? Need to combine with validation

**Use All Three if:**
- ? Want maximum flexibility
- ? Have complex configuration scenarios
- ? Need belt-and-suspenders approach

---

**Last Updated:** January 2025  
**Version:** 1.0.0  
**Target Framework:** .NET 10  
**Status:** ? Production Ready

---

**Need help?** Check the [Troubleshooting](#-troubleshooting) section or enable debug logging.
