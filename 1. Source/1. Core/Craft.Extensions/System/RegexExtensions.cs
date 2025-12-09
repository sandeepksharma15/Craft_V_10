#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Text.RegularExpressions;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class RegexExtensions
{
    /// <summary>
    /// Performs a regular expression match on the specified input string using the given regex pattern.
    /// If either the regex or the input string is null, returns an empty Match object.
    /// </summary>
    /// <param name="regex">The regular expression pattern to match.</param>
    /// <param name="source">The input string to match against.</param>
    /// <returns>
    /// A Match object representing the first match found if successful; otherwise, an empty Match object.
    /// </returns>
    public static Match RegexMatch(this Regex regex, string source)
    {
        if ((regex == null) || (source == null))
            return Match.Empty;

        var match = regex.Match(source);

        return match.Success ? match : Match.Empty;
    }

    /// <summary>
    /// Performs a regular expression match on the input string using the provided regex pattern.
    /// Returns Match.Empty if either the regex or the input string is null, or if no match is found.
    /// </summary>
    public static Match RegexMatch(this string? source, Regex? regex) =>
        regex!.RegexMatch(source!);
}
