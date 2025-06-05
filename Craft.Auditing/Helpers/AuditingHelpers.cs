using Craft.Domain;

namespace Craft.Auditing;

public static class AuditingHelpers
{
    /// <summary>
    /// Retrieves a list of model names that are derived from <see cref="BaseEntity"/>      and marked with the
    /// "DoNotAudit" attribute.
    /// </summary>
    /// <remarks>This method scans all loaded assemblies in the current application domain to identify     
    /// classes that inherit from <see cref="BaseEntity"/>, are non-abstract, and have the      "DoNotAudit" attribute
    /// applied. The names of these classes are returned as a list of strings.</remarks>
    /// <returns>A list of strings containing the names of models that are not auditable.      The list will be empty if no such
    /// models are found.</returns>
    public static IList<string> GetNonAuditableModels()
    {
        // Get the list of all the types inherited from class BaseEntity
        var modelNames = AppDomain.CurrentDomain.GetAssemblies()
               .SelectMany(s => s.GetTypes())
               .Where(t => typeof(BaseEntity).IsAssignableFrom(t)
                           && t.IsClass && !t.IsAbstract && t.HasDoNotAuditAttribute())
               .Select(t => t.Name)
               .ToList();

        return modelNames;
    }

    /// <summary>
    /// Retrieves a list of model names that inherit from <see cref="BaseEntity"/>  and are marked as auditable.
    /// </summary>
    /// <remarks>This method scans all loaded assemblies in the current application domain  to identify
    /// classes that derive from <see cref="BaseEntity"/>, are non-abstract,  and have the audit attribute applied. The
    /// names of these classes are returned  as a list of strings.</remarks>
    /// <returns>A list of strings containing the names of auditable models. The list will be empty  if no matching models are
    /// found.</returns>
    public static IList<string> GetAuditableModels()
    {
        // Get the list of all the types inherited from class BaseEntity
        var modelNames = AppDomain.CurrentDomain.GetAssemblies()
               .SelectMany(s => s.GetTypes())
               .Where(t => typeof(BaseEntity).IsAssignableFrom(t)
                           && t.IsClass && !t.IsAbstract && t.HasAuditAttribute())
               .Select(t => t.Name)
               .ToList();

        return modelNames;
    }

}
