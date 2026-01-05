using Craft.Domain;
using System.Reflection;

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
            .Where(a => !ShouldSkipAssembly(a))
            .SelectMany(s => GetTypesFromAssembly(s))
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
            .Where(a => !ShouldSkipAssembly(a))
            .SelectMany(s => GetTypesFromAssembly(s))
            .Where(t =>
                t.IsClass &&
                (includeAbstract || !t.IsAbstract) &&
                (!t.IsDefined(typeof(TAttribute), inherit: false)) &&
                (mustInheritFromBaseType
                    ? typeof(TBase).IsAssignableFrom(t)
                    : !typeof(TBase).IsAssignableFrom(t)))
            .Select(t => t.Name)];
    }

    /// <summary>
    /// Determines if an assembly should be skipped during type scanning.
    /// Skips system assemblies, WebAssembly, and JSInterop assemblies that can cause issues.
    /// </summary>
    private static bool ShouldSkipAssembly(Assembly assembly)
    {
        var name = assembly.GetName().Name;
        if (name == null)
            return true;

        return name.StartsWith("System.", StringComparison.Ordinal) ||
               name.StartsWith("Microsoft.Extensions.", StringComparison.Ordinal) ||
               name.StartsWith("Microsoft.AspNetCore.", StringComparison.Ordinal) ||
               name.StartsWith("Microsoft.JSInterop", StringComparison.Ordinal) ||
               name.StartsWith("WebAssembly", StringComparison.Ordinal) ||
               name.Equals("netstandard", StringComparison.Ordinal) ||
               name.Equals("mscorlib", StringComparison.Ordinal);
    }

    /// <summary>
    /// Safely gets types from an assembly, handling potential exceptions.
    /// </summary>
    private static Type[] GetTypesFromAssembly(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t != null).ToArray()!;
        }
        catch
        {
            return [];
        }
    }
}
