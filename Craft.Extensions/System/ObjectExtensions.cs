using System.Globalization;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class ObjectExtensions
{
    extension (object obj) 
    {
        /// <summary>
        /// Casts the current object to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to which the object will be cast.</typeparam>
        /// <returns>The current object cast to the specified type <typeparamref name="T"/>.</returns>
        public T AsType<T>()
            => (T)obj;

        /// <summary>
        /// Converts the current object to the specified value type.
        /// </summary>
        /// <remarks>If the object is <see langword="null"/>, the method returns the default value of 
        /// <typeparamref name="T"/>. If <typeparamref name="T"/> is <see cref="System.Guid"/>,  the method attempts to
        /// parse the object as a GUID. For other types, the method uses  <see cref="System.Convert.ChangeType(object,
        /// Type, IFormatProvider)"/> to perform the conversion.</remarks>
        /// <typeparam name="T">The target value type to convert to. Must be a non-nullable value type.</typeparam>
        /// <returns>The converted value of type <typeparamref name="T"/> if the conversion is successful;  otherwise, the
        /// default value of <typeparamref name="T"/>.</returns>
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

    extension <T> (T obj)
    {
        /// <summary>
        /// Applies the specified function to the current object if the given condition is true.
        /// </summary>
        /// <param name="condition">A boolean value that determines whether the function should be applied.</param>
        /// <param name="func">A function to apply to the current object if <paramref name="condition"/> is true. Must not be null.</param>
        /// <returns>The result of applying <paramref name="func"/> to the current object if <paramref name="condition"/> is
        /// true; otherwise, the current object unchanged.</returns>
        public T If(bool condition, Func<T, T> func)
        {
            if (condition && func is not null)
                return func(obj);

            return obj;
        }

        /// <summary>
        /// Executes the specified action on the object if the given condition is true.
        /// </summary>
        /// <param name="condition">A boolean value that determines whether the action should be executed. If <see langword="true"/>, the action
        /// is invoked; otherwise, it is ignored.</param>
        /// <param name="action">The action to perform on the object. Must not be <see langword="null"/> if the condition is <see
        /// langword="true"/>.</param>
        /// <returns>The current object, allowing for method chaining.</returns>
        public T If(bool condition, Action<T> action)
        {
            if (condition && action is not null)
                action(obj);

            return obj;
        }
    }
}
