using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Craft.QuerySpec;

/// <summary>
/// Bridges scalar model types that are stored as strings through an EF Core value converter
/// into QuerySpec's SQL LIKE search pipeline.
/// </summary>
public static class StringValueObjectSearch
{
    private static readonly MethodCallExpression PropertyMethodTemplate =
        (MethodCallExpression)((Expression<Func<object, object>>)(entity => EF.Property<object>(entity, string.Empty))).Body;

    /// <summary>
    /// Registers a string-backed scalar model type for server-side QuerySpec searches.
    /// </summary>
    public static ModelBuilder EnableStringSearch<TValue>(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        return modelBuilder;
    }

    internal static Expression GetSearchExpression(Expression expression)
    {
        expression = UnwrapConvert(expression);

        if (expression.Type == typeof(string))
            return expression;

        return TryBuildEfPropertyStringAccess(expression) ?? expression;
    }

    private static Expression? TryBuildEfPropertyStringAccess(Expression expression)
    {
        if (expression is not MemberExpression { Expression: not null } memberExpression)
            return null;

        var entityExpression = UnwrapConvert(memberExpression.Expression);

        if (entityExpression.NodeType != ExpressionType.Parameter)
            return null;

        var providerExpression = Expression.Call(
            PropertyMethodTemplate.Method,
            entityExpression,
            Expression.Constant(memberExpression.Member.Name));

        return Expression.Convert(providerExpression, typeof(string));
    }

    private static Expression UnwrapConvert(Expression expression)
    {
        while (expression is UnaryExpression
               {
                   NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked
               } unary)
        {
            expression = unary.Operand;
        }

        return expression;
    }
}
