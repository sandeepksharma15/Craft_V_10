using System.Linq.Expressions;
using System.Reflection;
using Craft.QuerySpec.Enums;
using Craft.QuerySpec.Helpers;

namespace Craft.QuerySpec.Builders;

public static class ExpressionBuilder
{
    private static readonly MethodInfo _containsMethod = typeof(string)
        .GetMethod("Contains", [typeof(string)]);

    private static readonly MethodInfo _endsWithMethod = typeof(string)
        .GetMethod("EndsWith", [typeof(string)]);

    private static readonly MethodInfo _equalsMethod = typeof(string)
    .GetMethod("Equals", [typeof(string)]);

    // Get The Methods' References
    private static readonly MethodInfo _startsWithMethod = typeof(string)
        .GetMethod("StartsWith", [typeof(string)]);

    private static readonly MethodInfo _toUpperMethod = typeof(string)
        .GetMethod("ToUpper", []);

    public static Expression<Func<T, bool>> CreateWhereExpression<T>(FilterCriteria filterInfo)
    {
        Expression exprBody;
        ParameterExpression lambdaParam = Expression.Parameter(typeof(T));

        if (filterInfo is null)
            return Expression.Lambda<Func<T, bool>>
                (Expression.Constant(true), Expression.Parameter(typeof(T), "_"));

        // Get The Name Of The Property
        MemberExpression leftExpression = Expression.Property(lambdaParam, filterInfo.Name);

        Type dataType = Type.GetType(filterInfo.TypeName);

        exprBody = CreateExpressionBody(leftExpression, dataType, filterInfo.Value, filterInfo.Comparison);

        return Expression.Lambda<Func<T, bool>>(exprBody, lambdaParam);
    }

    public static Expression<Func<T, bool>> CreateWhereExpression<T>(Expression<Func<T, object>> propExpr,
        string dataValue, ComparisonType comparison)
    {
        ParameterExpression lambdaParam = Expression.Parameter(typeof(T));

        var name = propExpr.GetPropertyInfo()?.Name;
        MemberExpression memberExpression = Expression.Property(lambdaParam, name);

        var dataType = memberExpression.Type;

        var exprBody = CreateExpressionBody(memberExpression, dataType, dataValue, comparison);

        return Expression.Lambda<Func<T, bool>>(exprBody, lambdaParam);
    }

    public static Expression<Func<T, object>> GetPropertyExpression<T>(string propName)
    {
        MemberExpression member;

        ParameterExpression lambdaParam = Expression.Parameter(typeof(T));

        try
        {
            // We want to honour only User-Defined Properties
            if (typeof(T).IsPrimitive)
                return null;

            member = Expression.Property(lambdaParam, propName);
        }
        catch (ArgumentException)
        {
            return null;
        }

        return Expression.Lambda<Func<T, object>>(member, lambdaParam);
    }

    private static Expression CreateExpressionBody(MemberExpression leftExpression, Type dataType,
                    string dataValue, ComparisonType comparison)
    {
        object value;

        // Get The Current Date Time Format Provider
        var dateTimeFormatProvider = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;

        if (dataType.Equals(typeof(TimeOnly)))
            value = TimeOnly.Parse(dataValue, dateTimeFormatProvider);
        else if (dataType.Equals(typeof(DateOnly)))
            value = DateTime.Parse(dataValue, dateTimeFormatProvider);
        else
            value = Convert.ChangeType(dataValue, dataType);

        return dataType == typeof(string)
            ? CreateStringExpressionBody(leftExpression, dataType, value, comparison)
            : CreateNonStringExpressionBody(leftExpression, value, comparison);
    }

    private static Expression CreateNonStringExpressionBody(MemberExpression leftExpression,
        object value, ComparisonType comparison)
    {
        // Create Expression Out Of The Constant Value
        Expression<Func<object>> closure = () => value;

        // Create RHS Expression
        UnaryExpression rightExpression = Expression.Convert(closure.Body, leftExpression.Type);

        return comparison switch
        {
            ComparisonType.GreaterThan => Expression.GreaterThan(leftExpression, rightExpression),
            ComparisonType.GreaterThanOrEqualTo => Expression.GreaterThanOrEqual(leftExpression, rightExpression),
            ComparisonType.LessThan => Expression.LessThan(leftExpression, rightExpression),
            ComparisonType.LessThanOrEqualTo => Expression.LessThanOrEqual(leftExpression, rightExpression),
            ComparisonType.NotEqualTo => Expression.NotEqual(leftExpression, rightExpression),
            _ => Expression.Equal(leftExpression, rightExpression),
        };
    }

    private static Expression CreateStringExpressionBody(MemberExpression leftExpression,
        Type dataType, object value, ComparisonType comparison)
    {
        // Convert The Value To Upper Case
        MethodCallExpression upperCaseValue = Expression.Call(Expression.Constant(value, dataType), _toUpperMethod);

        // Convert The Class Member To Upper Case
        MethodCallExpression upperMember = Expression.Call(leftExpression, _toUpperMethod);

        return comparison switch
        {
            ComparisonType.EndsWith => Expression.Call(upperMember, _endsWithMethod, upperCaseValue),
            ComparisonType.Contains => Expression.Call(upperMember, _containsMethod, upperCaseValue),
            ComparisonType.StartsWith => Expression.Call(upperMember, _startsWithMethod, upperCaseValue),
            ComparisonType.EqualTo => Expression.Call(upperMember, _equalsMethod, upperCaseValue),
            ComparisonType.NotEqualTo => Expression.Not(Expression.Call(upperMember, _equalsMethod, upperCaseValue)),
            _ => throw new ArgumentException("String type doesn't supports this comparison"),
        };
    }
}
