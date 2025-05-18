using System.Globalization;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class StandardStringExtensions
{
    /// <summary>
    /// Ensures that a given string ends with a specified character.
    /// If the string is null or empty, the original string is returned unchanged.
    /// </summary>
    /// <param name="source">The source string to ensure the ending of.</param>
    /// <param name="c">The character to ensure the string ends with.</param>
    /// <param name="comparisonType">The type of string comparison to use (default is Ordinal).</param>
    /// <returns>
    /// The original string if it already ends with the specified character,
    /// otherwise, a new string with the specified character appended to the end.
    /// </returns>
    public static string? EnsureEndsWith(this string source, char c, StringComparison comparisonType = StringComparison.Ordinal)
    {
        if (source is null) return source;

        return source.EndsWith(c.ToString(), comparisonType) ? source : source + c;
    }

    /// <summary>
    /// Ensures that a given string starts with a specified character.
    /// If the string is null, the original string is returned unchanged.
    /// </summary>
    /// <param name="source">The source string to ensure the starting of.</param>
    /// <param name="c">The character to ensure the string starts with.</param>
    /// <param name="comparisonType">The type of string comparison to use (default is Ordinal).</param>
    /// <returns>
    /// The original string if it already starts with the specified character,
    /// otherwise, a new string with the specified character prepended to the beginning.
    /// </returns>
    public static string? EnsureStartsWith(this string? source, char c, StringComparison comparisonType = StringComparison.Ordinal)
    {
        if (source is null) return source;

        return source.StartsWith(c.ToString(), comparisonType) ? source : c + source;
    }

    /// <summary>
    /// Converts the first character of a string to uppercase using the casing rules of the invariant culture.
    /// If the input string is null or empty, the original string is returned unchanged.
    /// </summary>
    /// <param name="source">The input string to convert.</param>
    /// <returns>
    /// A new string with the first character converted to uppercase,
    /// or the original string if it is null or empty.
    /// </returns>
    public static string FirstCharToUpper(this string source)
    {
        if (string.IsNullOrEmpty(source)) return source;

        return string.Concat(source[0]
            .ToString(CultureInfo.InvariantCulture)
            .ToUpperInvariant(), source.AsSpan(1));
    }

    /// <summary>
    /// Extension method for strings that extracts and returns the substring after the last occurrence
    /// of a specified delimiter. If the source string is null, the method returns null.
    /// </summary>
    /// <param name="source">The input string.</param>
    /// <param name="delimiter">The delimiter to identify the last occurrence (default is '.').</param>
    /// <returns>
    /// The substring after the last delimiter in the source string, or the entire string if the
    /// delimiter is not found. Returns null if the source string is null.
    /// </returns>
    public static string GetStringAfterLastDelimiter(this string source, char delimiter = '.')
    {
        if (string.IsNullOrEmpty(source)) return source;

        int lastDelimiterIndex = source.LastIndexOf(delimiter);

        return lastDelimiterIndex >= 0 ? source[(lastDelimiterIndex + 1)..] : source;
    }

    public static string GetClassName(this Type type)
        => type.ToString().GetStringAfterLastDelimiter();

    /// <summary>
    /// Checks whether the specified string is null, empty, or consists only of white-space characters.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>
    ///   <c>true</c> if the string is null, empty, or consists only of white-space characters; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsEmpty(this string value) => string.IsNullOrWhiteSpace(value);

    /// <summary>
    /// Checks whether the specified string is non-empty, meaning it is not null, empty, or consists only of white-space characters.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <returns>
    ///   <c>true</c> if the string is non-empty; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsNonEmpty(this string value) => !string.IsNullOrWhiteSpace(value);

    /// <summary>
    /// Checks whether the specified string is null or empty.
    /// </summary>
    /// <param name="str">The string to check.</param>
    /// <returns>
    ///   <c>true</c> if the string is null or empty; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsNullOrEmpty(this string str) => string.IsNullOrEmpty(str);

    /// <summary>
    /// Indicates whether a specified string is null, empty, or consists only of white-space characters.
    /// </summary>
    /// <param name="str">The string to check.</param>
    /// <returns>
    ///   <c>true</c> if the string is null, empty, or consists only of white-space characters; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsNullOrWhiteSpace(this string str) => string.IsNullOrWhiteSpace(str);

    /// <summary>
    /// Takes a string and an integer length and returns a substring from the beginning of the string
    /// with a length not exceeding the minimum of the specified length and the source string's length.
    /// </summary>
    /// <param name="source">The input string.</param>
    /// <param name="len">The maximum length of the substring to be returned.</param>
    /// <returns>A substring from the beginning of the input string.</returns>
    public static string Left(this string source, int len)
    {
        if (string.IsNullOrEmpty(source) || len >= source.Length)
            return source;

        return source[..len];
    }

    /// <summary>
    /// Parses the input string to the specified generic type.
    /// </summary>
    /// <typeparam name="TKey">The target type to parse the string into.</typeparam>
    /// <param name="value">The string value to parse.</param>
    /// <returns>
    /// The parsed value of type <typeparamref name="TKey"/> if successful;
    /// otherwise, the default value of <typeparamref name="TKey"/>.
    /// </returns>
    public static TKey? Parse<TKey>(this string? value)
    {
        try
        {
            return value != null
                ? (TKey)Convert.ChangeType(value, typeof(TKey))
                : default;
        }
        catch (Exception)
        {
            return default;
        }
    }

    /// <summary>
    /// Extracts the right substring of a given length from the source string.
    /// If the source string is null or empty, or if the specified length is greater than or equal to the source string's length,
    /// it returns the source string as is.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="len">The length of the right substring to extract.</param>
    /// <returns>The right substring of the specified length.</returns>
    public static string Right(this string source, int len)
    {
        if (string.IsNullOrEmpty(source) || len >= source.Length)
            return source;

        return source[^len..];
    }
}
