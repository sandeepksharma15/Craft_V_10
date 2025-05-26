using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Collections.Generic;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class EnumerableExtensions
{
    /// <summary>
    /// Converts a collection of objects into a dictionary suitable for use in a select list, where the keys represent
    /// the values and the values represent the display text.
    /// </summary>
    /// <remarks>If either <paramref name="valueField"/> or <paramref name="displayField"/> is <see
    /// langword="null"/>, the method falls back to using the string representation of the objects in the collection for
    /// both the keys and values. If an object in the collection is <see langword="null"/>, an empty string is used for
    /// both the key and value.</remarks>
    /// <typeparam name="T">The type of objects in the collection.</typeparam>
    /// <param name="items">The collection of objects to convert. Can be <see langword="null"/>.</param>
    /// <param name="valueField">The name of the property to use as the dictionary key. Must correspond to a property of type <see
    /// cref="string"/> or convertible to <see cref="string"/>.</param>
    /// <param name="displayField">The name of the property to use as the dictionary value. Must correspond to a property of type <see
    /// cref="string"/> or convertible to <see cref="string"/>.</param>
    /// <returns>A dictionary where the keys are the values of the specified <paramref name="valueField"/> property and the
    /// values are the values of the specified <paramref name="displayField"/> property. If <paramref name="items"/> is
    /// <see langword="null"/>, an empty dictionary is returned.</returns>
    public static Dictionary<string, string> GetListDataForSelect<T>(this IEnumerable<T>? items, string valueField, string displayField)
    {
        Dictionary<string, string> listItems = [];

        foreach (T item in items ?? [])
        {
            if (valueField != null && displayField != null)
            {
                var strValue = item!.GetType()!.GetProperty(valueField)?.GetValue(item)?.ToString() ?? string.Empty;
                var strDisplay = item.GetType()?.GetProperty(displayField)?.GetValue(item)?.ToString() ?? string.Empty;

                listItems.Add(strValue, strDisplay);
            }
            else
            {
                if (item is not null)
                    listItems.Add(item!.ToString()!, item.ToString() ?? string.Empty);
                else
                    listItems.Add(string.Empty, string.Empty);
            }
        }

        return listItems!;
    }

    /// <summary>
    /// Check if an item is in a collection.
    /// </summary>
    /// <param name="item">Item to check</param>
    /// <param name="collection">Collection of items</param>
    /// <typeparam name="T">Type of the items</typeparam>
    public static bool IsIn<T>(this T item, IEnumerable<T> collection)
        => collection.Contains(item);
}
