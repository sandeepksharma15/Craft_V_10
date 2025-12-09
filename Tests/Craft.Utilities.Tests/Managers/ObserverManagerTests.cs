using Craft.Utilities.Managers;
using Microsoft.Extensions.Logging;
using Moq;

namespace Craft.Utilities.Tests.Managers;

public class ObserverManagerTests
{
    private readonly Mock<ILogger<ObserverManager<int, string>>> loggerMock;
    private readonly ObserverManager<int, string> observerManager;

    public ObserverManagerTests()
    {
        loggerMock = new Mock<ILogger<ObserverManager<int, string>>>();
        observerManager = new ObserverManager<int, string>(loggerMock.Object);
    }

    [Fact]
    public void Clear_RemovesAllObservers()
    {
        // Arrange
        observerManager.Clear();
        observerManager.Subscribe(1, "Observer1");
        observerManager.Subscribe(2, "Observer2");
        observerManager.Subscribe(3, "Observer3");

        // Act
        observerManager.Clear();

        // Assert
        Assert.Empty(observerManager.Observers);
    }

    [Fact]
    public void Count_ReturnsCorrectNumberOfObservers()
    {
        // Arrange
        observerManager.Clear();
        observerManager.Clear();
        observerManager.Subscribe(1, "Observer1");
        observerManager.Subscribe(2, "Observer2");
        observerManager.Subscribe(3, "Observer3");

        // Act
        var count = observerManager.Count;

        // Assert
        Assert.Equal(3, count);
    }

    [Fact]
    public void GetEnumerator_ReturnsAllObservers()
    {
        // Arrange
        observerManager.Clear();
        observerManager.Subscribe(1, "Observer1");
        observerManager.Subscribe(2, "Observer2");
        observerManager.Subscribe(3, "Observer3");

        // Act
        var observers = observerManager.ToList();

        // Assert
        Assert.Equal(3, observers.Count);
        Assert.Contains("Observer1", observers);
        Assert.Contains("Observer2", observers);
        Assert.Contains("Observer3", observers);
    }

    [Fact]
    public async Task Notify_RemovesErroredObservers()
    {
        // Arrange
        observerManager.Clear();
        const string observer1 = "Observer1";
        const string observer2 = "Observer2";
        const string observer3 = "Observer3";

        observerManager.Subscribe(1, observer1);
        observerManager.Subscribe(2, observer2);
        observerManager.Subscribe(3, observer3);

        // Act
        static Task NotificationAsync(string observer)
        {
            return observer == observer2 ? throw new Exception("Notification failed") : Task.CompletedTask;
        }

        await observerManager.NotifyAsync(NotificationAsync);

        // Assert
        Assert.Equal(2, observerManager.Count);
        Assert.True(observerManager.Observers.ContainsKey(1));
        Assert.False(observerManager.Observers.ContainsKey(2)); // Observer2 should be removed
        Assert.True(observerManager.Observers.ContainsKey(3));
    }

    [Fact]
    public async Task NotifyAsync_NotifiesAllObservers()
    {
        // Arrange
        var notificationCalledCount = 0;
        observerManager.Clear();
        observerManager.Subscribe(1, "Observer1");
        observerManager.Subscribe(2, "Observer2");

        // Act
        async Task NotificationAsync(string _)
        {
            notificationCalledCount++;
            await Task.CompletedTask;
        }

        await observerManager.NotifyAsync(NotificationAsync);

        // Assert
        Assert.Equal(2, notificationCalledCount);
    }

    [Fact]
    public void Subscribe_AddsNewObserver()
    {
        // Arrange
        observerManager.Clear();

        // Act
        observerManager.Subscribe(1, "Observer1");

        // Assert
        Assert.Equal(1, observerManager.Count);
        Assert.True(observerManager.Observers.ContainsKey(1));
    }

    [Fact]
    public void Subscribe_UpdatesExistingObserver()
    {
        // Arrange
        observerManager.Clear();
        observerManager.Subscribe(1, "Observer1");

        // Act
        observerManager.Subscribe(1, "Observer2");

        // Assert
        Assert.Equal(1, observerManager.Count);
        Assert.True(observerManager.Observers.ContainsKey(1));
    }

    [Fact]
    public void Unsubscribe_RemovesObserver()
    {
        // Arrange
        observerManager.Clear();
        observerManager.Subscribe(1, "Observer1");

        // Act
        observerManager.Unsubscribe(1);

        // Assert
        Assert.Equal(0, observerManager.Count);
        Assert.False(observerManager.Observers.ContainsKey(1));
    }

    [Fact]
    public async Task NotifyAsync_WithPredicate_OnlyNotifiesMatchingObservers()
    {
        // Arrange
        observerManager.Clear();
        var notificationCalls = new Dictionary<int, int>
        {
            { 1, 0 },
            { 2, 0 },
            { 3, 0 }
        };

        observerManager.Subscribe(1, "Observer1");
        observerManager.Subscribe(2, "Observer2");
        observerManager.Subscribe(3, "Observer3");

        // Act
        Task NotificationAsync(string observer)
        {
            var id = int.Parse(observer.Replace("Observer", ""));
            notificationCalls[id]++;
            return Task.CompletedTask;
        }

        await observerManager.NotifyAsync(NotificationAsync, (id, _) => id == 1 || id == 3);

        // Assert
        Assert.Equal(1, notificationCalls[1]);
        Assert.Equal(0, notificationCalls[2]);
        Assert.Equal(1, notificationCalls[3]);
    }

    [Fact]
    public async Task NotifyAsync_WithPredicateReturningFalse_DoesNotNotifyAnyObservers()
    {
        // Arrange
        observerManager.Clear();
        var notificationCallCount = 0;

        observerManager.Subscribe(1, "Observer1");
        observerManager.Subscribe(2, "Observer2");

        // Act
        Task NotificationAsync(string _)
        {
            notificationCallCount++;
            return Task.CompletedTask;
        }

        await observerManager.NotifyAsync(NotificationAsync, (_, _) => false);

        // Assert
        Assert.Equal(0, notificationCallCount);
    }

    [Fact]
    public async Task NotifyAsync_WithPredicateUsingObserverValue_FiltersCorrectly()
    {
        // Arrange
        observerManager.Clear();
        var notifiedObservers = new List<string>();

        observerManager.Subscribe(1, "Alice");
        observerManager.Subscribe(2, "Bob");
        observerManager.Subscribe(3, "Andrew");

        // Act
        Task NotificationAsync(string observer)
        {
            notifiedObservers.Add(observer);
            return Task.CompletedTask;
        }

        await observerManager.NotifyAsync(NotificationAsync, (_, observer) => observer.StartsWith("A"));

        // Assert
        Assert.Equal(2, notifiedObservers.Count);
        Assert.Contains("Alice", notifiedObservers);
        Assert.Contains("Andrew", notifiedObservers);
        Assert.DoesNotContain("Bob", notifiedObservers);
    }

    [Fact]
    public async Task NotifyAsync_WithNullPredicate_NotifiesAllObservers()
    {
        // Arrange
        observerManager.Clear();
        var notificationCallCount = 0;

        observerManager.Subscribe(1, "Observer1");
        observerManager.Subscribe(2, "Observer2");
        observerManager.Subscribe(3, "Observer3");

        // Act
        Task NotificationAsync(string _)
        {
            notificationCallCount++;
            return Task.CompletedTask;
        }

        await observerManager.NotifyAsync(NotificationAsync, null);

        // Assert
        Assert.Equal(3, notificationCallCount);
    }

    [Fact]
    public async Task NotifyAsync_WithPredicateAndException_RemovesOnlyFailedObservers()
    {
        // Arrange
        observerManager.Clear();
        observerManager.Subscribe(1, "Observer1");
        observerManager.Subscribe(2, "Observer2");
        observerManager.Subscribe(3, "Observer3");

        // Act
        Task NotificationAsync(string observer)
        {
            if (observer == "Observer2")
                throw new Exception("Notification failed");
            return Task.CompletedTask;
        }

        await observerManager.NotifyAsync(NotificationAsync, (id, _) => id == 1 || id == 2);

        // Assert
        Assert.Equal(2, observerManager.Count);
        Assert.True(observerManager.Observers.ContainsKey(1));
        Assert.False(observerManager.Observers.ContainsKey(2));
        Assert.True(observerManager.Observers.ContainsKey(3));
    }
}
