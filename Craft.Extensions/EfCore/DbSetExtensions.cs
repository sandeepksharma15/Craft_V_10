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

        // Use GetDeclaredQueryFilters() instead of the obsolete GetQueryFilter()
        // GetDeclaredQueryFilters() returns IReadOnlyCollection<IQueryFilter>
        // We return the first filter's expression if any exist
        var declaredFilters = entityType?.GetDeclaredQueryFilters();

        if (declaredFilters == null || declaredFilters.Count == 0)
            return null;

        return declaredFilters.FirstOrDefault() is { Expression: Expression<Func<T, bool>> expr } 
            ? expr 
            : null;
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

    /// <summary>
    /// Applies a query filter to the specified <see cref="IQueryable{T}"/> source if a filter is defined for the
    /// corresponding <see cref="DbSet{T}"/>.
    /// </summary>
    /// <remarks>This method checks for a query filter associated with the specified <paramref name="dbSet"/>
    /// and applies it to the <paramref name="source"/> query if one exists. If no filter is defined, the original query
    /// is returned unmodified.</remarks>
    /// <typeparam name="T">The type of the entities in the query.</typeparam>
    /// <param name="source">The source query to which the filter will be applied.</param>
    /// <param name="dbSet">The <see cref="DbSet{T}"/> containing the query filter to apply.</param>
    /// <returns>An <see cref="IQueryable{T}"/> with the query filter applied if a filter is defined; otherwise, the original
    /// source query.</returns>
    public static IQueryable<T> ApplyQueryFilter<T>(this IQueryable<T> source, DbSet<T> dbSet) where T : class
    {
        var filter = dbSet.GetQueryFilter();

        return filter != null ? source.Where(filter) : source;
    }

    /// <summary>
    /// Conditionally includes a related entity in the query based on the specified condition.
    /// </summary>
    /// <remarks>This method is useful for dynamically including related entities in a query based on runtime
    /// conditions, such as user input or application state.</remarks>
    /// <typeparam name="T">The type of the entity in the query.</typeparam>
    /// <typeparam name="TProperty">The type of the related entity to include.</typeparam>
    /// <param name="source">The source queryable to apply the conditional include to.</param>
    /// <param name="condition">A boolean value indicating whether to include the related entity.</param>
    /// <param name="navigationPropertyPath">An expression specifying the related entity to include.</param>
    /// <returns>The original queryable if <paramref name="condition"/> is <see langword="false"/>; otherwise, the queryable with
    /// the specified related entity included.</returns>
    public static IQueryable<T> IncludeIf<T, TProperty>(this IQueryable<T> source, bool condition,
        Expression<Func<T, TProperty>> navigationPropertyPath) where T : class
    {
        return condition ? source.Include(navigationPropertyPath) : source;
    }

    /// <summary>
    /// Conditionally disables query filters applied to the source queryable.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the queryable.</typeparam>
    /// <param name="source">The source queryable to which the condition is applied.</param>
    /// <param name="ignore">A boolean value indicating whether to ignore query filters.  If <see langword="true"/>, query filters are
    /// ignored; otherwise, they are applied.</param>
    /// <returns>The source queryable with query filters ignored if <paramref name="ignore"/> is <see langword="true"/>,  or the
    /// original queryable with filters applied if <paramref name="ignore"/> is <see langword="false"/>.</returns>
    public static IQueryable<T> IgnoreQueryFiltersIf<T>(this IQueryable<T> source, bool ignore) where T : class
    {
        return ignore ? source.IgnoreQueryFilters() : source;
    }
}
