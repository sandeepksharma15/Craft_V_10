using System.ComponentModel;

namespace Craft.Domain;

/// <summary>
/// Represents the honorific titles used to address a person.
/// </summary>
public enum HonorificType
{
    /// <summary>
    /// Doctor honorific title.
    /// </summary>
    [Description("Dr")]
    Dr,

    /// <summary>
    /// Professor honorific title.
    /// </summary>
    [Description("Prof")]
    Prof,

    /// <summary>
    /// Mister honorific title (for men).
    /// </summary>
    [Description("Mr")]
    Mr,

    /// <summary>
    /// Miss honorific title (for unmarried women or general use).
    /// </summary>
    [Description("Ms")]
    Ms,

    /// <summary>
    /// Missus honorific title (for married women).
    /// </summary>
    [Description("Mrs")]
    Mrs
}
