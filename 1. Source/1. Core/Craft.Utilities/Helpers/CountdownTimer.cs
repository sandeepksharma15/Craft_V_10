using System.Timers;

namespace Craft.Utilities.Helpers;

public class CountdownTimer : IDisposable
{
    private readonly Lock _lock = new();
    private System.Timers.Timer? _timer;
    private readonly int _intervalMs;
    private readonly int _totalTicks;
    private int _currentTick;
    private bool _disposed;
    private bool _running;

    public event Action<int>? OnTick;
    public event Action? OnElapsed;

    /// <summary>
    /// Initializes a new CountdownTimer.
    /// </summary>
    /// <param name="timeoutSeconds">Total duration in seconds.</param>
    /// <param name="tickCount">Number of ticks (default 100 for percent granularity).</param>
    public CountdownTimer(int timeoutSeconds, int tickCount = 100)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(timeoutSeconds);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tickCount);

        _intervalMs = timeoutSeconds * 1000 / tickCount;
        _totalTicks = tickCount;
        _currentTick = 0;
        _timer = new System.Timers.Timer(_intervalMs);
        _timer.Elapsed += HandleTick;
        _timer.AutoReset = true;
        _timer.Enabled = false;
    }

    public void Start()
    {
        lock (_lock)
        {
            ObjectDisposedException.ThrowIf(_disposed, nameof(CountdownTimer));

            if (_running) return;

            _running = true;
            _timer?.Start();
        }
    }

    public void Stop()
    {
        lock (_lock)
        {
            ObjectDisposedException.ThrowIf(_disposed, nameof(CountdownTimer));

            if (!_running) return;

            _timer?.Stop();
            _running = false;
        }
    }

    public void Reset()
    {
        lock (_lock)
        {
            ObjectDisposedException.ThrowIf(_disposed, nameof(CountdownTimer));

            Stop();
            _currentTick = 0;
        }
    }

    private void HandleTick(object? sender, ElapsedEventArgs args)
    {
        int tick;
        bool finished = false;

        lock (_lock)
        {
            if (_disposed) return;

            _currentTick++;
            tick = _currentTick;

            if (_currentTick >= _totalTicks)
            {
                finished = true;
                _timer?.Stop();
                _running = false;
            }
        }
        // Fire events outside lock
        try { OnTick?.Invoke(tick); } catch { /* swallow */ }

        if (finished)
        {
            try { OnElapsed?.Invoke(); } catch { /* swallow */ }
        }
    }

    void IDisposable.Dispose() => Dispose();

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            lock (_lock)
            {
                _timer?.Stop();
                _timer?.Dispose();
                _timer = null;
                _running = false;
            }
        }

        _disposed = true;
    }
}
