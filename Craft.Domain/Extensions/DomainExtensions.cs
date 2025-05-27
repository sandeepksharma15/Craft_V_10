
#pragma warning disable IDE0130 // Namespace does not match folder structure
using System.ComponentModel;

namespace System;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class DomainExtensions
{
    /// <summary>
    /// Parses a string into the specified type. Returns default(TKey) if conversion fails.
    /// </summary>
    /// <typeparam name="TKey">The type to parse the string to.</typeparam>
    /// <param name="value">The string to parse.</param>
    /// <returns>The parsed value, or default(TKey) if parsing fails.</returns>
    public static TKey? Parse<TKey>(this string? value)
    {
        if (value is null)
            return default;

        try
        {
            // Special case: object type should return the string itself
            if (typeof(TKey) == typeof(object))
                return (TKey?)(object?)value;

            var converter = TypeDescriptor.GetConverter(typeof(TKey));

            if (converter.CanConvertFrom(typeof(string)))
            {
                var result = converter.ConvertFromInvariantString(value);
                return (TKey?)result;
            }
        }
        catch (Exception)
        {
            return default;
        }

        return default;
    }
}
