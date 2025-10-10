using System.Linq.Expressions;

namespace Craft.Utilities.Helpers;

/// <summary>
/// Provides functionality to replace occurrences of a specific parameter in an expression tree with a new expression.
/// </summary>
/// <remarks>This class is used internally to modify expression trees by substituting a specified parameter with a
/// given expression. It is particularly useful in scenarios where dynamic expression manipulation is required, such as
/// building LINQ queries or transforming lambda expressions.</remarks>
public sealed class ParameterReplacerVisitor : ExpressionVisitor
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

    public static Expression Replace(Expression expression, ParameterExpression oldParameter, Expression newExpression)
    {
        ArgumentNullException.ThrowIfNull(expression);
        ArgumentNullException.ThrowIfNull(oldParameter);
        ArgumentNullException.ThrowIfNull(newExpression);
        return oldParameter.Type != newExpression.Type
            ? throw new ArgumentException("The type of the new expression must match the type of the old parameter.")
            : new ParameterReplacerVisitor(oldParameter, newExpression).Visit(expression);
    }
}
