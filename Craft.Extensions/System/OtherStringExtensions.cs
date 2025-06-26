using System.Security.Cryptography;
using System.Text;

namespace Craft.Extensions.System;

public static class OtherStringExtensions
{
    /// <summary>
    /// Normalizes the line endings in the current string to the platform-specific line ending.
    /// </summary>
    public static string? NormalizeLineEndings(this string? str) =>
        str?
            .Replace("\r\n", "\n")
            .Replace("\r", "\n")
            .Replace("\n", Environment.NewLine);

    /// <summary>
    /// Finds the zero-based index of the nth occurrence of a specified character in the string.
    /// </summary>
    public static int NthIndexOf(this string? str, char c, int n)
    {
        if (str is null || n <= 0) return -1;

        int count = 0;

        for (int i = 0; i < str.Length; i++)
            if (str[i] == c && ++count == n)
                return i;

        return -1;
    }

    /// <summary>
    /// Removes all occurrences of the specified strings from the current string.
    /// </summary>
    public static string? RemoveAll(this string? str, params string[]? strings)
    {
        if (str is null || strings is null || strings.Length == 0)
            return str;

        var result = str;

        foreach (var s in strings)
            result = result.Replace(s, string.Empty);

        return result;
    }

    /// <summary>
    /// Removes extra spaces from the current string, leaving only single spaces between words.
    /// </summary>
    public static string? RemoveExtraSpaces(this string? str) =>
        string.IsNullOrEmpty(str)
            ? str
            : string.Join(" ", str.Split(' ', StringSplitOptions.RemoveEmptyEntries));

    /// <summary>
    /// Removes the specified postfixes from the current string, if any are present.
    /// </summary>
    public static string? RemovePostFix(this string? str, params string[]? postFixes) =>
        str.RemovePostFix(StringComparison.Ordinal, postFixes);

    /// <summary>
    /// Removes the specified postfix from the current string if it ends with any of the provided postfixes.
    /// </summary>
    public static string? RemovePostFix(this string? str, StringComparison comparisonType, params string[]? postFixes)
    {
        if (string.IsNullOrEmpty(str) || postFixes is null || postFixes.Length == 0)
            return str;

        foreach (string postFix in postFixes)
            if (str.EndsWith(postFix, comparisonType))
                return str[..^postFix.Length];

        return str;
    }

    /// <summary>
    /// Removes the specified prefixes from the current string, if any are present.
    /// </summary>
    public static string? RemovePreFix(this string? str, params string[]? preFixes) =>
        str.RemovePreFix(StringComparison.Ordinal, preFixes);

    /// <summary>
    /// Removes the first matching prefix from the current string, if any of the specified prefixes are found.
    /// </summary>
    public static string? RemovePreFix(this string? str, StringComparison comparisonType, params string[]? preFixes)
    {
        if (string.IsNullOrEmpty(str) || preFixes is null || preFixes.Length == 0)
            return str;

        foreach (string preFix in preFixes)
            if (str.StartsWith(preFix, comparisonType))
                return str[preFix.Length..];

        return str;
    }

    /// <summary>
    /// Replaces the first occurrence of a specified substring in the current string with another specified substring.
    /// </summary>
    public static string? ReplaceFirst(this string? str, string? search, string? replace, StringComparison comparisonType = StringComparison.Ordinal)
    {
        if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(search) || replace is null)
            return str;

        int index = str.IndexOf(search, comparisonType);

        if (index < 0)
            return str;

        return string.Concat(str.AsSpan(0, index), replace, str.AsSpan(index + search.Length));
    }

    /// <summary>
    /// Computes the MD5 hash of the current string and returns it as a hexadecimal string.
    /// </summary>
    public static string ToMd5(this string str)
    {
        ArgumentNullException.ThrowIfNull(str);

        byte[] inputBytes = Encoding.UTF8.GetBytes(str);
        byte[] hashBytes = MD5.HashData(inputBytes);

        StringBuilder sb = new(hashBytes.Length * 2);

        foreach (byte hashByte in hashBytes)
            sb.Append(hashByte.ToString("X2"));

        return sb.ToString();
    }

    /// <summary>
    /// Determines whether the specified string is a valid Base64-encoded string.
    /// </summary>
    /// <remarks>A valid Base64 string must contain only characters allowed in Base64 encoding  and must have
    /// a length that is a multiple of 4. This method returns <see langword="false"/>  for null, empty, or whitespace
    /// strings.</remarks>
    /// <param name="s">The string to validate as Base64-encoded.</param>
    /// <returns><see langword="true"/> if the specified string is a valid Base64-encoded string;  otherwise, <see
    /// langword="false"/>.</returns>
    public static bool IsBase64String(this string s)
    {
        if (s.IsNullOrWhiteSpace()) return false;

        Span<byte> buffer = new byte[s.Length + 1];

        return Convert.TryFromBase64String(s, buffer, out _);
    }
}
