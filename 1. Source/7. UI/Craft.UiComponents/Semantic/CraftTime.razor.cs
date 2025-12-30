using Craft.UiComponents;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace Craft.UiComponents.Semantic;

/// <summary>
/// Represents a semantic time element with machine-readable datetime value.
/// Provides automatic formatting and locale support.
/// </summary>
public partial class CraftTime : CraftComponent
{
    /// <summary>
    /// Gets or sets the machine-readable datetime value in ISO 8601 format.
    /// If not specified, will be auto-generated from <see cref="Value"/>.
    /// </summary>
    [Parameter] public string? DateTime { get; set; }

    /// <summary>
    /// Gets or sets the date/time value to display and format.
    /// </summary>
    [Parameter] public DateTimeOffset? Value { get; set; }

    /// <summary>
    /// Gets or sets the format string for displaying the datetime.
    /// Uses standard .NET format strings. Default is "g" (general short date/time).
    /// </summary>
    [Parameter] public string? Format { get; set; } = "g";

    /// <summary>
    /// Gets or sets the culture to use for formatting.
    /// If not specified, uses current culture.
    /// </summary>
    [Parameter] public CultureInfo? Culture { get; set; }

    /// <summary>
    /// Gets or sets whether to show the relative time (e.g., "2 hours ago").
    /// When true, overrides Format parameter.
    /// </summary>
    [Parameter] public bool ShowRelativeTime { get; set; }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        // Auto-generate DateTime attribute from Value if not provided
        if (Value.HasValue && string.IsNullOrEmpty(DateTime))
        {
            DateTime = Value.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
        }
    }

    /// <summary>
    /// Returns the base CSS class for this component.
    /// </summary>
    protected override string? GetComponentCssClass() => "craft-time";

    /// <summary>
    /// Gets the datetime string for the datetime attribute.
    /// </summary>
    private string? GetDateTimeString()
    {
        return DateTime;
    }

    /// <summary>
    /// Gets the display content for the time element.
    /// </summary>
    private RenderFragment GetDisplayContent() => builder =>
    {
        if (ChildContent != null)
        {
            builder.AddContent(0, ChildContent);
        }
        else if (Value.HasValue)
        {
            var displayText = ShowRelativeTime 
                ? GetRelativeTimeString(Value.Value)
                : FormatDateTime(Value.Value);
            
            builder.AddContent(1, displayText);
        }
    };

    /// <summary>
    /// Formats the datetime value using the specified format and culture.
    /// </summary>
    private string FormatDateTime(DateTimeOffset value)
    {
        var culture = Culture ?? CultureInfo.CurrentCulture;
        return value.ToString(Format, culture);
    }

    /// <summary>
    /// Gets a relative time string (e.g., "2 hours ago", "in 3 days").
    /// </summary>
    private string GetRelativeTimeString(DateTimeOffset value)
    {
        var now = DateTimeOffset.UtcNow;
        var timeSpan = now - value.ToUniversalTime();
        var isPast = timeSpan.TotalSeconds > 0;
        timeSpan = timeSpan.Duration();

        string relativeString;

        if (timeSpan.TotalSeconds < 60)
        {
            relativeString = "just now";
        }
        else if (timeSpan.TotalMinutes < 60)
        {
            var minutes = (int)timeSpan.TotalMinutes;
            relativeString = isPast 
                ? $"{minutes} minute{(minutes == 1 ? "" : "s")} ago"
                : $"in {minutes} minute{(minutes == 1 ? "" : "s")}";
        }
        else if (timeSpan.TotalHours < 24)
        {
            var hours = (int)timeSpan.TotalHours;
            relativeString = isPast
                ? $"{hours} hour{(hours == 1 ? "" : "s")} ago"
                : $"in {hours} hour{(hours == 1 ? "" : "s")}";
        }
        else if (timeSpan.TotalDays < 30)
        {
            var days = (int)timeSpan.TotalDays;
            relativeString = isPast
                ? $"{days} day{(days == 1 ? "" : "s")} ago"
                : $"in {days} day{(days == 1 ? "" : "s")}";
        }
        else if (timeSpan.TotalDays < 365)
        {
            var months = (int)(timeSpan.TotalDays / 30);
            relativeString = isPast
                ? $"{months} month{(months == 1 ? "" : "s")} ago"
                : $"in {months} month{(months == 1 ? "" : "s")}";
        }
        else
        {
            var years = (int)(timeSpan.TotalDays / 365);
            relativeString = isPast
                ? $"{years} year{(years == 1 ? "" : "s")} ago"
                : $"in {years} year{(years == 1 ? "" : "s")}";
        }

        return relativeString;
    }
}
