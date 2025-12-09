using System.Globalization;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class StandardStringExtensions
{
    /// <summary>
    /// Ensures that the specified character is at the start of the string.
    /// </summary>
    public static string? EnsureStartsWith(this string? source, char c, StringComparison comparisonType = StringComparison.Ordinal)
        => source is null
            ? null
            : source.StartsWith(c.ToString(), comparisonType) ? source : c + source;

    /// <summary>
    /// Ensures that the specified character is at the end of the string.
    /// </summary>
    public static string? EnsureEndsWith(this string? source, char c, StringComparison comparisonType = StringComparison.Ordinal)
        => source is null
            ? null
            : source.EndsWith(c.ToString(), comparisonType) ? source : source + c;

    /// <summary>
    /// Converts the first character of the string to uppercase, while leaving the rest of the string unchanged.
    /// </summary>
    public static string? FirstCharToUpper(this string? source)
        => string.IsNullOrEmpty(source)
            ? source
            : string.Concat(source[0].ToString(CultureInfo.InvariantCulture).ToUpperInvariant(), source.AsSpan(1));

    /// <summary>
    /// Retrieves the substring that appears after the last occurrence of the specified delimiter.
    /// </summary>
    public static string? GetStringAfterLastDelimiter(this string? source, char delimiter = '.')
        => string.IsNullOrEmpty(source)
            ? source
            : source.LastIndexOf(delimiter) is int idx && idx >= 0
                ? source[(idx + 1)..]
                : source;

    /// <summary>
    /// Determines whether the source string is null, empty, or consists only of white-space characters.
    /// </summary>
    public static bool IsEmpty(this string? source) => string.IsNullOrWhiteSpace(source);

    /// <summary>
    /// Determines whether the source string is not null, empty, or consists only of white-space characters.
    /// </summary>
    public static bool IsNonEmpty(this string? source) => !string.IsNullOrWhiteSpace(source);

    /// <summary>
    /// Determines whether the source string is null or an empty string.
    /// </summary>
    public static bool IsNullOrEmpty(this string? source) => string.IsNullOrEmpty(source);

    /// <summary>
    /// Determines whether the source string is null, empty, or consists only of white-space characters.
    /// </summary>
    public static bool IsNullOrWhiteSpace(this string? source) => string.IsNullOrWhiteSpace(source);

    /// <summary>
    /// Returns a substring containing the leftmost characters of the source string, up to the specified length.
    /// </summary>
    public static string? Left(this string? source, int len) =>
        string.IsNullOrEmpty(source) || len >= source.Length
            ? source
            : source[..len];

    /// <summary>
    /// Returns the specified number of characters from the end of the string.
    /// </summary>
    public static string? Right(this string? source, int len) =>
        string.IsNullOrEmpty(source) || len >= source.Length
            ? source
            : source[^len..];
}
