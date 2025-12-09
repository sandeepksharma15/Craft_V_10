#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class DateTimeExtensions
{
    /// <summary>
    /// Returns a new DateTime with the time component set to 00:00:00.000.
    /// </summary>
    public static DateTime ClearTime(this DateTime dateTime)
        => new(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, 0, dateTime.Kind);

    /// <summary>
    /// Determines if the specified DayOfWeek is a weekday (Monday through Friday).
    /// </summary>
    public static bool IsWeekday(this DayOfWeek dayOfWeek)
        => dayOfWeek is >= DayOfWeek.Monday and <= DayOfWeek.Friday;

    /// <summary>
    /// Determines if the specified DayOfWeek is a weekend (Saturday or Sunday).
    /// </summary>
    public static bool IsWeekend(this DayOfWeek dayOfWeek)
        => dayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    /// <summary>
    /// Sets the DateTimeKind of a nullable DateTime to Utc, if it has a value.
    /// </summary>
    public static DateTime? SetKindUtc(this DateTime? dateTime)
        => dateTime.HasValue ? dateTime.Value.SetKindUtc() : null;

    /// <summary>
    /// Sets the DateTimeKind of a DateTime to Utc, if it is not already Utc.
    /// </summary>
    public static DateTime SetKindUtc(this DateTime dateTime)
        => dateTime.Kind == DateTimeKind.Utc
            ? dateTime
            : DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
}
