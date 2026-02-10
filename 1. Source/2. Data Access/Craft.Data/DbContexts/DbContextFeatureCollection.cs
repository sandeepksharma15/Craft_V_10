using Microsoft.EntityFrameworkCore;

namespace Craft.Data;

/// <summary>
/// Collection of DbContext features that can be applied to configure conventions, model, and save behavior.
/// </summary>
public class DbContextFeatureCollection : List<IDbContextFeature>
{
    /// <summary>
    /// Applies conventions from all registered features.
    /// </summary>
    public void ApplyConventions(ModelConfigurationBuilder builder)
    {
        foreach (var feature in this)
            feature.ConfigureConventions(builder);
    }

    /// <summary>
    /// Applies model configuration from all registered features.
    /// </summary>
    public void ApplyModel(ModelBuilder builder)
    {
        foreach (var feature in this)
            feature.ConfigureModel(builder);
    }

    /// <summary>
    /// Invokes OnBeforeSaveChanges on all registered features.
    /// </summary>
    public void BeforeSaveChanges(DbContext context, KeyType userId)
    {
        foreach (var feature in this)
            feature.OnBeforeSaveChanges(context, userId);
    }

    /// <summary>
    /// Gets all entity types that need DbSets registered based on features.
    /// </summary>
    public IEnumerable<Type> GetRequiredDbSetTypes()
    {
        return this
            .Where(f => f.RequiresDbSet && f is IDbSetProvider)
            .Cast<IDbSetProvider>()
            .Select(p => p.EntityType)
            .Distinct();
    }

    /// <summary>
    /// Fluent API: Adds a feature to the collection.
    /// </summary>
    public DbContextFeatureCollection AddFeature<TFeature>(TFeature feature) where TFeature : IDbContextFeature
    {
        Add(feature);
        return this;
    }

    /// <summary>
    /// Fluent API: Adds a feature by type (creates instance with default constructor).
    /// </summary>
    public DbContextFeatureCollection AddFeature<TFeature>() where TFeature : IDbContextFeature, new()
    {
        Add(new TFeature());
        return this;
    }
}

