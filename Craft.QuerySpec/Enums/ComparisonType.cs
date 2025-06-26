using System.ComponentModel;

namespace Craft.QuerySpec;

public enum ComparisonType
{
    [Description("Equal To")]
    EqualTo,

    [Description("Not Equal To")]
    NotEqualTo,

    [Description("Greater Than")]
    GreaterThan,

    [Description("Greater Than Or Equal To")]
    GreaterThanOrEqualTo,

    [Description("Less Than")]
    LessThan,

    [Description("Less Than Or Equal To")]
    LessThanOrEqualTo,

    [Description("Contains")]
    Contains,

    [Description("Starts With")]
    StartsWith,

    [Description("Ends With")]
    EndsWith
}
