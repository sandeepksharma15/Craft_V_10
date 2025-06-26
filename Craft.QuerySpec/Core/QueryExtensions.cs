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
        if (query is null) return true;

        return query.SortOrderBuilder is null || query.SortOrderBuilder.OrderDescriptorList.Count == 0;
    }
}
