using System.ComponentModel;
using System.Linq.Expressions;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Reflection;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class ReflectionExtensions
{
    /// <summary>
    /// Gets a PropertyDescriptor for a specified member by name within the given Type.
    /// Supports nested properties using dot notation (e.g., "NestedClass.Property").
    /// </summary>
    public static PropertyDescriptor? GetMemberByName(this Type type, string memberName)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(memberName);

        var members = TypeDescriptor.GetProperties(type);

        if (!memberName.Contains('.'))
            return members.Find(memberName, true);

        var memberNameParts = memberName.Split('.', 2);
        var topLevelMember = members.Find(memberNameParts[0], true);

        return topLevelMember?.GetChildProperties()?.Find(memberNameParts[1], true);
    }

    /// <summary>
    /// Retrieves the name of a property from a lambda expression.
    /// </summary>
    public static string GetMemberName<TSource, TProperty>(this Expression<Func<TSource, TProperty>> property)
        => property.GetPropertyInfo().Name;

    /// <summary>
    /// Gets the underlying type of the member represented by the lambda expression.
    /// </summary>
    public static Type? GetMemberType<TSource, TProperty>(this Expression<Func<TSource, TProperty>> property)
    {
        var type = property.GetPropertyInfo().GetMemberUnderlyingType();

        return Nullable.GetUnderlyingType(type!) ?? type;
    }

    /// <summary>
    /// Gets the <see cref="PropertyInfo"/> of a specified property by name from the given <see cref="Type"/>.
    /// Throws an <see cref="ArgumentException"/> if the property is not found.
    /// </summary>
    public static PropertyInfo GetPropertyInfo(this Type objType, string name)
    {
        ArgumentNullException.ThrowIfNull(objType);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var property = objType.GetProperty(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        return property ?? throw new ArgumentException($"Property '{name}' not found in type '{objType.FullName}'", nameof(name));
    }

    /// <summary>
    /// Gets the <see cref="PropertyInfo"/> from a property access expression represented by the given lambda expression.
    /// Throws an <see cref="ArgumentException"/> if the expression is not a valid property access expression.
    /// </summary>
    public static PropertyInfo GetPropertyInfo<TSource, TProperty>(this Expression<Func<TSource, TProperty>> expression)
        => (PropertyInfo)GetMemberExpression(expression.Body).Member;

    /// <summary>
    /// Gets the <see cref="PropertyInfo"/> from a property access expression represented by the given lambda expression.
    /// Throws an <see cref="ArgumentException"/> if the expression is not a valid property access expression.
    /// </summary>
    public static PropertyInfo GetPropertyInfo<T>(this Expression<Func<T, object>> expression)
        => (PropertyInfo)GetMemberExpression(expression.Body).Member;

    /// <summary>
    /// Retrieves the PropertyInfo object from a lambda expression representing a property access.
    /// </summary>
    public static PropertyInfo GetPropertyInfo(this LambdaExpression expression)
        => (PropertyInfo)GetMemberExpression(expression.Body).Member;

    /// <summary>
    /// Deep clones an object using reflection. Only public instance properties are cloned.
    /// </summary>
    public static T GetClone<T>(this T input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var type = typeof(T);
        var visited = new Dictionary<object, object>(ReferenceEqualityComparer.Instance);

        return (T)CloneObject(input!, type, visited);
    }

    /// <summary>
    /// Retrieves all properties declared on the specified type and its base types.
    /// </summary>
    /// <remarks>This method retrieves both public and non-public instance properties, including those
    /// declared only on the specified type and its base types. Properties are returned in the order they are
    /// encountered while traversing the type hierarchy.</remarks>
    /// <param name="type">The type whose properties are to be retrieved. This parameter cannot be <see langword="null"/>.</param>
    /// <returns>A list of <see cref="PropertyInfo"/> objects representing all properties declared on the specified type and its
    /// base types.</returns>
    public static List<PropertyInfo> GetAllProperties(this Type? type)
    {
        ArgumentNullException.ThrowIfNull(type);

        var properties = new List<PropertyInfo>();
        while (type != null)
        {
            properties.AddRange(
                type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            );

            type = type.BaseType;
        }

        return properties;
    }

    /// <summary>
    /// Sets the value of a specified property on the given object.
    /// </summary>
    /// <remarks>This method uses reflection to locate and set the value of the specified property.  Both
    /// public and non-public instance properties are considered. The property must be writable.</remarks>
    /// <param name="obj">The object whose property value is to be set. Cannot be <see langword="null"/>.</param>
    /// <param name="propertyName">The name of the property to set. This is case-sensitive and cannot be <see langword="null"/> or empty.</param>
    /// <param name="value">The value to assign to the specified property. The value must be compatible with the property's type.</param>
    /// <exception cref="ArgumentException">Thrown if the specified property is not found, is not writable, or <paramref name="propertyName"/> is empty.</exception>
    public static void SetPropertyValue(this object obj, string propertyName, object value)
    {
        var prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

        if (prop == null || !prop.CanWrite)
            throw new ArgumentException($"Property '{propertyName}' not found or not writable.");

        prop.SetValue(obj, value);
    }

    public static object? GetPropertyValue(this object obj, string propertyName)
    {
        var prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

        return prop == null ? throw new ArgumentException($"Property '{propertyName}' not found.") : prop.GetValue(obj);
    }

    // --- Private helpers ---

    private static MemberExpression GetMemberExpression(Expression body)
    {
        if (body is MemberExpression memberExpr)
            return memberExpr;

        if (body is UnaryExpression unary && unary.Operand is MemberExpression member)
            return member;

        throw new ArgumentException("Invalid expression. Expected a property access expression.");
    }

    private static object CloneObject(object input, Type type, Dictionary<object, object> visited)
    {
        if (visited.TryGetValue(input, out var existing))
            return existing;

        var clonedObj = Activator.CreateInstance(type)!;
        visited.Add(input, clonedObj);

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!property.CanWrite) continue;

            var value = property.GetValue(input);
            var propertyType = property.PropertyType;

            if (value != null && propertyType.IsClass && !propertyType.FullName!.StartsWith("System.", StringComparison.Ordinal))
            {
                var clonedValue = CloneObject(value, propertyType, visited);
                property.SetValue(clonedObj, clonedValue);
            }
            else
            {
                property.SetValue(clonedObj, value);
            }
        }

        return clonedObj;
    }

    // --- Optional: Add a reference equality comparer for visited dictionary ---
    private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        public static ReferenceEqualityComparer Instance { get; } = new();
        public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);
        public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
    }
}
