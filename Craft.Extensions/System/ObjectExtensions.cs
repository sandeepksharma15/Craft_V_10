using System.Globalization;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class ObjectExtensions
{
    public static T AsType<T>(this object obj) where T : class => (T)obj;

    /// <summary>
    /// Can be used to conditionally perform a function
    /// on an object and return the modified or the original object.
    /// It is useful for chained calls.
    /// </summary>
    /// <param name="obj">An object</param>
    /// <param name="condition">A condition</param>
    /// <param name="func">A function that is executed only if the condition is <c>true</c></param>
    /// <typeparam name="T">Type of the object</typeparam>
    /// <returns>
    /// Returns the modified object (by the <paramref name="func"/> if the <paramref name="condition"/> is <c>true</c>)
    /// or the original object if the <paramref name="condition"/> is <c>false</c>
    /// </returns>
    public static T If<T>(this T obj, bool condition, Func<T, T> func)
    {
        if (condition && func is not null)
            return func(obj);

        return obj;
    }

    /// <summary>
    /// Can be used to conditionally perform an action
    /// on an object and return the original object.
    /// It is useful for chained calls on the object.
    /// </summary>
    /// <param name="obj">An object</param>
    /// <param name="condition">A condition</param>
    /// <param name="action">An action that is executed only if the condition is <c>true</c></param>
    /// <typeparam name="T">Type of the object</typeparam>
    /// <returns>
    /// Returns the original object.
    /// </returns>
    public static T If<T>(this T obj, bool condition, Action<T> action)
    {
        if (condition && action is not null)
            action(obj);

        return obj;
    }

    /// <summary>
    /// Converts an object to a value of the specified type <typeparamref name="T"/>.
    /// Handles special cases like converting to GUID. Returns the default value if the object is null.
    /// Uses culture-specific conversion for non-GUID types.
    /// </summary>
    /// <typeparam name="T">The target value type.</typeparam>
    /// <param name="obj">The object to be converted.</param>
    /// <returns>The converted value of type <typeparamref name="T"/>.</returns>
    public static T ToValue<T>(this object obj) where T : struct
    {
        if (obj is null)
            return default;

        if (typeof(T) == typeof(Guid) && Guid.TryParse(obj.ToString(), out Guid guid))
            return (T)(ValueType)guid;

        return obj is IConvertible convertible
            ? (T)Convert.ChangeType(convertible, typeof(T), CultureInfo.CurrentCulture)
            : default;
    }
}
