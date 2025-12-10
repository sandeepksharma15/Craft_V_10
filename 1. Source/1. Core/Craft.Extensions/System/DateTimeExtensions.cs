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

    /// <summary>
    /// Calculates the age in years from the specified date to now.
    /// </summary>
    public static int CalculateAge(this DateTime dateOfBirth)
        => dateOfBirth.CalculateAge(DateTime.Today);

    /// <summary>
    /// Calculates the age in years from the specified date to a reference date.
    /// </summary>
    public static int CalculateAge(this DateTime dateOfBirth, DateTime referenceDate)
    {
        var age = referenceDate.Year - dateOfBirth.Year;

        if (referenceDate.Month < dateOfBirth.Month ||
            (referenceDate.Month == dateOfBirth.Month && referenceDate.Day < dateOfBirth.Day))
            age--;

        return age;
    }

    /// <summary>
    /// Returns the first day of the month for the specified date.
    /// </summary>
    public static DateTime StartOfMonth(this DateTime dateTime)
        => new(dateTime.Year, dateTime.Month, 1, 0, 0, 0, 0, dateTime.Kind);

    /// <summary>
    /// Returns the last day of the month for the specified date.
    /// </summary>
    public static DateTime EndOfMonth(this DateTime dateTime)
        => new DateTime(dateTime.Year, dateTime.Month, DateTime.DaysInMonth(dateTime.Year, dateTime.Month), 23, 59, 59, 999, dateTime.Kind);

    /// <summary>
    /// Returns the first day of the year for the specified date.
    /// </summary>
    public static DateTime StartOfYear(this DateTime dateTime)
        => new(dateTime.Year, 1, 1, 0, 0, 0, 0, dateTime.Kind);

    /// <summary>
    /// Returns the last day of the year for the specified date.
    /// </summary>
    public static DateTime EndOfYear(this DateTime dateTime)
        => new(dateTime.Year, 12, 31, 23, 59, 59, 999, dateTime.Kind);

    /// <summary>
    /// Returns the first day of the week (Monday) for the specified date.
    /// </summary>
    public static DateTime StartOfWeek(this DateTime dateTime, DayOfWeek startOfWeek = DayOfWeek.Monday)
    {
        var diff = (7 + (dateTime.DayOfWeek - startOfWeek)) % 7;
        return dateTime.AddDays(-diff).ClearTime();
    }

    /// <summary>
    /// Returns the last day of the week (Sunday) for the specified date.
    /// </summary>
    public static DateTime EndOfWeek(this DateTime dateTime, DayOfWeek startOfWeek = DayOfWeek.Monday)
    {
        var startDate = dateTime.StartOfWeek(startOfWeek);
        return new DateTime(startDate.Year, startDate.Month, startDate.Day, 23, 59, 59, 999, dateTime.Kind).AddDays(6);
    }

    /// <summary>
    /// Adds the specified number of business days (Monday-Friday) to the date.
    /// </summary>
    public static DateTime AddBusinessDays(this DateTime dateTime, int businessDays)
    {
        var sign = Math.Sign(businessDays);
        var unsignedDays = Math.Abs(businessDays);
        var result = dateTime;

        for (var i = 0; i < unsignedDays; i++)
        {
            do
            {
                result = result.AddDays(sign);
            }
            while (result.DayOfWeek.IsWeekend());
        }

        return result;
    }

    /// <summary>
    /// Determines whether the specified date is a weekday (Monday through Friday).
    /// </summary>
    public static bool IsWeekday(this DateTime dateTime)
        => dateTime.DayOfWeek.IsWeekday();

    /// <summary>
    /// Determines whether the specified date is a weekend (Saturday or Sunday).
    /// </summary>
    public static bool IsWeekend(this DateTime dateTime)
        => dateTime.DayOfWeek.IsWeekend();

    /// <summary>
    /// Determines whether the specified date is today.
    /// </summary>
    public static bool IsToday(this DateTime dateTime)
        => dateTime.Date == DateTime.Today;

    /// <summary>
    /// Determines whether the specified date is in the past.
    /// </summary>
    public static bool IsPast(this DateTime dateTime)
        => dateTime < DateTime.Now;

    /// <summary>
    /// Determines whether the specified date is in the future.
    /// </summary>
    public static bool IsFuture(this DateTime dateTime)
        => dateTime > DateTime.Now;

    /// <summary>
    /// Sets the time component of the DateTime to the specified values.
    /// </summary>
    public static DateTime SetTime(this DateTime dateTime, int hour, int minute = 0, int second = 0, int millisecond = 0)
        => new(dateTime.Year, dateTime.Month, dateTime.Day, hour, minute, second, millisecond, dateTime.Kind);

    /// <summary>
    /// Returns a DateTime representing the specified time on the date.
    /// </summary>
    public static DateTime At(this DateTime dateTime, TimeSpan time)
        => dateTime.ClearTime().Add(time);

    /// <summary>
    /// Determines whether the date falls between the start and end dates (inclusive).
    /// </summary>
    public static bool IsBetween(this DateTime dateTime, DateTime startDate, DateTime endDate)
        => dateTime >= startDate && dateTime <= endDate;
}
