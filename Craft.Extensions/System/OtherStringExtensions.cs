using System.Security.Cryptography;
using System.Text;

namespace Craft.Extensions.System;

public static class OtherStringExtensions
{
    extension(string? str)
    {
        /// <summary>
        /// Normalizes line endings in a string by replacing different line ending formats
        /// (CRLF, CR, LF) with the system's default line ending (Environment.NewLine).
        /// </summary>
        /// <param name="str">The input string to normalize line endings.</param>
        /// <returns>A string with normalized line endings.</returns>
        public string? NormalizeLineEndings()
        {
            return str?
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Replace("\n", Environment.NewLine);
        }

        /// <summary>
        /// Returns the index of the Nth occurrence of a specified character in the given string.
        /// </summary>
        /// <param name="str">The input string.</param>
        /// <param name="c">The character to find.</param>
        /// <param name="n">The occurrence count (1-based).</param>
        /// <returns>
        /// The index of the Nth occurrence of the specified character if found; otherwise, -1.
        /// </returns>
        public int NthIndexOf(char c, int n)
        {
            if (string.IsNullOrEmpty(str)) return -1;

            int count = 0;

            for (int i = 0; i < str.Length; i++)
                if (str[i] == c && ++count == n)
                    return i;

            return -1;
        }

        /// <summary>
        /// Removes all occurrences of specified strings from the source string.
        /// </summary>
        /// <param name="source">The input string.</param>
        /// <param name="strings">Strings to be removed from the source string.</param>
        /// <returns>A new string with specified occurrences removed.</returns>
        public string? RemoveAll(params string[]? strings)
        {
            if (str == null || strings == null || strings.Length == 0)
                return str;

            foreach (var s in strings)
                str = str.Replace(s, string.Empty);

            return str;
        }

        /// <summary>
        /// Removes extra spaces between words and trims the input string.
        /// </summary>
        /// <param name="input">The input string to be processed.</param>
        /// <returns>A string with no more than a single space between words, trimmed.</returns>
        public string? RemoveExtraSpaces()
        {
            if (string.IsNullOrEmpty(str)) return str;

            // Split the string into words and remove empty entries
            var words = str.Split([' '], StringSplitOptions.RemoveEmptyEntries);

            // Join the words with a single space
            return string.Join(" ", words);
        }

        /// <summary>
        /// Removes specified postfixes from the end of the input string using ordinal comparison.
        /// If the input string is null or empty, or if the array of postfixes is null or empty, the original string is returned.
        /// </summary>
        /// <param name="str">The input string.</param>
        /// <param name="postFixes">An array of postfixes to remove.</param>
        /// <returns>A string with the specified postfixes removed.</returns>
        public string? RemovePostFix(params string[]? postFixes)
            => str?.RemovePostFix(StringComparison.Ordinal, postFixes);

        /// <summary>
        /// Removes specified postfixes from the end of the input string based on the provided comparison type.
        /// If the input string is null or empty, or if the array of postfixes is null or empty, the original string is returned.
        /// The comparison is case-sensitive (Ordinal) by default, but you can specify a different comparison type.
        /// </summary>
        /// <param name="str">The input string.</param>
        /// <param name="comparisonType">The StringComparison type for postfix matching.</param>
        /// <param name="postFixes">An array of postfixes to remove.</param>
        /// <returns>A string with the specified postfixes removed.</returns>
        public string? RemovePostFix(StringComparison comparisonType, params string[]? postFixes)
        {
            if (string.IsNullOrEmpty(str) || postFixes?.Length == 0)
                return str;

            foreach (string postFix in postFixes!)
                if (str.EndsWith(postFix, comparisonType))
                    return str.Left(str.Length - postFix.Length);

            return str;
        }

        /// <summary>
        /// Removes specified prefixes from the beginning of the input string using ordinal comparison.
        /// If the input string is null or empty, or if the array of prefixes is null or empty, the original string is returned.
        /// </summary>
        /// <param name="str">The input string.</param>
        /// <param name="preFixes">An array of prefixes to remove from the input string.</param>
        /// <returns>A new string with specified prefixes removed from the beginning.</returns>
        public string? RemovePreFix(params string[]? preFixes)
            => str?.RemovePreFix(StringComparison.Ordinal, preFixes);

        /// <summary>
        /// Removes specified prefixes from the beginning of the input string based on the provided comparison type.
        /// If the input string is null or empty, or if the array of prefixes is null or empty, the original string is returned.
        /// </summary>
        /// <param name="str">The input string.</param>
        /// <param name="comparisonType">The type of string comparison to use.</param>
        /// <param name="preFixes">An array of prefixes to remove from the input string.</param>
        /// <returns>A new string with specified prefixes removed from the beginning.</returns>
        public string? RemovePreFix(StringComparison comparisonType, params string[]? preFixes)
        {
            if (string.IsNullOrEmpty(str) || preFixes?.Length == 0)
                return str;

            foreach (string preFix in preFixes!)
                if (str.StartsWith(preFix, comparisonType))
                    return str.Right(str.Length - preFix.Length);

            return str;
        }

        /// <summary>
        /// Replace the first occurrence of the search string with the replacement string.
        /// </summary>
        /// <param name="str">The original string.</param>
        /// <param name="search">The string to search for.</param>
        /// <param name="replace">The string to replace the first occurrence of the search string.</param>
        /// <param name="comparisonType">Type of string comparison to use.</param>
        /// <returns>A new string with the first occurrence of the search string replaced.</returns>
        public string? ReplaceFirst(string? search, string? replace, StringComparison comparisonType = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(search) || replace is null)
                return str;

            int index = str.IndexOf(search, comparisonType);

            if (index < 0)
                return str;

            return string.Concat(str.AsSpan(0, index), replace, str.AsSpan(index + search.Length));
        }

        /// <summary>
        /// Calculates the MD5 hash of the input string.
        /// </summary>
        /// <param name="str">The input string to calculate the MD5 hash for.</param>
        /// <returns>A hexadecimal string representation of the MD5 hash.</returns>
        /// <exception cref="ArgumentException">Thrown if the input string is null or empty.</exception>
        public string ToMd5()
        {
            // Ensure the input string is not null or empty
            ArgumentNullException.ThrowIfNull(str);

            // Convert the input string to bytes
            byte[] inputBytes = Encoding.UTF8.GetBytes(str);

            // Calculate the MD5 hash
            byte[] hashBytes = MD5.HashData(inputBytes);

            // Use StringBuilder constructor with capacity
            StringBuilder sb = new(hashBytes.Length * 2);

            // Convert each byte of the hash to its hexadecimal representation
            foreach (byte hashByte in hashBytes)
                sb.Append(hashByte.ToString("X2"));

            return sb.ToString();
        }
    }
}
