using System.ComponentModel;

namespace Craft.Auditing;

public enum EntityChangeType
{
    [Description("")]
    None,

    [Description("Created")]
    Created,

    [Description("Updated")]
    Updated,

    [Description("Deleted")]
    Deleted
}
