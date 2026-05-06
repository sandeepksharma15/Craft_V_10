namespace Craft.Utilities.Helpers;

/// <summary>
/// Provides string similarity and distance utilities.
/// </summary>
public static class TextSimilarity
{
    /// <summary>
    /// Computes the standard iterative Levenshtein (edit) distance between two strings.
    /// Returns the number of single-character edits (insertions, deletions, substitutions)
    /// required to transform <paramref name="source"/> into <paramref name="target"/>.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="target">The target string.</param>
    /// <returns>
    /// The edit distance between the two strings.
    /// Returns <c>0</c> when both strings are identical.
    /// </returns>
    public static int LevenshteinDistance(string source, string target)
    {
        if (string.IsNullOrEmpty(source)) return target?.Length ?? 0;
        if (string.IsNullOrEmpty(target)) return source.Length;

        var d = new int[source.Length + 1, target.Length + 1];

        for (var i = 0; i <= source.Length; i++) d[i, 0] = i;
        for (var j = 0; j <= target.Length; j++) d[0, j] = j;

        for (var i = 1; i <= source.Length; i++)
        {
            for (var j = 1; j <= target.Length; j++)
            {
                var cost = source[i - 1] == target[j - 1] ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }

        return d[source.Length, target.Length];
    }
}
