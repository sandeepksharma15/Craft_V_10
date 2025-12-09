using Craft.Utilities.Helpers;

namespace Craft.Utilities.Tests.Helpers;

public class CountdownTimerTests
{
    [Fact]
    public void Constructor_InvalidTimeout_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CountdownTimer(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CountdownTimer(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CountdownTimer(1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CountdownTimer(1, -5));
    }

    [Fact]
    public void Start_Stop_Reset_BasicUsage_DoesNotThrow()
    {
        using var timer = new CountdownTimer(1, 10);
        timer.Start();
        timer.Stop();
        timer.Reset();
    }

    [Fact]
    public void Start_WhenDisposed_Throws()
    {
        var timer = new CountdownTimer(1, 10);
        timer.Dispose();
        Assert.Throws<ObjectDisposedException>(() => timer.Start());
    }

    [Fact]
    public void Stop_WhenDisposed_Throws()
    {
        var timer = new CountdownTimer(1, 10);
        timer.Dispose();
        Assert.Throws<ObjectDisposedException>(() => timer.Stop());
    }

    [Fact]
    public void Reset_WhenDisposed_Throws()
    {
        var timer = new CountdownTimer(1, 10);
        timer.Dispose();
        Assert.Throws<ObjectDisposedException>(() => timer.Reset());
    }

    [Fact]
    public void Dispose_MultipleTimes_DoesNotThrow()
    {
        var timer = new CountdownTimer(1, 10);
        timer.Dispose();
        timer.Dispose();
    }

    [Fact]
    public void OnTick_IsCalled_CorrectNumberOfTimes()
    {
        using var timer = new CountdownTimer(1, 5);
        int tickCount = 0;
        timer.OnTick += _ => Interlocked.Increment(ref tickCount);
        timer.Start();
        Thread.Sleep(1200); // Wait for timer to finish
        Assert.Equal(5, tickCount);
    }

    [Fact]
    public void OnElapsed_IsCalled_WhenComplete()
    {
        using var timer = new CountdownTimer(1, 3);
        bool elapsed = false;
        timer.OnElapsed += () => elapsed = true;
        timer.Start();
        Thread.Sleep(1100);
        Assert.True(elapsed);
    }

    [Fact]
    public void Stop_PreventsFurtherTicks()
    {
        using var timer = new CountdownTimer(1, 10);
        int tickCount = 0;
        timer.OnTick += _ => Interlocked.Increment(ref tickCount);
        timer.Start();
        Thread.Sleep(200); // Let a few ticks happen
        timer.Stop();
        int afterStop = tickCount;
        Thread.Sleep(500);
        Assert.Equal(afterStop, tickCount); // No new ticks after stop
    }

    [Fact]
    public void Reset_ResetsTickCount()
    {
        using var timer = new CountdownTimer(1, 5);
        int tickCount = 0;
        timer.OnTick += _ => Interlocked.Increment(ref tickCount);
        timer.Start();
        Thread.Sleep(300);
        timer.Reset();
        int afterReset = tickCount;
        timer.Start();
        Thread.Sleep(1200);
        Assert.True(tickCount > afterReset);
    }

    [Fact]
    public void OnTick_OnElapsed_AreThreadSafe()
    {
        using var timer = new CountdownTimer(1, 10);
        int tickCount = 0;
        int elapsedCount = 0;
        timer.OnTick += _ => Interlocked.Increment(ref tickCount);
        timer.OnElapsed += () => Interlocked.Increment(ref elapsedCount);
        timer.Start();
        Thread.Sleep(1200);
        Assert.Equal(10, tickCount);
        Assert.Equal(1, elapsedCount);
    }
}
