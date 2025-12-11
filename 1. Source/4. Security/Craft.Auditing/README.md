# Craft.Auditing

A comprehensive auditing library for Entity Framework Core that automatically tracks entity changes with support for soft deletes, retention policies, and customizable serialization.

## Overview

Craft.Auditing provides a robust audit trail system that captures entity changes in your application. It seamlessly integrates with Entity Framework Core to track create, update, and delete operations, including soft delete scenarios.

## Features

- **Automatic Change Tracking**: Captures all entity changes with old and new values
- **Soft Delete Support**: Properly handles entities implementing `ISoftDelete`
- **Property-Level Control**: Include or exclude specific properties from auditing using attributes
- **Navigation Property Tracking**: Optionally track changes to related entities
- **Retention Policies**: Configure automatic archival of audit entries
- **Custom Serialization**: Customize JSON serialization options for audit data
- **Type-Safe Filtering**: Helper methods to filter auditable entities by attributes
- **Query Extensions**: Fluent API for querying audit trails
- **Deserialization Helpers**: Easy access to audit data with type-safe methods
- **Comparison Tools**: Compare audit entries and track differences
- **Validation**: Built-in validation for audit trail integrity
- **Background Archival**: Automated archival service for old audit entries

## Installation

Add a reference to the Craft.Auditing project in your application:

```xml
<ProjectReference Include="path\to\Craft.Auditing\Craft.Auditing.csproj" />
```

## Quick Start

### 1. Implement IAuditTrailDbContext

Your DbContext should implement `IAuditTrailDbContext`:

```csharp
public class ApplicationDbContext : DbContext, IAuditTrailDbContext
{
    public DbSet<AuditTrail> AuditTrails { get; set; }
    
    // Your other DbSets...
}
```

### 2. Configure Entity with ModelBuilder Extension

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Use the convenient extension method to configure AuditTrail with all constraints and indexes
    modelBuilder.ConfigureAuditTrail();
    
    // Other configurations...
}
```

### 3. Create Audit Trails

```csharp
public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    var auditEntries = new List<AuditTrail>();
    var userId = GetCurrentUserId(); // Your method to get current user ID
    
    foreach (var entry in ChangeTracker.Entries())
    {
        if (entry.State == EntityState.Added || 
            entry.State == EntityState.Modified || 
            entry.State == EntityState.Deleted)
        {
            var auditEntry = await AuditTrail.CreateAsync(entry, userId, cancellationToken);
            auditEntries.Add(auditEntry);
        }
    }
    
    AuditTrails.AddRange(auditEntries);
    return await base.SaveChangesAsync(cancellationToken);
}
```

## Core Components

### AuditTrail Class

The main entity that stores audit information.

**Properties:**
- `TableName`: Name of the audited entity/table
- `ChangeType`: Type of change (Created, Updated, Deleted, None)
- `DateTimeUTC`: When the change occurred
- `KeyValues`: JSON of primary key values
- `OldValues`: JSON of previous property values
- `NewValues`: JSON of new property values
- `ChangedColumns`: JSON array of changed column names
- `UserId`: ID of the user who made the change
- `ArchiveAfter`: Optional date for automatic archival
- `IsArchived`: Whether the entry has been archived

**Constants:**
- `MaxChangedColumnsLength`: 2000 characters
- `MaxKeyValuesLength`: 1000 characters
- `MaxTableNameLength`: 255 characters

### Factory Methods

**Synchronous Creation:**
```csharp
var auditTrail = AuditTrail.Create(entityEntry, userId);
```

**Asynchronous Creation:**
```csharp
var auditTrail = await AuditTrail.CreateAsync(entityEntry, userId, cancellationToken);
```

**Direct Constructor:**
```csharp
var auditTrail = new AuditTrail(entityEntry, userId);
```

## Attributes

### AuditAttribute

Explicitly marks a class or property for auditing.

**Class-level:**
```csharp
[Audit]
public class Product : BaseEntity
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

**Property-level:**
```csharp
public class User : BaseEntity
{
    [Audit]
    public string Email { get; set; }
    
    [Audit]
    public DateTime LastLoginAt { get; set; }
}
```

### DoNotAuditAttribute

Excludes a class or property from auditing.

**Class-level:**
```csharp
[DoNotAudit]
public class TemporaryData : BaseEntity
{
    public string Data { get; set; }
}
```

**Property-level:**
```csharp
public class User : BaseEntity
{
    public string Email { get; set; }
    
    [DoNotAudit]
    public string PasswordHash { get; set; }
    
    [DoNotAudit]
    public string RefreshToken { get; set; }
}
```

## Configuration Options

### Retention Policies

**Global Default:**
```csharp
// Set default retention to 90 days for all audit entries
AuditTrail.DefaultRetentionDays = 90;

// Disable default retention
AuditTrail.DefaultRetentionDays = null;
```

**Per-Entry Retention:**
```csharp
var auditTrail = new AuditTrail(entityEntry, userId);
auditTrail.SetRetentionPolicy(30); // Archive after 30 days
```

### Custom Serialization

```csharp
var customOptions = new JsonSerializerOptions
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
};

AuditTrail.ConfigureSerializerOptions(customOptions);

// Reset to default
AuditTrail.ConfigureSerializerOptions(null);
```

**Default Serializer Options:**
- WriteIndented: false
- DefaultIgnoreCondition: WhenWritingNull
- ReferenceHandler: IgnoreCycles
- MaxDepth: 32

### Navigation Properties

```csharp
// Include navigation property changes (default)
AuditTrail.IncludeNavigationProperties = true;

// Exclude navigation property changes for performance
AuditTrail.IncludeNavigationProperties = false;
```

## Query Extensions

Powerful fluent API for querying audit trails with method chaining support.

### Available Query Methods

**Filter by Entity:**
```csharp
// Get audits for a specific entity instance
var audits = await context.AuditTrails
    .ForEntity<Product>(productId)
    .ToListAsync();
```

**Filter by Table:**
```csharp
// Get all audits for a table
var productAudits = await context.AuditTrails
    .ForTable("Product")
    .ToListAsync();
```

**Filter by Date Range:**
```csharp
// Get audits within a date range
var audits = await context.AuditTrails
    .InDateRange(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow)
    .ToListAsync();
```

**Filter by User:**
```csharp
// Get audits by specific user
var userAudits = await context.AuditTrails
    .ByUser(userId)
    .ToListAsync();
```

**Filter by Change Type:**
```csharp
// Get only create operations
var creates = await context.AuditTrails
    .OfChangeType(EntityChangeType.Created)
    .ToListAsync();
```

**Filter by Archival Status:**
```csharp
// Get archived entries
var archived = await context.AuditTrails
    .Archived()
    .ToListAsync();

// Get non-archived entries
var active = await context.AuditTrails
    .NotArchived()
    .ToListAsync();

// Get entries pending archival
var pendingArchival = await context.AuditTrails
    .PendingArchival()
    .ToListAsync();
```

**Time-based Filters:**
```csharp
// Get recent entries (default: last 7 days)
var recent = await context.AuditTrails
    .Recent()
    .ToListAsync();

// Get recent entries (custom days)
var last30Days = await context.AuditTrails
    .Recent(30)
    .ToListAsync();
```

**Ordering:**
```csharp
// Order by most recent first
var newest = await context.AuditTrails
    .OrderByMostRecent()
    .ToListAsync();

// Order by oldest first
var oldest = await context.AuditTrails
    .OrderByOldest()
    .ToListAsync();
```

### Chaining Query Methods

```csharp
// Complex query with multiple filters
var audits = await context.AuditTrails
    .ForTable("Order")
    .ByUser(currentUserId)
    .OfChangeType(EntityChangeType.Updated)
    .NotArchived()
    .Recent(30)
    .OrderByMostRecent()
    .Take(100)
    .ToListAsync();
```

## Deserialization Helpers

Easy access to audit data with type-safe deserialization methods.

### Deserialize to Dictionaries

```csharp
var audit = await context.AuditTrails.FirstAsync();

// Get old values as dictionary
var oldValues = audit.GetOldValuesAsDictionary();

// Get new values as dictionary
var newValues = audit.GetNewValuesAsDictionary();

// Get key values as dictionary
var keyValues = audit.GetKeyValuesAsDictionary();

// Get changed columns as list
var changedColumns = audit.GetChangedColumnsAsList();
```

### Get Specific Property Values

```csharp
// Get property value (returns JsonElement)
var oldName = audit.GetOldValue("Name");
var newName = audit.GetNewValue("Name");

// Get typed property value
var oldPrice = audit.GetOldValue<decimal>("Price");
var newPrice = audit.GetNewValue<decimal>("Price");

// Check if property changed
bool nameChanged = audit.HasPropertyChanged("Name");
```

### Deserialize to Typed Objects

```csharp
// Deserialize entire old/new values to typed object
var oldProduct = audit.DeserializeOldValues<Product>();
var newProduct = audit.DeserializeNewValues<Product>();

if (oldProduct != null && newProduct != null)
{
    Console.WriteLine($"Price changed from {oldProduct.Price} to {newProduct.Price}");
}
```

## Comparison & Difference Analysis

Track and compare changes between audit entries.

### Compare Two Audit Entries

```csharp
var audits = await context.AuditTrails
    .ForEntity<Product>(productId)
    .OrderBy(a => a.DateTimeUTC)
    .ToListAsync();

if (audits.Count >= 2)
{
    var comparison = audits[1].CompareTo(audits[0]);
    
    if (comparison.HasDifferences)
    {
        foreach (var diff in comparison.Differences)
        {
            if (diff.HasChanged)
            {
                Console.WriteLine($"{diff.PropertyName}: {diff.OldValue} ? {diff.NewValue}");
            }
        }
    }
}
```

### Get Property Changes

```csharp
var audit = await context.AuditTrails
    .Where(a => a.ChangeType == EntityChangeType.Updated)
    .FirstAsync();

// Get all property changes
var changes = audit.GetPropertyChanges();

foreach (var change in changes.Where(c => c.HasChanged))
{
    Console.WriteLine($"{change.PropertyName} changed");
}
```

### Check Property Modification

```csharp
// Check if specific property was modified (for Updated entries)
if (audit.PropertyWasModified("Price"))
{
    var oldValue = audit.GetOldValue<decimal>("Price");
    var newValue = audit.GetNewValue<decimal>("Price");
    Console.WriteLine($"Price changed from {oldValue} to {newValue}");
}
```

### Get Changed Properties

```csharp
// Get dictionary of all changed properties with old and new values
var changedProps = audit.GetChangedProperties();

foreach (var (propertyName, (oldValue, newValue)) in changedProps)
{
    Console.WriteLine($"{propertyName}: {oldValue} ? {newValue}");
}
```

## Validation

Built-in validation for audit trail integrity.

### Validate Audit Entry

```csharp
var audit = new AuditTrail(entityEntry, userId);

var validation = AuditTrailValidator.Validate(audit);

if (!validation.IsValid)
{
    foreach (var error in validation.Errors)
    {
        Console.WriteLine($"Error: {error}");
    }
}

if (validation.HasWarnings)
{
    foreach (var warning in validation.Warnings)
    {
        Console.WriteLine($"Warning: {warning}");
    }
}
```

### Length Validation

```csharp
// Check if value will exceed maximum length
bool exceeds = AuditTrailValidator.WillExceedMaxLength(
    longString, 
    AuditTrail.MaxTableNameLength, 
    out int actualLength
);

if (exceeds)
{
    // Truncate safely
    var truncated = AuditTrailValidator.TruncateIfNeeded(
        longString, 
        AuditTrail.MaxTableNameLength,
        "..." // custom suffix
    );
}
```

### Validation Rules

The validator checks for:
- **Required Fields**: TableName, DateTimeUTC must be set
- **Maximum Lengths**: Enforces max lengths for TableName, KeyValues, ChangedColumns
- **Date Validation**: Ensures DateTimeUTC is set and ArchiveAfter >= DateTimeUTC
- **JSON Validation**: Validates JSON format for all JSON fields
- **UTC Compliance**: Warns if dates are not in UTC format

## Background Archival Service

Automated background service for archiving old audit entries.

### Register the Service

```csharp
// In Program.cs or Startup.cs
services.AddAuditTrailArchivalService(
    archivalInterval: TimeSpan.FromHours(24) // Run daily
);

// Or use default interval (24 hours)
services.AddAuditTrailArchivalService();
```

### How It Works

The service:
1. Runs at configured intervals (default: 24 hours)
2. Queries for audit entries where `ArchiveAfter <= DateTime.UtcNow` and `IsArchived == false`
3. Marks eligible entries as archived
4. Saves changes to the database
5. Logs the archival process

### Manual Archival

```csharp
// Archive specific entry
audit.Archive();
await context.SaveChangesAsync();

// Restore from archive
audit.Unarchive();
await context.SaveChangesAsync();

// Check if should be archived
if (audit.ShouldBeArchived())
{
    audit.Archive();
    await context.SaveChangesAsync();
}
```

## Archival Management

### Archiving Entries

```csharp
auditTrail.Archive();        // Mark as archived
auditTrail.Unarchive();      // Restore from archive
bool shouldArchive = auditTrail.ShouldBeArchived(); // Check if eligible
```

### Query Archived Entries

```csharp
// Using query extensions
var archivedEntries = await context.AuditTrails
    .Archived()
    .ToListAsync();

var activeEntries = await context.AuditTrails
    .NotArchived()
    .ToListAsync();

var pendingArchival = await context.AuditTrails
    .PendingArchival()
    .ToListAsync();
```

### Manual Archival Process

```csharp
// Archive old entries
public async Task ArchiveOldAuditEntries()
{
    var entriesToArchive = await context.AuditTrails
        .PendingArchival()
        .ToListAsync();
    
    foreach (var entry in entriesToArchive)
    {
        entry.Archive();
    }
    
    await context.SaveChangesAsync();
}

// Clean up archived entries older than 7 years
public async Task DeleteArchivedEntries()
{
    var cutoffDate = DateTime.UtcNow.AddYears(-7);
    var oldEntries = await context.AuditTrails
        .Archived()
        .Where(a => a.DateTimeUTC < cutoffDate)
        .ToListAsync();
    
    context.AuditTrails.RemoveRange(oldEntries);
    await context.SaveChangesAsync();
}
```

## Helper Methods

### AuditingHelpers Class

**Get Auditable Entities:**
```csharp
// BaseEntity types with [Audit] attribute
var auditableTypes = AuditingHelpers.GetAuditableBaseEntityTypes();

// BaseEntity types with [DoNotAudit] attribute
var nonAuditableTypes = AuditingHelpers.GetNonAuditableBaseEntityTypes();

// Non-BaseEntity types with [Audit] attribute
var otherAuditableTypes = AuditingHelpers.GetAuditableNonBaseEntityTypes();

// Non-BaseEntity types with [DoNotAudit] attribute
var otherNonAuditableTypes = AuditingHelpers.GetNonAuditableNonBaseEntityTypes();
```

**Generic Type Filtering:**
```csharp
// Get types with specific attribute
var typesWithAttribute = AuditingHelpers.GetTypesWithAttribute<BaseEntity, AuditAttribute>(
    includeAbstract: false,
    mustInheritFromBaseType: true
);

// Get types without specific attribute
var typesWithoutAttribute = AuditingHelpers.GetTypesWithoutAttribute<BaseEntity, DoNotAuditAttribute>(
    includeAbstract: false,
    mustInheritFromBaseType: true
);
```

## Extension Methods

### TypeExtensions

```csharp
// Check if type has attributes
bool hasAudit = typeof(Product).HasAuditAttribute();
bool hasDoNotAudit = typeof(TemporaryData).HasDoNotAuditAttribute();

// Check if property has attributes
var property = typeof(User).GetProperty("PasswordHash");
bool propertyHasDoNotAudit = property.HasDoNotAuditAttribute();
bool propertyHasAudit = property.HasAuditAttribute();
```

## Interfaces

### IAuditTrail

Core interface defining audit trail contract with properties:
- ChangedColumns
- ChangeType
- DateTimeUTC
- KeyValues
- NewValues
- OldValues
- TableName
- ArchiveAfter
- IsArchived

Inherits from:
- `ISoftDelete`: Soft delete support
- `IHasUser`: User tracking

### IAuditTrailDbContext

Interface for DbContext with audit trail support:
```csharp
public interface IAuditTrailDbContext
{
    DbSet<AuditTrail> AuditTrails { get; set; }
}
```

## Entity Change Types

```csharp
public enum EntityChangeType
{
    None,       // No change detected
    Created,    // Entity added
    Updated,    // Entity modified
    Deleted     // Entity deleted (including soft delete)
}
```

## Usage Examples

### Basic Auditing

```csharp
public class Product : BaseEntity
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
}

// Changes are automatically audited when SaveChanges is called
var product = new Product { Name = "Widget", Price = 99.99m };
context.Products.Add(product);
await context.SaveChangesAsync(); // Audit entry created
```

### Excluding Sensitive Data

```csharp
public class User : BaseEntity
{
    public string Email { get; set; }
    public string Username { get; set; }
    
    [DoNotAudit]
    public string PasswordHash { get; set; }
    
    [DoNotAudit]
    public string SecurityStamp { get; set; }
}
```

### Soft Delete Auditing

```csharp
public class Document : BaseEntity, ISoftDelete
{
    public string Title { get; set; }
    public bool IsDeleted { get; set; }
}

// Soft delete is captured as EntityChangeType.Deleted
document.IsDeleted = true;
await context.SaveChangesAsync(); // Audit shows as Deleted
```

### Complete SaveChanges Integration

```csharp
public class ApplicationDbContext : DbContext, IAuditTrailDbContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IHttpContextAccessor httpContextAccessor) 
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public DbSet<AuditTrail> AuditTrails { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure audit trail with indexes
        modelBuilder.ConfigureAuditTrail();
        
        // Other entity configurations
        modelBuilder.Entity<Product>();
        modelBuilder.Entity<Order>();
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var auditEntries = new List<AuditTrail>();
        
        foreach (var entry in ChangeTracker.Entries())
        {
            // Skip audit entries themselves to avoid recursion
            if (entry.Entity is AuditTrail)
                continue;
                
            // Skip entities marked with [DoNotAudit]
            if (entry.Entity.GetType().HasDoNotAuditAttribute())
                continue;
            
            if (entry.State == EntityState.Added ||
                entry.State == EntityState.Modified ||
                entry.State == EntityState.Deleted)
            {
                var audit = await AuditTrail.CreateAsync(entry, userId, cancellationToken);
                
                // Validate before adding
                var validation = AuditTrailValidator.Validate(audit);
                if (validation.IsValid)
                {
                    auditEntries.Add(audit);
                }
                else
                {
                    // Log validation errors
                    _logger.LogWarning("Audit validation failed: {Errors}", 
                        string.Join(", ", validation.Errors));
                }
            }
        }
        
        if (auditEntries.Count > 0)
        {
            AuditTrails.AddRange(auditEntries);
        }
        
        return await base.SaveChangesAsync(cancellationToken);
    }
    
    private long GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.NameIdentifier);
            
        return userIdClaim != null && long.TryParse(userIdClaim.Value, out var userId)
            ? userId
            : 0; // System user
    }
}
```

### Querying with Extensions

```csharp
// Get complete audit history for an entity
var productHistory = await context.AuditTrails
    .ForEntity<Product>(productId)
    .OrderByOldest()
    .ToListAsync();

// Build a timeline
foreach (var audit in productHistory)
{
    var changes = audit.GetPropertyChanges()
        .Where(c => c.HasChanged)
        .ToList();
        
    Console.WriteLine($"{audit.DateTimeUTC}: {audit.ChangeType}");
    foreach (var change in changes)
    {
        Console.WriteLine($"  {change.PropertyName}: {change.OldValue} ? {change.NewValue}");
    }
}
```

### Advanced Filtering

```csharp
// Get all price changes for products in the last month
var priceChanges = await context.AuditTrails
    .ForTable("Product")
    .OfChangeType(EntityChangeType.Updated)
    .Recent(30)
    .ToListAsync();

var significantChanges = priceChanges
    .Where(a => a.HasPropertyChanged("Price"))
    .Select(a => new
    {
        ProductId = a.GetKeyValuesAsDictionary()?["Id"],
        OldPrice = a.GetOldValue<decimal>("Price"),
        NewPrice = a.GetNewValue<decimal>("Price"),
        ChangedAt = a.DateTimeUTC,
        ChangedBy = a.UserId
    })
    .Where(c => Math.Abs(c.NewPrice - c.OldPrice) > 10) // >$10 change
    .ToList();
```

### Retention Policy with Background Service

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Configure retention policy
AuditTrail.DefaultRetentionDays = 365; // 1 year retention

// Add background archival service (runs daily)
builder.Services.AddAuditTrailArchivalService(TimeSpan.FromDays(1));

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();
app.Run();
```

### Comparing Audit Versions

```csharp
// Track price history and calculate average increase
var priceAudits = await context.AuditTrails
    .ForEntity<Product>(productId)
    .Where(a => a.HasPropertyChanged("Price"))
    .OrderByOldest()
    .ToListAsync();

for (int i = 1; i < priceAudits.Count; i++)
{
    var comparison = priceAudits[i].CompareTo(priceAudits[i - 1]);
    var priceDiff = comparison.Differences
        .FirstOrDefault(d => d.PropertyName == "Price");
        
    if (priceDiff != null && priceDiff.HasChanged)
    {
        var oldPrice = decimal.Parse(priceDiff.OldValue?.ToString() ?? "0");
        var newPrice = decimal.Parse(priceDiff.NewValue?.ToString() ?? "0");
        var change = newPrice - oldPrice;
        var percentChange = (change / oldPrice) * 100;
        
        Console.WriteLine($"Price changed by {percentChange:F2}% on {priceAudits[i].DateTimeUTC}");
    }
}
```

## Best Practices

1. **Use Factory Methods**: Prefer `Create` or `CreateAsync` over direct constructor for better testability
2. **Configure Retention Early**: Set `DefaultRetentionDays` at application startup
3. **Exclude Sensitive Data**: Always use `[DoNotAudit]` on passwords, tokens, and secrets
4. **Archive Regularly**: Use the background service or implement custom archival strategy
5. **Index Wisely**: The `ConfigureAuditTrail()` extension adds recommended indexes automatically
6. **Monitor Size**: Audit tables can grow large; implement archival and cleanup strategies
7. **Validate Before Saving**: Use `AuditTrailValidator.Validate()` in SaveChanges override
8. **Use Query Extensions**: Leverage fluent API for cleaner, more maintainable queries
9. **Deserialize Safely**: Use typed deserialization methods to avoid JSON parsing errors
10. **Test Auditing Logic**: Write integration tests to ensure critical entities are properly audited
11. **Avoid Recursion**: Exclude `AuditTrail` entities from auditing in SaveChanges
12. **Handle Bulk Operations**: Consider disabling auditing for bulk inserts/updates for performance

## Database Migration

Generate a migration for the AuditTrail table:

```bash
dotnet ef migrations add AddAuditTrail
dotnet ef database update
```

The `ConfigureAuditTrail()` extension method automatically configures:
- Primary key
- Required fields
- Maximum lengths
- Indexes on commonly queried fields
- Composite indexes for performance

**Generated Indexes:**
- `IX_AuditTrail_TableName`
- `IX_AuditTrail_DateTimeUTC`
- `IX_AuditTrail_UserId`
- `IX_AuditTrail_ChangeType`
- `IX_AuditTrail_IsArchived`
- `IX_AuditTrail_ArchiveAfter` (filtered)
- `IX_AuditTrail_TableName_DateTimeUTC` (composite)
- `IX_AuditTrail_UserId_DateTimeUTC` (composite)

## Thread Safety

- Static properties (`DefaultRetentionDays`, `IncludeNavigationProperties`, `SerializerOptions`) are thread-safe for reads
- Changing static configuration during runtime should be done at application startup
- Individual `AuditTrail` instances are not thread-safe and should not be shared across threads
- The background archival service uses scoped services and is thread-safe

## Performance Considerations

- **Navigation Properties**: Disable if not needed (`IncludeNavigationProperties = false`)
- **Batch Operations**: Consider disabling auditing for bulk operations
- **Serialization**: Custom serializer options can impact performance
- **Database Size**: Implement retention and archival to manage audit table growth
- **Indexing**: The ModelBuilder extension configures optimal indexes automatically
- **Query Optimization**: Use query extensions for efficient filtering
- **Validation**: Validate audit entries before saving to catch issues early

## Dependencies

- **Craft.Domain**: Core domain abstractions
- **Craft.Extensions**: Extension methods  
- **Microsoft.EntityFrameworkCore**: EF Core framework
- **Microsoft.Extensions.Hosting.Abstractions**: For background service
- **.NET 10**: Target framework

## API Reference

### Query Extensions
- `ForEntity<TEntity>(long entityId)` - Filter by entity
- `ForTable(string tableName)` - Filter by table
- `InDateRange(DateTime from, DateTime to)` - Date range filter
- `ByUser(KeyType userId)` - Filter by user
- `PendingArchival()` - Get entries pending archival
- `OfChangeType(EntityChangeType)` - Filter by change type
- `Archived()` / `NotArchived()` - Filter by archival status
- `Recent(int days = 7)` - Get recent entries
- `OrderByMostRecent()` / `OrderByOldest()` - Ordering

### Deserialization Helpers
- `GetOldValuesAsDictionary()` / `GetNewValuesAsDictionary()` - Deserialize to dictionaries
- `GetKeyValuesAsDictionary()` - Get key values
- `GetChangedColumnsAsList()` - Get changed columns
- `DeserializeOldValues<T>()` / `DeserializeNewValues<T>()` - Type-safe deserialization
- `GetOldValue(string)` / `GetNewValue(string)` - Get property values
- `GetOldValue<TValue>(string)` / `GetNewValue<TValue>(string)` - Typed property getters
- `HasPropertyChanged(string)` - Check if property changed

### Comparison Methods
- `CompareTo(AuditTrail?)` - Compare two audit entries
- `PropertyWasModified(string)` - Check if property was modified
- `GetPropertyChanges()` - Get all property changes
- `GetPropertyValue(string, bool)` - Get property value
- `GetChangedProperties()` - Get changed properties dictionary

### Validation
- `AuditTrailValidator.Validate(AuditTrail)` - Validate audit entry
- `AuditTrailValidator.WillExceedMaxLength(string, int, out int)` - Check length
- `AuditTrailValidator.TruncateIfNeeded(string, int, string)` - Safe truncation

### Configuration
- `modelBuilder.ConfigureAuditTrail()` - Configure with ModelBuilder
- `services.AddAuditTrailArchivalService(TimeSpan?)` - Add background service

## License

This project is part of the Craft framework. See the main repository for license information.

## Contributing

Contributions are welcome! Please ensure:
- All tests pass (unit and integration)
- Code coverage remains above 90%
- Follow existing code style and patterns
- Add tests for new features
- Update README for new functionality

## Support

For issues, questions, or contributions, please refer to the main Craft repository at https://github.com/sandeepksharma15/Craft_V_10
