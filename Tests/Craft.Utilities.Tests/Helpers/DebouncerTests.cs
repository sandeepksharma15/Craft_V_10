using Craft.Utilities.Helpers;

namespace Craft.Utilities.Tests.Helpers;

public class DebouncerTests : IDisposable
{
    private readonly Debouncer _debouncer = new();

    [Fact]
    public async Task Debounce_ExecutesActionOnce_AfterInterval()
    {
        int callCount = 0;
        Func<Task> action = () => { callCount++; return Task.CompletedTask; };

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

    public void Dispose()
    {
        _debouncer.Dispose();
    }
}
