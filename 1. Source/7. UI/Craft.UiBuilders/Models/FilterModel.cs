using Craft.QuerySpec;

namespace Craft.UiBuilders.Models;

/// <summary>
/// Represents a filter model for advanced search functionality.
/// Contains column information, comparison operator, value, and logical operator for combining with other filters.
/// </summary>
public sealed class FilterModel
{
    /// <summary>
    /// The column property name being filtered.
    /// </summary>
    public string ColumnName { get; set; } = string.Empty;

    /// <summary>
    /// The display title of the column.
    /// </summary>
    public string ColumnTitle { get; set; } = string.Empty;

    /// <summary>
    /// The type of the property being filtered.
    /// </summary>
    public Type PropertyType { get; set; } = typeof(string);

    /// <summary>
    /// The comparison operator to apply.
    /// </summary>
    public ComparisonType Operator { get; set; } = ComparisonType.EqualTo;

    /// <summary>
    /// The value to compare against.
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// The logical operator to combine this filter with the previous one (AND/OR).
    /// Null for the first filter.
    /// </summary>
    public LogicalOperatorType? LogicalOperator { get; set; }

    /// <summary>
    /// Converts this FilterModel to a FilterCriteria for use with Query specifications.
    /// </summary>
    public FilterCriteria ToFilterCriteria()
    {
        return new FilterCriteria(PropertyType, ColumnName, Value, Operator);
    }

    /// <summary>
    /// Gets a human-readable description of this filter.
    /// </summary>
    public string GetDescription()
    {
        string operatorText = Operator switch
        {
            ComparisonType.EqualTo => "=",
            ComparisonType.NotEqualTo => "≠",
            ComparisonType.GreaterThan => ">",
            ComparisonType.GreaterThanOrEqualTo => "≥",
            ComparisonType.LessThan => "<",
            ComparisonType.LessThanOrEqualTo => "≤",
            ComparisonType.Contains => "contains",
            ComparisonType.StartsWith => "starts with",
            ComparisonType.EndsWith => "ends with",
            _ => "="
        };

        string valueText = Value switch
        {
            null => "null",
            DateTime dt => dt.ToString("yyyy-MM-dd"),
            bool b => b ? "Yes" : "No",
            _ => Value.ToString() ?? string.Empty
        };

        return $"{ColumnTitle} {operatorText} {valueText}";
    }
}

/// <summary>
/// Represents logical operators for combining multiple filters.
/// </summary>
public enum LogicalOperatorType
{
    /// <summary>
    /// Both conditions must be true.
    /// </summary>
    And,

    /// <summary>
    /// At least one condition must be true.
    /// </summary>
    Or
}
