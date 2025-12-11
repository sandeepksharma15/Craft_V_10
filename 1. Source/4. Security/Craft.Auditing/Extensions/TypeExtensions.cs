using System.Reflection;

namespace Craft.Auditing;

public static class TypeExtensions
{
    /// <summary>
    /// Determines whether the specified type is decorated with the <see cref="DoNotAuditAttribute"/>.
    /// </summary>
    /// <param name="type">The type to inspect for the presence of the <see cref="DoNotAuditAttribute"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="DoNotAuditAttribute"/> is applied to the specified type; otherwise,
    /// <see langword="false"/>.</returns>
    public static bool HasDoNotAuditAttribute(this Type type)
        => type.IsDefined(typeof(DoNotAuditAttribute), inherit: false);

    /// <summary>
    /// Determines whether the specified type is decorated with the <see cref="AuditAttribute"/>.
    /// </summary>
    /// <param name="type">The type to inspect for the presence of the <see cref="AuditAttribute"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="AuditAttribute"/> is applied to the specified type;  otherwise, <see
    /// langword="false"/>.</returns>
    public static bool HasAuditAttribute(this Type type)
        => type.IsDefined(typeof(AuditAttribute), inherit: false);

    /// <summary>
    /// Determines whether the specified property is decorated with the <see cref="DoNotAuditAttribute"/>.
    /// </summary>
    /// <param name="propertyInfo">The property to inspect for the presence of the <see cref="DoNotAuditAttribute"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="DoNotAuditAttribute"/> is applied to the specified property; otherwise,
    /// <see langword="false"/>.</returns>
    public static bool HasDoNotAuditAttribute(this PropertyInfo propertyInfo)
        => propertyInfo.IsDefined(typeof(DoNotAuditAttribute), inherit: false);

    /// <summary>
    /// Determines whether the specified property is decorated with the <see cref="AuditAttribute"/>.
    /// </summary>
    /// <param name="propertyInfo">The property to inspect for the presence of the <see cref="AuditAttribute"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="AuditAttribute"/> is applied to the specified property; otherwise,
    /// <see langword="false"/>.</returns>
    public static bool HasAuditAttribute(this PropertyInfo propertyInfo)
        => propertyInfo.IsDefined(typeof(AuditAttribute), inherit: false);
}
