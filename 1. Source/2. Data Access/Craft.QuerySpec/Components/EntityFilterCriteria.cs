using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Craft.QuerySpec;

/// <summary>
/// Represents a specification for filtering a collection of entities of type <typeparamref name="T"/>.
/// Encapsulates a compiled predicate function for efficient evaluation with thread-safe lazy compilation.
/// </summary>
/// <typeparam name="T">The type of the entities to be filtered. Must be a reference type.</typeparam>
/// <remarks>
/// This class provides an abstraction over LINQ expressions, allowing for both
/// expression tree manipulation and efficient compiled predicate evaluation.
/// The filter function is compiled lazily and cached for performance.
/// </remarks>
[Serializable]
public sealed class EntityFilterCriteria<T> : IEquatable<EntityFilterCriteria<T>> where T : class
{
    private readonly Lazy<Func<T, bool>> _filterFunc;
    private readonly int _hashCode;

    /// Gets the original LINQ expression that defines the filter criteria.
    public Expression<Func<T, bool>> Filter { get; }

    /// Gets the compiled predicate function for efficient entity evaluation.
    public Func<T, bool> FilterFunc => _filterFunc.Value;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityFilterCriteria{T}"/> class.
    /// </summary>
    /// <param name="filter">The LINQ expression that defines the filter criteria.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="filter"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the filter expression is invalid or cannot be compiled.</exception>
    public EntityFilterCriteria([NotNull] Expression<Func<T, bool>> filter)
    {
        ArgumentNullException.ThrowIfNull(filter, nameof(filter));

        ValidateExpression(filter);

        Filter = filter;
        _filterFunc = new Lazy<Func<T, bool>>(CompileExpression, LazyThreadSafetyMode.ExecutionAndPublication);
        _hashCode = ComputeHashCode(filter);
    }

    /// <summary>
    /// Checks whether the specified entity matches the filter criteria.
    /// </summary>
    /// <param name="entity">The entity to be evaluated against the filter criteria.</param>
    /// <returns><c>true</c> if the entity matches the filter criteria; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the filter function compilation fails.</exception>
    /// <remarks>
    /// This method provides a safe wrapper around the compiled filter function with proper error handling.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Matches([NotNull] T entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        try
        {
            return FilterFunc.Invoke(entity);
        }
        catch (Exception ex) when (ex is not ArgumentNullException)
        {
            throw new InvalidOperationException(
                $"Failed to evaluate entity of type '{typeof(T).Name}' against filter criteria. " +
                $"This may indicate an issue with the filter expression or entity state.", ex);
        }
    }

    /// <summary>
    /// Determines whether the current instance is equal to another <see cref="EntityFilterCriteria{T}"/> instance.
    /// </summary>
    /// <param name="other">The other instance to compare with.</param>
    /// <returns><c>true</c> if the instances are equal; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Two instances are considered equal if their filter expressions have the same string representation.
    /// This provides semantic equality for filter criteria.
    /// </remarks>
    public bool Equals(EntityFilterCriteria<T>? other)
    {
        return other is not null && (ReferenceEquals(this, other) || string.Equals(Filter.ToString(), other.Filter.ToString(), StringComparison.Ordinal));
    }

    /// <summary>
    /// Determines whether the current instance is equal to another object.
    /// </summary>
    /// <param name="obj">The object to compare with.</param>
    /// <returns><c>true</c> if the objects are equal; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj) => Equals(obj as EntityFilterCriteria<T>);

    /// <summary>
    /// Returns the hash code for the current instance.
    /// </summary>
    /// <returns>A hash code for the current instance.</returns>
    /// <remarks>
    /// The hash code is computed once during construction and cached for performance.
    /// It is based on the string representation of the filter expression.
    /// </remarks>
    public override int GetHashCode() => _hashCode;

    /// <summary>
    /// Returns a string representation of the filter criteria.
    /// </summary>
    /// <returns>A string that represents the current filter expression.</returns>
    public override string ToString() => Filter.ToString();

    /// <summary>
    /// Determines whether two <see cref="EntityFilterCriteria{T}"/> instances are equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><c>true</c> if the instances are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(EntityFilterCriteria<T>? left, EntityFilterCriteria<T>? right)
    {
        return left is null ? right is null : left.Equals(right);
    }

    /// <summary>
    /// Determines whether two <see cref="EntityFilterCriteria{T}"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first instance to compare.</param>
    /// <param name="right">The second instance to compare.</param>
    /// <returns><c>true</c> if the instances are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(EntityFilterCriteria<T>? left, EntityFilterCriteria<T>? right) => !(left == right);

    /// <summary>
    /// Validates the provided expression to ensure it can be safely compiled and executed.
    /// </summary>
    /// <param name="expression">The expression to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the expression is invalid.</exception>
    private static void ValidateExpression(Expression<Func<T, bool>> expression)
    {
        try
        {
            // Attempt to compile the expression to catch any compilation errors early
            _ = expression.Compile();
        }
        catch (Exception ex)
        {
            throw new ArgumentException(
                $"The provided filter expression cannot be compiled. Expression: {expression}",
                nameof(expression), ex);
        }
    }

    /// <summary>
    /// Compiles the filter expression with enhanced error handling.
    /// </summary>
    /// <returns>The compiled predicate function.</returns>
    /// <exception cref="InvalidOperationException">Thrown when compilation fails.</exception>
    private Func<T, bool> CompileExpression()
    {
        try
        {
            return Filter.Compile();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to compile filter expression for type '{typeof(T).Name}'. Expression: {Filter}", ex);
        }
    }

    /// <summary>
    /// Computes a hash code for the given expression.
    /// </summary>
    /// <param name="expression">The expression to compute the hash code for.</param>
    /// <returns>A hash code for the expression.</returns>
    private static int ComputeHashCode(Expression<Func<T, bool>> expression)
    {
        return HashCode.Combine(typeof(T), expression.ToString());
    }
}
