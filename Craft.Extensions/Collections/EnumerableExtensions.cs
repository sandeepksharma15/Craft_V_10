using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Collections.Generic;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class EnumerableExtensions
{
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
}
