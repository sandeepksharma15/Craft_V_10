# Craft.Data Quick Reference Guide

## ?? Quick Start

### Basic Setup

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Configure databases
builder.Services.ConfigureDatabases(builder.Configuration);

// Add health checks
builder.Services.AddHealthChecks()
    .AddDatabaseHealthCheck<AppDbContext>();

var app = builder.Build();

// Run migrations
await app.MigrateDatabaseAsync<AppDbContext>();

app.MapHealthChecks("/health");
app.Run();
```

---

## ?? Configuration

### appsettings.json

```json
{
  "DatabaseOptions": {
    "ConnectionString": "Server=localhost;Database=MyDb;Trusted_Connection=True;",
    "DbProvider": "mssql",
    "CommandTimeout": 30,
    "MaxRetryCount": 3,
    "MaxRetryDelay": 15,
    "EnableDetailedErrors": false,
    "EnableSensitiveDataLogging": false
  },
  "MultiTenantOptions": {
    "IsEnabled": true
  }
}
```

---

## ??? DbContext Patterns

### Pattern 1: Simple DbContext

```csharp
public class AppDbContext : BaseDbContext<AppDbContext>
{
    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ITenant currentTenant,
        ICurrentUser currentUser,
        ILoggerFactory loggerFactory)
        : base(options, currentTenant, currentUser, loggerFactory)
    {
        Features.AddCommonFeatures();
    }

    public DbSet<Customer> Customers { get; set; }
}
```

### Pattern 2: Identity DbContext

```csharp
public class AppDbContext : BaseIdentityDbContext<AppDbContext>
{
    public AppDbContext(
        DbContextOptions options,
        ITenant currentTenant,
        ICurrentUser currentUser,
        ILoggerFactory loggerFactory)
        : base(options, currentTenant, currentUser, loggerFactory)
    {
        Features
            .AddIdentity()
            .AddCommonFeatures();
    }

    public DbSet<Customer> Customers { get; set; }
}
```

---

## ?? Health Checks

### DbContext Health Check

```csharp
services.AddHealthChecks()
    .AddDatabaseHealthCheck<AppDbContext>(
        name: "database",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "db", "ready" });
```

### Connection String Health Check

```csharp
services.AddHealthChecks()
    .AddDatabaseConnectionHealthCheck(
        connectionString: "Server=...",
        dbProvider: "mssql",
        name: "external-db");
```

### Using DatabaseOptions

```csharp
var dbOptions = builder.Configuration
    .GetSection("DatabaseOptions")
    .Get<DatabaseOptions>();

services.AddHealthChecks()
    .AddDatabaseHealthCheck(dbOptions);
```

---

## ??? Migrations

### Apply Migrations and Seeders

```csharp
await app.MigrateDatabaseAsync<AppDbContext>();
```

### Create Database

```csharp
await app.EnsureDatabaseCreatedAsync<AppDbContext>();
```

### Run Only Seeders

```csharp
await app.RunSeedersAsync();
```

### Delete Database (Development Only!)

```csharp
if (app.Environment.IsDevelopment())
{
    await app.DeleteDatabaseAsync<AppDbContext>();
}
```

### Get Database Info

```csharp
var dbInfo = app.GetDatabaseInfo<AppDbContext>();
Console.WriteLine($"Provider: {dbInfo.ProviderName}");
Console.WriteLine($"Can Connect: {dbInfo.CanConnect}");
Console.WriteLine($"Is Relational: {dbInfo.IsRelational}");
Console.WriteLine($"Is InMemory: {dbInfo.IsInMemory}");
```

---

## ?? Features

### Add All Common Features

```csharp
Features.AddCommonFeatures();
// Adds: AuditTrail, SoftDelete, Concurrency
```

### Add Individual Features

```csharp
Features
    .AddAuditTrail()
    .AddSoftDelete()
    .AddConcurrency()
    .AddMultiTenancy(currentTenant);
```

### Add Identity Feature

```csharp
Features.AddIdentity(tablePrefix: "Id_");
```

---

## ?? Manual Configuration

### Using Extension Method

```csharp
services.AddDbContext<AppDbContext>((sp, options) =>
{
    options.UseDatabase(
        dbProvider: "mssql",
        connectionString: "Server=...",
        maxRetryCount: 3,
        maxRetryDelay: 15,
        commandTimeout: 30,
        serviceProvider: sp);
});
```

### Using DatabaseOptions

```csharp
services.AddDbContext<AppDbContext>((sp, options) =>
{
    var dbOptions = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
    
    options.UseDatabase(
        dbOptions.DbProvider,
        dbOptions.ConnectionString,
        dbOptions,
        sp);
});
```

---

## ?? Logging

### Log Levels

- **Trace:** Connection details (masked), provider normalization
- **Debug:** Context creation, tenant resolution, provider selection
- **Information:** Migration progress, seeder execution
- **Warning:** Database deletion, configuration issues
- **Error:** Provider not found, migration failures

### Enable Debug Logging

```json
{
  "Logging": {
    "LogLevel": {
      "Craft.Data": "Debug",
      "Default": "Information"
    }
  }
}
```

---

## ?? Security

### Connection String Masking

```csharp
// Automatically masked in logs
logger.LogDebug("Connection: {ConnectionString}", maskedString);
// Output: "Connection: Server=localhost;Database=MyDb;..."
```

### Options Validation

```csharp
// Automatically validated at startup
services.ConfigureDatabases(configuration);
// Throws if DbProvider or ConnectionString is missing
```

---

## ?? Testing

### Mock Logger for Tests

```csharp
services.AddLogging();
var sp = services.BuildServiceProvider();
var logger = sp.GetRequiredService<ILogger<TenantDbContextFactory<TContext>>>();
```

### InMemory Database

```csharp
services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("TestDb"));
```

---

## ?? Monitoring

### Health Check Endpoint

```http
GET /health
```

**Response (Healthy):**
```json
{
  "status": "Healthy",
  "results": {
    "database": {
      "status": "Healthy",
      "duration": "00:00:00.0234567"
    }
  }
}
```

### Readiness Endpoint

```csharp
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
```

---

## ? Performance Tips

1. **Use DbContext pooling** for single-tenant apps
2. **Enable query caching** with NoTracking (default in BaseDbContext)
3. **Configure connection pooling** in connection string
4. **Set appropriate CommandTimeout** for long-running queries
5. **Use health check intervals** to avoid constant DB polling

---

## ?? Troubleshooting

### Issue: "Provider not supported"

**Solution:** Ensure provider is registered in DI:
```csharp
services.AddSingleton<IDatabaseProvider, SqlServerDatabaseProvider>();
```

### Issue: "No connection string"

**Solution:** Check appsettings.json has `DatabaseOptions` section

### Issue: Health check always failing

**Solution:** Increase timeout:
```csharp
.AddDatabaseHealthCheck<AppDbContext>(timeout: TimeSpan.FromSeconds(10))
```

### Issue: Migrations not applying

**Solution:** Check logs for migration errors:
```csharp
builder.Logging.SetMinimumLevel(LogLevel.Debug);
```

---

## ?? Additional Resources

- [Craft.Data README](./README.md)
- [Improvements Summary](./IMPROVEMENTS_SUMMARY.md)
- [DbContext Features Guide](./DbContexts/README.md)
- [ASP.NET Core Health Checks](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)

---

**Last Updated:** 2025
**Version:** 1.0.33+
