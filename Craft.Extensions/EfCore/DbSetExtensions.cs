using System.Linq.Expressions;
using Craft.Extensions.Expressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.EntityFrameworkCore;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class DbSetExtensions
{
    /// <summary>
    /// Retrieves the query filter expression configured for a given entity type within the current model.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="dbSet">The DbSet representing the entity type.</param>
    /// <returns>The query filter expression, or null if none is configured.</returns>
    public static Expression<Func<T, bool>>? GetQueryFilter<T>(this DbSet<T> dbSet) where T : class
    {
        ArgumentNullException.ThrowIfNull(dbSet);

        var model = dbSet.GetService<IModel>();
        var entityType = model.FindEntityType(typeof(T));
        return entityType?.GetQueryFilter() as Expression<Func<T, bool>>;
    }

    /// <summary>
    /// Conditionally includes or excludes automatically included navigation properties in a query.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="includeDetails">Whether to include navigation properties.</param>
    /// <returns>The modified queryable.</returns>
    public static IQueryable<T> IncludeDetails<T>(this IQueryable<T> source, bool includeDetails) where T : class
    {
        ArgumentNullException.ThrowIfNull(source);
        return includeDetails ? source : source.IgnoreAutoIncludes();
    }

    /// <summary>
    /// Removes a specific condition from the existing query filter for a given DbSet, if present.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="dbSet">The DbSet to modify the query filter for.</param>
    /// <param name="condition">The condition to remove.</param>
    /// <returns>
    /// An IQueryable with the modified query filter, or with all filters removed if the filter is empty.
    /// </returns>
    public static IQueryable<T> RemoveFromQueryFilter<T>(this DbSet<T> dbSet, Expression<Func<T, bool>> condition) where T : class
    {
        ArgumentNullException.ThrowIfNull(dbSet);
        ArgumentNullException.ThrowIfNull(condition);

        var queryFilter = dbSet.GetQueryFilter();
        if (queryFilter == null)
            return dbSet.IgnoreQueryFilters();

        var newQueryFilter = queryFilter.RemoveCondition(condition);

        return newQueryFilter == null
            ? dbSet.IgnoreQueryFilters()
            : dbSet.IgnoreQueryFilters().Where(newQueryFilter);
    }

    public static IQueryable<T> ApplyQueryFilter<T>(this IQueryable<T> source, DbSet<T> dbSet) where T : class
    {
        var filter = dbSet.GetQueryFilter();
        return filter != null ? source.Where(filter) : source;
    }

    public static IQueryable<T> IncludeIf<T, TProperty>(this IQueryable<T> source, bool condition, Expression<Func<T, TProperty>> navigationPropertyPath) where T : class
    {
        return condition ? source.Include(navigationPropertyPath) : source;
    }

    public static IQueryable<T> IgnoreQueryFiltersIf<T>(this IQueryable<T> source, bool ignore) where T : class
    {
        return ignore ? source.IgnoreQueryFilters() : source;
    }
}
