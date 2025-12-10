using System.ComponentModel;

namespace Craft.Domain;

/// <summary>
/// Represents the gender classification of a person.
/// </summary>
public enum GenderType
{
    /// <summary>
    /// Male gender.
    /// </summary>
    [Description("Male")]
    Male,

    /// <summary>
    /// Female gender.
    /// </summary>
    [Description("Female")]
    Female,

    /// <summary>
    /// Other gender or prefer not to specify.
    /// </summary>
    [Description("Other")]
    Other
}
