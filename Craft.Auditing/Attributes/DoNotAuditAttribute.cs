namespace Craft.Auditing;

/// <summary>
/// Disables auditing for the decorated class. 
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class DoNotAuditAttribute : Attribute { }
