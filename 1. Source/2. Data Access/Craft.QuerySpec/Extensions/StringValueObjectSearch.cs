using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.QuerySpec;

/// <summary>
/// Bridges scalar model types stored as strings through an EF Core value converter
/// into QuerySpec's SQL LIKE search pipeline.
/// </summary>
public static class StringValueObjectSearch
{
    private static readonly ConcurrentDictionary<Type, byte> SearchableTypes = new();

    private static readonly MethodInfo AsStringMethod = typeof(StringValueObjectSearch)
        .GetMethod(nameof(AsString), BindingFlags.Public | BindingFlags.Static)
        ?? throw new InvalidOperationException($"Unable to locate {nameof(AsString)}.");

    /// <summary>
    /// Registers the EF Core translator used by QuerySpec string-backed scalar searches.
    /// Call this on the service collection used to configure the DbContext.
    /// </summary>
    public static IServiceCollection AddQuerySpecStringValueObjectSearch(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IMethodCallTranslatorPlugin, StringValueObjectMethodCallTranslatorPlugin>();
        return services;
    }

    /// <summary>
    /// Marks a scalar model type as searchable through its string provider representation.
    /// This does not change the EF model or database schema.
    /// </summary>
    public static void EnableStringSearch<TValue>()
        => SearchableTypes.TryAdd(typeof(TValue), 0);

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

    internal static bool IsSearchable(Type type) => SearchableTypes.ContainsKey(type);

    internal static bool IsAsStringMethod(MethodInfo method)
        => method.IsGenericMethod &&
           method.GetGenericMethodDefinition() == AsStringMethod;

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

internal sealed class StringValueObjectMethodCallTranslatorPlugin : IMethodCallTranslatorPlugin
{
    public StringValueObjectMethodCallTranslatorPlugin()
        => Translators = [new StringValueObjectMethodCallTranslator()];

    public IEnumerable<IMethodCallTranslator> Translators { get; }
}

internal sealed class StringValueObjectMethodCallTranslator : IMethodCallTranslator
{
    public SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (!StringValueObjectSearch.IsAsStringMethod(method) || arguments.Count != 1)
            return null;

        var modelType = method.GetGenericArguments()[0];

        if (!StringValueObjectSearch.IsSearchable(modelType))
            throw new InvalidOperationException(
                $"Type '{modelType.Name}' has not been enabled for QuerySpec string search.");

        return arguments[0];
    }
}
