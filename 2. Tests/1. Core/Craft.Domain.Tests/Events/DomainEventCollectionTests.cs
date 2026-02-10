namespace Craft.Domain.Tests.Events;

public class DomainEventCollectionTests
{
    #region Test Implementations

    private sealed class TestEvent : DomainEventBase
    {
        public string Message { get; }
        public TestEvent(string message) => Message = message;
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldCreateEmptyCollection()
    {
        // Arrange & Act
        var collection = new DomainEventCollection();

        // Assert
        Assert.Empty(collection.DomainEvents);
    }

    #endregion

    #region AddDomainEvent Tests

    [Fact]
    public void AddDomainEvent_ShouldAddEventToCollection()
    {
        // Arrange
        var collection = new DomainEventCollection();
        var domainEvent = new TestEvent("test");

        // Act
        collection.AddDomainEvent(domainEvent);

        // Assert
        Assert.Single(collection.DomainEvents);
        Assert.Contains(domainEvent, collection.DomainEvents);
    }

    [Fact]
    public void AddDomainEvent_ShouldAddMultipleEvents()
    {
        // Arrange
        var collection = new DomainEventCollection();
        var event1 = new TestEvent("event1");
        var event2 = new TestEvent("event2");
        var event3 = new TestEvent("event3");

        // Act
        collection.AddDomainEvent(event1);
        collection.AddDomainEvent(event2);
        collection.AddDomainEvent(event3);

        // Assert
        Assert.Equal(3, collection.DomainEvents.Count);
    }

    [Fact]
    public void AddDomainEvent_ShouldThrowArgumentNullException_WhenEventIsNull()
    {
        // Arrange
        var collection = new DomainEventCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => collection.AddDomainEvent(null!));
    }

    [Fact]
    public void AddDomainEvent_ShouldPreserveOrder()
    {
        // Arrange
        var collection = new DomainEventCollection();
        var event1 = new TestEvent("first");
        var event2 = new TestEvent("second");
        var event3 = new TestEvent("third");

        // Act
        collection.AddDomainEvent(event1);
        collection.AddDomainEvent(event2);
        collection.AddDomainEvent(event3);

        // Assert
        var events = collection.DomainEvents.ToList();
        Assert.Equal(event1, events[0]);
        Assert.Equal(event2, events[1]);
        Assert.Equal(event3, events[2]);
    }

    #endregion

    #region RemoveDomainEvent Tests

    [Fact]
    public void RemoveDomainEvent_ShouldRemoveExistingEvent()
    {
        // Arrange
        var collection = new DomainEventCollection();
        var domainEvent = new TestEvent("test");
        collection.AddDomainEvent(domainEvent);

        // Act
        var result = collection.RemoveDomainEvent(domainEvent);

        // Assert
        Assert.True(result);
        Assert.Empty(collection.DomainEvents);
    }

    [Fact]
    public void RemoveDomainEvent_ShouldReturnFalse_WhenEventNotFound()
    {
        // Arrange
        var collection = new DomainEventCollection();
        var addedEvent = new TestEvent("added");
        var notAddedEvent = new TestEvent("not added");
        collection.AddDomainEvent(addedEvent);

        // Act
        var result = collection.RemoveDomainEvent(notAddedEvent);

        // Assert
        Assert.False(result);
        Assert.Single(collection.DomainEvents);
    }

    [Fact]
    public void RemoveDomainEvent_ShouldThrowArgumentNullException_WhenEventIsNull()
    {
        // Arrange
        var collection = new DomainEventCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => collection.RemoveDomainEvent(null!));
    }

    [Fact]
    public void RemoveDomainEvent_ShouldOnlyRemoveSpecificEvent()
    {
        // Arrange
        var collection = new DomainEventCollection();
        var event1 = new TestEvent("event1");
        var event2 = new TestEvent("event2");
        var event3 = new TestEvent("event3");
        collection.AddDomainEvent(event1);
        collection.AddDomainEvent(event2);
        collection.AddDomainEvent(event3);

        // Act
        collection.RemoveDomainEvent(event2);

        // Assert
        Assert.Equal(2, collection.DomainEvents.Count);
        Assert.Contains(event1, collection.DomainEvents);
        Assert.DoesNotContain(event2, collection.DomainEvents);
        Assert.Contains(event3, collection.DomainEvents);
    }

    #endregion

    #region ClearDomainEvents Tests

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        // Arrange
        var collection = new DomainEventCollection();
        collection.AddDomainEvent(new TestEvent("event1"));
        collection.AddDomainEvent(new TestEvent("event2"));
        collection.AddDomainEvent(new TestEvent("event3"));

        // Act
        collection.ClearDomainEvents();

        // Assert
        Assert.Empty(collection.DomainEvents);
    }

    [Fact]
    public void ClearDomainEvents_ShouldBeIdempotent()
    {
        // Arrange
        var collection = new DomainEventCollection();

        // Act (clear empty collection multiple times)
        collection.ClearDomainEvents();
        collection.ClearDomainEvents();

        // Assert
        Assert.Empty(collection.DomainEvents);
    }

    #endregion

    #region DomainEvents Property Tests

    [Fact]
    public void DomainEvents_ShouldReturnReadOnlyCollection()
    {
        // Arrange
        var collection = new DomainEventCollection();
        collection.AddDomainEvent(new TestEvent("test"));

        // Act
        var events = collection.DomainEvents;

        // Assert
        Assert.IsAssignableFrom<IReadOnlyCollection<IDomainEvent>>(events);
    }

    [Fact]
    public void DomainEvents_ShouldNotAllowDirectModification()
    {
        // Arrange
        var collection = new DomainEventCollection();
        collection.AddDomainEvent(new TestEvent("test"));

        // Act & Assert
        // The returned collection should be read-only
        var events = collection.DomainEvents;
        Assert.False(events is ICollection<IDomainEvent> { IsReadOnly: false });
    }

    #endregion

    #region IHasDomainEvents Interface Tests

    [Fact]
    public void DomainEventCollection_ShouldImplementIHasDomainEvents()
    {
        // Arrange & Act
        var collection = new DomainEventCollection();

        // Assert
        Assert.IsAssignableFrom<IHasDomainEvents>(collection);
    }

    [Fact]
    public void IHasDomainEvents_Methods_ShouldWorkThroughInterface()
    {
        // Arrange
        IHasDomainEvents collection = new DomainEventCollection();
        var domainEvent = new TestEvent("test");

        // Act
        collection.AddDomainEvent(domainEvent);

        // Assert
        Assert.Single(collection.DomainEvents);

        // Act
        var removed = collection.RemoveDomainEvent(domainEvent);

        // Assert
        Assert.True(removed);
        Assert.Empty(collection.DomainEvents);
    }

    #endregion
}
