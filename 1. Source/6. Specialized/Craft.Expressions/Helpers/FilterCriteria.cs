using System.Linq.Expressions;
using System.Reflection;

namespace Craft.Expressions;

public sealed record FilterCriteria
{
    public Type? PropertyType { get; }               // The type of the property being filtered.
    public string? Name { get; }                     // The property name being filtered.
    public object? Value { get; }                   // The value to compare with.
    public ComparisonType Comparison { get; }       // The type of comparison to perform.

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
            bool isNullable = !propertyType.IsValueType // Reference type
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
}
