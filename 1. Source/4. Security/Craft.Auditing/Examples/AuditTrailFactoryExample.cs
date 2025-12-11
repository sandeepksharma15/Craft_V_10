using Craft.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Craft.Auditing.Examples;

/// <summary>
/// Examples demonstrating the factory pattern for creating AuditTrail instances.
/// </summary>
public static class AuditTrailFactoryExample
{
    /// <summary>
    /// Example 1: Using the synchronous Create factory method.
    /// This is the recommended approach for simple scenarios.
    /// </summary>
    public static void CreateAuditTrailSynchronously(EntityEntry entityEntry, KeyType userId)
    {
        // ? Recommended: Use factory method
        var auditTrail = AuditTrail.Create(entityEntry, userId);

        // Add to context and save
        // context.Set<AuditTrail>().Add(auditTrail);
        // context.SaveChanges();
    }

    /// <summary>
    /// Example 2: Using the asynchronous CreateAsync factory method.
    /// This allows for future async operations and better scalability.
    /// </summary>
    public static async Task CreateAuditTrailAsynchronously(EntityEntry entityEntry, KeyType userId)
    {
        // ? Recommended: Use async factory method
        var auditTrail = await AuditTrail.CreateAsync(entityEntry, userId);

        // Add to context and save
        // await context.Set<AuditTrail>().AddAsync(auditTrail);
        // await context.SaveChangesAsync();
    }

    /// <summary>
    /// Example 3: Using CreateAsync with cancellation token.
    /// Useful for long-running operations that need to be cancellable.
    /// </summary>
    public static async Task CreateAuditTrailWithCancellation(
        EntityEntry entityEntry, 
        KeyType userId, 
        CancellationToken cancellationToken)
    {
        // ? Pass cancellation token for cancellable operations
        var auditTrail = await AuditTrail.CreateAsync(entityEntry, userId, cancellationToken);

        // Add to context and save
        // await context.Set<AuditTrail>().AddAsync(auditTrail, cancellationToken);
        // await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Example 4: Constructor usage (backward compatibility).
    /// The constructor is still available but factory methods are recommended.
    /// </summary>
    public static void UsingConstructor(EntityEntry entityEntry, KeyType userId)
    {
        // ?? Still works but factory methods are recommended
        var auditTrail = new AuditTrail(entityEntry, userId);

        // Add to context and save
        // context.Set<AuditTrail>().Add(auditTrail);
        // context.SaveChanges();
    }

    /// <summary>
    /// Example 5: Creating multiple audit trails efficiently.
    /// </summary>
    public static async Task<List<AuditTrail>> CreateMultipleAuditTrails(
        IEnumerable<EntityEntry> entityEntries, 
        KeyType userId)
    {
        var auditTrails = new List<AuditTrail>();
        
        foreach (var entityEntry in entityEntries)
        {
            var auditTrail = await AuditTrail.CreateAsync(entityEntry, userId);
            auditTrails.Add(auditTrail);
        }

        return auditTrails;
    }

    /// <summary>
    /// Example 6: Error handling with factory methods.
    /// </summary>
    public static async Task<AuditTrail?> CreateWithErrorHandling(EntityEntry? entityEntry, KeyType userId)
    {
        try
        {
            if (entityEntry == null)
                return null;

            var auditTrail = await AuditTrail.CreateAsync(entityEntry, userId);
            return auditTrail;
        }
        catch (ArgumentNullException ex)
        {
            // Handle null entity entry
            Console.WriteLine($"Invalid entity entry: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            // Handle other errors
            Console.WriteLine($"Error creating audit trail: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Example 7: Usage in a custom audit service.
    /// </summary>
    public class CustomAuditService
    {
        public async Task<AuditTrail> CreateAuditEntryAsync(
            EntityEntry entityEntry,
            KeyType userId,
            CancellationToken cancellationToken = default)
        {
            // Use the async factory method
            var auditTrail = await AuditTrail.CreateAsync(entityEntry, userId, cancellationToken);

            // Future: Could add custom processing here
            // - Encrypt sensitive data
            // - Add additional metadata
            // - Call external audit services
            // - Validate against compliance rules

            return auditTrail;
        }

        public AuditTrail CreateAuditEntry(EntityEntry entityEntry, KeyType userId)
        {
            // Use the sync factory method for simple scenarios
            var auditTrail = AuditTrail.Create(entityEntry, userId);

            // Add any custom processing
            
            return auditTrail;
        }
    }

    /// <summary>
    /// Comparison: Factory Method vs Constructor
    /// </summary>
    public static class FactoryVsConstructor
    {
        // ? GOOD: Factory method - clearer intent, allows async
        public static AuditTrail UsingFactoryMethod(EntityEntry entry, KeyType userId)
        {
            return AuditTrail.Create(entry, userId);
        }

        // ? BETTER: Async factory method - future-proof, scalable
        public static async Task<AuditTrail> UsingAsyncFactoryMethod(EntityEntry entry, KeyType userId)
        {
            return await AuditTrail.CreateAsync(entry, userId);
        }

        // ?? OK: Constructor - works but less flexible
        public static AuditTrail UsingConstructor(EntityEntry entry, KeyType userId)
        {
            return new AuditTrail(entry, userId);
        }
    }

    /// <summary>
    /// Benefits of Factory Pattern
    /// </summary>
    public static class Benefits
    {
        /*
         * 1. **Flexibility**: Factory methods can be async, constructors cannot
         * 2. **Extensibility**: Easy to add new creation logic without breaking existing code
         * 3. **Testability**: Easier to mock and test factory methods
         * 4. **Clarity**: Method names like Create/CreateAsync express intent clearly
         * 5. **Future-Proof**: Allows for async operations to be added later without breaking changes
         * 6. **Validation**: Can perform complex validation before object creation
         * 7. **Caching**: Could implement caching or pooling if needed
         * 8. **Dependency Injection**: Easier to integrate with DI containers
         * 
         * Future Extensibility Examples:
         * - Load navigation properties asynchronously
         * - Call external services for additional audit data
         * - Validate changes against external compliance systems
         * - Encrypt sensitive data asynchronously
         * - Compress large audit payloads
         * - Send audit events to message queues
         */
    }

    /// <summary>
    /// Migration Guide: Constructor to Factory Method
    /// </summary>
    public static class MigrationGuide
    {
        // BEFORE: Using constructor
        public static void OldWay(EntityEntry entry, KeyType userId)
        {
            var audit = new AuditTrail(entry, userId);
        }

        // AFTER: Using factory method
        public static void NewWay(EntityEntry entry, KeyType userId)
        {
            var audit = AuditTrail.Create(entry, userId);
        }

        // AFTER: Using async factory method (recommended)
        public static async Task<AuditTrail> NewAsyncWay(EntityEntry entry, KeyType userId)
        {
            var audit = await AuditTrail.CreateAsync(entry, userId);
            return audit;
        }

        /*
         * Migration Steps:
         * 1. Replace `new AuditTrail(entry, userId)` with `AuditTrail.Create(entry, userId)`
         * 2. For async code, use `await AuditTrail.CreateAsync(entry, userId)`
         * 3. Constructor still works for backward compatibility
         * 4. No breaking changes - both approaches work
         */
    }
}
