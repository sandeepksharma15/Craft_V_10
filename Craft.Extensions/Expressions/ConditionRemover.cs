using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace Craft.Extensions.Expressions;

/// <summary>
/// Provides methods to remove a specified condition from an expression.
/// </summary>
public static class ConditionRemover
{
    /// <summary>
    /// Removes the specified condition from the given expression.
    /// </summary>
    /// <typeparam name="T">The type of the parameter in the expression.</typeparam>
    /// <param name="expression">The original expression.</param>
    /// <param name="condition">The condition to remove.</param>
    /// <returns>
    /// A new expression with the condition removed, or null if the conditions are equivalent.
    /// </returns>
    public static Expression<Func<T, bool>>? RemoveCondition<T>(this Expression<Func<T, bool>> expression,
        Expression<Func<T, bool>> condition)
    {
        ArgumentNullException.ThrowIfNull(expression);
        ArgumentNullException.ThrowIfNull(condition);

        if (IsEquivalentCondition(expression.Body, condition.Body))
            return null;

        var visitor = new ConditionRemoverVisitor<T>(condition.Body);
        var modifiedBody = visitor.Visit(expression.Body);

        return Expression.Lambda<Func<T, bool>>(modifiedBody, expression.Parameters);
    }

    /// <summary>
    /// Removes specified conditions from the given expression, returning a new expression with the conditions removed.
    /// </summary>
    /// <remarks>This method iterates through the provided conditions and removes any matching conditions from
    /// the original expression. If the resulting expression is equivalent to one of the conditions, the method returns
    /// <see langword="null"/>.</remarks>
    /// <typeparam name="T">The type of the parameter in the expression.</typeparam>
    /// <param name="expression">The original expression from which conditions will be removed. Cannot be <see langword="null"/>.</param>
    /// <param name="conditions">An array of conditions to remove from the expression. If the array is <see langword="null"/> or empty, the
    /// original expression is returned.</param>
    /// <returns>A new expression with the specified conditions removed, or <see langword="null"/> if the entire expression is
    /// equivalent to one of the conditions.</returns>
    public static Expression<Func<T, bool>>? RemoveConditions<T>(this Expression<Func<T, bool>> expression,
        params Expression<Func<T, bool>>[] conditions)
    {
        ArgumentNullException.ThrowIfNull(expression);

        if (conditions == null || conditions.Length == 0) return expression;

        var body = expression.Body;

        foreach (var cond in conditions)
        {
            if (IsEquivalentCondition(body, cond.Body))
                return null;
            var visitor = new ConditionRemoverVisitor<T>(cond.Body);
            body = visitor.Visit(body);
        }

        return Expression.Lambda<Func<T, bool>>(body, expression.Parameters);
    }

    /// <summary>
    /// Replaces all occurrences of a specified condition within a given expression with a new condition.
    /// </summary>
    /// <remarks>This method is useful for dynamically modifying expressions, such as when building or
    /// transforming LINQ queries. The replacement is performed by traversing the expression tree and substituting
    /// occurrences of <paramref name="oldCondition"/> with <paramref name="newCondition"/>.</remarks>
    /// <typeparam name="T">The type of the parameter in the expressions.</typeparam>
    /// <param name="expression">The original expression in which the condition will be replaced. Cannot be <see langword="null"/>.</param>
    /// <param name="oldCondition">The condition to be replaced. Cannot be <see langword="null"/>.</param>
    /// <param name="newCondition">The condition to replace the old condition with. Cannot be <see langword="null"/>.</param>
    /// <returns>A new expression with all occurrences of <paramref name="oldCondition"/> replaced by <paramref
    /// name="newCondition"/>.</returns>
    public static Expression<Func<T, bool>> ReplaceCondition<T>(this Expression<Func<T, bool>> expression,
        Expression<Func<T, bool>> oldCondition,
        Expression<Func<T, bool>> newCondition)
    {
        ArgumentNullException.ThrowIfNull(expression);
        ArgumentNullException.ThrowIfNull(oldCondition);
        ArgumentNullException.ThrowIfNull(newCondition);

        var visitor = new ConditionReplacerVisitor<T>(oldCondition.Body, newCondition.Body);
        var modifiedBody = visitor.Visit(expression.Body);

        return Expression.Lambda<Func<T, bool>>(modifiedBody, expression.Parameters);
    }

    /// <summary>
    /// Checks if two expressions are semantically equivalent.
    /// </summary>
    public static bool IsEquivalentCondition(Expression expr1, Expression expr2)
    {
        if (expr1.CanReduce) expr1 = expr1.Reduce();
        if (expr2.CanReduce) expr2 = expr2.Reduce();

        // Use built-in Expression equality if available, otherwise fallback to semantic comparison
        return ExpressionEqualityComparer.Instance.Equals(expr1, expr2)
            || new ExpressionSemanticEqualityComparer().Equals(expr1, expr2);
    }

    /// <summary>
    /// Visitor class responsible for removing a specific condition from an expression tree.
    /// </summary>
    private sealed class ConditionRemoverVisitor<T> : ExpressionVisitor
    {
        private readonly Expression _conditionToRemove;

        public ConditionRemoverVisitor(Expression conditionToRemove)
        {
            _conditionToRemove = conditionToRemove;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.AndAlso)
            {
                if (IsEquivalentCondition(node.Left, _conditionToRemove))
                    return node.Right;
                if (IsEquivalentCondition(node.Right, _conditionToRemove))
                    return node.Left;
            }
            return base.VisitBinary(node);
        }
    }

    private sealed class ConditionReplacerVisitor<T> : ExpressionVisitor
    {
        private readonly Expression _oldCondition;
        private readonly Expression _newCondition;

        public ConditionReplacerVisitor(Expression oldCondition, Expression newCondition)
        {
            _oldCondition = oldCondition;
            _newCondition = newCondition;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.AndAlso)
            {
                if (IsEquivalentCondition(node.Left, _oldCondition))
                    return Expression.MakeBinary(node.NodeType, _newCondition, node.Right);
                if (IsEquivalentCondition(node.Right, _oldCondition))
                    return Expression.MakeBinary(node.NodeType, node.Left, _newCondition);
            }
            return base.VisitBinary(node);
        }
    }
}
