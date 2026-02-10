namespace Craft.Data;

/// <summary>
/// Marker interface for DbContext instances configured for read-only operations.
/// Read-only contexts are optimized for query performance with no change tracking.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Benefits of Read-Only Contexts:</strong>
/// </para>
/// <list type="bullet">
/// <item>
/// <description>
/// <strong>Performance:</strong> No change tracking overhead, faster query execution.
/// </description>
/// </item>
/// <item>
/// <description>
/// <strong>Memory:</strong> Reduced memory footprint by not tracking entities.
/// </description>
/// </item>
/// <item>
/// <description>
/// <strong>Scalability:</strong> Better suited for read-heavy scenarios and read replicas.
/// </description>
/// </item>
/// </list>
/// <para>
/// <strong>Usage Pattern:</strong>
/// </para>
/// <code>
/// public class ReadOnlyAppDbContext : BaseDbContext&lt;ReadOnlyAppDbContext&gt;, IReadOnlyDbContext
/// {
///     public ReadOnlyAppDbContext(
///         DbContextOptions&lt;ReadOnlyAppDbContext&gt; options,
///         ITenant tenant,
///         ICurrentUser currentUser)
///         : base(options, tenant, currentUser)
///     {
///         // Configure for read-only operations
///         ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
///         ChangeTracker.AutoDetectChangesEnabled = false;
///         ChangeTracker.LazyLoadingEnabled = false;
///     }
///     
///     // Override to prevent modifications
///     public override int SaveChanges(bool acceptAllChangesOnSuccess)
///     {
///         throw new InvalidOperationException("This is a read-only context. SaveChanges is not allowed.");
///     }
///     
///     public override Task&lt;int&gt; SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
///     {
///         throw new InvalidOperationException("This is a read-only context. SaveChangesAsync is not allowed.");
///     }
/// }
/// </code>
/// <para>
/// <strong>Connection String Configuration:</strong>
/// </para>
/// <para>
/// For production scenarios, configure read-only contexts to use read replicas:
/// </para>
/// <code>
/// "DatabaseOptions": {
///   "ConnectionString": "Server=primary;Database=app;",
///   "ReadOnlyConnectionString": "Server=replica;Database=app;ApplicationIntent=ReadOnly;"
/// }
/// </code>
/// </remarks>
public interface IReadOnlyDbContext
{
}
