using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Craft.Data;

/// <summary>
/// Marker interface for entity types that represent database materialized views.
/// Materialized views are pre-computed, cached query results stored as database objects.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Purpose:</strong>
/// </para>
/// <para>
/// Materialized views improve query performance for complex aggregations and joins
/// by storing pre-computed results. They trade freshness for speed.
/// </para>
/// <para>
/// <strong>Usage Pattern:</strong>
/// </para>
/// <code>
/// [Table("vw_SalesSummary")] // Maps to materialized view
/// [PrimaryKey(nameof(ProductId), nameof(Year), nameof(Month))]
/// public class SalesSummaryView : IMaterializedView
/// {
///     public int ProductId { get; set; }
///     public int Year { get; set; }
///     public int Month { get; set; }
///     public decimal TotalSales { get; set; }
///     public int OrderCount { get; set; }
///     public DateTime LastRefreshed { get; set; }
/// }
/// </code>
/// <para>
/// <strong>Configuration in DbContext:</strong>
/// </para>
/// <code>
/// protected override void OnModelCreating(ModelBuilder modelBuilder)
/// {
///     modelBuilder.Entity&lt;SalesSummaryView&gt;()
///         .ToView("vw_SalesSummary")  // SQL Server
///         .HasNoKey();                 // Views typically don't have keys
///         
///     // Or for PostgreSQL materialized views:
///     modelBuilder.Entity&lt;SalesSummaryView&gt;()
///         .ToTable("vw_SalesSummary", t => t.ExcludeFromMigrations());
/// }
/// </code>
/// <para>
/// <strong>Refresh Strategies:</strong>
/// </para>
/// <list type="bullet">
/// <item>
/// <description>
/// <strong>Manual:</strong> Use <see cref="MaterializedViewExtensions.RefreshMaterializedViewAsync"/> to refresh on-demand.
/// </description>
/// </item>
/// <item>
/// <description>
/// <strong>Scheduled:</strong> Use background jobs (Hangfire, Quartz) to refresh periodically.
/// </description>
/// </item>
/// <item>
/// <description>
/// <strong>Trigger-based:</strong> Database triggers refresh views automatically on data changes.
/// </description>
/// </item>
/// </list>
/// <para>
/// <strong>Database-Specific Notes:</strong>
/// </para>
/// <list type="bullet">
/// <item>
/// <description>
/// <strong>PostgreSQL:</strong> Native materialized views with REFRESH MATERIALIZED VIEW.
/// </description>
/// </item>
/// <item>
/// <description>
/// <strong>SQL Server:</strong> Indexed views (requires SCHEMABINDING and specific requirements).
/// </description>
/// </item>
/// <item>
/// <description>
/// <strong>MySQL:</strong> No native support; simulate with tables + triggers.
/// </description>
/// </item>
/// </list>
/// </remarks>
public interface IMaterializedView
{
}

/// <summary>
/// Extension methods for working with materialized views.
/// </summary>
public static class MaterializedViewExtensions
{
    /// <summary>
    /// Refreshes a materialized view by executing database-specific refresh commands.
    /// </summary>
    /// <typeparam name="TView">The materialized view entity type.</typeparam>
    /// <param name="context">The DbContext containing the view.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the async operation.</returns>
    /// <exception cref="NotSupportedException">Thrown when the database provider doesn't support materialized views.</exception>
    /// <remarks>
    /// This method executes raw SQL commands specific to each database provider:
    /// <list type="bullet">
    /// <item>PostgreSQL: REFRESH MATERIALIZED VIEW</item>
    /// <item>SQL Server: Indexed views refresh automatically</item>
    /// <item>MySQL: Not supported (throws exception)</item>
    /// </list>
    /// </remarks>
    public static async Task RefreshMaterializedViewAsync<TView>(
        this DbContext context,
        CancellationToken cancellationToken = default)
        where TView : class, IMaterializedView
    {
        ArgumentNullException.ThrowIfNull(context);

        var entityType = context.Model.FindEntityType(typeof(TView)) 
            ?? throw new InvalidOperationException($"Entity type {typeof(TView).Name} is not part of the model.");
        var storeObjectId = StoreObjectIdentifier.Create(entityType, StoreObjectType.Table);
        var viewName = storeObjectId?.Name 
            ?? throw new InvalidOperationException($"Table/View name not configured for {typeof(TView).Name}");
        
        var schema = storeObjectId?.Schema;
        var fullName = string.IsNullOrEmpty(schema) ? viewName : $"{schema}.{viewName}";

        // Detect database provider
        var providerName = context.Database.ProviderName?.ToLowerInvariant();

        var sql = providerName switch
        {
            "microsoft.entityframeworkcore.sqlserver" =>
                // SQL Server indexed views refresh automatically
                // We can force a recompute by updating statistics
                $"UPDATE STATISTICS {fullName}",

            "npgsql.entityframeworkcore.postgresql" =>
                // PostgreSQL materialized view refresh
                $"REFRESH MATERIALIZED VIEW {fullName}",

            "pomelo.entityframeworkcore.mysql" or "mysql.data.entityframeworkcore" =>
                throw new NotSupportedException(
                    "MySQL does not natively support materialized views. " +
                    "Consider using a table with triggers or manual refresh logic."),

            _ => throw new NotSupportedException(
                $"Materialized view refresh is not supported for provider '{context.Database.ProviderName}'")
        };

        await context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    /// <summary>
    /// Refreshes a materialized view concurrently (PostgreSQL only).
    /// Allows queries to continue while the view is being refreshed.
    /// </summary>
    /// <typeparam name="TView">The materialized view entity type.</typeparam>
    /// <param name="context">The DbContext containing the view.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the async operation.</returns>
    public static async Task RefreshMaterializedViewConcurrentlyAsync<TView>(
        this DbContext context,
        CancellationToken cancellationToken = default)
        where TView : class, IMaterializedView
    {
        ArgumentNullException.ThrowIfNull(context);

        var entityType = context.Model.FindEntityType(typeof(TView)) 
            ?? throw new InvalidOperationException($"Entity type {typeof(TView).Name} is not part of the model.");
        var storeObjectId = StoreObjectIdentifier.Create(entityType, StoreObjectType.Table);
        var viewName = storeObjectId?.Name 
            ?? throw new InvalidOperationException($"Table/View name not configured for {typeof(TView).Name}");
        
        var schema = storeObjectId?.Schema;
        var fullName = string.IsNullOrEmpty(schema) ? viewName : $"{schema}.{viewName}";

        // Only PostgreSQL supports concurrent refresh
        var providerName = context.Database.ProviderName?.ToLowerInvariant();
        
        if (providerName != "npgsql.entityframeworkcore.postgresql")
        {
            throw new NotSupportedException(
                "Concurrent materialized view refresh is only supported on PostgreSQL. " +
                $"Current provider: {context.Database.ProviderName}");
        }

        await context.Database.ExecuteSqlAsync(
            $"REFRESH MATERIALIZED VIEW CONCURRENTLY {fullName}",
            cancellationToken);
    }
}
