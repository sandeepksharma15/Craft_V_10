using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Craft.Data;

/// <summary>
/// Value converter for ensuring all <see cref="DateTime"/> values are treated as UTC when stored or retrieved from the database.
/// </summary>
public class DateTimeToDateTimeUtc : ValueConverter<DateTime, DateTime>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DateTimeToDateTimeUtc"/> class, which ensures that <see cref="DateTime"/> values are converted to UTC kind.
    /// </summary>
    /// <remarks>
    /// This class is designed to handle <see cref="DateTime"/> values by specifying their <see cref="DateTimeKind"/> as UTC.
    /// It can be used in scenarios where consistent handling of UTC <see cref="DateTime"/> values is required.
    /// </remarks>

    // Convert to UTC kind when saving to the database; return as-is when reading from the database.
    public DateTimeToDateTimeUtc() : base(dateTime =>
        DateTime.SpecifyKind(dateTime, DateTimeKind.Utc), dateTime => dateTime)
    {
    }
}
