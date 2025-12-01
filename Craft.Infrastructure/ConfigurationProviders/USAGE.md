# Configuration Encryption - Usage Examples

This document provides practical examples for using the configuration encryption feature in your applications.

## Table of Contents
1. [Prerequisites](#prerequisites)
2. [Quick Start](#quick-start)
3. [Real-World Examples](#real-world-examples)
4. [Testing](#testing)

---

## Prerequisites

### 1. Generate Encryption Keys

```bash
# Navigate to the KeySafe tool
cd Apps/Craft.KeySafe

# Generate new AES-256 keys
dotnet run -- -g

# Output will show:
# Generated Key: wXzE5vL9kN2fR8tY6mQ3pW7jH4nA1cV0dG8kL2pQ9m==
# Generated IV: xP9mK3nR7tY2wQ5vJ8nM4pL7k==
```

### 2. Set Environment Variables

**Windows (PowerShell):**
```powershell
[System.Environment]::SetEnvironmentVariable('AES_ENCRYPTION_KEY', 'wXzE5vL9kN2fR8tY6mQ3pW7jH4nA1cV0dG8kL2pQ9m==', 'User')
[System.Environment]::SetEnvironmentVariable('AES_ENCRYPTION_IV', 'xP9mK3nR7tY2wQ5vJ8nM4pL7k==', 'User')
```

**Windows (Command Prompt):**
```cmd
setx AES_ENCRYPTION_KEY "wXzE5vL9kN2fR8tY6mQ3pW7jH4nA1cV0dG8kL2pQ9m=="
setx AES_ENCRYPTION_IV "xP9mK3nR7tY2wQ5vJ8nM4pL7k=="
```

**Linux/Mac:**
```bash
export AES_ENCRYPTION_KEY="wXzE5vL9kN2fR8tY6mQ3pW7jH4nA1cV0dG8kL2pQ9m=="
export AES_ENCRYPTION_IV="xP9mK3nR7tY2wQ5vJ8nM4pL7k=="
```

### 3. Encrypt Sensitive Values

```bash
# Encrypt a database password
cd Apps/Craft.KeySafe
dotnet run -- -e "MyDatabasePassword123"

# Output:
# Encrypted: aB3cD4eF5gH6iJ7kL8mN9oP0qR1sT2uV3wX4yZ5==
```

---

## Quick Start

### Minimal Setup

**appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MyApp;User Id=sa;Password=ENC:aB3cD4eF5gH6iJ7kL8mN9oP0qR1sT2uV3wX4yZ5=="
  }
}
```

**Program.cs:**
```csharp
using Craft.Infrastructure.ConfigurationProviders;

var builder = WebApplication.CreateBuilder(args);

// Add decryption support - must be called AFTER adding configuration sources
builder.Configuration.AddDecryption();

var app = builder.Build();

// Use configuration normally - values are automatically decrypted
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// connectionString now contains the decrypted password

app.MapGet("/", () => "Hello World!");
app.Run();
```

---

## Real-World Examples

### Example 1: ASP.NET Core Web API

**appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-sql.example.com;Database=ProductionDB;User Id=app_user;Password=ENC:xY9zA1bC2dE3fG4hI5jK6lM7nO8pQ9rS0tU1vW2=="
  },
  "JwtSettings": {
    "SecretKey": "ENC:aB3cD4eF5gH6iJ7kL8mN9oP0qR1sT2uV3wX4yZ5==",
    "Issuer": "MyApp",
    "Audience": "MyAppUsers",
    "ExpirationMinutes": 60
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "noreply@myapp.com",
    "SmtpPassword": "ENC:pQ9rS0tU1vW2xY3zA4bC5dE6fG7hI8jK9lM0nO1==",
    "EnableSsl": true
  },
  "ExternalApis": {
    "Stripe": {
      "PublicKey": "pk_live_1234567890",
      "SecretKey": "ENC:kL8mN9oP0qR1sT2uV3wX4yZ5aB6cD7eF8gH9iJ0=="
    }
  }
}
```

**Program.cs:**
```csharp
using Craft.Infrastructure.ConfigurationProviders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ? Add decryption BEFORE using configuration
builder.Configuration.AddDecryption();

// Configure JWT authentication with decrypted secret
var jwtSecret = builder.Configuration["JwtSettings:SecretKey"]!;
var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            ValidateLifetime = true
        };
    });

// Configure database with decrypted connection string
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

---

### Example 2: Using IOptions Pattern

**appsettings.json:**
```json
{
  "EmailOptions": {
    "Provider": "smtp",
    "From": "noreply@myapp.com",
    "DisplayName": "My Application",
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "noreply@myapp.com",
    "SmtpPassword": "ENC:aB3cD4eF5gH6iJ7kL8mN9oP0qR1sT2uV3wX4yZ5==",
    "EnableSsl": true
  }
}
```

**EmailOptions.cs:**
```csharp
public class EmailOptions
{
    public string Provider { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty; // Will be decrypted
    public bool EnableSsl { get; set; }
}
```

**Program.cs:**
```csharp
using Craft.Infrastructure.ConfigurationProviders;

var builder = WebApplication.CreateBuilder(args);

// Approach 1: Provider-level decryption
builder.Configuration.AddDecryption();

// Configure options
builder.Services.Configure<EmailOptions>(
    builder.Configuration.GetSection("EmailOptions"));

// Approach 2: Additional IOptions-level decryption (belt and suspenders)
builder.Services.AddOptionsDecryption<EmailOptions>();

var app = builder.Build();
app.Run();
```

**EmailService.cs:**
```csharp
using Microsoft.Extensions.Options;

public class EmailService
{
    private readonly EmailOptions _options;

    public EmailService(IOptions<EmailOptions> options)
    {
        _options = options.Value;
        // _options.SmtpPassword is automatically decrypted
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        using var client = new SmtpClient(_options.SmtpServer, _options.SmtpPort)
        {
            EnableSsl = _options.EnableSsl,
            Credentials = new NetworkCredential(
                _options.SmtpUsername, 
                _options.SmtpPassword) // ? Already decrypted
        };

        var message = new MailMessage(_options.From, to, subject, body);
        await client.SendMailAsync(message);
    }
}
```

---

### Example 3: Multiple Environments

**appsettings.Development.json:**
```json
{
  "Database": {
    "ConnectionString": "Server=localhost;Database=DevDB;Integrated Security=true;",
    "ApiKey": "dev-plain-text-key-for-local-testing"
  }
}
```

**appsettings.Staging.json:**
```json
{
  "Database": {
    "ConnectionString": "ENC:xY9zA1bC2dE3fG4hI5jK6lM7nO8pQ9rS0tU1vW2==",
    "ApiKey": "ENC:aB3cD4eF5gH6iJ7kL8mN9oP0qR1sT2uV3wX4yZ5=="
  }
}
```

**appsettings.Production.json:**
```json
{
  "Database": {
    "ConnectionString": "ENC:pQ9rS0tU1vW2xY3zA4bC5dE6fG7hI8jK9lM0nO1==",
    "ApiKey": "ENC:kL8mN9oP0qR1sT2uV3wX4yZ5aB6cD7eF8gH9iJ0=="
  }
}
```

**Program.cs:**
```csharp
var builder = WebApplication.CreateBuilder(args);

// Load environment-specific configuration
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// Add decryption for all environments
builder.Configuration.AddDecryption();

var app = builder.Build();
app.Run();
```

---

### Example 4: Azure Key Vault + Encrypted Config

**Program.cs:**
```csharp
using Azure.Identity;
using Craft.Infrastructure.ConfigurationProviders;

var builder = WebApplication.CreateBuilder(args);

// 1. Load from Azure Key Vault (for encryption keys themselves)
if (builder.Environment.IsProduction())
{
    var keyVaultUri = new Uri($"https://{builder.Configuration["KeyVaultName"]}.vault.azure.net/");
    builder.Configuration.AddAzureKeyVault(keyVaultUri, new DefaultAzureCredential());
    
    // Override environment variables with values from Key Vault
    var keyFromVault = builder.Configuration["AES-ENCRYPTION-KEY"];
    var ivFromVault = builder.Configuration["AES-ENCRYPTION-IV"];
    
    if (!string.IsNullOrEmpty(keyFromVault) && !string.IsNullOrEmpty(ivFromVault))
    {
        Environment.SetEnvironmentVariable("AES_ENCRYPTION_KEY", keyFromVault);
        Environment.SetEnvironmentVariable("AES_ENCRYPTION_IV", ivFromVault);
    }
}

// 2. Add decryption for configuration files
builder.Configuration.AddDecryption();

var app = builder.Build();
app.Run();
```

---

### Example 5: Custom Prefix

**appsettings.json:**
```json
{
  "Database": {
    "Password": "ENCRYPTED:aB3cD4eF5gH6iJ7kL8mN9oP0qR1sT2uV3wX4yZ5=="
  },
  "ApiKeys": {
    "Stripe": "SECRET:xY9zA1bC2dE3fG4hI5jK6lM7nO8pQ9rS0tU1vW2=="
  }
}
```

**Program.cs:**
```csharp
using Craft.Infrastructure.ConfigurationProviders;

var builder = WebApplication.CreateBuilder(args);

// Use custom prefix "ENCRYPTED:" instead of default "ENC:"
builder.Configuration.AddDecryption("ENCRYPTED:");

// For multiple prefixes, you'd need to apply decryption multiple times
// or standardize on one prefix

var app = builder.Build();
app.Run();
```

---

### Example 6: Post-Processing Approach (Legacy)

**Program.cs:**
```csharp
using Craft.Infrastructure.ConfigurationProviders;

var builder = WebApplication.CreateBuilder(args);

// Must cast to IConfigurationRoot for post-processing
var config = builder.Configuration as IConfigurationRoot;
config?.DecryptConfiguration();

var app = builder.Build();
app.Run();
```

---

## Testing

### Unit Test Example

```csharp
using Craft.Infrastructure.ConfigurationProviders;
using Craft.Utilities.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

public class ConfigurationTests
{
    [Fact]
    public void Configuration_DecryptsEncryptedDatabasePassword()
    {
        // Arrange
        var keySafeService = new KeySafeService();
        var plainPassword = "MySecretPassword123";
        var encryptedPassword = keySafeService.Encrypt(plainPassword);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:Server"] = "localhost",
                ["Database:Password"] = $"ENC:{encryptedPassword}",
                ["Database:Database"] = "TestDB"
            })
            .AddDecryption()
            .Build();

        // Act
        var password = config["Database:Password"];
        var server = config["Database:Server"];

        // Assert
        Assert.Equal(plainPassword, password);
        Assert.Equal("localhost", server);
    }
}
```

### Integration Test Example

```csharp
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Api_UsesDecryptedConfiguration()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/config/test");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("ENC:", content); // Ensure no encrypted values are exposed
    }
}
```

---

## Troubleshooting Common Issues

### Issue: Values Not Decrypting

**Check:**
1. Environment variables are set correctly:
   ```bash
   echo $AES_ENCRYPTION_KEY
   echo $AES_ENCRYPTION_IV
   ```

2. `AddDecryption()` is called AFTER all configuration sources:
   ```csharp
   // ? Wrong
   builder.Configuration.AddDecryption();
   builder.Configuration.AddJsonFile("appsettings.json");

   // ? Correct
   builder.Configuration.AddJsonFile("appsettings.json");
   builder.Configuration.AddDecryption();
   ```

3. Prefix matches (default is `ENC:`):
   ```json
   {
     "Password": "ENC:encrypted-value-here"
   }
   ```

### Issue: Decryption Fails

**Check:**
1. The value was encrypted with the SAME keys
2. The encrypted value is valid Base64
3. Re-encrypt if keys have changed:
   ```bash
   cd Apps/Craft.KeySafe
   dotnet run -- -e "YourValue"
   ```

### Issue: Works Locally But Not in Production

**Check:**
1. Production environment variables are set
2. Keys are loaded from secure storage (Azure Key Vault, etc.)
3. Application has permissions to read Key Vault

---

## Best Practices

1. **Never commit encryption keys to source control**
2. **Use different keys per environment**
3. **Rotate keys regularly**
4. **Encrypt only sensitive data** (passwords, API keys, connection strings)
5. **Document what's encrypted** (add comments in config files)
6. **Test decryption in CI/CD pipeline**
7. **Use Azure Key Vault or AWS Secrets Manager in production**
8. **Enable logging during development** to catch issues early

---

For more information, see the [README](./README.md).
