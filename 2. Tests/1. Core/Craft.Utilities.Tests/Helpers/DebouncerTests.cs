using Craft.Utilities.Helpers;

namespace Craft.Utilities.Tests.Helpers;

public class DebouncerTests : IDisposable
{
    private readonly Debouncer _debouncer = new();

    [Fact]
    public async Task Debounce_ExecutesActionOnce_AfterInterval()
    {
        int callCount = 0;
        Task action() { callCount++; return Task.CompletedTask; }

        _debouncer.Debounce(100, action);
        _debouncer.Debounce(100, action);
        _debouncer.Debounce(100, action);

        await Task.Delay(200);

        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task Throttle_ExecutesActionAtMostOncePerInterval()
    {
        int callCount = 0;
        Task action() { callCount++; return Task.CompletedTask; }

        _debouncer.Throttle(100, action);
        _debouncer.Throttle(100, action);
        _debouncer.Throttle(100, action);

        await Task.Delay(200);

        Assert.Equal(1, callCount);

        // Wait for interval to pass, then call again
        _debouncer.Throttle(100, action);

        await Task.Delay(200);

        Assert.Equal(2, callCount);
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes_Safely()
    {
        _debouncer.Dispose();
        _debouncer.Dispose(); // Should not throw
    }

    [Fact]
    public async Task Debounce_DoesNotExecuteAction_AfterDispose()
    {
        int callCount = 0;
        Task action() { callCount++; return Task.CompletedTask; }

        _debouncer.Debounce(100, action);
        _debouncer.Dispose();

        await Task.Delay(200);

        Assert.Equal(0, callCount);
    }

    [Fact]
    public async Task Debounce_HandlesExceptionGracefully()
    {
        int callCount = 0;
        Task action()
        {
            callCount++;
            throw new InvalidOperationException("Test exception");
        }

        _debouncer.Debounce(100, action);

        await Task.Delay(200);

        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task Throttle_HandlesExceptionGracefully()
    {
        int callCount = 0;
        Task action()
        {
            callCount++;
            throw new InvalidOperationException("Test exception");
        }

        _debouncer.Throttle(100, action);

        await Task.Delay(200);

        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task Debounce_MultipleRapidCalls_ExecutesOnlyOnce()
    {
        int callCount = 0;
        Task action() { callCount++; return Task.CompletedTask; }

        for (int i = 0; i < 10; i++)
        {
            _debouncer.Debounce(100, action);
            await Task.Delay(10);
        }

        await Task.Delay(200);

        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task Throttle_ConsecutiveCalls_RespectsInterval()
    {
        int callCount = 0;
        Task action() { callCount++; return Task.CompletedTask; }

        _debouncer.Throttle(100, action);
        await Task.Delay(150);
        _debouncer.Throttle(100, action);
        await Task.Delay(150);
        _debouncer.Throttle(100, action);
        await Task.Delay(150);

        Assert.Equal(3, callCount);
    }

    [Fact]
    public async Task Debounce_WithTaskCancelledException_DoesNotPropagate()
    {
        int callCount = 0;
        Task action()
        {
            callCount++;
            throw new TaskCanceledException();
        }

        _debouncer.Debounce(100, action);

        await Task.Delay(200);

        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task Throttle_WithTaskCancelledException_DoesNotPropagate()
    {
        int callCount = 0;
        Task action()
        {
            callCount++;
            throw new TaskCanceledException();
        }

        _debouncer.Throttle(100, action);

        await Task.Delay(200);

        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task Debounce_ConcurrentCalls_ThreadSafe()
    {
        int callCount = 0;
        Task action() { Interlocked.Increment(ref callCount); return Task.CompletedTask; }

        var tasks = Enumerable.Range(0, 20).Select(_ => Task.Run(() => _debouncer.Debounce(100, action)));
        await Task.WhenAll(tasks);

        await Task.Delay(200);

        Assert.Equal(1, callCount);
    }

    public void Dispose()
    {
        _debouncer.Dispose();
    }
}
