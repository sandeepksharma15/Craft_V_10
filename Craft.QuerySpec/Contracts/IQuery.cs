using System.Linq.Expressions;
using Craft.QuerySpec.Builders;

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
    bool AsNoTracking { get; internal set; }
    bool AsSplitQuery { get; internal set; }
    bool IgnoreAutoIncludes { get; internal set; }
    bool IgnoreQueryFilters { get; internal set; }

    int? Skip { get; set; }
    int? Take { get; set; }

    SortOrderBuilder<T>? SortOrderBuilder { get; }
    Func<IEnumerable<T>, IEnumerable<T>>? PostProcessingAction { get; internal set; }
    SqlLikeSearchCriteriaBuilder<T>? SqlLikeSearchCriteriaBuilder { get; }
    EntityFilterBuilder<T>? EntityFilterBuilder { get; }

    void Clear();
    bool IsSatisfiedBy(T entity);
    void SetPage(int page, int pageSize);
}
