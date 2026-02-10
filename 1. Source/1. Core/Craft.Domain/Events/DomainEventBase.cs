namespace Craft.Domain;

/// <summary>
/// Abstract base class for domain events providing common functionality.
/// </summary>
/// <remarks>
/// <para>Inherit from this class when creating domain events:</para>
/// <code>
/// public sealed class OrderPlacedEvent : DomainEventBase
/// {
///     public Guid OrderId { get; }
///     public decimal TotalAmount { get; }
///     
///     public OrderPlacedEvent(Guid orderId, decimal totalAmount)
///     {
///         OrderId = orderId;
///         TotalAmount = totalAmount;
///     }
/// }
/// </code>
/// </remarks>
[Serializable]
public abstract class DomainEventBase : IDomainEvent, IEquatable<DomainEventBase>
{
    /// <summary>
    /// Gets the unique identifier for this event instance.
    /// </summary>
    public Guid EventId { get; }

    /// <summary>
    /// Gets the UTC timestamp when the event occurred.
    /// </summary>
    public DateTime OccurredOnUtc { get; }

    /// <summary>
    /// Gets the type name of the event for serialization and routing purposes.
    /// </summary>
    public string EventType => GetType().Name;

    /// <summary>
    /// Gets an optional correlation identifier for tracing related events.
    /// </summary>
    public Guid? CorrelationId { get; init; }

    /// <summary>
    /// Gets an optional causation identifier linking this event to its cause.
    /// </summary>
    public Guid? CausationId { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainEventBase"/> class.
    /// </summary>
    protected DomainEventBase()
    {
        EventId = Guid.NewGuid();
        OccurredOnUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainEventBase"/> class with a specific timestamp.
    /// </summary>
    /// <param name="occurredOnUtc">The UTC timestamp when the event occurred.</param>
    protected DomainEventBase(DateTime occurredOnUtc)
    {
        EventId = Guid.NewGuid();
        OccurredOnUtc = occurredOnUtc;
    }

    /// <summary>
    /// Determines whether two domain events are equal based on their EventId.
    /// </summary>
    public static bool operator ==(DomainEventBase? left, DomainEventBase? right)
        => Equals(left, right);

    /// <summary>
    /// Determines whether two domain events are not equal.
    /// </summary>
    public static bool operator !=(DomainEventBase? left, DomainEventBase? right)
        => !Equals(left, right);

    /// <inheritdoc />
    public bool Equals(DomainEventBase? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return EventId == other.EventId;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is DomainEventBase other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
        => EventId.GetHashCode();

    /// <inheritdoc />
    public override string ToString()
        => $"{EventType} ({EventId}) at {OccurredOnUtc:O}";
}
