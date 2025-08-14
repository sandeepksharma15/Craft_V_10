using System.Linq.Expressions;
using Craft.Domain;
using Craft.MultiTenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Craft.Data;

public static class QueryFilterExtension
{
    public const string SoftDeleteFilterName = nameof(ISoftDelete);
    public const string TenantFilterName = nameof(IHasTenant);

    /// <summary>
    /// Adds a global query filter to all entities in the model that implement the specified interface type.
    /// </summary>
    public static ModelBuilder AddGlobalQueryFilter<T>(this ModelBuilder modelBuilder, string filterName, 
        Expression<Func<T, bool>> filter)
    {
        if (typeof(T).IsInterface == false)
            throw new ArgumentException("Type argument must be an interface.", nameof(T));

        ArgumentNullException.ThrowIfNull(modelBuilder);
        ArgumentException.ThrowIfNullOrWhiteSpace(filterName, nameof(filterName));
        ArgumentNullException.ThrowIfNull(filter);

        // Get entities that implement interface T
        var entities = modelBuilder
            .Model
            .GetEntityTypes()
            .Where(e => e.ClrType.IsAssignableTo(typeof(T)))
            .Select(e => e.ClrType);

        foreach (var entity in entities)
        {
            // Get existing named query filters
            var declaredFilters = modelBuilder
                .Entity(entity)
                .Metadata
                .GetDeclaredQueryFilters();

            // Skip if a filter with the specified name already exists
            if (declaredFilters.Any(f => f.Key == filterName))
                continue;

            var parameter = Expression.Parameter(entity, "e");
            var filterBody = ReplacingExpressionVisitor.Replace(filter.Parameters.Single(), parameter, filter.Body);

            // Apply the query filter
            modelBuilder
                .Entity(entity)
                .HasQueryFilter(filterName, Expression.Lambda(filterBody, parameter));
        }

        return modelBuilder;
    }

    /// <summary>
    /// Excludes entities where IsDeleted is true.
    /// </summary>
    public static ModelBuilder AddGlobalSoftDeleteFilter(this ModelBuilder modelBuilder)
    {
        return modelBuilder.AddGlobalQueryFilter<ISoftDelete>(SoftDeleteFilterName, e => !e.IsDeleted);
    }

    /// <summary>
    /// Includes only entities matching the current tenant's ID.
    /// </summary>
    public static ModelBuilder AddGlobalTenantFilter(this ModelBuilder modelBuilder, ICurrentTenant currentTenant)
    {
        return modelBuilder.AddGlobalQueryFilter<IHasTenant>(TenantFilterName, e => e.TenantId == currentTenant.Id);
    }

    /// <summary>
    /// Includes soft-deleted entities.
    /// </summary>
    public static IQueryable<T>? IncludeSoftDeleted<T>(this IQueryable<T> query) where T : class, ISoftDelete, new()
    {
        if (query is null) return null;

        return query.IgnoreQueryFilters([SoftDeleteFilterName]);
    }

    /// <summary>
    /// Includes all tenants.
    /// </summary>
    public static IQueryable<T>? IncludeAllTenants<T>(this IQueryable<T> query) where T : class, IHasTenant, new()
    {
        if (query is null) return null;

        return query.IgnoreQueryFilters([TenantFilterName]);
    }

    /// <summary>
    /// Includes all tenants and soft-deleted entities.
    /// </summary>
    public static IQueryable<T>? IncludeAllTenantsAndSoftDelete<T>(this IQueryable<T> query) 
        where T : class, IHasTenant, ISoftDelete, new()
    {
        if (query is null) return null;

        return query.IgnoreQueryFilters([TenantFilterName, SoftDeleteFilterName]);
    }
}
