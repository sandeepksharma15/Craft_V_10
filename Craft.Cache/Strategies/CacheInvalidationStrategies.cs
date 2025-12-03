namespace Craft.Cache;

/// <summary>
/// Defines the contract for cache invalidation strategies.
/// </summary>
public interface ICacheInvalidationStrategy
{
    /// <summary>
    /// Gets the patterns to invalidate.
    /// </summary>
    /// <returns>The patterns to match for invalidation.</returns>
    IEnumerable<string> GetPatternsToInvalidate();
}

/// <summary>
/// Base abstract class for cache invalidation strategies.
/// </summary>
public abstract class CacheInvalidationStrategy : ICacheInvalidationStrategy
{
    public abstract IEnumerable<string> GetPatternsToInvalidate();
}

/// <summary>
/// Strategy that invalidates all entries for a specific entity type.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class EntityTypeInvalidationStrategy<TEntity> : CacheInvalidationStrategy
{
    private readonly ICacheKeyGenerator _keyGenerator;

    public EntityTypeInvalidationStrategy(ICacheKeyGenerator keyGenerator)
    {
        _keyGenerator = keyGenerator ?? throw new ArgumentNullException(nameof(keyGenerator));
    }

    public override IEnumerable<string> GetPatternsToInvalidate()
    {
        yield return _keyGenerator.GenerateEntityPattern<TEntity>();
    }
}

/// <summary>
/// Strategy that invalidates entries for specific entity IDs.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class EntityIdInvalidationStrategy<TEntity> : CacheInvalidationStrategy
{
    private readonly ICacheKeyGenerator _keyGenerator;
    private readonly IEnumerable<object> _ids;

    public EntityIdInvalidationStrategy(ICacheKeyGenerator keyGenerator, params object[] ids)
    {
        _keyGenerator = keyGenerator ?? throw new ArgumentNullException(nameof(keyGenerator));
        _ids = ids ?? throw new ArgumentNullException(nameof(ids));
    }

    public override IEnumerable<string> GetPatternsToInvalidate()
    {
        return _ids.Select(id => _keyGenerator.GenerateEntityKey<TEntity>(id));
    }
}

/// <summary>
/// Strategy that invalidates entries matching specific patterns.
/// </summary>
public class PatternInvalidationStrategy : CacheInvalidationStrategy
{
    private readonly string[] _patterns;

    public PatternInvalidationStrategy(params string[] patterns)
    {
        _patterns = patterns ?? throw new ArgumentNullException(nameof(patterns));
    }

    public override IEnumerable<string> GetPatternsToInvalidate()
    {
        return _patterns;
    }
}

/// <summary>
/// Strategy that invalidates related entities based on dependencies.
/// </summary>
public class DependentEntityInvalidationStrategy : CacheInvalidationStrategy
{
    private readonly ICacheKeyGenerator _keyGenerator;
    private readonly Dictionary<Type, List<object>> _dependencies;

    public DependentEntityInvalidationStrategy(ICacheKeyGenerator keyGenerator)
    {
        _keyGenerator = keyGenerator ?? throw new ArgumentNullException(nameof(keyGenerator));
        _dependencies = new Dictionary<Type, List<object>>();
    }

    public DependentEntityInvalidationStrategy AddDependency<TEntity>(params object[] ids)
    {
        var type = typeof(TEntity);
        if (!_dependencies.ContainsKey(type))
            _dependencies[type] = new List<object>();

        _dependencies[type].AddRange(ids);
        return this;
    }

    public DependentEntityInvalidationStrategy AddTypeDependency<TEntity>()
    {
        var type = typeof(TEntity);
        if (!_dependencies.ContainsKey(type))
            _dependencies[type] = new List<object>();
        return this;
    }

    public override IEnumerable<string> GetPatternsToInvalidate()
    {
        var patterns = new List<string>();

        foreach (var (type, ids) in _dependencies)
        {
            if (ids.Count == 0)
            {
                // Invalidate entire type
                var method = typeof(ICacheKeyGenerator)
                    .GetMethod(nameof(ICacheKeyGenerator.GenerateEntityPattern))!
                    .MakeGenericMethod(type);
                var pattern = (string)method.Invoke(_keyGenerator, new object?[] { null })!;
                patterns.Add(pattern);
            }
            else
            {
                // Invalidate specific IDs
                var method = typeof(ICacheKeyGenerator)
                    .GetMethod(nameof(ICacheKeyGenerator.GenerateEntityKey))!
                    .MakeGenericMethod(type);
                foreach (var id in ids)
                {
                    var key = (string)method.Invoke(_keyGenerator, new[] { id })!;
                    patterns.Add(key);
                }
            }
        }

        return patterns;
    }
}
