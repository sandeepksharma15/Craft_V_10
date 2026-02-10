namespace Craft.Domain.Tests.Events;

public class DomainEventBaseTests
{
    #region Test Implementations

    private sealed class TestDomainEvent : DomainEventBase
    {
        public string Data { get; }

        public TestDomainEvent(string data) => Data = data;

        public TestDomainEvent(string data, DateTime occurredOnUtc) : base(occurredOnUtc)
            => Data = data;
    }

    private sealed class AnotherDomainEvent : DomainEventBase
    {
        public int Value { get; }

        public AnotherDomainEvent(int value) => Value = value;
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldGenerateUniqueEventId()
    {
        // Arrange & Act
        var event1 = new TestDomainEvent("test1");
        var event2 = new TestDomainEvent("test2");

        // Assert
        Assert.NotEqual(Guid.Empty, event1.EventId);
        Assert.NotEqual(Guid.Empty, event2.EventId);
        Assert.NotEqual(event1.EventId, event2.EventId);
    }

    [Fact]
    public void Constructor_ShouldSetOccurredOnUtcToCurrentTime()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var domainEvent = new TestDomainEvent("test");

        // Assert
        var after = DateTime.UtcNow;
        Assert.InRange(domainEvent.OccurredOnUtc, before, after);
    }

    [Fact]
    public void Constructor_WithTimestamp_ShouldUseProvidedTimestamp()
    {
        // Arrange
        var specificTime = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);

        // Act
        var domainEvent = new TestDomainEvent("test", specificTime);

        // Assert
        Assert.Equal(specificTime, domainEvent.OccurredOnUtc);
    }

    #endregion

    #region EventType Tests

    [Fact]
    public void EventType_ShouldReturnClassName()
    {
        // Arrange & Act
        var domainEvent = new TestDomainEvent("test");

        // Assert
        Assert.Equal("TestDomainEvent", domainEvent.EventType);
    }

    [Fact]
    public void EventType_ShouldBeDifferentForDifferentEventTypes()
    {
        // Arrange & Act
        var event1 = new TestDomainEvent("test");
        var event2 = new AnotherDomainEvent(42);

        // Assert
        Assert.NotEqual(event1.EventType, event2.EventType);
    }

    #endregion

    #region CorrelationId and CausationId Tests

    [Fact]
    public void CorrelationId_ShouldBeNullByDefault()
    {
        // Arrange & Act
        var domainEvent = new TestDomainEvent("test");

        // Assert
        Assert.Null(domainEvent.CorrelationId);
    }

    [Fact]
    public void CorrelationId_CanBeSetViaInitProperty()
    {
        // Arrange
        var correlationId = Guid.NewGuid();

        // Act
        var domainEvent = new TestDomainEvent("test") { CorrelationId = correlationId };

        // Assert
        Assert.Equal(correlationId, domainEvent.CorrelationId);
    }

    [Fact]
    public void CausationId_ShouldBeNullByDefault()
    {
        // Arrange & Act
        var domainEvent = new TestDomainEvent("test");

        // Assert
        Assert.Null(domainEvent.CausationId);
    }

    [Fact]
    public void CausationId_CanBeSetViaInitProperty()
    {
        // Arrange
        var causationId = Guid.NewGuid();

        // Act
        var domainEvent = new TestDomainEvent("test") { CausationId = causationId };

        // Assert
        Assert.Equal(causationId, domainEvent.CausationId);
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_ShouldReturnTrue_ForSameInstance()
    {
        // Arrange
        var domainEvent = new TestDomainEvent("test");

        // Act & Assert
        Assert.True(domainEvent.Equals(domainEvent));
    }

    [Fact]
    public void Equals_ShouldReturnFalse_ForNull()
    {
        // Arrange
        var domainEvent = new TestDomainEvent("test");

        // Act & Assert
        Assert.False(domainEvent.Equals(null));
    }

    [Fact]
    public void Equals_ShouldReturnFalse_ForDifferentEvents()
    {
        // Arrange
        var event1 = new TestDomainEvent("test");
        var event2 = new TestDomainEvent("test");

        // Act & Assert (different EventIds)
        Assert.False(event1.Equals(event2));
    }

    [Fact]
    public void EqualityOperator_ShouldReturnTrue_ForSameInstance()
    {
        // Arrange
        var domainEvent = new TestDomainEvent("test");
        var sameReference = domainEvent;

        // Act & Assert
        Assert.True(domainEvent == sameReference);
    }

    [Fact]
    public void EqualityOperator_ShouldReturnTrue_ForBothNull()
    {
        // Arrange
        DomainEventBase? event1 = null;
        DomainEventBase? event2 = null;

        // Act & Assert
        Assert.True(event1 == event2);
    }

    [Fact]
    public void InequalityOperator_ShouldReturnTrue_ForDifferentEvents()
    {
        // Arrange
        var event1 = new TestDomainEvent("test");
        var event2 = new TestDomainEvent("test");

        // Act & Assert
        Assert.True(event1 != event2);
    }

    #endregion

    #region GetHashCode Tests

    [Fact]
    public void GetHashCode_ShouldBeConsistent()
    {
        // Arrange
        var domainEvent = new TestDomainEvent("test");

        // Act
        var hash1 = domainEvent.GetHashCode();
        var hash2 = domainEvent.GetHashCode();

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void GetHashCode_ShouldBeDifferent_ForDifferentEvents()
    {
        // Arrange
        var event1 = new TestDomainEvent("test");
        var event2 = new TestDomainEvent("test");

        // Act & Assert (highly likely to be different due to different EventIds)
        Assert.NotEqual(event1.GetHashCode(), event2.GetHashCode());
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ShouldIncludeEventTypeAndEventId()
    {
        // Arrange
        var domainEvent = new TestDomainEvent("test");

        // Act
        var result = domainEvent.ToString();

        // Assert
        Assert.Contains("TestDomainEvent", result);
        Assert.Contains(domainEvent.EventId.ToString(), result);
    }

    #endregion

    #region IDomainEvent Interface Tests

    [Fact]
    public void DomainEventBase_ShouldImplementIDomainEvent()
    {
        // Arrange & Act
        var domainEvent = new TestDomainEvent("test");

        // Assert
        Assert.IsAssignableFrom<IDomainEvent>(domainEvent);
    }

    [Fact]
    public void IDomainEvent_Properties_ShouldBeAccessible()
    {
        // Arrange
        IDomainEvent domainEvent = new TestDomainEvent("test");

        // Assert
        Assert.NotEqual(Guid.Empty, domainEvent.EventId);
        Assert.NotEqual(default, domainEvent.OccurredOnUtc);
        Assert.Equal("TestDomainEvent", domainEvent.EventType);
    }

    #endregion
}
