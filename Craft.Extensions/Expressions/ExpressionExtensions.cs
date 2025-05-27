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
    public static LambdaExpression CreateMemberExpression(this Type type, string memberName)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(memberName);

        // Check for property or field existence
        var member = (type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                  ?? (MemberInfo?)type.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) 
                  ?? throw new ArgumentException($"Property or field '{memberName}' not found on type '{type}'", nameof(memberName));

        var parameter = Expression.Parameter(type, "x");

        Expression memberAccess = member is PropertyInfo info
            ? Expression.Property(parameter, info)
            : Expression.Field(parameter, (FieldInfo)member);

        return Expression.Lambda(memberAccess, parameter);
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
        var type = typeof(T);
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyOrFieldName);

        var member = (type.GetProperty(propertyOrFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                  ?? (MemberInfo?)type.GetField(propertyOrFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) 
                  ?? throw new ArgumentException($"Property or field '{propertyOrFieldName}' not found on type '{type}'", nameof(propertyOrFieldName));

        var parameter = Expression.Parameter(type, "x");
        Expression memberAccess = member is PropertyInfo info
            ? Expression.Property(parameter, info)
            : Expression.Field(parameter, (FieldInfo)member);

        return Expression.Lambda<Func<T, TResult>>(memberAccess, parameter);
    }

    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var body = Expression.AndAlso(
            Expression.Invoke(expr1, parameter),
            Expression.Invoke(expr2, parameter));
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var body = Expression.OrElse(
            Expression.Invoke(expr1, parameter),
            Expression.Invoke(expr2, parameter));
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}
