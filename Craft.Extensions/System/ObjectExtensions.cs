using System.Globalization;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class ObjectExtensions
{
    extension(object obj)
    {
        /// <summary>
        /// Casts the current object to the specified type.
        /// </summary>
        public T AsType<T>() => (T)obj;

        /// <summary>
        /// Converts the current object to the specified value type.
        /// </summary>
        public T ToValue<T>() where T : struct
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

    extension<T>(T obj)
    {
        /// <summary>
        /// Applies the specified function to the current object if the given condition is true.
        /// </summary>
        public T If(bool condition, Func<T, T> func) =>
            (condition && func is not null) ? func(obj) : obj;

        /// <summary>
        /// Executes the specified action on the object if the given condition is true.
        /// </summary>
        public T If(bool condition, Action<T> action)
        {
            if (condition && action is not null)
                action(obj);

            return obj;
        }
    }
}
