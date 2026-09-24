using Craft.Core;
using Craft.Extensions.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace Craft.QuerySpec;

public static class SearchExtension
{
    private static readonly MemberExpression Functions = Expression
        .Property(null, typeof(EF)
        .GetProperty(nameof(EF.Functions)) ?? throw new TargetException("The EF.Functions not found!"));

    private static readonly MethodInfo LikeMethodInfo = typeof(DbFunctionsExtensions)
            .GetMethod(nameof(DbFunctionsExtensions.Like), [typeof(DbFunctions), typeof(string), typeof(string)])
        ?? throw new TargetException("The EF.Functions.Like not found");

    /// <summary>
    /// Filters <paramref name="source"/> by applying an SQL LIKE operation.
    /// </summary>
    public static IQueryable<T>? Search<T>(this IQueryable<T> source, IEnumerable<SqlLikeSearchInfo<T>> criterias)
        where T : class
        => Search(source, criterias, null);

    /// <summary>
    /// Filters <paramref name="source"/> by applying an SQL LIKE operation, using EF Core metadata
    /// to recognise scalar properties whose provider representation is a string.
    /// </summary>
    internal static IQueryable<T>? Search<T>(
        this IQueryable<T> source,
        IEnumerable<SqlLikeSearchInfo<T>> criterias,
        IReadOnlyEntityType? entityType)
        where T : class
    {
        if (criterias is null)
            return source;

        Expression? expression = null;
        var parameter = Expression.Parameter(typeof(T), "x");

        foreach (var group in criterias
                     .Where(criteria => criteria is not null &&
                                        criteria.SearchItem is not null &&
                                        !string.IsNullOrEmpty(criteria.SearchString))
                     .GroupBy(criteria => criteria.SearchGroup)
                     .OrderBy(group => group.Key))
        {
            Expression? groupExpression = null;

            foreach (var criteria in group)
            {
                var propertySelector = ParameterReplacerVisitor.Replace(
                    criteria.SearchItem!,
                    criteria.SearchItem!.Parameters[0],
                    parameter) as LambdaExpression
                    ?? throw new InvalidExpressionException();

                var searchTermAsExpression =
                    ((Expression<Func<string>>)(() => criteria.SearchString!)).Body;

                var searchExpression = GetSearchExpression(propertySelector.Body, entityType);

                var likeExpression = Expression.Call(
                    null,
                    LikeMethodInfo,
                    Functions,
                    searchExpression,
                    searchTermAsExpression);

                groupExpression = groupExpression is null
                    ? likeExpression
                    : Expression.OrElse(groupExpression, likeExpression);
            }

            if (groupExpression is not null)
            {
                expression = expression is null
                    ? groupExpression
                    : Expression.AndAlso(expression, groupExpression);
            }
        }

        return expression is null
            ? source
            : source.Where(Expression.Lambda<Func<T, bool>>(expression, parameter));
    }

    private static Expression GetSearchExpression(
        Expression expression,
        IReadOnlyEntityType? entityType)
    {
        expression = UnwrapConvert(expression);

        if (expression.Type == typeof(string))
            return expression;

        if (entityType is not null &&
            TryGetMappedProperty(expression, entityType, out var property))
        {
            var providerClrType = property.GetTypeMapping().Converter?.ProviderClrType
                                  ?? property.GetProviderClrType()
                                  ?? property.ClrType;

            if (providerClrType == typeof(string))
                return ConvertStringBackedProperty(expression, property);
        }

        throw new NotSupportedException(
            $"SQL LIKE search requires a string expression or a mapped property with a string provider type. " +
            $"Property type '{expression.Type.Name}' is not supported.");
    }

    private static Expression ConvertStringBackedProperty(
        Expression expression,
        IReadOnlyProperty property)
    {
        var modelType = Nullable.GetUnderlyingType(expression.Type) ?? expression.Type;
        var conversion = FindConversionOperator(modelType, typeof(string));

        if (conversion is null)
        {
            throw new NotSupportedException(
                $"Property '{property.DeclaringType.Name}.{property.Name}' is stored as string, but " +
                $"'{modelType.Name}' does not expose an implicit or explicit conversion to string that can be " +
                "represented in the LINQ expression tree.");
        }

        if (Nullable.GetUnderlyingType(expression.Type) is null)
            return Expression.Convert(expression, typeof(string), conversion);

        var hasValue = Expression.Property(expression, nameof(Nullable<int>.HasValue));
        var value = Expression.Property(expression, nameof(Nullable<int>.Value));
        var convertedValue = Expression.Convert(value, typeof(string), conversion);

        return Expression.Condition(
            hasValue,
            convertedValue,
            Expression.Constant(null, typeof(string)));
    }

    private static MethodInfo? FindConversionOperator(Type sourceType, Type targetType)
    {
        static bool IsConversion(MethodInfo method, Type source, Type target)
        {
            if (method.Name is not ("op_Implicit" or "op_Explicit") ||
                method.ReturnType != target)
                return false;

            var parameters = method.GetParameters();
            return parameters.Length == 1 && parameters[0].ParameterType == source;
        }

        var sourceConversion = sourceType
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(method => IsConversion(method, sourceType, targetType));

        if (sourceConversion is not null || sourceType == targetType)
            return sourceConversion;

        return targetType
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(method => IsConversion(method, sourceType, targetType));
    }

    private static bool TryGetMappedProperty(
        Expression expression,
        IReadOnlyEntityType entityType,
        out IReadOnlyProperty property)
    {
        expression = UnwrapConvert(expression);

        if (expression is MemberExpression
            {
                Expression: ParameterExpression,
                Member: PropertyInfo propertyInfo
            } &&
            entityType.FindProperty(propertyInfo.Name) is { } mappedProperty)
        {
            property = mappedProperty;
            return true;
        }

        property = null!;
        return false;
    }

    private static Expression UnwrapConvert(Expression expression)
    {
        while (expression is UnaryExpression
               {
                   NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked,
                   Method: null
               } unary)
        {
            expression = unary.Operand;
        }

        return expression;
    }
}
