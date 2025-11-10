# DbContext Feature System

## Overview
The Craft.Data library provides a flexible, feature-based DbContext system that allows consumers to declaratively configure their database contexts with common behaviors like audit trails, soft deletes, multi-tenancy, and ASP.NET Core Identity integration.

## Architecture

### Base Classes
- **`BaseDbContext<TContext>`**: Foundation for non-Identity DbContexts with feature support
- **`BaseIdentityDbContext<TContext>`**: Foundation for Identity-enabled DbContexts with feature support (uses default CraftUser, CraftRole, KeyType)
- **`BaseIdentityDbContext<TContext, TUser, TRole, TKey>`**: Generic Identity DbContext for custom user/role types

### Features
Features are pluggable components that extend DbContext behavior:

1. **AuditTrailFeature**: Automatically tracks entity changes for entities marked with `[Audit]` attribute
2. **SoftDeleteFeature**: Converts hard deletes to soft deletes for entities implementing `ISoftDelete`
3. **MultiTenancyFeature**: Filters queries and sets tenant ID for entities implementing `IHasTenant`
4. **ConcurrencyFeature**: Manages concurrency stamps for optimistic concurrency control
5. **IdentityFeature**: Configures ASP.NET Core Identity with custom table naming

## Usage Examples

### Example 1: Simple DbContext with Audit Trail

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
        // Enable audit trail feature
        Features.AddAuditTrail();
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
}
```

### Example 2: DbContext with Multiple Features

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
        // Enable multiple features
        Features
            .AddAuditTrail()
            .AddSoftDelete()
            .AddConcurrency()
            .AddMultiTenancy(currentTenant);
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
}
```

### Example 3: DbContext with All Common Features

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
        // Enable all common features at once
        Features.AddCommonFeatures();
    }

    public DbSet<Customer> Customers { get; set; }
}
```

### Example 4: Identity DbContext with Default Types

```csharp
public class AppIdentityDbContext : BaseIdentityDbContext<AppIdentityDbContext>
{
    public AppIdentityDbContext(
        DbContextOptions<AppIdentityDbContext> options,
        ITenant currentTenant,
        ICurrentUser currentUser,
        ILoggerFactory loggerFactory)
        : base(options, currentTenant, currentUser, loggerFactory)
    {
        // Configure Identity with custom table prefix
        Features
            .AddIdentity(tablePrefix: "Id_")
            .AddAuditTrail()
            .AddSoftDelete();
    }

    // LoginHistories and RefreshTokens DbSets are automatically included
    
    public DbSet<Customer> Customers { get; set; }
}
```

### Example 5: Identity DbContext with Custom User/Role Types

```csharp
public class CustomUser : CraftUser<Guid>
{
    public string Department { get; set; }
}

public class CustomRole : CraftRole<Guid>
{
    public int Priority { get; set; }
}

public class AppIdentityDbContext : BaseIdentityDbContext<AppIdentityDbContext, CustomUser, CustomRole, Guid>
{
    public AppIdentityDbContext(
        DbContextOptions options,
        ITenant currentTenant,
        ICurrentUser currentUser,
        ILoggerFactory loggerFactory)
        : base(options, currentTenant, currentUser, loggerFactory)
    {
        // Configure Identity with custom types
        Features
            .AddIdentity<CustomUser, CustomRole, Guid>(tablePrefix: "Auth_")
            .AddCommonFeatures();
    }

    public DbSet<Customer> Customers { get; set; }
}
```

### Example 6: Custom Conventions and Model Configuration

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

    protected override void ConfigureAdditionalConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Add custom conventions
        configurationBuilder
            .Properties<decimal>()
            .HavePrecision(18, 2);
    }

    protected override void ConfigureAdditionalModel(ModelBuilder modelBuilder)
    {
        // Add custom model configuration
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
        });
    }

    protected override void ConfigureAdditionalOptions(DbContextOptionsBuilder optionsBuilder)
    {
        // Add custom options
        optionsBuilder.EnableDetailedErrors();
    }

    public DbSet<Customer> Customers { get; set; }
}
```

### Example 7: Creating a Custom Feature

```csharp
public class UserTrackingFeature : IDbContextFeature
{
    public void OnBeforeSaveChanges(DbContext context, KeyType userId)
    {
        var entries = context.ChangeTracker.Entries<IHasUser>()
            .Where(e => e.State == EntityState.Added)
            .ToList();

        foreach (var entry in entries)
        {
            entry.Entity.SetUserId(userId);
        }
    }

    public void ConfigureModel(ModelBuilder modelBuilder)
    {
        // Optional: Add indexes on UserId
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IHasUser).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex(nameof(IHasUser.UserId));
            }
        }
    }
}

// Usage in DbContext
public class AppDbContext : BaseDbContext<AppDbContext>
{
    public AppDbContext(/* ... */) : base(/* ... */)
    {
        Features.AddFeature(new UserTrackingFeature());
    }
}
```

## Feature Behavior

### AuditTrailFeature
- Automatically adds `DbSet<AuditTrail>` to the context
- Tracks changes for entities marked with `[Audit]` attribute
- Captures: table name, changed columns, old/new values, user ID, timestamp
- Supports soft delete detection

### SoftDeleteFeature
- Applies global query filter to exclude `IsDeleted = true` entities
- Converts `DbContext.Remove()` calls to set `IsDeleted = true`
- Works with entities implementing `ISoftDelete`

### MultiTenancyFeature
- Applies global query filter for `TenantId == currentTenant.Id`
- Automatically sets `TenantId` on entity creation
- Works with entities implementing `IHasTenant`

### ConcurrencyFeature
- Generates new `ConcurrencyStamp` GUID on modifications
- Configures `ConcurrencyStamp` as concurrency token
- Works with entities implementing `IHasConcurrency`

### IdentityFeature
- Configures ASP.NET Core Identity tables with custom naming
- Default table prefix: `Id_` (Users, Roles, UserRoles, etc.)
- Includes `LoginHistories` and `RefreshTokens` DbSets
- Supports custom user/role types

## Dependency Injection Setup

```csharp
// In Program.cs or Startup.cs
services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    var connectionString = configuration.GetConnectionString("Default");
    options.UseSqlServer(connectionString);
});

// Or with factory pattern for multi-tenancy
services.AddScoped<IDbContextFactory<AppDbContext>, TenantDbContextFactory<AppDbContext>>();
```

## Best Practices

1. **Feature Order**: Features are applied in registration order. Order matters for some features.
2. **Performance**: Use `AddCommonFeatures()` for standard setups rather than individual registration.
3. **Custom Features**: Implement `IDbContextFeature` for custom behaviors.
4. **DbSet Registration**: Features implementing `IDbSetProvider` automatically register DbSets.
5. **Testing**: Features can be tested independently by mocking `DbContext` and `ICurrentUser`.

## Migration from Legacy Code

If you have existing code using manual audit trails or soft deletes:

1. Replace manual logic with feature registration
2. Remove SaveChanges/SaveChangesAsync overrides
3. Add `[Audit]` attributes to entities that need auditing
4. Implement `ISoftDelete` on entities that need soft delete

## Troubleshooting

### AuditTrail table not created
- Ensure `AddAuditTrail()` is called in constructor
- Run migrations: `dotnet ef migrations add AddAuditTrail`

### Global query filters not working
- Check that entities implement the correct interfaces (`ISoftDelete`, `IHasTenant`)
- Verify feature is registered before context is used

### Identity tables have wrong names
- Ensure `AddIdentity()` is called with correct `tablePrefix`
- Check that `BaseIdentityDbContext` is used instead of `BaseDbContext`

## See Also
- `IDbContextFeature` - Feature interface
- `DbContextFeatureCollection` - Feature collection
- `AuditingHelpers` - Helper methods for discovering auditable entities
