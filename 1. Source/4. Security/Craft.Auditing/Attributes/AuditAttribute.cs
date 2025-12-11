namespace Craft.Auditing;

/// <summary>
/// Enables auditing for the decorated class or property. 
/// When applied to a class, the entire entity is included in auditing.
/// When applied to a property, that property is explicitly included even if the class has DoNotAudit.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, Inherited = false)]
public class AuditAttribute : Attribute { }
