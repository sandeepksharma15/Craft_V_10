using System.Linq.Expressions;
using System.Reflection;

namespace Craft.QuerySpec;

/// <summary>
/// Provides utilities to build LINQ expressions for filtering entities based on dynamic criteria.
/// </summary>
public static class ExpressionBuilder
{
    // Cached MethodInfo references for string operations
    private static readonly MethodInfo _containsMethod = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])
        ?? throw new InvalidOperationException("Could not find 'Contains' method on string.");
    private static readonly MethodInfo _endsWithMethod = typeof(string).GetMethod(nameof(string.EndsWith), [typeof(string)])
        ?? throw new InvalidOperationException("Could not find 'EndsWith' method on string.");
    private static readonly MethodInfo _equalsMethod = typeof(string).GetMethod(nameof(string.Equals), [typeof(string)])
        ?? throw new InvalidOperationException("Could not find 'Equals' method on string.");
    private static readonly MethodInfo _startsWithMethod = typeof(string).GetMethod(nameof(string.StartsWith), [typeof(string)])
        ?? throw new InvalidOperationException("Could not find 'StartsWith' method on string.");
    private static readonly MethodInfo _toUpperMethod = typeof(string).GetMethod(nameof(string.ToUpper), [])
        ?? throw new InvalidOperationException("Could not find 'ToUpper' method on string.");

    /// <summary>
    /// Creates a LINQ where expression for the given filter criteria.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="filterInfo">The filter criteria.</param>
    /// <returns>A lambda expression for filtering.</returns>
    /// <exception cref="ArgumentNullException">Thrown if filterInfo is null.</exception>
    public static Expression<Func<T, bool>> CreateWhereExpression<T>(FilterCriteria filterInfo)
    {
        ArgumentNullException.ThrowIfNull(filterInfo);
        ArgumentException.ThrowIfNullOrEmpty(filterInfo.Name, nameof(filterInfo.Name));

        _ = typeof(T).GetProperty(filterInfo.Name)
            ?? throw new ArgumentException($"Property '{filterInfo.Name}' does not exist on type '{typeof(T).Name}'.");

        ParameterExpression lambdaParam = Expression.Parameter(typeof(T));
        MemberExpression leftExpression = Expression.Property(lambdaParam, filterInfo.Name);

        Type dataType = filterInfo.PropertyType;
        object? value = filterInfo.Value;

        Expression exprBody = CreateExpressionBody(leftExpression, dataType, value, filterInfo.Comparison);

        return Expression.Lambda<Func<T, bool>>(exprBody, lambdaParam);
    }

    /// <summary>
    /// Creates a LINQ where expression for the given property selector and comparison.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="propExpr">Property selector expression.</param>
    /// <param name="dataValue">Value to compare with.</param>
    /// <param name="comparison">Comparison type.</param>
    /// <returns>A lambda expression for filtering.</returns>
    /// <exception cref="ArgumentNullException">Thrown if propExpr is null.</exception>
    public static Expression<Func<T, bool>> CreateWhereExpression<T>(Expression<Func<T, object>> propExpr,
        object? dataValue, ComparisonType comparison)
    {
        ArgumentNullException.ThrowIfNull(propExpr, nameof(propExpr));

        ParameterExpression lambdaParam = Expression.Parameter(typeof(T));

        var name = propExpr.GetPropertyInfo()?.Name;

        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Could not extract property name from expression.", nameof(propExpr));

        MemberExpression memberExpression = Expression.Property(lambdaParam, name);

        var dataType = memberExpression.Type;
        var exprBody = CreateExpressionBody(memberExpression, dataType, dataValue, comparison);

        return Expression.Lambda<Func<T, bool>>(exprBody, lambdaParam);
    }

    /// <summary>
    /// Gets a property selector expression for the given property name.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="propName">The property name.</param>
    /// <returns>A property selector expression, or null if not found or not user-defined.</returns>
    public static Expression<Func<T, object>>? GetPropertyExpression<T>(string propName)
    {
        if (string.IsNullOrWhiteSpace(propName)) return null;

        if (typeof(T).IsPrimitive) return null;

        ParameterExpression lambdaParam = Expression.Parameter(typeof(T));

        try
        {
            var member = Expression.Property(lambdaParam, propName);

            return Expression.Lambda<Func<T, object>>(Expression.Convert(member, typeof(object)), lambdaParam);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    // Creates the body of the filter expression for the given property, type, value, and comparison.
    private static Expression CreateExpressionBody(MemberExpression leftExpression, Type dataType, object? value, ComparisonType comparison)
    {
        return dataType == typeof(string)
            ? CreateStringExpressionBody(leftExpression, dataType, value, comparison)
            : CreateNonStringExpressionBody(leftExpression, value, comparison);
    }

    // Creates the body of the filter expression for non-string types.
    private static Expression CreateNonStringExpressionBody(MemberExpression leftExpression, object? value, ComparisonType comparison)
    {
        var typedValue = value == null ? null : Convert.ChangeType(value, leftExpression.Type);

        var rightExpression = Expression.Constant(typedValue, leftExpression.Type);

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

    // Creates the body of the filter expression for string types, using case-insensitive comparison.
    private static Expression CreateStringExpressionBody(MemberExpression leftExpression, Type dataType, object? value, ComparisonType comparison)
    {
        var upperCaseValue = Expression.Call(Expression.Constant(value, dataType), _toUpperMethod);
        var upperMember = Expression.Call(leftExpression, _toUpperMethod);

        return comparison switch
        {
            ComparisonType.EndsWith => Expression.Call(upperMember, _endsWithMethod, upperCaseValue),
            ComparisonType.Contains => Expression.Call(upperMember, _containsMethod, upperCaseValue),
            ComparisonType.StartsWith => Expression.Call(upperMember, _startsWithMethod, upperCaseValue),
            ComparisonType.EqualTo => Expression.Call(upperMember, _equalsMethod, upperCaseValue),
            ComparisonType.NotEqualTo => Expression.Not(Expression.Call(upperMember, _equalsMethod, upperCaseValue)),
            _ => throw new ArgumentException("String type doesn't support this comparison", nameof(comparison)),
        };
    }
}
