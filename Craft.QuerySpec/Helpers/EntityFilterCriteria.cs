using System.Linq.Expressions;

namespace Craft.QuerySpec;

/// <summary>
/// Represents a specification for filtering a collection of entities of type <typeparamref name="T"/>.
/// Encapsulates a compiled predicate function for efficient evaluation.
/// </summary>
/// <typeparam name="T">The type of the entities to be filtered.</typeparam>
[Serializable]
public sealed class EntityFilterCriteria<T> where T : class
{
    private readonly Lazy<Func<T, bool>> _filterFunc;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityFilterCriteria{T}"/> class with the specified filter expression.
    /// </summary>
    /// <param name="filter">The filter expression to use for entity filtering.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="filter"/> is null.</exception>
    public EntityFilterCriteria(Expression<Func<T, bool>>? filter)
    {
        if (filter is null)
            throw new ArgumentNullException(nameof(filter), "Filter expression cannot be null.");

        Filter = filter;

        _filterFunc = new Lazy<Func<T, bool>>(Filter.Compile, isThreadSafe: true);
    }

    /// <summary>
    /// Gets the filter expression used for entity filtering.
    /// </summary>
    public Expression<Func<T, bool>> Filter { get; }

    /// <summary>
    /// Gets the compiled filter function for efficient evaluation.
    /// </summary>
    public Func<T, bool> FilterFunc => _filterFunc.Value;

    /// <summary>
    /// Checks whether the specified entity matches the filter criteria.
    /// </summary>
    /// <param name="entity">The entity to be evaluated.</param>
    /// <returns>True if the entity matches the filter; otherwise, false.</returns>
    public bool Matches(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        return FilterFunc.Invoke(entity);
    }

    /// <summary>
    /// Returns a string representation of the filter criteria.
    /// </summary>
    public override string ToString() => Filter.ToString() ?? base.ToString()!;
}
