namespace Craft.Domain;

/// <summary> 
/// Base class for all value objects. 
/// Value objects are immutable objects that are defined by their attributes 
/// rather than identity. 
/// </summary>
public abstract class ValueObject : DomainObject, IEquatable<ValueObject>
{
    /// <summary>
    /// Gets the components used for equality comparison.
    /// Override this method to specify which properties define equality.
    /// </summary>
    /// <returns>An enumerable of components that define this value object's identity.</returns>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    /// <inheritdoc />
    public virtual bool Equals(ValueObject? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Aggregate(17, (current, component) =>
                current * 31 + (component?.GetHashCode() ?? 0));
    }
}

/// <summary> 
/// Base class for immutable value objects that wrap a single primitive value. 
/// </summary> 
/// <typeparam name="TValue"> 
/// The underlying primitive type. 
/// </typeparam>
public abstract class SingleValueObject<TValue> : ValueObject, IComparable<SingleValueObject<TValue>>
    where TValue : IComparable<TValue>
{
    /// <summary>
    /// Gets the underlying value.
    /// </summary>
    public TValue Value { get; private init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SingleValueObject{TValue}"/> class.
    /// </summary>
    /// <param name="value">The underlying value.</param>
    protected SingleValueObject(TValue value)
        => Value = value;

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <inheritdoc />
    public int CompareTo(SingleValueObject<TValue>? other)
    {
        if (other is null)
            return 1;

        return Value.CompareTo(other.Value);
    }

    /// <summary>
    /// Implicitly converts the value object to its underlying value.
    /// </summary>
    public static implicit operator TValue(SingleValueObject<TValue> valueObject)
        => valueObject.Value;

    /// <inheritdoc />
    public override string ToString()
        => Value?.ToString() ?? string.Empty;
}
