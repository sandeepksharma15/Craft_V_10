using System.Linq.Expressions;

namespace Craft.QuerySpec;

public interface IQuery<T, TResult> : IQuery<T>
    where T : class
    where TResult : class
{
    new Func<IEnumerable<TResult>, IEnumerable<TResult>>? PostProcessingAction { get; internal set; }

    QuerySelectBuilder<T, TResult>? QuerySelectBuilder { get; }

    Expression<Func<T, IEnumerable<TResult>>>? SelectorMany { get; internal set; }

    new void Clear();
}

public interface IQuery<T> where T : class
{
    /// <summary>
    /// Gets or sets a value indicating whether the query should be tracked by the DbContext.
    /// </summary>
    /// <remarks>
    /// Setting this to true disables change tracking for the duration of the query,
    /// which can improve performance for read-only queries.
    /// </remarks>
    bool AsNoTracking { get; internal set; }

    /// <summary>
    /// Gets or sets a value indicating whether the query should be split into multiple SQL queries.
    /// </summary>
    /// <remarks>
    /// This is useful for queries that may produce large results sets, as it can help
    /// to reduce the amount of data loaded into memory at once.
    /// </remarks>
    bool AsSplitQuery { get; internal set; }

    /// <summary>
    /// Gets or sets a value indicating whether automatic including of related entities should be ignored.
    /// </summary>
    /// <remarks>
    /// Setting this to true prevents EF Core from automatically including related entities
    /// when they are not explicitly specified in the query.
    /// </remarks>
    bool IgnoreAutoIncludes { get; internal set; }

    /// <summary>
    /// Gets or sets a value indicating whether query filters should be ignored.
    /// </summary>
    /// <remarks>
    /// This can be useful for debugging or for queries that need to bypass security
    /// or data shaping filters.
    /// </remarks>
    bool IgnoreQueryFilters { get; internal set; }

    /// <summary>
    /// Gets or sets the number of records to skip before starting to return records.
    /// </summary>
    /// <remarks>
    /// This is useful for pagination, allowing you to skip a certain number of records
    /// and only return the following set.
    /// </remarks>
    int? Skip { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of records to return.
    /// </summary>
    /// <remarks>
    /// This is useful for limiting the number of records returned by the query,
    /// especially in combination with <see cref="Skip"/> for pagination.
    /// </remarks>
    int? Take { get; set; }

    /// <summary>
    /// Gets the sort order builder, used to specify sorting of the query results.
    /// </summary>
    SortOrderBuilder<T>? SortOrderBuilder { get; internal set; }

    /// <summary>
    /// Gets or sets a delegate for post-processing the query results.
    /// </summary>
    /// <remarks>
    /// This allows for additional processing of the results after the query has been executed,
    /// but before it is returned to the caller.
    /// </remarks>
    Func<IEnumerable<T>, IEnumerable<T>>? PostProcessingAction { get; internal set; }

    /// <summary>
    /// Gets the SQL LIKE search criteria builder, used to specify SQL LIKE conditions for the query.
    /// </summary>
    SqlLikeSearchCriteriaBuilder<T>? SqlLikeSearchCriteriaBuilder { get; internal set; }

    /// <summary>
    /// Gets the entity filter builder, used to specify filters for the query results.
    /// </summary>
    EntityFilterBuilder<T>? EntityFilterBuilder { get; internal set; }

    /// <summary>
    /// Gets the collection of include expressions for eagerly loading related entities.
    /// </summary>
    /// <remarks>
    /// This allows for dynamic specification of navigation properties to include in the query,
    /// avoiding the need for AutoInclude configuration in the DbContext.
    /// </remarks>
    List<IncludeExpression>? IncludeExpressions { get; internal set; }

    /// <summary>
    /// Clears the query specifications, resetting all properties to their default values.
    /// </summary>
    void Clear();

    /// <summary>
    /// Determines whether the specified entity satisfies the query criteria.
    /// </summary>
    /// <param name="entity">The entity to test.</param>
    /// <returns>true if the entity satisfies the query criteria; otherwise, false.</returns>
    bool IsSatisfiedBy(T entity);

    /// <summary>
    /// Sets the pagination for the query, specifying the page number and page size.
    /// </summary>
    /// <param name="page">The page number (0-based).</param>
    /// <param name="pageSize">The number of records per page.</param>
    void SetPage(int page, int pageSize);
}
