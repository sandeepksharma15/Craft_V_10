using System.Linq.Expressions;
using System.Reflection;

namespace Craft.QuerySpec;

/// <summary>
/// Represents a filter criterion for querying entities, including property type, name, value, and comparison type.
/// </summary>
public sealed record FilterCriteria
{
    /// <summary>
    /// The type of the property being filtered.
    /// </summary>
    public Type PropertyType { get; }

    /// <summary>
    /// The property name being filtered.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The value to compare with.
    /// </summary>
    public object? Value { get; }

    /// <summary>
    /// The type of comparison to perform.
    /// </summary>
    public ComparisonType Comparison { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="FilterCriteria"/>.
    /// </summary>
    /// <param name="propertyType">The type of the property.</param>
    /// <param name="name">The property name.</param>
    /// <param name="value">The value to compare with.</param>
    /// <param name="comparison">The comparison type.</param>
    /// <exception cref="ArgumentNullException">Thrown if any required argument is null.</exception>
    public FilterCriteria(Type propertyType, string name, object? value, ComparisonType comparison = ComparisonType.EqualTo)
    {
        ArgumentNullException.ThrowIfNull(propertyType, nameof(propertyType));
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        // Check if value is null and propertyType is not nullable
        if (value is null)
        {
            bool isNullable =
                !propertyType.IsValueType // Reference type
                || (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>));

            if (!isNullable)
                throw new ArgumentException($"Value cannot be null for non-nullable type '{propertyType.FullName}'.", nameof(value));
        }

        PropertyType = propertyType;
        Name = name;
        Value = value;
        Comparison = comparison;
    }

    /// <summary>
    /// Creates a LINQ expression that represents a filter condition based on the specified criteria.
    /// </summary>
    /// <typeparam name="T">The type of the entity to which the filter will be applied.</typeparam>
    /// <param name="filterInfo">The filter criteria used to construct the expression. Cannot be <see langword="null"/>.</param>
    /// <returns>A LINQ expression of type <see cref="Expression{Func{T, Boolean}}"/> that can be used to filter a collection of
    /// <typeparamref name="T"/>.</returns>
    public static Expression<Func<T, bool>> GetExpression<T>(FilterCriteria filterInfo)
    {
        ArgumentNullException.ThrowIfNull(filterInfo, nameof(filterInfo));

        return ExpressionBuilder.CreateWhereExpression<T>(filterInfo);
    }

    /// <summary>
    /// Creates a <see cref="FilterCriteria"/> from a property selector and comparison value.
    /// </summary>
    public static FilterCriteria GetFilterInfo<T>(Expression<Func<T, object>> propName, object compareWith, ComparisonType comparisonType)
    {
        ArgumentNullException.ThrowIfNull(propName, nameof(propName));

        MemberInfo prop = propName.GetPropertyInfo<T>()
            ?? throw new ArgumentException($"You must pass a lambda of the form: '() => {{Class}}.{{Property}}'", nameof(propName));

        string name = prop.Name;
        Type? type = prop.GetMemberUnderlyingType();

        if (type?.IsEnum == true)
        {
            type = typeof(int);
            compareWith = (int)compareWith;
        }

        if (Nullable.GetUnderlyingType(type!) != null)
            type = type?.GetNonNullableType();

        return new FilterCriteria(type!, name, compareWith, comparisonType);
    }

    /// <summary>
    /// Creates a <see cref="FilterCriteria"/> from a binary expression (e.g., x => x.Property == value).
    /// </summary>
    public static FilterCriteria GetFilterInfo<T>(Expression<Func<T, bool>> whereExpr)
    {
        ArgumentNullException.ThrowIfNull(whereExpr, nameof(whereExpr));

        if (IsValidExpression(whereExpr))
            return ParseExpression(whereExpr);

        throw new ArgumentException("Invalid expression format. Only simple binary expressions are supported.", nameof(whereExpr));
    }

    /// <summary>
    /// Builds a lambda expression for this filter criteria.
    /// </summary>
    public Expression<Func<T, bool>> GetExpression<T>()
        => ExpressionBuilder.CreateWhereExpression<T>(this);

    private static bool IsValidExpression<T>(Expression<Func<T, bool>> expression)
    {
        return expression.Body is BinaryExpression binary &&
            binary.Left is MemberExpression left &&
            binary.Right is ConstantExpression &&
            left.Member is MemberInfo _;
    }

    private static FilterCriteria ParseExpression<T>(Expression<Func<T, bool>> expression)
    {
        if (expression.Body is not BinaryExpression binaryExpression)
            throw new ArgumentException("Expression body must be a binary expression.", nameof(expression));

        if (binaryExpression.Left is not MemberExpression leftExpression)
            throw new ArgumentException("Left side of expression must be a property.", nameof(expression));

        if (binaryExpression.Right is not ConstantExpression rightExpression)
            throw new ArgumentException("Right side of expression must be a constant.", nameof(expression));

        var dataType = leftExpression.Type;
        var comparedValue = rightExpression.Value;
        var propertyName = leftExpression.Member.Name;

        var comparisonOperator = binaryExpression.NodeType switch
        {
            ExpressionType.Equal => ComparisonType.EqualTo,
            ExpressionType.NotEqual => ComparisonType.NotEqualTo,
            ExpressionType.GreaterThan => ComparisonType.GreaterThan,
            ExpressionType.LessThan => ComparisonType.LessThan,
            ExpressionType.GreaterThanOrEqual => ComparisonType.GreaterThanOrEqualTo,
            ExpressionType.LessThanOrEqual => ComparisonType.LessThanOrEqualTo,
            _ => throw new ArgumentException($"Comparison operator '{binaryExpression.NodeType}' not supported.", nameof(expression)),
        };

        return new FilterCriteria(dataType, propertyName, comparedValue, comparisonOperator);
    }
}
