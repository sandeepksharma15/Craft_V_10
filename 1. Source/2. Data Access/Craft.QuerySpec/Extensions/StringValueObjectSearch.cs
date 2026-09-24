using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Linq.Expressions;
using System.Reflection;

namespace Craft.QuerySpec;

/// <summary>
/// Bridges scalar model types that are stored as strings through an EF Core value converter
/// into QuerySpec's SQL LIKE search pipeline.
/// </summary>
public static class StringValueObjectSearch
{
    private static readonly MethodInfo AsStringMethod = typeof(StringValueObjectSearch)
        .GetMethod(nameof(AsString), BindingFlags.Public | BindingFlags.Static)
        ?? throw new InvalidOperationException($"Unable to locate {nameof(AsString)}.");

    /// <summary>
    /// Registers a string-backed scalar model type for server-side QuerySpec searches.
    /// The mapped EF property must use a value converter whose provider CLR type is string.
    /// </summary>
    public static ModelBuilder EnableStringSearch<TValue>(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        var method = AsStringMethod.MakeGenericMethod(typeof(TValue));

        modelBuilder.HasDbFunction(method)
            .HasTranslation(arguments => arguments[0]);

        return modelBuilder;
    }

    /// <summary>
    /// Marker used only inside translated LINQ queries.
    /// </summary>
    public static string AsString<TValue>(TValue value)
        => throw new InvalidOperationException(
            $"{nameof(AsString)} may only be used inside an EF Core query.");

    internal static Expression GetSearchExpression(Expression expression)
    {
        expression = UnwrapConvert(expression);

        if (expression.Type == typeof(string))
            return expression;

        var method = AsStringMethod.MakeGenericMethod(expression.Type);
        return Expression.Call(method, expression);
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
