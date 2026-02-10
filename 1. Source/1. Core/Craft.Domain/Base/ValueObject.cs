namespace Craft.Domain;

/// <summary>
/// Abstract base class for value objects providing structural equality semantics.
/// </summary>
/// <remarks>
/// <para>Value objects are immutable objects that are defined by their attributes rather than identity.
/// Two value objects are equal if all their properties are equal.</para>
/// <para>
/// <b>Characteristics of value objects:</b>
/// <list type="bullet">
///   <item><description>Immutability - once created, cannot be changed</description></item>
///   <item><description>Structural equality - compared by value, not reference</description></item>
///   <item><description>Self-validation - always in a valid state</description></item>
///   <item><description>Side-effect free - operations return new instances</description></item>
/// </list>
/// </para>
/// <para>
/// <b>Usage example:</b>
/// <code>
/// public sealed class Money : ValueObject
/// {
///     public decimal Amount { get; }
///     public string Currency { get; }
///     
///     public Money(decimal amount, string currency)
///     {
///         Amount = amount;
///         Currency = currency ?? throw new ArgumentNullException(nameof(currency));
///     }
///     
///     protected override IEnumerable&lt;object?&gt; GetEqualityComponents()
///     {
///         yield return Amount;
///         yield return Currency;
///     }
/// }
/// </code>
/// </para>
/// </remarks>
[Serializable]
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Gets the components used for equality comparison.
    /// Override this method to specify which properties define equality.
    /// </summary>
    /// <returns>An enumerable of components that define this value object's identity.</returns>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    /// <summary>
    /// Determines whether two value objects are equal.
    /// </summary>
    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two value objects are not equal.
    /// </summary>
    public static bool operator !=(ValueObject? left, ValueObject? right)
        => !(left == right);

    /// <inheritdoc />
    public bool Equals(ValueObject? other)
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
    public override bool Equals(object? obj)
        => obj is ValueObject other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Aggregate(17, (current, component) =>
                current * 31 + (component?.GetHashCode() ?? 0));
    }

    /// <summary>
    /// Creates a copy of this value object.
    /// Override in derived classes if deep copying is needed.
    /// </summary>
    /// <returns>A copy of this value object.</returns>
    public virtual ValueObject GetCopy()
        => (ValueObject)MemberwiseClone();
}

/// <summary>
/// Abstract base class for single-value objects providing type-safe wrappers around primitive types.
/// </summary>
/// <typeparam name="TValue">The type of the underlying value.</typeparam>
/// <remarks>
/// <para>Use this for simple value objects that wrap a single primitive value:</para>
/// <code>
/// public sealed class Email : SingleValueObject&lt;string&gt;
/// {
///     public Email(string value) : base(value)
///     {
///         if (!IsValidEmail(value))
///             throw new ArgumentException("Invalid email format", nameof(value));
///     }
///     
///     private static bool IsValidEmail(string value) => /* validation logic */;
/// }
/// </code>
/// </remarks>
[Serializable]
public abstract class SingleValueObject<TValue> : ValueObject, IComparable<SingleValueObject<TValue>>
    where TValue : IComparable<TValue>
{
    /// <summary>
    /// Gets the underlying value.
    /// </summary>
    public TValue Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SingleValueObject{TValue}"/> class.
    /// </summary>
    /// <param name="value">The underlying value.</param>
    protected SingleValueObject(TValue value)
    {
        Value = value;
    }

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
