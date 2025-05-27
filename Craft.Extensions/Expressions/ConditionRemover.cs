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
    private static bool IsEquivalentCondition(Expression expr1, Expression expr2)
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
