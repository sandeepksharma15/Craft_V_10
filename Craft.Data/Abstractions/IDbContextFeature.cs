using Microsoft.EntityFrameworkCore;

namespace Craft.Data;

/// <summary>
/// Defines a pluggable feature for DbContext configuration and behavior.
/// Features can contribute conventions, model configuration, and save pipeline hooks.
/// </summary>
public interface IDbContextFeature
{
    /// <summary>
    /// Configure model-level conventions (e.g., value conversions, default behaviors).
    /// </summary>
    /// <param name="configurationBuilder">The configuration builder for model conventions.</param>
    void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) { }

    /// <summary>
    /// Configure entity types, relationships, and table mappings.
    /// </summary>
    /// <param name="modelBuilder">The model builder for entity configuration.</param>
    void ConfigureModel(ModelBuilder modelBuilder) { }

    /// <summary>
    /// Hook invoked before SaveChanges/SaveChangesAsync to apply feature-specific logic
    /// (e.g., audit trails, soft deletes, timestamps).
    /// </summary>
    /// <param name="context">The DbContext instance.</param>
    /// <param name="userId">The current user ID.</param>
    void OnBeforeSaveChanges(DbContext context, KeyType userId) { }

    /// <summary>
    /// Indicates if this feature requires a DbSet to be registered on the context.
    /// If true, the feature should implement <see cref="IDbSetProvider"/> to define the entity type.
    /// </summary>
    bool RequiresDbSet => false;
}

/// <summary>
/// Marker interface for features that need to register a DbSet on the context.
/// </summary>
public interface IDbSetProvider
{
    /// <summary>
    /// Gets the entity type that should be registered as a DbSet.
    /// </summary>
    Type EntityType { get; }
}
