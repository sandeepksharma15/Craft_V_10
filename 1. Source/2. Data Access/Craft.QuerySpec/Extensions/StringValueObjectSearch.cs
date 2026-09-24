using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Craft.QuerySpec;

/// <summary>
/// Bridges scalar model types stored as strings through an EF Core value converter
/// into QuerySpec's SQL LIKE search pipeline.
/// </summary>
public static class StringValueObjectSearch
{
    /// <summary>
    /// Registers a string-backed scalar model type for server-side QuerySpec searches.
    /// The marker method lives on a closed generic type, but is itself non-generic,
    /// which EF Core supports as a DbFunction mapping.
    /// </summary>
    public static ModelBuilder EnableStringSearch<TValue>(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        var method = StringValueObjectSearch<TValue>.AsStringMethod;

        modelBuilder.HasDbFunction(method)
            .HasTranslation(arguments =>
            {
                var argument = arguments[0];

                // The mapped property carries its value converter/type mapping. Expose the
                // provider representation as string so EF's normal LIKE translation can use it.
                return new SqlUnaryExpression(
                    ExpressionType.Convert,
                    argument,
                    typeof(string),
                    argument.TypeMapping);
            });

        return modelBuilder;
    }

    internal static Expression GetSearchExpression(Expression expression)
    {
        expression = UnwrapConvert(expression);

        if (expression.Type == typeof(string))
            return expression;

        var markerType = typeof(StringValueObjectSearch<>).MakeGenericType(expression.Type);
        var method = markerType.GetMethod(
            nameof(StringValueObjectSearch<int>.AsString),
            BindingFlags.Public | BindingFlags.Static)
            ?? throw new InvalidOperationException(
                $"Unable to locate string-search marker for '{expression.Type}'.");

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

/// <summary>
/// Per-value-type marker. AsString is deliberately non-generic: EF Core does not allow
/// generic methods to be registered as DbFunctions, while a method on a closed generic
/// declaring type is a concrete MethodInfo.
/// </summary>
public static class StringValueObjectSearch<TValue>
{
    internal static readonly MethodInfo AsStringMethod =
        typeof(StringValueObjectSearch<TValue>).GetMethod(
            nameof(AsString),
            BindingFlags.Public | BindingFlags.Static)
        ?? throw new InvalidOperationException(
            $"Unable to locate {nameof(AsString)} for '{typeof(TValue)}'.");

    public static string AsString(TValue value)
        => throw new InvalidOperationException(
            $"{nameof(AsString)} may only be used inside an EF Core query.");
}
