#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class DateTimeExtensions
{
    extension(DateTime dateTime)
    {
        /// <summary>
        /// Removes the time component from the current <see cref="DateTime"/> instance, returning a new <see cref="DateTime"/> with the time set to 00:00:00.000.
        /// </summary>
        /// <returns>A <see cref="DateTime"/> representing the same date as the current instance, but with the time component cleared.</returns>
        public DateTime ClearTime() =>
            new(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, 0, dateTime.Kind);

        /// <summary>
        /// Converts the current <see cref="DateTime"/> instance to have a <see cref="DateTimeKind"/> of <see
        /// cref="DateTimeKind.Utc"/>.
        /// </summary>
        /// <remarks>If the <see cref="DateTime.Kind"/> property of the current instance is already <see
        /// cref="DateTimeKind.Utc"/>, the original <see cref="DateTime"/> is returned unchanged. Otherwise, a new <see
        /// cref="DateTime"/> instance with the same date and time value but with <see cref="DateTimeKind.Utc"/> is
        /// returned.</remarks>
        /// <returns>A <see cref="DateTime"/> instance with the same date and time value as the current instance, but with a <see
        /// cref="DateTimeKind"/> of <see cref="DateTimeKind.Utc"/>.</returns>
        public DateTime SetKindUtc()
        {
            if (dateTime.Kind == DateTimeKind.Utc)
                return dateTime;

            return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        }
    }

    extension(DateTime? dateTime)
    {
        /// <summary>
        /// Converts the value of the nullable <see cref="DateTime"/> to UTC if it has a value.
        /// </summary>
        /// <returns>A <see cref="DateTime"/> with the <see cref="DateTimeKind"/> set to <see cref="DateTimeKind.Utc"/>,  or <see
        /// langword="null"/> if the nullable <see cref="DateTime"/> has no value.</returns>
        public DateTime? SetKindUtc()
        {
            return dateTime.HasValue ? dateTime.Value.SetKindUtc() : null;
        }
    }

    extension(DayOfWeek dayOfWeek)
    {
        /// <summary>
        /// Determines whether the current day is a weekday.
        /// </summary>
        /// <returns><see langword="true"/> if the current day is Monday, Tuesday, Wednesday, Thursday, or Friday; otherwise, <see langword="false"/>.</returns>
        public bool IsWeekday() =>
            dayOfWeek is DayOfWeek.Monday or DayOfWeek.Tuesday or DayOfWeek.Wednesday or DayOfWeek.Thursday or DayOfWeek.Friday;

        /// <summary>
        /// Determines whether the current day is a weekend.
        /// </summary>
        /// <returns><see langword="true"/> if the current day is either Saturday or Sunday; otherwise, <see langword="false"/>.</returns>
        public bool IsWeekend() =>
            dayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
    }
}
