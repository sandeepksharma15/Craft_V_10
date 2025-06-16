namespace Craft.Utilities.Helpers;

public class Debouncer
{
    System.Timers.Timer? timer;
    DateTime timerStarted { get; set; } = DateTime.UtcNow.AddYears(-1);

    public void Debounce(int interval, Func<Task> action)
    {
        timer?.Stop();
        timer = null!;

        timer = new System.Timers.Timer() { Interval = interval, Enabled = false, AutoReset = false };
        timer.Elapsed += (s, e) =>
        {
            if (timer == null)
                return;

            timer?.Stop();
            timer = null;

            try
            {
                Task.Run(action);
            }
            catch (TaskCanceledException)
            {
                //
            }
        };

        timer.Start();
    }

    public void Throttle(int interval, Func<Task> action)
    {
        timer?.Stop();
        timer = null;

        var curTime = DateTime.UtcNow;

        if (curTime.Subtract(timerStarted).TotalMilliseconds < interval)
            interval -= (int)curTime.Subtract(timerStarted).TotalMilliseconds;

        timer = new System.Timers.Timer() { Interval = interval, Enabled = false, AutoReset = false };
        timer.Elapsed += (s, e) =>
        {
            if (timer == null)
                return;

            timer?.Stop();
            timer = null;

            try
            {
                Task.Run(action);
            }
            catch (TaskCanceledException)
            {
                //
            }
        };

        timer.Start();
        timerStarted = curTime;
    }
}
