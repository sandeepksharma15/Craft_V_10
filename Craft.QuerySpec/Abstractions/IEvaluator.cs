namespace Craft.QuerySpec;

public interface IEvaluator
{
    IQueryable<T>? GetQuery<T>(IQueryable<T> queryable, IQuery<T>? query) where T : class;
}
