#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class DateTimeExtensions
{
    public static DateTime ClearTime(this DateTime dateTime)
    {
        return dateTime.Subtract(new TimeSpan(0,
            dateTime.Hour,
            dateTime.Minute,
            dateTime.Second,
            dateTime.Millisecond));
    }

    public static bool IsWeekday(this DayOfWeek dayOfWeek)
    {
        return dayOfWeek.IsIn(DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday);
    }

    public static bool IsWeekend(this DayOfWeek dayOfWeek)
    {
        return dayOfWeek.IsIn(DayOfWeek.Saturday, DayOfWeek.Sunday);
    }

    public static DateTime? SetKindUtc(this DateTime? dateTime)
    {
        return dateTime.HasValue ? dateTime.Value.SetKindUtc() : null;
    }

    public static DateTime SetKindUtc(this DateTime dateTime)
    {
        if (dateTime.Kind == DateTimeKind.Utc)
            return dateTime;

        return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
    }
}
