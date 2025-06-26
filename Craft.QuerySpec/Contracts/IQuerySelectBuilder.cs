using System.Linq.Expressions;
using Craft.QuerySpec.Builders;

namespace Craft.QuerySpec;

public interface IQuerySelectBuilder<T, TResult>
    where T : class
    where TResult : class
{
    QuerySelectBuilder<T, TResult> Add(LambdaExpression column, LambdaExpression assignTo);

    QuerySelectBuilder<T, TResult> Add(LambdaExpression column);

    Expression<Func<T, TResult>>? Build();

    void Clear();
}

public interface IQuerySelectBuilder<T> : IQuerySelectBuilder<T, T> where T : class;
