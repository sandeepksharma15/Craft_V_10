using System.Globalization;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class ObjectExtensions
{
    /// <summary>
    /// Casts the object to the specified reference type.
    /// </summary>
    public static T AsType<T>(this object obj) where T : class => (T)obj;

    /// <summary>
    /// Conditionally applies a function to an object and returns the result, or the original object if the condition is false.
    /// Useful for fluent chaining.
    /// </summary>
    public static T If<T>(this T obj, bool condition, Func<T, T> func)
    {
        if (condition && func is not null)
            return func(obj);

        return obj;
    }

    /// <summary>
    /// Conditionally performs an action on an object and returns the original object.
    /// Useful for fluent chaining.
    /// </summary>
    public static T If<T>(this T obj, bool condition, Action<T> action)
    {
        if (condition && action is not null)
            action(obj);

        return obj;
    }

    /// <summary>
    /// Converts an object to a value of the specified value type <typeparamref name="T"/>.
    /// Handles special cases like converting to Guid. Returns the default value if the object is null or conversion fails.
    /// Uses culture-specific conversion for non-Guid types.
    /// </summary>
    public static T ToValue<T>(this object obj) where T : struct
    {
        if (obj is null)
            return default;

        if (typeof(T) == typeof(Guid))
        {
            if (Guid.TryParse(obj.ToString(), out var guid))
                return (T)(object)guid;

            return default;
        }

        return obj is IConvertible convertible
            ? (T)Convert.ChangeType(convertible, typeof(T), CultureInfo.CurrentCulture)
            : default;
    }
}
