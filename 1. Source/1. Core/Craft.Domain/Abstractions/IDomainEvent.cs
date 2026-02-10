namespace Craft.Domain;

/// <summary>
/// Marker interface for domain events.
/// Domain events represent something significant that happened in the domain.
/// </summary>
/// <remarks>
/// <para>Domain events are used to:</para>
/// <list type="bullet">
///   <item><description>Decouple domain logic from side effects</description></item>
///   <item><description>Enable eventual consistency between aggregates</description></item>
///   <item><description>Support event-driven architectures and CQRS patterns</description></item>
///   <item><description>Integrate with message brokers like MediatR</description></item>
/// </list>
/// <para>
/// Events should be named in past tense (e.g., OrderPlaced, UserRegistered, PaymentProcessed)
/// as they represent facts that have already occurred.
/// </para>
/// </remarks>
public interface IDomainEvent
{
    /// <summary>
    /// Gets the unique identifier for this event instance.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Gets the UTC timestamp when the event occurred.
    /// </summary>
    DateTime OccurredOnUtc { get; }

    /// <summary>
    /// Gets the type name of the event for serialization and routing purposes.
    /// </summary>
    string EventType { get; }
}
