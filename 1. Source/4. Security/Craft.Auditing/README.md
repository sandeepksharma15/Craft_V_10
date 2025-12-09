# Craft.Auditing

Craft.Auditing is a .NET 10 library that provides attribute-driven, extensible auditing for entity changes in your applications. It is designed to work seamlessly with Entity Framework Core and supports tracking of create, update, and delete operations, including soft deletes.

## Features
- **Attribute-based Auditing**: Use `[Audit]` and `[DoNotAudit]` attributes to control which entities are audited.
- **Comprehensive Change Tracking**: Captures changed columns, old and new values, key values, and change type (Created, Updated, Deleted).
- **Entity Framework Core Integration**: Designed to work with EF Core's `EntityEntry` and supports soft delete patterns.
- **Extensible Model**: Interfaces like `IAuditTrail` and `IAuditTrailDbContext` allow for easy customization and integration.
- **Helpers for Discovery**: Static helpers to discover auditable and non-auditable types in your domain model.

## Getting Started

### Installation
Add a project reference to `Craft.Auditing` in your .NET 10 solution:

```
dotnet add reference ../Craft.Auditing/Craft.Auditing.csproj
```

### Usage
1. **Mark Entities for Auditing**
   - Decorate your entity classes with `[Audit]` to enable auditing, or `[DoNotAudit]` to exclude them.

   ```csharp
   [Audit]
   public class MyEntity : BaseEntity { /* ... */ }
   ```

2. **Integrate with DbContext**
   - Implement `IAuditTrailDbContext` in your `DbContext` to expose the `AuditTrails` DbSet.
   - Use the `AuditTrail` model to record changes in your save logic (e.g., in `SaveChanges`).

3. **Access Audit Data**
   - Query the `AuditTrails` table for a history of changes, including who made them and when.

### Example: Discovering Auditable Types

```csharp
var auditableTypes = AuditingHelpers.GetAuditableBaseEntityTypes();
```

## Dependencies
- .NET 10
- Entity Framework Core (for change tracking and persistence)
- Depends on `Craft.Domain` and `Craft.Extensions` projects

## Contributing
See the root `CONTRIBUTING.md` for guidelines.

## License
See the `LICENSE` file in this directory for details.

---
For more details, review the source code and XML documentation in the project.
