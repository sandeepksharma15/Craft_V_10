using System.ComponentModel;

namespace Craft.Domain;

public enum HonorificType
{
    [Description("Dr")]
    Dr,

    [Description("Prof")]
    Prof,

    [Description("Mr")]
    Mr,

    [Description("Ms")]
    Ms,

    [Description("Mrs")]
    Mrs
}
