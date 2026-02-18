using System.Linq.Expressions;
using System.Reflection;

namespace Craft.Extensions.Expressions;

public static class ExpressionExtensions
{
    /// <summary>
    /// Creates a LambdaExpression for accessing a specified property or field of the given type.
    /// </summary>
    /// <typeparam name="T">The type containing the property or field.</typeparam>
    /// <param name="propertyOrFieldName">The name of the property or field.</param>
    /// <returns>A LambdaExpression representing access to the specified member.</returns>
    /// <exception cref="ArgumentException">Thrown if the property or field name is null, empty, or not found.</exception>
    public static LambdaExpression CreateMemberExpression<T>(this string propertyOrFieldName)
        => typeof(T).CreateMemberExpression(propertyOrFieldName);

    /// <summary>
    /// Creates a LambdaExpression for accessing a specified property or field of the given type.
    /// </summary>
    /// <param name="type">The type containing the property or field.</param>
    /// <param name="memberName">The name of the property or field.</param>
    /// <returns>A LambdaExpression representing access to the specified member.</returns>
    /// <exception cref="ArgumentException">Thrown if the member name is null, empty, or not found.</exception>
    public static LambdaExpression CreateMemberExpression(this Type? type, string? memberName)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(memberName);

        // Try to get property or field (static or instance)
        var member = (MemberInfo?)type.GetProperty(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
                  ?? type.GetField(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
                  ?? throw new ArgumentException($"Property or field '{memberName}' not found on type '{type.FullName}'.", nameof(memberName));

        bool isStatic = member is PropertyInfo pi ? (pi.GetMethod?.IsStatic ?? pi.SetMethod?.IsStatic ?? false)
                      : member is FieldInfo fi && fi.IsStatic;

        ParameterExpression? parameter = isStatic ? null : Expression.Parameter(type, "x");
        Expression memberAccess = member switch
        {
            PropertyInfo prop => Expression.Property(isStatic ? null : parameter, prop),
            FieldInfo field => Expression.Field(isStatic ? null : parameter, field),
            _ => throw new InvalidOperationException("Member must be a property or field.")
        };

        return Expression.Lambda(memberAccess, parameter != null ? [parameter] : []);
    }

    /// <summary>
    /// Creates a strongly-typed LambdaExpression for accessing a specified property or field of the given type.
    /// </summary>
    /// <typeparam name="T">The type containing the property or field.</typeparam>
    /// <typeparam name="TResult">The type of the property or field.</typeparam>
    /// <param name="propertyOrFieldName">The name of the property or field.</param>
    /// <returns>An Expression&lt;Func&lt;T, TResult&gt;&gt; representing access to the specified member.</returns>
    /// <exception cref="ArgumentException">Thrown if the property or field name is null, empty, or not found.</exception>
    public static Expression<Func<T, TResult>> CreateMemberExpression<T, TResult>(this string propertyOrFieldName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyOrFieldName);

        var type = typeof(T);

        // Only instance members are supported here (no static members)
        var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        var member = type.GetProperty(propertyOrFieldName, bindingFlags) as MemberInfo
                  ?? type.GetField(propertyOrFieldName, bindingFlags) as MemberInfo
                  ?? throw new ArgumentException($"Property or field '{propertyOrFieldName}' not found on type '{type.FullName}'.", nameof(propertyOrFieldName));

        var parameter = Expression.Parameter(type, "x");

        Expression memberAccess = member switch
        {
            PropertyInfo prop => Expression.Property(parameter, prop),
            FieldInfo field => Expression.Field(parameter, field),
            _ => throw new InvalidOperationException("Only properties and fields are supported.")
        };

        // Ensure the member type is assignable to TResult
        var memberType = member switch
        {
            PropertyInfo prop => prop.PropertyType,
            FieldInfo field => field.FieldType,
            _ => throw new InvalidOperationException()
        };

        return !typeof(TResult).IsAssignableFrom(memberType)
            ? throw new InvalidOperationException(
                $"Member '{propertyOrFieldName}' type '{memberType}' cannot be assigned to '{typeof(TResult)}'.")
            : Expression.Lambda<Func<T, TResult>>(memberAccess, parameter);
    }

    /// <summary>
    /// Creates a strongly-typed LambdaExpression for accessing a static property or field.
    /// </summary>
    /// <typeparam name="TResult">The return type of the static member.</typeparam>
    /// <param name="type">The type that declares the static member.</param>
    /// <param name="memberName">The name of the static property or field.</param>
    /// <returns>An Expression&lt;Func&lt;TResult&gt;&gt; representing access to the static member.</returns>
    /// <exception cref="ArgumentNullException">Thrown if type or memberName is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the member is not found or not static.</exception>
    public static Expression<Func<TResult>> CreateStaticMemberExpression<TResult>(this Type type, string memberName)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(memberName);

        const BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        var member = (MemberInfo?)type.GetProperty(memberName, bindingFlags)
                  ?? type.GetField(memberName, bindingFlags)
                  ?? throw new ArgumentException($"Static property or field '{memberName}' not found on type '{type.FullName}'.", nameof(memberName));

        Expression memberAccess = member switch
        {
            PropertyInfo prop when prop.GetMethod?.IsStatic == true =>
                Expression.Property(null, prop),
            FieldInfo field when field.IsStatic =>
                Expression.Field(null, field),
            _ => throw new ArgumentException($"Member '{memberName}' on type '{type.FullName}' is not static.", nameof(memberName))
        };

        var memberType = member switch
        {
            PropertyInfo prop => prop.PropertyType,
            FieldInfo field => field.FieldType,
            _ => throw new InvalidOperationException()
        };

        return !typeof(TResult).IsAssignableFrom(memberType)
            ? throw new InvalidOperationException(
                $"Member '{memberName}' type '{memberType}' cannot be assigned to '{typeof(TResult)}'.")
            : Expression.Lambda<Func<TResult>>(memberAccess);
    }

    /// <summary>
    /// Combines two boolean expressions into a single expression that evaluates to <see langword="true"/> only if both
    /// expressions evaluate to <see langword="true"/>.
    /// </summary>
    /// <remarks>This method creates a new expression by invoking both input expressions with the same
    /// parameter and combining their results using a logical AND operation. The resulting expression can be used in
    /// LINQ queries or other scenarios where expressions are required.</remarks>
    /// <typeparam name="T">The type of the parameter in the expressions.</typeparam>
    /// <param name="expr1">The first boolean expression to combine.</param>
    /// <param name="expr2">The second boolean expression to combine.</param>
    /// <returns>A new expression that represents the logical AND of <paramref name="expr1"/> and <paramref name="expr2"/>.</returns>
    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2)
    {
        var parameter = Expression.Parameter(typeof(T), "x");

        var body = Expression.AndAlso(
            Expression.Invoke(expr1, parameter),
            Expression.Invoke(expr2, parameter));

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    /// <summary>
    /// Combines two boolean expressions into a single expression using a logical OR operation.
    /// </summary>
    /// <remarks>The resulting expression evaluates to <see langword="true"/> if either <paramref
    /// name="expr1"/> or <paramref name="expr2"/> evaluates to <see langword="true"/> for a given input.</remarks>
    /// <typeparam name="T">The type of the parameter in the expressions.</typeparam>
    /// <param name="expr1">The first boolean expression to combine. Cannot be <see langword="null"/>.</param>
    /// <param name="expr2">The second boolean expression to combine. Cannot be <see langword="null"/>.</param>
    /// <returns>A new expression that represents the logical OR of <paramref name="expr1"/> and <paramref name="expr2"/>.</returns>
    public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2)
    {
        var parameter = Expression.Parameter(typeof(T), "x");

        var body = Expression.OrElse(
            Expression.Invoke(expr1, parameter),
            Expression.Invoke(expr2, parameter));

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    /// <summary>
    /// Gets the full property path from a lambda expression, including navigation properties.
    /// </summary>
    /// <remarks>
    /// This method handles simple properties (e.g., x => x.Name returns "Name"),
    /// navigation properties (e.g., x => x.Location.Name returns "Location.Name"),
    /// null-forgiving operators (e.g., x => x.Location!.Name), and
    /// null-conditional operators (e.g., x => x.Location?.Name).
    /// </remarks>
    /// <typeparam name="T">The type of the parameter in the expression.</typeparam>
    /// <param name="expression">The lambda expression representing property access.</param>
    /// <returns>The full dot-separated property path.</returns>
    /// <exception cref="ArgumentNullException">Thrown if expression is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the expression is not a valid property access expression.</exception>
    public static string GetFullPropertyPath<T>(this Expression<Func<T, object>> expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        return GetFullPropertyPathFromExpression(expression.Body);
    }

    /// <summary>
    /// Gets the full property path from a lambda expression, including navigation properties.
    /// </summary>
    /// <remarks>
    /// This method handles simple properties (e.g., x => x.Name returns "Name"),
    /// navigation properties (e.g., x => x.Location.Name returns "Location.Name"),
    /// null-forgiving operators (e.g., x => x.Location!.Name), and
    /// null-conditional operators (e.g., x => x.Location?.Name).
    /// </remarks>
    /// <param name="expression">The lambda expression representing property access.</param>
    /// <returns>The full dot-separated property path.</returns>
    /// <exception cref="ArgumentNullException">Thrown if expression is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the expression is not a valid property access expression.</exception>
    public static string GetFullPropertyPath(this LambdaExpression expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        return GetFullPropertyPathFromExpression(expression.Body);
    }

    /// <summary>
    /// Gets the final (leaf) property name from a lambda expression.
    /// </summary>
    /// <remarks>
    /// For navigation properties like x => x.Location.Name, this returns only "Name".
    /// Use <see cref="GetFullPropertyPath{T}"/> if you need the full path.
    /// </remarks>
    /// <typeparam name="T">The type of the parameter in the expression.</typeparam>
    /// <param name="expression">The lambda expression representing property access.</param>
    /// <returns>The final property name in the expression chain.</returns>
    /// <exception cref="ArgumentNullException">Thrown if expression is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the expression is not a valid property access expression.</exception>
    public static string GetFinalPropertyName<T>(this Expression<Func<T, object>> expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        var path = GetFullPropertyPathFromExpression(expression.Body);
        var lastDotIndex = path.LastIndexOf('.');

        return lastDotIndex >= 0 ? path[(lastDotIndex + 1)..] : path;
    }

    private static string GetFullPropertyPathFromExpression(Expression expression)
    {
        var parts = new List<string>();
        var current = expression;

        while (current is not null)
        {
            switch (current)
            {
                case UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } unary:
                    // Handle boxing conversion (x => (object)x.Property)
                    current = unary.Operand;
                    break;

                case MemberExpression member:
                    parts.Add(member.Member.Name);
                    current = member.Expression;
                    break;

                case MethodCallExpression methodCall when IsNullConditionalAccess(methodCall):
                    // Handle null-conditional access (x?.Property compiles to method call)
                    var memberArg = GetMemberFromNullConditional(methodCall);

                    if (memberArg is not null)
                    {
                        parts.Add(memberArg.Member.Name);
                        current = memberArg.Expression;
                    }
                    else
                    {
                        current = null;
                    }
                    break;

                case ParameterExpression:
                    // Reached the root parameter, stop
                    current = null;
                    break;

                default:
                    throw new ArgumentException(
                        $"Expression type '{current.GetType().Name}' is not supported for property path extraction. " +
                        "Expected a property or field access expression.",
                        nameof(expression));
            }
        }

        if (parts.Count == 0)
            throw new ArgumentException("Could not extract property path from expression.", nameof(expression));

        parts.Reverse();

        return string.Join(".", parts);
    }

    private static bool IsNullConditionalAccess(MethodCallExpression methodCall)
    {
        // Null-conditional member access compiles to a call to a special method
        // Check if this looks like a null-conditional pattern
        return methodCall.Method.Name == "get_Item" ||
               (methodCall.Object is null && methodCall.Arguments.Count > 0);
    }

    private static MemberExpression? GetMemberFromNullConditional(MethodCallExpression methodCall)
    {
        // Try to extract the member expression from null-conditional access patterns
        if (methodCall.Arguments.Count > 0 && methodCall.Arguments[0] is MemberExpression member)
            return member;

        return null;
    }
}
