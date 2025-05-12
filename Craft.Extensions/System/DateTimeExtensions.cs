#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class DateTimeExtensions
{
    extension(DateTime dateTime)
    {
        public DateTime ClearTime()
        {
            return dateTime.Subtract(new TimeSpan(0,
                dateTime.Hour,
                dateTime.Minute,
                dateTime.Second,
                dateTime.Millisecond));
        }

        public DateTime SetKindUtc()
        {
            if (dateTime.Kind == DateTimeKind.Utc)
                return dateTime;

            return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        }
    }

    extension (DateTime? dateTime)
    {
        public DateTime? SetKindUtc()
        {
            return dateTime.HasValue ? dateTime.Value.SetKindUtc() : null;
        }
    }

    extension (DayOfWeek dayOfWeek)
    {
        public bool IsWeekday()
        {
            return dayOfWeek.IsIn(DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday);
        }

        public bool IsWeekend()
        {
            return dayOfWeek.IsIn(DayOfWeek.Saturday, DayOfWeek.Sunday);
        }
    }
}
