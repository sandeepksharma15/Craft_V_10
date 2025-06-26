using System.Linq.Expressions;

namespace Craft.QuerySpec;

/// <summary>
/// Represents a specification for filtering a collection of entities of type `T`.
/// Encapsulates a compiled predicate function for efficient evaluation.
/// </summary>
/// <typeparam name="T">The type of the entities to be filtered.</typeparam>
[Serializable]
public class EntityFilterCriteria<T> where T : class
{
    private readonly Lazy<Func<T, bool>> filterFunc;

    public EntityFilterCriteria(Expression<Func<T, bool>>? filter)
    {
        ArgumentNullException.ThrowIfNull(filter);

        Filter = filter;
        filterFunc = new Lazy<Func<T, bool>>(Filter.Compile);
    }

    public Expression<Func<T, bool>> Filter { get; }
    public Func<T, bool> FilterFunc => filterFunc.Value;

    /// <summary>
    /// Checks whether the specified entity matches the filter criteria.
    /// </summary>
    /// <param name="entity">The entity to be evaluated.</param>
    /// <returns>True if the entity matches the filter, false otherwise.</returns>
    public bool Matches(T entity) => FilterFunc.Invoke(entity);
}
