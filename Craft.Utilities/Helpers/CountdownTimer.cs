using System.Timers;

namespace Craft.Utilities.Helpers;

public class CountdownTimer : IDisposable
{
    private System.Timers.Timer _timer;
    private readonly int _timeout;
    private int _percentComplete;
    private bool _disposed;

    public Action<int> OnTick;
    public Action OnElapsed;

    public CountdownTimer(int timeout)
    {
        _timeout = timeout * 1000 / 100;
        _percentComplete = 0;
        SetupTimer();
    }

    public void Start()
    {
        _timer?.Start();
    }

    private void SetupTimer()
    {
        _timer = new System.Timers.Timer(_timeout);
        _timer.Elapsed += HandleTick;
        _timer.AutoReset = false;
    }

    private void HandleTick(object sender, ElapsedEventArgs args)
    {
        _percentComplete++;
        OnTick?.Invoke(_percentComplete);

        if (_percentComplete == 100)
            OnElapsed?.Invoke();
        else
        {
            SetupTimer();
            Start();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _timer?.Dispose();
            _timer = null;
        }

        _disposed = true;
    }
}
