namespace Craft.Auditing;

/// <summary>
/// Enables auditing for the decorated class. 
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class AuditAttribute : Attribute { }
