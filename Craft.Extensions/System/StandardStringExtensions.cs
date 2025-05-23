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
        public string? EnsureStartsWith(char c, StringComparison comparisonType = StringComparison.Ordinal)
            => source is null
                ? null
                : source.StartsWith(c.ToString(), comparisonType) ? source : c + source;

        /// <summary>
        /// Ensures that the specified character is at the end of the string.
        /// </summary>
        public string? EnsureEndsWith(char c, StringComparison comparisonType = StringComparison.Ordinal)
            => source is null
                ? null
                : source.EndsWith(c.ToString(), comparisonType) ? source : source + c;

        /// <summary>
        /// Converts the first character of the string to uppercase, while leaving the rest of the string unchanged.
        /// </summary>
        public string? FirstCharToUpper()
        {
            if (string.IsNullOrEmpty(source)) return source;

            return string.Concat(source[0].ToString(CultureInfo.InvariantCulture).ToUpperInvariant(),
                source.AsSpan(1));
        }

        /// <summary>
        /// Retrieves the substring that appears after the last occurrence of the specified delimiter.
        /// </summary>
        public string? GetStringAfterLastDelimiter(char delimiter = '.')
        {
            if (string.IsNullOrEmpty(source)) return source;

            int lastDelimiterIndex = source.LastIndexOf(delimiter);

            return lastDelimiterIndex >= 0 ? source[(lastDelimiterIndex + 1)..] : source;
        }

        /// <summary>
        /// Determines whether the source string is null, empty, or consists only of white-space characters.
        /// </summary>
        public bool IsEmpty() => string.IsNullOrWhiteSpace(source);

        /// <summary>
        /// Determines whether the source string is not null, empty, or consists only of white-space characters.
        /// </summary>
        public bool IsNonEmpty() => !string.IsNullOrWhiteSpace(source);

        /// <summary>
        /// Determines whether the source string is null or an empty string.
        /// </summary>
        public bool IsNullOrEmpty() => string.IsNullOrEmpty(source);

        /// <summary>
        /// Determines whether the source string is null, empty, or consists only of white-space characters.
        /// </summary>
        public bool IsNullOrWhiteSpace() => string.IsNullOrWhiteSpace(source);

        /// <summary>
        /// Returns a substring containing the leftmost characters of the source string, up to the specified length.
        /// </summary>
        public string? Left(int len)
        {
            if (string.IsNullOrEmpty(source) || len >= source.Length)
                return source;

            return source[..len];
        }

        /// <summary>
        /// Returns the specified number of characters from the end of the string.
        /// </summary>
        public string? Right(int len)
        {
            if (string.IsNullOrEmpty(source) || len >= source.Length)
                return source;

            return source[^len..];
        }
    }
}
