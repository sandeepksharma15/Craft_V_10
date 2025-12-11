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

### 2. Configure Entity

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<AuditTrail>();
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

## Archival Management

### Archiving Entries

```csharp
auditTrail.Archive();        // Mark as archived
auditTrail.Unarchive();      // Restore from archive
bool shouldArchive = auditTrail.ShouldBeArchived(); // Check if eligible
```

### Automatic Archival Query

```csharp
var entriesToArchive = await context.AuditTrails
    .Where(a => !a.IsArchived && 
                a.ArchiveAfter.HasValue && 
                a.ArchiveAfter.Value <= DateTime.UtcNow)
    .ToListAsync();

foreach (var entry in entriesToArchive)
{
    entry.Archive();
}

await context.SaveChangesAsync();
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
document.Delete();
await context.SaveChangesAsync(); // Audit shows as Deleted
```

### Querying Audit History

```csharp
// Get all changes for a specific entity
var productAudits = await context.AuditTrails
    .Where(a => a.TableName == "Product" && a.KeyValues.Contains("\"Id\":123"))
    .OrderByDescending(a => a.DateTimeUTC)
    .ToListAsync();

// Get recent updates
var recentUpdates = await context.AuditTrails
    .Where(a => a.ChangeType == EntityChangeType.Updated && 
                a.DateTimeUTC >= DateTime.UtcNow.AddDays(-7))
    .ToListAsync();

// Get changes by specific user
var userChanges = await context.AuditTrails
    .Where(a => a.UserId == userId)
    .OrderByDescending(a => a.DateTimeUTC)
    .Take(100)
    .ToListAsync();
```

### Retention Policy Implementation

```csharp
// Configure global retention
AuditTrail.DefaultRetentionDays = 365;

// Archive old entries (background job)
public async Task ArchiveOldAuditEntries()
{
    var entriesToArchive = await context.AuditTrails
        .Where(a => a.ShouldBeArchived())
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
        .Where(a => a.IsArchived && a.DateTimeUTC < cutoffDate)
        .ToListAsync();
    
    context.AuditTrails.RemoveRange(oldEntries);
    await context.SaveChangesAsync();
}
```

## Best Practices

1. **Use Factory Methods**: Prefer `Create` or `CreateAsync` over direct constructor for better testability
2. **Configure Retention Early**: Set `DefaultRetentionDays` at application startup
3. **Exclude Sensitive Data**: Always use `[DoNotAudit]` on passwords, tokens, and secrets
4. **Archive Regularly**: Implement a background job to archive old audit entries
5. **Index Wisely**: Add database indexes on `TableName`, `DateTimeUTC`, `UserId`, and `IsArchived`
6. **Monitor Size**: Audit tables can grow large; implement archival and cleanup strategies
7. **Deserialize with Care**: NewValues/OldValues contain JSON; deserialize to appropriate types
8. **Test Auditing Logic**: Write tests to ensure critical entities are properly audited

## Database Migration

Generate a migration for the AuditTrail table:

```bash
dotnet ef migrations add AddAuditTrail
dotnet ef database update
```

**Recommended Indexes:**
```sql
CREATE INDEX IX_AuditTrails_TableName ON HK_AuditTrail(TableName);
CREATE INDEX IX_AuditTrails_DateTimeUTC ON HK_AuditTrail(DateTimeUTC);
CREATE INDEX IX_AuditTrails_UserId ON HK_AuditTrail(UserId);
CREATE INDEX IX_AuditTrails_IsArchived ON HK_AuditTrail(IsArchived);
CREATE INDEX IX_AuditTrails_ArchiveAfter ON HK_AuditTrail(ArchiveAfter) WHERE ArchiveAfter IS NOT NULL;
```

## Thread Safety

- Static properties (`DefaultRetentionDays`, `IncludeNavigationProperties`, `SerializerOptions`) are thread-safe for reads
- Changing static configuration during runtime should be done at application startup
- Individual `AuditTrail` instances are not thread-safe and should not be shared across threads

## Performance Considerations

- **Navigation Properties**: Disable if not needed (`IncludeNavigationProperties = false`)
- **Batch Operations**: Consider auditing only critical entities for bulk operations
- **Serialization**: Custom serializer options can impact performance
- **Database Size**: Implement retention and archival to manage audit table growth
- **Indexing**: Proper indexes significantly improve query performance

## Dependencies

- **Craft.Domain**: Core domain abstractions
- **Craft.Extensions**: Extension methods
- **Microsoft.EntityFrameworkCore**: EF Core framework
- **.NET 10**: Target framework

## License

This project is part of the Craft framework. See the main repository for license information.

## Contributing

Contributions are welcome! Please ensure all tests pass and maintain code coverage above 90%.

## Support

For issues, questions, or contributions, please refer to the main Craft repository.
