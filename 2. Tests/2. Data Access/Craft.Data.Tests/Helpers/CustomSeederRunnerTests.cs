using Craft.Value.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Craft.Value.Tests.Helpers;

public class CustomSeederRunnerTests
{
    [Fact]
    public async Task RunSeedersAsync_NoSeeders_DoesNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        var provider = services.BuildServiceProvider();
        var runner = new CustomSeederRunner(provider);
        var token = CancellationToken.None;

        // Act / Assert
        await runner.RunSeedersAsync(token);
    }

    [Fact]
    public async Task RunSeedersAsync_SingleSeeder_InvokesInitializeOnceWithToken()
    {
        // Arrange
        var token = new CancellationTokenSource().Token;
        var seederMock = new Mock<ICustomSeeder>(MockBehavior.Strict);
        seederMock
            .Setup(s => s.InitializeAsync(token))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var services = new ServiceCollection();
        services.AddSingleton(seederMock.Object);
        var provider = services.BuildServiceProvider();
        var runner = new CustomSeederRunner(provider);

        // Act
        await runner.RunSeedersAsync(token);

        // Assert
        seederMock.Verify(s => s.InitializeAsync(token), Times.Once);
    }

    [Fact]
    public async Task RunSeedersAsync_MultipleSeeders_InvokesAllInRegistrationOrder()
    {
        // Arrange
        var callOrder = new List<int>();
        var token = CancellationToken.None;

        var first = new Mock<ICustomSeeder>();
        first.Setup(s => s.InitializeAsync(token))
            .Callback(() => callOrder.Add(1))
            .Returns(Task.CompletedTask);

        var second = new Mock<ICustomSeeder>();
        second.Setup(s => s.InitializeAsync(token))
            .Callback(() => callOrder.Add(2))
            .Returns(Task.CompletedTask);

        var third = new Mock<ICustomSeeder>();
        third.Setup(s => s.InitializeAsync(token))
            .Callback(() => callOrder.Add(3))
            .Returns(Task.CompletedTask);

        var services = new ServiceCollection();
        services.AddSingleton(first.Object);
        services.AddSingleton(second.Object);
        services.AddSingleton(third.Object);
        var provider = services.BuildServiceProvider();
        var runner = new CustomSeederRunner(provider);

        // Act
        await runner.RunSeedersAsync(token);

        // Assert
        Assert.Equal(new[] { 1, 2, 3 }, callOrder);
    }

    [Fact]
    public async Task RunSeedersAsync_SeederThrows_ExceptionPropagatesAndStopsLaterSeeders()
    {
        // Arrange
        var token = CancellationToken.None;
        var first = new Mock<ICustomSeeder>();
        first.Setup(s => s.InitializeAsync(token))
            .ThrowsAsync(new InvalidOperationException("boom"));

        var second = new Mock<ICustomSeeder>();
        var secondCalled = false;
        second.Setup(s => s.InitializeAsync(token))
            .Callback(() => secondCalled = true)
            .Returns(Task.CompletedTask);

        var services = new ServiceCollection();
        services.AddSingleton(first.Object);
        services.AddSingleton(second.Object);
        var provider = services.BuildServiceProvider();
        var runner = new CustomSeederRunner(provider);

        // Act / Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => runner.RunSeedersAsync(token));
        Assert.Equal("boom", ex.Message);
        Assert.False(secondCalled);
    }

    [Fact]
    public async Task RunSeedersAsync_CancellationTokenPassedThrough()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var token = cts.Token;
        var seederMock = new Mock<ICustomSeeder>(MockBehavior.Strict);
        seederMock
            .Setup(s => s.InitializeAsync(token))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var services = new ServiceCollection();
        services.AddSingleton(seederMock.Object);
        var provider = services.BuildServiceProvider();
        var runner = new CustomSeederRunner(provider);

        // Act
        await runner.RunSeedersAsync(token);

        // Assert
        seederMock.Verify();
    }

    [Fact]
    public async Task RunSeedersAsync_CancelledToken_StillPassedAndSeederCanReact()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();
        var token = cts.Token;

        var seederMock = new Mock<ICustomSeeder>();
        seederMock
            .Setup(s => s.InitializeAsync(token))
            .Returns(async () =>
            {
                // Simulate respecting cancellation
                if (token.IsCancellationRequested)
                    await Task.FromCanceled(token);
            });

        var services = new ServiceCollection();
        services.AddSingleton(seederMock.Object);
        var provider = services.BuildServiceProvider();
        var runner = new CustomSeederRunner(provider);

        // Act / Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => runner.RunSeedersAsync(token));
        seederMock.Verify(s => s.InitializeAsync(token), Times.Once);
    }
}

