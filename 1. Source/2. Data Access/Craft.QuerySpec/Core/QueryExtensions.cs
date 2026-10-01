using Craft.Core;
namespace Craft.QuerySpec;

public static class QueryExtensions
{
    public static IQuery<T>? AsNoTracking<T>(this IQuery<T> query) where T : class
    {
        if (query is null) return null;

        query.AsNoTracking = true;
        return query;
    }

    public static IQuery<T>? AsSplitQuery<T>(this IQuery<T> query) where T : class
    {
        if (query is null) return null;

        query.AsSplitQuery = true;
        return query;
    }

    public static IQuery<T>? IgnoreAutoIncludes<T>(this IQuery<T> query) where T : class
    {
        if (query is null) return null;

        query.IgnoreAutoIncludes = true;

        return query;
    }

    public static IQuery<T>? IgnoreQueryFilters<T>(this IQuery<T> query) where T : class
    {
        if (query is null) return null;

        query.IgnoreQueryFilters = true;
        return query;
    }

    public static IQuery<T>? Skip<T>(this IQuery<T> query, int? skip) where T : class
    {
        if (query is null) return null;

        query.Skip = skip;
        return query;
    }

    public static IQuery<T>? Take<T>(this IQuery<T> query, int? take) where T : class
    {
        if (query is null) return null;

        query.Take = take;
        return query;
    }

    public static IQuery<T>? SetPostProcessingAction<T>(this IQuery<T> query, Func<IEnumerable<T>, IEnumerable<T>> postProcessingAction) where T : class
    {
        if (query is null) return null;

        query.PostProcessingAction = postProcessingAction;
        return query;
    }

    public static IQuery<T, TResult>? SetPostProcessingAction<T, TResult>(this IQuery<T, TResult> query, Func<IEnumerable<TResult>, IEnumerable<TResult>> postProcessingAction) where T : class where TResult : class
    {
        if (query is null) return null;

        query.PostProcessingAction = postProcessingAction;
        return query;
    }

    public static bool IsWithoutOrder<T>(this IQuery<T>? query) where T : class
    {
        return query is null || query.SortOrderBuilder is null || query.SortOrderBuilder.OrderDescriptorList.Count == 0;
    }

    /// <summary>
    /// Applies advanced filter metadata to a query using the appropriate query builder.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="query">The query to update.</param>
    /// <param name="filterCriteria">The advanced filter criteria to apply.</param>
    /// <param name="searchGroup">The search group to use when routing string-backed value object filters through SQL-like search.</param>
    /// <returns>The updated query.</returns>
    public static IQuery<T>? ApplyFilterCriteria<T>(this IQuery<T> query, EntityFilterCriteria<T> filterCriteria, int searchGroup = 1)
        where T : class
    {
        if (query is null)
            return null;

        ArgumentNullException.ThrowIfNull(filterCriteria);

        var metadata = filterCriteria.Metadata;
        if (metadata?.PropertyType.IsStringBackedValueObject() == true)
        {
            var searchValue = metadata.Value?.ToString();

            if (string.IsNullOrWhiteSpace(searchValue))
                return query;

            query.Search(metadata.Name, $"%{searchValue.Trim()}%", searchGroup);
            return query;
        }

        query.EntityFilterBuilder?.Add(filterCriteria);
        return query;
    }
}

