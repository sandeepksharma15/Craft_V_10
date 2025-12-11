namespace Craft.Auditing;

/// <summary>
/// Disables auditing for the decorated class or property. 
/// When applied to a class, the entire entity is excluded from auditing.
/// When applied to a property, only that property is excluded from audit trail.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, Inherited = false)]
public class DoNotAuditAttribute : Attribute { }
