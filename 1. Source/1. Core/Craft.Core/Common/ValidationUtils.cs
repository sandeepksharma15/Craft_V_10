using System;
using System.Collections.Generic;
using System.Linq;

namespace Craft.Core.Common;

/// <summary>
/// Utility methods for validation and error aggregation.
/// </summary>
public static class ValidationUtils
{
    /// <summary>
    /// Returns true if the value is null or, for strings, empty/whitespace.
    /// </summary>
    public static bool IsNullOrEmpty(object? value)
    {
        if (value is null)
            return true;
        if (value is string s)
            return string.IsNullOrWhiteSpace(s);
        if (value is ICollection<object> coll)
            return coll.Count == 0;
        return false;
    }

    /// <summary>
    /// Aggregates error messages from multiple IServiceResult instances.
    /// </summary>
    public static List<string> AggregateErrors(params IServiceResult[] results)
    {
        return results
            .Where(r => r.Errors != null)
            .SelectMany(r => r.Errors!)
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .ToList();
    }

    /// <summary>
    /// Validates that a required argument is not null or empty.
    /// Throws ArgumentNullException if invalid.
    /// </summary>
    public static void RequireArgument(object? value, string paramName)
    {
        if (IsNullOrEmpty(value))
            throw new ArgumentNullException(paramName);
    }
}
