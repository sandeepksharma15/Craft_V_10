using System.Security.Cryptography;
using System.Text;

namespace Craft.Extensions.System;

public static class OtherStringExtensions
{
    extension(string? str)
    {
        /// <summary>
        /// Normalizes the line endings in the current string to the platform-specific line ending.
        /// </summary>
        /// <remarks>This method replaces all occurrences of "\r\n", "\r", and "\n" in the string with the
        /// value of <see cref="Environment.NewLine"/>. It is useful for ensuring consistent line endings across
        /// different platforms.</remarks>
        /// <returns>A new string with all line endings replaced by the platform-specific line ending defined by <see
        /// cref="Environment.NewLine"/>, or <see langword="null"/> if the current string is <see langword="null"/>.</returns>
        public string? NormalizeLineEndings()
        {
            return str?
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Replace("\n", Environment.NewLine);
        }

        /// <summary>
        /// Finds the zero-based index of the nth occurrence of a specified character in the string.
        /// </summary>
        /// <remarks>This method searches the string sequentially from the beginning. If the specified
        /// character  does not appear at least n times, the method returns -1.</remarks>
        /// <param name="c">The character to locate in the string.</param>
        /// <param name="n">The occurrence number of the character to find. Must be greater than 0.</param>
        /// <returns>The zero-based index of the nth occurrence of the specified character, or -1 if the character  does not
        /// occur n times in the string or if the string is null.</returns>
        public int NthIndexOf(char c, int n)
        {
            if (str is null || n <= 0) return -1;

            int count = 0;

            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == c)
                {
                    count++;

                    if (count == n)
                        return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Removes all occurrences of the specified strings from the current string.
        /// </summary>
        /// <remarks>This method performs a case-sensitive replacement for each string in the <paramref
        /// name="strings"/> array. If the current string is null, the method returns null.</remarks>
        /// <param name="strings">An array of strings to remove from the current string. Can be null or empty.</param>
        /// <returns>The modified string with all specified strings removed.  If <paramref name="strings"/> is null, empty, or
        /// contains no elements, the original string is returned.</returns>
        public string? RemoveAll(params string[]? strings)
        {
            if (str == null || strings is null || strings?.Length == 0)
                return str;

            var result = str;

            foreach (var s in strings!)
                result = result.Replace(s, string.Empty);

            return result;
        }

        /// <summary>
        /// Removes extra spaces from the current string, leaving only single spaces between words.
        /// </summary>
        /// <remarks>This method trims leading and trailing spaces and ensures that words within the
        /// string are separated by a single space. If the string is null or empty, it returns the original
        /// string.</remarks>
        /// <returns>A new string with extra spaces removed, or the original string if it is null or empty.</returns>
        public string? RemoveExtraSpaces()
        {
            if (string.IsNullOrEmpty(str)) return str;

            // Split the string into words and remove empty entries
            var words = str.Split([' '], StringSplitOptions.RemoveEmptyEntries);

            // Join the words with a single space
            return string.Join(" ", words);
        }

        /// <summary>
        /// Removes the specified postfixes from the current string, if any are present.
        /// </summary>
        /// <param name="postFixes">An array of postfixes to remove from the string. If <see langword="null"/> or empty, no operation is
        /// performed.</param>
        /// <returns>A new string with the specified postfix removed, if found; otherwise, the original string.  Returns <see
        /// langword="null"/> if the current string is <see langword="null"/>.</returns>
        public string? RemovePostFix(params string[]? postFixes)
            => str?.RemovePostFix(StringComparison.Ordinal, postFixes);

        /// <summary>
        /// Removes the specified postfix from the current string if it ends with any of the provided postfixes.
        /// </summary>
        /// <remarks>If multiple postfixes match, the first matching postfix in the <paramref
        /// name="postFixes"/> array is removed. If <paramref name="postFixes"/> is null or empty, or if the string is
        /// null or empty, the original string is returned.</remarks>
        /// <param name="comparisonType">The <see cref="StringComparison"/> option to use when comparing the string with the postfixes.</param>
        /// <param name="postFixes">An array of postfixes to check for removal. If the string ends with any of these postfixes, the matching
        /// postfix will be removed.</param>
        /// <returns>A new string with the matching postfix removed, or the original string if no matching postfix is found.</returns>
        public string? RemovePostFix(StringComparison comparisonType, params string[]? postFixes)
        {
            if (string.IsNullOrEmpty(str) || postFixes is null || postFixes?.Length == 0)
                return str;

            foreach (string postFix in postFixes!)
                if (str.EndsWith(postFix, comparisonType))
                    return str.Left(str.Length - postFix.Length);

            return str;
        }

        /// <summary>
        /// Removes the specified prefixes from the current string, if any are present.
        /// </summary>
        /// <remarks>The comparison is performed using <see cref="StringComparison.Ordinal"/>. If multiple
        /// prefixes match, only the first match in the <paramref name="preFixes"/> array is removed.</remarks>
        /// <param name="preFixes">An array of prefixes to remove from the string. Can be null or empty.</param>
        /// <returns>The string with the first matching prefix removed, or the original string if no prefixes match. Returns <see
        /// langword="null"/> if the current string is <see langword="null"/>.</returns>
        public string? RemovePreFix(params string[]? preFixes)
            => str?.RemovePreFix(StringComparison.Ordinal, preFixes);

        /// <summary>
        /// Removes the first matching prefix from the current string, if any of the specified prefixes are found.
        /// </summary>
        /// <remarks>The method checks each prefix in the order they are provided and removes the first
        /// one that matches. If the string is null or empty, or if no prefixes are provided, the original string is
        /// returned.</remarks>
        /// <param name="comparisonType">The <see cref="StringComparison"/> option to use when comparing the prefixes.</param>
        /// <param name="preFixes">An array of prefixes to check for. If null or empty, the method returns the original string.</param>
        /// <returns>A new string with the first matching prefix removed, or the original string if no matching prefix is found.</returns>
        public string? RemovePreFix(StringComparison comparisonType, params string[]? preFixes)
        {
            if (string.IsNullOrEmpty(str) || preFixes is null || preFixes?.Length == 0)
                return str;

            foreach (string preFix in preFixes!)
                if (str.StartsWith(preFix, comparisonType))
                    return str.Right(str.Length - preFix.Length);

            return str;
        }

        /// <summary>
        /// Replaces the first occurrence of a specified substring in the current string with another specified
        /// substring.
        /// </summary>
        /// <remarks>This method performs a case-sensitive or case-insensitive search based on the
        /// specified <paramref name="comparisonType"/>.</remarks>
        /// <param name="search">The substring to locate and replace. Cannot be null or empty.</param>
        /// <param name="replace">The substring to replace the first occurrence of <paramref name="search"/>. Cannot be null.</param>
        /// <param name="comparisonType">The type of string comparison to use when searching for <paramref name="search"/>. Defaults to <see
        /// cref="StringComparison.Ordinal"/>.</param>
        /// <returns>A new string with the first occurrence of <paramref name="search"/> replaced by <paramref name="replace"/>.
        /// If <paramref name="search"/> is not found, or if the current string is null or empty, the original string is
        /// returned.</returns>
        public string? ReplaceFirst(string? search, string? replace, StringComparison comparisonType = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(search) || replace is null)
                return str;

            int index = str!.IndexOf(search!, comparisonType);

            if (index < 0)
                return str;

            return string.Concat(str.AsSpan(0, index), replace, str.AsSpan(index + search!.Length));
        }

        /// <summary>
        /// Computes the MD5 hash of the current string and returns it as a hexadecimal string.
        /// </summary>
        /// <remarks>The method converts the string to UTF-8 encoded bytes, computes the MD5 hash,  and
        /// formats the hash as a hexadecimal string in uppercase.</remarks>
        /// <returns>A string representing the MD5 hash of the input string in uppercase hexadecimal format.</returns>
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
