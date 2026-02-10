using Craft.Core;
namespace Craft.QuerySpec;

/// <summary>
/// Interface for query evaluators that apply specific query specifications to IQueryable.
/// </summary>
public interface IEvaluator
{
    /// <summary>
    /// Applies query specification to the queryable.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="queryable">The queryable to evaluate. Must not be null.</param>
    /// <param name="query">The query specification to apply. May be null.</param>
    /// <returns>The modified queryable with the specification applied.</returns>
    IQueryable<T> GetQuery<T>(IQueryable<T> queryable, IQuery<T>? query) where T : class;
}

