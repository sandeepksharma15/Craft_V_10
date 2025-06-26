using System.Linq.Expressions;

namespace Craft.QuerySpec.Extensions;

internal sealed class ParameterReplacerVisitor : ExpressionVisitor
{
    private readonly Expression newExpression;
    private readonly ParameterExpression oldParameter;

    private ParameterReplacerVisitor(ParameterExpression oldParameter, Expression newExpression)
    {
        this.oldParameter = oldParameter;
        this.newExpression = newExpression;
    }

    protected override Expression VisitParameter(ParameterExpression p)
    {
        return p == oldParameter ? newExpression : p;
    }

    internal static Expression Replace(Expression expression, ParameterExpression oldParameter, Expression newExpression)
    {
        return new ParameterReplacerVisitor(oldParameter, newExpression).Visit(expression);
    }
}
