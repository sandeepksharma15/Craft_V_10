using Craft.Domain;

namespace Craft.Auditing;

public static class AuditingHelpers
{
    /// <summary>
    /// Retrieves a list of model names that are derived from <see cref="BaseEntity"/> and marked with the
    /// "DoNotAudit" attribute.
    /// </summary>
    public static IList<string> GetNonAuditableBaseEntityTypes()
        => GetTypesWithAttribute<BaseEntity, DoNotAuditAttribute>();

    /// <summary>
    /// Retrieves a list of model names that inherit from <see cref="BaseEntity"/> and are marked as auditable.
    /// </summary>
    public static IList<string> GetAuditableBaseEntityTypes()
        => GetTypesWithAttribute<BaseEntity, AuditAttribute>();

    /// <summary>
    /// Retrieves a list of model names that are NOT derived from <see cref="BaseEntity"/> but are marked with the
    /// "DoNotAudit" attribute.
    /// </summary>
    public static IList<string> GetNonAuditableNonBaseEntityTypes()
        => GetTypesWithAttribute<BaseEntity, DoNotAuditAttribute>(mustInheritFromBaseType: false);

    /// <summary>
    /// Retrieves a list of model names that are NOT derived from <see cref="BaseEntity"/> but are marked as auditable.
    /// </summary>
    public static IList<string> GetAuditableNonBaseEntityTypes()
        => GetTypesWithAttribute<BaseEntity, AuditAttribute>(mustInheritFromBaseType: false);

    /// <summary>
    /// Retrieves a list of model names that are derived from <typeparamref name="TBase"/> and marked 
    /// with <typeparamref name="TAttribute"/>.
    /// </summary>
    public static IList<string> GetTypesWithAttribute<TBase, TAttribute>(
        bool includeAbstract = false,
        bool mustInheritFromBaseType = true)
        where TAttribute : Attribute
    {
        return [.. AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(t =>
                t.IsClass &&
                (includeAbstract || !t.IsAbstract) &&
                t.IsDefined(typeof(TAttribute), inherit: false) &&
                (mustInheritFromBaseType
                    ? typeof(TBase).IsAssignableFrom(t)
                    : !typeof(TBase).IsAssignableFrom(t)))
            .Select(t => t.Name)];
    }

    /// <summary>
    /// Retrieves a list of model names that are derived from <typeparamref name="TBase"/> and are NOT marked 
    /// with <typeparamref name="TAttribute"/>.
    /// </summary>
    public static IList<string> GetTypesWithoutAttribute<TBase, TAttribute>(
        bool includeAbstract = false,
        bool mustInheritFromBaseType = true)
        where TAttribute : Attribute
    {
        return [.. AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(t =>
                t.IsClass &&
                (includeAbstract || !t.IsAbstract) &&
                (!t.IsDefined(typeof(TAttribute), inherit: false)) &&
                (mustInheritFromBaseType
                    ? typeof(TBase).IsAssignableFrom(t)
                    : !typeof(TBase).IsAssignableFrom(t)))
            .Select(t => t.Name)];
    }
}
