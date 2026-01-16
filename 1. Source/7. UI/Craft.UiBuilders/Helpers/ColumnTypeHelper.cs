using Craft.QuerySpec;

namespace Craft.UiBuilders.Helpers;

/// <summary>
/// Utility class for determining column types and valid comparison operators for advanced search.
/// </summary>
public static class ColumnTypeHelper
{
    /// <summary>
    /// Gets the valid comparison operators for a given property type.
    /// </summary>
    /// <param name="propertyType">The type of the property.</param>
    /// <returns>List of valid comparison operators for the type.</returns>
    public static List<ComparisonType> GetValidOperators(Type propertyType)
    {
        Type underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        if (IsNumericType(underlyingType))
            return
            [
                ComparisonType.EqualTo,
                ComparisonType.NotEqualTo,
                ComparisonType.GreaterThan,
                ComparisonType.GreaterThanOrEqualTo,
                ComparisonType.LessThan,
                ComparisonType.LessThanOrEqualTo
            ];

        if (underlyingType == typeof(string))
            return
            [
                ComparisonType.EqualTo,
                ComparisonType.NotEqualTo,
                ComparisonType.Contains,
                ComparisonType.StartsWith,
                ComparisonType.EndsWith
            ];

        if (underlyingType == typeof(DateTime) || underlyingType == typeof(DateTimeOffset) || underlyingType == typeof(DateOnly))
            return
            [
                ComparisonType.EqualTo,
                ComparisonType.NotEqualTo,
                ComparisonType.GreaterThan,
                ComparisonType.GreaterThanOrEqualTo,
                ComparisonType.LessThan,
                ComparisonType.LessThanOrEqualTo
            ];

        if (underlyingType == typeof(bool))
            return [ComparisonType.EqualTo];

        if (underlyingType.IsEnum)
            return [ComparisonType.EqualTo];

        return [ComparisonType.EqualTo, ComparisonType.NotEqualTo];
    }

    /// <summary>
    /// Gets a human-readable display name for a comparison operator.
    /// </summary>
    public static string GetOperatorDisplayName(ComparisonType comparisonType)
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
    /// Determines if a type is numeric.
    /// </summary>
    public static bool IsNumericType(Type type)
    {
        Type underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        return underlyingType == typeof(byte)
            || underlyingType == typeof(sbyte)
            || underlyingType == typeof(short)
            || underlyingType == typeof(ushort)
            || underlyingType == typeof(int)
            || underlyingType == typeof(uint)
            || underlyingType == typeof(long)
            || underlyingType == typeof(ulong)
            || underlyingType == typeof(float)
            || underlyingType == typeof(double)
            || underlyingType == typeof(decimal);
    }

    /// <summary>
    /// Determines if a type is a date/time type.
    /// </summary>
    public static bool IsDateTimeType(Type type)
    {
        Type underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        return underlyingType == typeof(DateTime)
            || underlyingType == typeof(DateTimeOffset)
            || underlyingType == typeof(DateOnly);
    }

    /// <summary>
    /// Determines if a type is a boolean.
    /// </summary>
    public static bool IsBooleanType(Type type)
    {
        Type underlyingType = Nullable.GetUnderlyingType(type) ?? type;
        return underlyingType == typeof(bool);
    }

    /// <summary>
    /// Determines if a type is an enum.
    /// </summary>
    public static bool IsEnumType(Type type)
    {
        Type underlyingType = Nullable.GetUnderlyingType(type) ?? type;
        return underlyingType.IsEnum;
    }

    /// <summary>
    /// Gets all enum values for a given enum type.
    /// </summary>
    public static List<(string Name, object Value)> GetEnumValues(Type enumType)
    {
        Type underlyingType = Nullable.GetUnderlyingType(enumType) ?? enumType;

        if (!underlyingType.IsEnum)
            return [];

        return Enum.GetValues(underlyingType)
            .Cast<object>()
            .Select(value => (Name: value.ToString() ?? string.Empty, Value: value))
            .ToList();
    }
}
