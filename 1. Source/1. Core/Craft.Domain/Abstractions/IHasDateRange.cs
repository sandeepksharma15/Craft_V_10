namespace Craft.Domain;

/// <summary>
/// Abstraction for models that represent a time-bounded assignment.
/// A record is considered "current" when it has a <see cref="StartDate"/> and no <see cref="EndDate"/>.
/// </summary>
public interface IHasDateRange
{
    /// <summary>The date the assignment began; <see langword="null"/> when not yet recorded.</summary>
    DateOnly? StartDate { get; set; }

    /// <summary>The date the assignment ended; <see langword="null"/> means the assignment is currently active.</summary>
    DateOnly? EndDate { get; set; }

    /// <summary>Returns <see langword="true"/> when this is the active (open-ended) assignment.</summary>
    bool IsCurrentAssignment => StartDate.HasValue && !EndDate.HasValue;

    /// <summary>
    /// Determines whether a given date falls within the date range.
    /// </summary>
    /// <param name="date">The date to check.</param>
    /// <returns><see langword="true"/> if the date is within the range; otherwise, <see langword="false"/>.</returns>
    bool IsDateInRange(DateOnly date) => StartDate.HasValue && date >= StartDate.Value && (!EndDate.HasValue || date <= EndDate.Value);

    /// <summary>
    /// Determines whether the date range of this assignment overlaps with another date range.
    /// </summary>
    /// <param name="otherStart">The start date of the other date range.</param>
    /// <param name="otherEnd">The end date of the other date range.</param>
    /// <returns><see langword="true"/> if both date ranges have valid start and end dates and the ranges overlap; otherwise,
    /// <see langword="false"/>.</returns>
    bool OverlapsWith(DateOnly otherStart, DateOnly otherEnd) =>
        StartDate.HasValue && EndDate.HasValue &&
        otherStart <= EndDate.Value && otherEnd >= StartDate.Value;

    /// <summary>
    /// Determines whether this date range overlaps with another date range.
    /// </summary>
    /// <param name="other">The date range to compare against.</param>
    /// <returns><see langword="true"/> if both date ranges have valid start and end dates and the ranges overlap; otherwise,
    /// <see langword="false"/>.</returns>
    bool OverlapsWith(IHasDateRange other) =>
        StartDate.HasValue && EndDate.HasValue &&
        other.StartDate.HasValue && other.EndDate.HasValue &&
        other.StartDate.Value <= EndDate.Value && other.EndDate.Value >= StartDate.Value;

    /// <summary>
    /// Determines whether the current date falls within the date range.
    /// </summary>
    /// <returns><see langword="true"/> if the current date is within the date range; otherwise, <see langword="false"/>.</returns>
    bool IsTodayInRange() => IsDateInRange(DateOnly.FromDateTime(DateTime.Today));

    /// <summary>
    /// Determines whether the date range is in the past.
    /// </summary>
    /// <returns><see langword="true"/> if the date range has ended; otherwise, <see langword="false"/>.</returns>
    bool IsInPast() => EndDate.HasValue && EndDate.Value < DateOnly.FromDateTime(DateTime.Today);

    /// <summary>
    /// Determines whether the date range is in the future.
    /// </summary>
    /// <returns><see langword="true"/> if the date range has not started yet; otherwise, <see langword="false"/>.</returns>
    bool IsInFuture() => StartDate.HasValue && StartDate.Value > DateOnly.FromDateTime(DateTime.Today);
}
