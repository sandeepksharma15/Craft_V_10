using Craft.Core;
namespace Craft.QuerySpec;

public interface ISelectEvaluator
{
    IQueryable<TResult>? GetQuery<T, TResult>(IQueryable<T> queryable, IQuery<T, TResult>? query)
        where T : class
        where TResult : class;

    IQueryable<T>? GetQuery<T>(IQueryable<T> queryable, IQuery<T>? query)
        where T : class;
}

