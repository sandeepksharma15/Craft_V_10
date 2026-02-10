using System.Collections.ObjectModel;

namespace Craft.Domain;

/// <summary>
/// Defines a contract for entities that can raise domain events.
/// </summary>
/// <remarks>
/// <para>Typically implemented by aggregate roots to record domain events
/// that occurred during a business operation. Events are collected and
/// dispatched after the aggregate is persisted.</para>
/// <para>
/// <b>Usage pattern:</b>
/// <list type="number">
///   <item><description>Entity raises events during domain operations</description></item>
///   <item><description>Repository/Unit of Work persists the entity</description></item>
///   <item><description>Event dispatcher publishes collected events</description></item>
///   <item><description>Events are cleared after successful dispatch</description></item>
/// </list>
/// </para>
/// </remarks>
public interface IHasDomainEvents
{
    /// <summary>
    /// Gets the collection of domain events raised by this entity.
    /// </summary>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Adds a domain event to the collection.
    /// </summary>
    /// <param name="domainEvent">The domain event to add.</param>
    void AddDomainEvent(IDomainEvent domainEvent);

    /// <summary>
    /// Removes a domain event from the collection.
    /// </summary>
    /// <param name="domainEvent">The domain event to remove.</param>
    /// <returns>True if the event was removed; otherwise, false.</returns>
    bool RemoveDomainEvent(IDomainEvent domainEvent);

    /// <summary>
    /// Clears all domain events from the collection.
    /// </summary>
    void ClearDomainEvents();
}

/// <summary>
/// Provides a default implementation for managing domain events.
/// </summary>
/// <remarks>
/// This class can be used as a composition helper for entities that need domain event support
/// but cannot inherit from a base class that provides it.
/// </remarks>
public sealed class DomainEventCollection : IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <inheritdoc />
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <inheritdoc />
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    /// <inheritdoc />
    public bool RemoveDomainEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        return _domainEvents.Remove(domainEvent);
    }

    /// <inheritdoc />
    public void ClearDomainEvents() => _domainEvents.Clear();
}
