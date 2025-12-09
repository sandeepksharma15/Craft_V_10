using System.ComponentModel;

namespace Craft.Domain;

public enum GenderType
{
    [Description("Male")]
    Male,

    [Description("Female")]
    Female,

    [Description("Other")]
    Other
}
