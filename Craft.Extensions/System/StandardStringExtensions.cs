using System.Globalization;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class StandardStringExtensions
{
    extension(string? source)
    {
        /// <summary>
        /// Ensures that the specified character is at the start of the string.
        /// </summary>
        /// <param name="c">The character to ensure is at the start of the string.</param>
        /// <param name="comparisonType">The type of string comparison to use when checking if the string starts with the specified character.</param>
        /// <returns>The original string if it already starts with the specified character; otherwise, a new string with the
        /// character prepended. If the source string is <see langword="null"/>, <see langword="null"/> is returned.</returns>
        public string? EnsureStartsWith(char c, StringComparison comparisonType = StringComparison.Ordinal)
        {
            if (source is null) return source;

            return source.StartsWith(c.ToString(), comparisonType) ? source : c + source;
        }

        /// <summary>
        /// Ensures that the specified character is at the end of the string.
        /// </summary>
        /// <param name="c">The character to ensure is at the end of the string.</param>
        /// <param name="comparisonType">The type of string comparison to use when checking if the string starts with the specified character.</param>
        /// <returns>The original string if it already starts with the specified character; otherwise, a new string with the
        /// character prepended. If the source string is <see langword="null"/>, <see langword="null"/> is returned.</returns>
        public string? EnsureEndsWith(char c, StringComparison comparisonType = StringComparison.Ordinal)
        {
            if (source is null) return source;

            return source.EndsWith(c.ToString(), comparisonType) ? source : source + c;
        }

        /// <summary>
        /// Converts the first character of the string to uppercase, while leaving the rest of the string unchanged.
        /// </summary>
        /// <remarks>This method uses an invariant culture to convert the first character to uppercase. 
        /// If the string is null or empty, the method returns the input string without modification.</remarks>
        /// <returns>A new string with the first character converted to uppercase, or the original string if it is null or empty.</returns>
        public string? FirstCharToUpper()
        {
            if (string.IsNullOrEmpty(source)) return source;

            return string.Concat(source[0]
                .ToString(CultureInfo.InvariantCulture)
                .ToUpperInvariant(), source.AsSpan(1));
        }

        /// <summary>
        /// Retrieves the substring that appears after the last occurrence of the specified delimiter.
        /// </summary>
        /// <param name="delimiter">The character used as the delimiter. Defaults to <see langword="'.'" /> if not specified.</param>
        /// <returns>The substring that follows the last occurrence of the specified delimiter.  If the delimiter is not found,
        /// or if the source string is <see langword="null" /> or empty,  the original source string is returned.</returns>
        public string? GetStringAfterLastDelimiter(char delimiter = '.')
        {
            if (string.IsNullOrEmpty(source)) return source;

            int lastDelimiterIndex = source.LastIndexOf(delimiter);

            return lastDelimiterIndex >= 0 ? source[(lastDelimiterIndex + 1)..] : source;
        }

        /// <summary>
        /// Determines whether the source string is null, empty, or consists only of white-space characters.
        /// </summary>
        /// <returns><see langword="true"/> if the source string is null, empty, or consists only of white-space characters;
        /// otherwise, <see langword="false"/>.</returns>
        public bool IsEmpty() => string.IsNullOrWhiteSpace(source);

        /// <summary>
        /// Determines whether the source string is not null, empty, or consists only of white-space characters.
        /// </summary>
        /// <returns><see langword="true"/> if the source string is not null, not empty, and does not consist solely of
        /// white-space characters;  otherwise, <see langword="false"/>.</returns>
        public bool IsNonEmpty() => !string.IsNullOrWhiteSpace(source);

        /// <summary>
        /// Determines whether the source string is null or an empty string.
        /// </summary>
        /// <returns><see langword="true"/> if the source string is <see langword="null"/> or an empty string ("");  otherwise,
        /// <see langword="false"/>.</returns>
        public bool IsNullOrEmpty() => string.IsNullOrEmpty(source);

        /// <summary>
        /// Determines whether the source string is null, empty, or consists only of white-space characters.
        /// </summary>
        /// <returns><see langword="true"/> if the source string is null, empty, or consists only of white-space characters;
        /// otherwise, <see langword="false"/>.</returns>
        public bool IsNullOrWhiteSpace() => string.IsNullOrWhiteSpace(source);

        /// <summary>
        /// Returns a substring containing the leftmost characters of the source string, up to the specified length.
        /// </summary>
        /// <param name="len">The number of characters to include in the substring. Must be non-negative.</param>
        /// <returns>A substring containing the leftmost <paramref name="len"/> characters of the source string,  or the entire
        /// source string if <paramref name="len"/> is greater than or equal to the length of the source string. Returns
        /// <see langword="null"/> if the source string is <see langword="null"/>.</returns>
        public string? Left(int len)
        {
            if (string.IsNullOrEmpty(source) || len >= source.Length)
                return source;

            return source[..len];
        }

        /// <summary>
        /// Returns the specified number of characters from the end of the string.
        /// </summary>
        /// <param name="len">The number of characters to retrieve from the end of the string. Must be non-negative.</param>
        /// <returns>A string containing the last <paramref name="len"/> characters of the source string,  or the entire string
        /// if <paramref name="len"/> is greater than or equal to the string's length.  Returns <see langword="null"/>
        /// if the source string is <see langword="null"/>.</returns>
        public string? Right(int len)
        {
            if (string.IsNullOrEmpty(source) || len >= source.Length)
                return source;

            return source[^len..];
        }
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

}
