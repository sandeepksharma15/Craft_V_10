using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

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

        return index < 0 ? str : string.Concat(str.AsSpan(0, index), replace, str.AsSpan(index + search.Length));
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

    /// <summary>
    /// Truncates the string to the specified maximum length.
    /// </summary>
    public static string? Truncate(this string? str, int maxLength)
    {
        if (string.IsNullOrEmpty(str) || str.Length <= maxLength)
            return str;

        return str[..maxLength];
    }

    /// <summary>
    /// Truncates the string to the specified maximum length and appends an ellipsis if truncated.
    /// </summary>
    public static string? TruncateWithEllipsis(this string? str, int maxLength, string ellipsis = "...")
    {
        if (string.IsNullOrEmpty(str) || str.Length <= maxLength)
            return str;

        return maxLength <= ellipsis.Length
            ? str[..maxLength]
            : str[..(maxLength - ellipsis.Length)] + ellipsis;
    }

    /// <summary>
    /// Converts a string to PascalCase (e.g., "hello world" -> "HelloWorld").
    /// </summary>
    public static string? ToPascalCase(this string? str)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        var words = str.Split([' ', '_', '-'], StringSplitOptions.RemoveEmptyEntries);
        var result = new StringBuilder();

        foreach (var word in words)
            if (word.Length > 0)
                result.Append(char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant());

        return result.ToString();
    }

    /// <summary>
    /// Converts a string to camelCase (e.g., "hello world" -> "helloWorld").
    /// </summary>
    public static string? ToCamelCase(this string? str)
    {
        var pascal = str.ToPascalCase();

        return string.IsNullOrEmpty(pascal)
            ? pascal
            : char.ToLowerInvariant(pascal[0]) + pascal[1..];
    }

    /// <summary>
    /// Converts a string to snake_case (e.g., "HelloWorld" -> "hello_world").
    /// </summary>
    public static string? ToSnakeCase(this string? str)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        var result = Regex.Replace(str, "([a-z0-9])([A-Z])", "$1_$2");
        return result.Replace(' ', '_').Replace('-', '_').ToLowerInvariant();
    }

    /// <summary>
    /// Converts a string to kebab-case (e.g., "HelloWorld" -> "hello-world").
    /// </summary>
    public static string? ToKebabCase(this string? str)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        var result = Regex.Replace(str, "([a-z0-9])([A-Z])", "$1-$2");
        return result.Replace(' ', '-').Replace('_', '-').ToLowerInvariant();
    }

    /// <summary>
    /// Reverses the characters in the string.
    /// </summary>
    public static string? Reverse(this string? str)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        var charArray = str.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }

    /// <summary>
    /// Counts the number of occurrences of a substring within the string.
    /// </summary>
    public static int CountOccurrences(this string? str, string substring, StringComparison comparisonType = StringComparison.Ordinal)
    {
        if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(substring))
            return 0;

        int count = 0;
        int index = 0;

        while ((index = str.IndexOf(substring, index, comparisonType)) != -1)
        {
            count++;
            index += substring.Length;
        }

        return count;
    }

    /// <summary>
    /// Determines whether the string contains only digits.
    /// </summary>
    public static bool IsNumeric(this string? str)
    {
        if (string.IsNullOrEmpty(str))
            return false;

        return str.All(char.IsDigit);
    }

    /// <summary>
    /// Determines whether the string contains only letters.
    /// </summary>
    public static bool IsAlphabetic(this string? str)
    {
        if (string.IsNullOrEmpty(str))
            return false;

        return str.All(char.IsLetter);
    }

    /// <summary>
    /// Determines whether the string contains only letters and digits.
    /// </summary>
    public static bool IsAlphanumeric(this string? str)
    {
        if (string.IsNullOrEmpty(str))
            return false;

        return str.All(char.IsLetterOrDigit);
    }
}
