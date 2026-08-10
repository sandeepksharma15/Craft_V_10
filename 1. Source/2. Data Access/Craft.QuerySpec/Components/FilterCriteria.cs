using Craft.Core;
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
    /// Optional display title for UI components. Defaults to the property name if not provided.
    /// </summary>
    public string DisplayTitle { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="FilterCriteria"/>.
    /// </summary>
    /// <param name="propertyType">The type of the property.</param>
    /// <param name="name">The property name.</param>
    /// <param name="value">The value to compare with.</param>
    /// <param name="comparison">The comparison type.</param>
    /// <param name="displayTitle">Optional display title for UI components. If null, uses the property name.</param>
    /// <exception cref="ArgumentNullException">Thrown if any required argument is null.</exception>
    public FilterCriteria(Type propertyType, string name, object? value, ComparisonType comparison = ComparisonType.EqualTo, string? displayTitle = null)
    {
        ArgumentNullException.ThrowIfNull(propertyType, nameof(propertyType));
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        // Check if value is null and propertyType is not nullable
        if (value is null)
        {
            bool isNullable = !propertyType.IsValueType // Reference type
                || (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>));

            if (!isNullable)
                throw new ArgumentException($"Value cannot be null for non-nullable type '{propertyType.FullName}'.", nameof(value));
        }

        PropertyType = propertyType;
        Name = name;
        Value = value;
        Comparison = comparison;
        DisplayTitle = displayTitle ?? name;
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

        return TryParseExpression(whereExpr, out var criteria)
            ? criteria
            : throw new ArgumentException("Invalid expression format. Only common transport-safe expressions are supported.", nameof(whereExpr));
    }

    /// <summary>
    /// Builds a lambda expression for this filter criteria.
    /// </summary>
    public Expression<Func<T, bool>> GetExpression<T>()
        => ExpressionBuilder.CreateWhereExpression<T>(this);

    private static bool TryParseExpression<T>(Expression<Func<T, bool>> expression, out FilterCriteria criteria)
    {
        ArgumentNullException.ThrowIfNull(expression);

        if (TryParseBinaryExpression(expression.Body, out criteria))
            return true;

        if (TryParseStringMethodExpression(expression.Body, out criteria))
            return true;

        if (TryParseBooleanMemberExpression(expression.Body, out criteria))
            return true;

        criteria = null!;
        return false;
    }

    private static FilterCriteria ParseExpression<T>(Expression<Func<T, bool>> expression)
    {
        return TryParseExpression(expression, out var criteria)
            ? criteria
            : throw new ArgumentException("Invalid expression format. Only common transport-safe expressions are supported.", nameof(expression));
    }

    private static bool TryParseBinaryExpression(Expression expression, out FilterCriteria criteria)
    {
        criteria = null!;

        if (expression is not BinaryExpression binaryExpression)
            return false;

        if (TryGetMemberExpression(binaryExpression.Left, out var leftExpression) &&
            TryEvaluateValue(binaryExpression.Right, out var rightValue))
        {
            criteria = CreateFilterCriteria(leftExpression, rightValue, GetComparisonType(binaryExpression.NodeType));
            return true;
        }

        if (TryGetMemberExpression(binaryExpression.Right, out var rightMemberExpression) &&
            TryEvaluateValue(binaryExpression.Left, out var leftValue))
        {
            criteria = CreateFilterCriteria(rightMemberExpression, leftValue, GetComparisonType(InvertBinaryOperator(binaryExpression.NodeType)));
            return true;
        }

        return false;
    }

    private static bool TryParseStringMethodExpression(Expression expression, out FilterCriteria criteria)
    {
        criteria = null!;

        if (expression is not MethodCallExpression methodCallExpression ||
            methodCallExpression.Object is null ||
            methodCallExpression.Arguments.Count != 1 ||
            !TryGetMemberExpression(methodCallExpression.Object, out var memberExpression) ||
            memberExpression.Type != typeof(string) ||
            !TryEvaluateValue(methodCallExpression.Arguments[0], out var argumentValue))
            return false;

        var comparison = methodCallExpression.Method.Name switch
        {
            nameof(string.Contains) => ComparisonType.Contains,
            nameof(string.StartsWith) => ComparisonType.StartsWith,
            nameof(string.EndsWith) => ComparisonType.EndsWith,
            _ => throw new ArgumentException($"Method '{methodCallExpression.Method.Name}' is not supported for transport-safe filter metadata.", nameof(expression)),
        };

        criteria = CreateFilterCriteria(memberExpression, argumentValue, comparison);
        return true;
    }

    private static bool TryParseBooleanMemberExpression(Expression expression, out FilterCriteria criteria)
    {
        criteria = null!;

        if (TryGetMemberExpression(expression, out var memberExpression) && IsBooleanType(memberExpression.Type))
        {
            criteria = CreateFilterCriteria(memberExpression, true, ComparisonType.EqualTo);
            return true;
        }

        if (expression is UnaryExpression { NodeType: ExpressionType.Not } unaryExpression &&
            TryGetMemberExpression(unaryExpression.Operand, out memberExpression) &&
            IsBooleanType(memberExpression.Type))
        {
            criteria = CreateFilterCriteria(memberExpression, false, ComparisonType.EqualTo);
            return true;
        }

        return false;
    }

    private static FilterCriteria CreateFilterCriteria(MemberExpression memberExpression, object? comparedValue, ComparisonType comparisonType)
    {
        ArgumentNullException.ThrowIfNull(memberExpression);

        var dataType = memberExpression.Type;

        if (Nullable.GetUnderlyingType(dataType) is { } underlyingType)
            dataType = underlyingType;

        if (dataType.IsEnum)
        {
            dataType = typeof(int);

            if (comparedValue is not null)
                comparedValue = Convert.ToInt32(comparedValue);
        }

        return new FilterCriteria(dataType, GetMemberPath(memberExpression), comparedValue, comparisonType);
    }

    private static string GetMemberPath(MemberExpression memberExpression)
    {
        ArgumentNullException.ThrowIfNull(memberExpression);

        var memberNames = new Stack<string>();
        Expression? currentExpression = memberExpression;

        while (currentExpression is MemberExpression currentMemberExpression)
        {
            memberNames.Push(currentMemberExpression.Member.Name);
            currentExpression = StripConvert(currentMemberExpression.Expression);
        }

        return currentExpression is ParameterExpression
            ? string.Join('.', memberNames)
            : throw new ArgumentException("Expression must target an entity member.", nameof(memberExpression));
    }

    private static ComparisonType GetComparisonType(ExpressionType expressionType)
        => expressionType switch
        {
            ExpressionType.Equal => ComparisonType.EqualTo,
            ExpressionType.NotEqual => ComparisonType.NotEqualTo,
            ExpressionType.GreaterThan => ComparisonType.GreaterThan,
            ExpressionType.LessThan => ComparisonType.LessThan,
            ExpressionType.GreaterThanOrEqual => ComparisonType.GreaterThanOrEqualTo,
            ExpressionType.LessThanOrEqual => ComparisonType.LessThanOrEqualTo,
            _ => throw new ArgumentException($"Comparison operator '{expressionType}' not supported.", nameof(expressionType)),
        };

    private static ExpressionType InvertBinaryOperator(ExpressionType expressionType)
        => expressionType switch
        {
            ExpressionType.Equal => ExpressionType.Equal,
            ExpressionType.NotEqual => ExpressionType.NotEqual,
            ExpressionType.GreaterThan => ExpressionType.LessThan,
            ExpressionType.GreaterThanOrEqual => ExpressionType.LessThanOrEqual,
            ExpressionType.LessThan => ExpressionType.GreaterThan,
            ExpressionType.LessThanOrEqual => ExpressionType.GreaterThanOrEqual,
            _ => throw new ArgumentException($"Comparison operator '{expressionType}' not supported.", nameof(expressionType)),
        };

    private static bool TryGetMemberExpression(Expression expression, out MemberExpression memberExpression)
    {
        var strippedExpression = StripConvert(expression);
        memberExpression = strippedExpression as MemberExpression ?? null!;

        if (memberExpression is null)
            return false;

        var currentExpression = StripConvert(memberExpression.Expression);

        while (currentExpression is MemberExpression currentMemberExpression)
            currentExpression = StripConvert(currentMemberExpression.Expression);

        return currentExpression is ParameterExpression;
    }

    private static bool TryEvaluateValue(Expression expression, out object? value)
    {
        var strippedExpression = StripConvert(expression);

        if (ContainsParameterReference(strippedExpression))
        {
            value = null;
            return false;
        }

        try
        {
            var boxedExpression = Expression.Convert(strippedExpression, typeof(object));
            value = Expression.Lambda<Func<object?>>(boxedExpression).Compile().Invoke();
            return true;
        }
        catch (Exception)
        {
            value = null;
            return false;
        }
    }

    private static Expression StripConvert(Expression? expression)
    {
        while (expression is UnaryExpression unaryExpression &&
               (unaryExpression.NodeType == ExpressionType.Convert || unaryExpression.NodeType == ExpressionType.ConvertChecked))
        {
            expression = unaryExpression.Operand;
        }

        return expression ?? throw new ArgumentNullException(nameof(expression));
    }

    private static bool ContainsParameterReference(Expression expression)
    {
        var visitor = new ParameterReferenceVisitor();
        visitor.Visit(expression);
        return visitor.ContainsParameterReference;
    }

    private static bool IsBooleanType(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return type == typeof(bool) || type == typeof(bool?);
    }

    private sealed class ParameterReferenceVisitor : ExpressionVisitor
    {
        public bool ContainsParameterReference { get; private set; }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            ContainsParameterReference = true;
            return base.VisitParameter(node);
        }
    }
}

