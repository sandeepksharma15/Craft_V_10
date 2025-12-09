namespace Craft.Utilities.Helpers;

/// <summary>
/// Provides debouncing and throttling functionality for asynchronous actions.
/// </summary>
public class Debouncer : IDisposable
{
    private System.Timers.Timer? _timer;
    private DateTime _lastActionTime = DateTime.UtcNow.AddYears(-1);
    private readonly Lock _lock = new();
    private bool _disposed;

    /// <summary>
    /// Debounces the specified action, ensuring it only executes after the interval has elapsed without further calls.
    /// </summary>
    /// <param name="interval">The debounce interval in milliseconds.</param>
    /// <param name="action">The asynchronous action to execute.</param>
    public void Debounce(int interval, Func<Task> action)
    {
        lock (_lock)
        {
            DisposeTimer();

            _timer = new System.Timers.Timer(interval) { AutoReset = false, Enabled = false };

            _timer.Elapsed += async (s, e) =>
            {
                lock (_lock)
                {
                    DisposeTimer();
                }

                try
                {
                    await action();
                }
                catch (TaskCanceledException) { /* Swallow */ }
                catch (Exception) { /* Optionally log */ }
            };

            _timer.Start();
        }
    }

    /// <summary>
    /// Throttles the specified action, ensuring it only executes at most once per interval.
    /// </summary>
    /// <param name="interval">The throttle interval in milliseconds.</param>
    /// <param name="action">The asynchronous action to execute.</param>
    public void Throttle(int interval, Func<Task> action)
    {
        lock (_lock)
        {
            var now = DateTime.UtcNow;

            var elapsed = (int)(now - _lastActionTime).TotalMilliseconds;

            var delay = Math.Max(1, interval - elapsed);

            DisposeTimer();

            _timer = new System.Timers.Timer(delay) { AutoReset = false, Enabled = false };

            _timer.Elapsed += async (s, e) =>
            {
                lock (_lock)
                {
                    DisposeTimer();
                    _lastActionTime = DateTime.UtcNow;
                }

                try
                {
                    await action();
                }
                catch (TaskCanceledException) { /* Swallow */ }
                catch (Exception) { /* Optionally log */ }
            };
            _timer.Start();
        }
    }

    private void DisposeTimer()
    {
        if (_timer != null)
        {
            _timer.Stop();
            _timer.Dispose();
            _timer = null;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        lock (_lock)
        {
            DisposeTimer();
            _disposed = true;
        }

        GC.SuppressFinalize(this);
    }
}
