#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Collections.Generic;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class DictionaryExtensions
{

    /// <summary>
    /// Gets the value associated with the specified key, or adds and returns a new value if the key does not exist.
    /// </summary>
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> valueFactory)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(dictionary);
        ArgumentNullException.ThrowIfNull(valueFactory);

        if (dictionary.TryGetValue(key, out var value))
            return value;

        value = valueFactory(key);
        dictionary.Add(key, value);

        return value;
    }

    /// <summary>
    /// Gets the value associated with the specified key, or adds and returns a default value if the key does not exist.
    /// </summary>
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(dictionary);

        if (dictionary.TryGetValue(key, out var value))
            return value;

        dictionary.Add(key, defaultValue);

        return defaultValue;
    }

    /// <summary>
    /// Adds or updates the value associated with the specified key.
    /// </summary>
    public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(dictionary);

        if (dictionary.ContainsKey(key))
            dictionary[key] = value;
        else
            dictionary.Add(key, value);
    }

    /// <summary>
    /// Merges the specified source dictionary into the target dictionary, optionally overwriting existing keys.
    /// </summary>
    public static IDictionary<TKey, TValue> Merge<TKey, TValue>(this IDictionary<TKey, TValue> target, IDictionary<TKey, TValue> source, bool overwriteExisting = true)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(source);

        foreach (var kvp in source)
        {
            if (overwriteExisting || !target.ContainsKey(kvp.Key))
                target[kvp.Key] = kvp.Value;
        }

        return target;
    }

    /// <summary>
    /// Converts a dictionary to a query string format (e.g., "key1=value1&amp;key2=value2").
    /// </summary>
    public static string ToQueryString<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(dictionary);

        if (dictionary.Count == 0)
            return string.Empty;

        return string.Join("&", dictionary.Select(kvp => $"{Uri.EscapeDataString(kvp.Key.ToString()!)}={Uri.EscapeDataString(kvp.Value?.ToString() ?? string.Empty)}"));
    }

    /// <summary>
    /// Safely removes a key from the dictionary and returns whether the key was present.
    /// </summary>
    public static bool TryRemove<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(dictionary);

        return dictionary.Remove(key);
    }

    /// <summary>
    /// Safely removes a key from the dictionary and returns the removed value if present.
    /// </summary>
    public static bool TryRemove<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, out TValue? value)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(dictionary);

        if (dictionary.TryGetValue(key, out value))
        {
            dictionary.Remove(key);
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Creates a shallow copy of the dictionary.
    /// </summary>
    public static Dictionary<TKey, TValue> Clone<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(dictionary);

        return new Dictionary<TKey, TValue>(dictionary);
    }

    /// <summary>
    /// Inverts the dictionary, swapping keys and values. If duplicate values exist, the last occurrence is kept.
    /// </summary>
    public static Dictionary<TValue, TKey> Invert<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        where TKey : notnull
        where TValue : notnull
    {
        ArgumentNullException.ThrowIfNull(dictionary);

        var inverted = new Dictionary<TValue, TKey>();

        foreach (var kvp in dictionary)
            inverted[kvp.Value] = kvp.Key;

        return inverted;
    }
}
