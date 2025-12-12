namespace Craft.Jobs;

/// <summary>
/// Helper class for common cron expressions.
/// </summary>
public static class CronExpressions
{
    /// <summary>
    /// Run every minute.
    /// </summary>
    public const string EveryMinute = "* * * * *";

    /// <summary>
    /// Run every 5 minutes.
    /// </summary>
    public const string Every5Minutes = "*/5 * * * *";

    /// <summary>
    /// Run every 15 minutes.
    /// </summary>
    public const string Every15Minutes = "*/15 * * * *";

    /// <summary>
    /// Run every 30 minutes.
    /// </summary>
    public const string Every30Minutes = "*/30 * * * *";

    /// <summary>
    /// Run every hour.
    /// </summary>
    public const string Hourly = "0 * * * *";

    /// <summary>
    /// Run daily at midnight.
    /// </summary>
    public const string Daily = "0 0 * * *";

    /// <summary>
    /// Run daily at 1 AM.
    /// </summary>
    public const string DailyAt1AM = "0 1 * * *";

    /// <summary>
    /// Run daily at 2 AM.
    /// </summary>
    public const string DailyAt2AM = "0 2 * * *";

    /// <summary>
    /// Run weekly on Sunday at midnight.
    /// </summary>
    public const string Weekly = "0 0 * * 0";

    /// <summary>
    /// Run monthly on the 1st at midnight.
    /// </summary>
    public const string Monthly = "0 0 1 * *";

    /// <summary>
    /// Run on weekdays at 9 AM.
    /// </summary>
    public const string WeekdaysAt9AM = "0 9 * * 1-5";

    /// <summary>
    /// Creates a cron expression for running every N minutes.
    /// </summary>
    public static string EveryNMinutes(int minutes)
    {
        if (minutes < 1 || minutes > 59)
            throw new ArgumentException("Minutes must be between 1 and 59", nameof(minutes));

        return $"*/{minutes} * * * *";
    }

    /// <summary>
    /// Creates a cron expression for running every N hours.
    /// </summary>
    public static string EveryNHours(int hours)
    {
        if (hours < 1 || hours > 23)
            throw new ArgumentException("Hours must be between 1 and 23", nameof(hours));

        return $"0 */{hours} * * *";
    }

    /// <summary>
    /// Creates a cron expression for running daily at a specific hour.
    /// </summary>
    public static string DailyAtHour(int hour, int minute = 0)
    {
        if (hour < 0 || hour > 23)
            throw new ArgumentException("Hour must be between 0 and 23", nameof(hour));

        if (minute < 0 || minute > 59)
            throw new ArgumentException("Minute must be between 0 and 59", nameof(minute));

        return $"{minute} {hour} * * *";
    }
}
