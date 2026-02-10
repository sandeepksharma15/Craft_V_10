using Craft.Core;
namespace Craft.QuerySpec;

/// <summary>
/// Extension methods for <see cref="ComparisonType"/> and type-based comparison operator retrieval.
/// </summary>
public static class ComparisonTypeExtensions
{
    /// <summary>
    /// Gets a human-readable display name for a comparison operator.
    /// </summary>
    /// <param name="comparisonType">The comparison type.</param>
    /// <returns>A human-readable display name with symbol representation.</returns>
    public static string GetDisplayName(this ComparisonType comparisonType)
    {
        return comparisonType switch
        {
            ComparisonType.EqualTo => "Equal To (=)",
            ComparisonType.NotEqualTo => "Not Equal To (≠)",
            ComparisonType.GreaterThan => "Greater Than (>)",
            ComparisonType.GreaterThanOrEqualTo => "Greater Than Or Equal (≥)",
            ComparisonType.LessThan => "Less Than (<)",
            ComparisonType.LessThanOrEqualTo => "Less Than Or Equal (≤)",
            ComparisonType.Contains => "Contains",
            ComparisonType.StartsWith => "Starts With",
            ComparisonType.EndsWith => "Ends With",
            _ => comparisonType.ToString()
        };
    }

    /// <summary>
    /// Gets the valid comparison operators for a given property type.
    /// </summary>
    /// <param name="propertyType">The type of the property.</param>
    /// <returns>List of valid comparison operators for the type.</returns>
    public static List<ComparisonType> GetValidComparisonOperators(this Type propertyType)
    {
        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        if (underlyingType.IsNumeric())
            return [
                ComparisonType.EqualTo,
                ComparisonType.NotEqualTo,
                ComparisonType.GreaterThan,
                ComparisonType.GreaterThanOrEqualTo,
                ComparisonType.LessThan,
                ComparisonType.LessThanOrEqualTo
            ];

        if (underlyingType == typeof(string))
            return [
                ComparisonType.EqualTo,
                ComparisonType.NotEqualTo,
                ComparisonType.Contains,
                ComparisonType.StartsWith,
                ComparisonType.EndsWith
            ];

        if (underlyingType.IsDateTime())
            return [
                ComparisonType.EqualTo,
                ComparisonType.NotEqualTo,
                ComparisonType.GreaterThan,
                ComparisonType.GreaterThanOrEqualTo,
                ComparisonType.LessThan,
                ComparisonType.LessThanOrEqualTo
            ];

        if (underlyingType.IsBoolean())
            return [ComparisonType.EqualTo];

        if (underlyingType.IsEnumType())
            return [ComparisonType.EqualTo];

        // Default fallback for unknown types (treat as string-like for search flexibility)
        return [
            ComparisonType.EqualTo,
            ComparisonType.NotEqualTo,
            ComparisonType.Contains,
            ComparisonType.StartsWith,
            ComparisonType.EndsWith
        ];
    }
}

