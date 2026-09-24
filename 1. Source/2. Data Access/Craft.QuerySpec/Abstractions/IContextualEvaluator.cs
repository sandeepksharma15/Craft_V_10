using Craft.Core;

namespace Craft.QuerySpec;

/// <summary>
/// Evaluator that can use EF Core model metadata while applying a query specification.
/// </summary>
public interface IContextualEvaluator : IEvaluator
{
    IQueryable<T> GetQuery<T>(
        IQueryable<T> queryable,
        IQuery<T>? query,
        QueryEvaluationContext context)
        where T : class;
}
