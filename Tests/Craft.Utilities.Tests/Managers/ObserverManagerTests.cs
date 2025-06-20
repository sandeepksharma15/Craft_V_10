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
            if (observer == observer2)
                throw new Exception("Notification failed");

            return Task.CompletedTask;
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
}
