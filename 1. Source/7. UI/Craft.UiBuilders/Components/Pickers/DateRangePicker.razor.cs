using Craft.UiComponents;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Craft.UiBuilders.Components;

/// <summary>
/// A date range picker component that allows users to select a start date, an end date, or both.
/// Both dates default to today on load. Selection is immediate — no confirmation button is required.
/// </summary>
public partial class DateRangePicker : CraftComponent
{
    #region Parameters

    /// <summary>
    /// The selected start date. Defaults to today if not provided.
    /// </summary>
    [Parameter] public DateTime? StartDate { get; set; }

    /// <summary>
    /// Callback invoked when the start date changes.
    /// </summary>
    [Parameter] public EventCallback<DateTime?> StartDateChanged { get; set; }

    /// <summary>
    /// The selected end date. Defaults to today if not provided.
    /// </summary>
    [Parameter] public DateTime? EndDate { get; set; }

    /// <summary>
    /// Callback invoked when the end date changes.
    /// </summary>
    [Parameter] public EventCallback<DateTime?> EndDateChanged { get; set; }

    /// <summary>
    /// Callback invoked whenever either date changes, passing the current (StartDate, EndDate) tuple.
    /// </summary>
    [Parameter] public EventCallback<(DateTime? Start, DateTime? End)> OnDateRangeChanged { get; set; }

    /// <summary>
    /// Label displayed above the start date picker. Defaults to "From".
    /// </summary>
    [Parameter] public string StartLabel { get; set; } = "From";

    /// <summary>
    /// Label displayed above the end date picker. Defaults to "To".
    /// </summary>
    [Parameter] public string EndLabel { get; set; } = "To";

    /// <summary>
    /// Input variant for both pickers. Defaults to <see cref="Variant.Outlined"/>.
    /// </summary>
    [Parameter] public Variant Variant { get; set; } = Variant.Outlined;

    /// <summary>
    /// Controls how the calendar panel is displayed. Defaults to <see cref="PickerVariant.Inline"/>.
    /// </summary>
    [Parameter] public PickerVariant PickerVariant { get; set; } = PickerVariant.Inline;

    #endregion

    #region Lifecycle

    protected override void OnInitialized()
    {
        base.OnInitialized();
        StartDate ??= DateTime.Today;
        EndDate ??= DateTime.Today;
    }

    #endregion

    #region Handlers

    private async Task OnStartDateChangedAsync(DateTime? date)
    {
        StartDate = date;

        // Keep end date valid: if end is now before the new start, align it
        if (StartDate.HasValue && EndDate.HasValue && EndDate < StartDate)
        {
            EndDate = StartDate;
            await EndDateChanged.InvokeAsync(EndDate);
        }

        await StartDateChanged.InvokeAsync(StartDate);
        await OnDateRangeChanged.InvokeAsync((StartDate, EndDate));
    }

    private async Task OnEndDateChangedAsync(DateTime? date)
    {
        EndDate = date;

        // Keep start date valid: if start is now after the new end, align it
        if (StartDate.HasValue && EndDate.HasValue && StartDate > EndDate)
        {
            StartDate = EndDate;
            await StartDateChanged.InvokeAsync(StartDate);
        }

        await EndDateChanged.InvokeAsync(EndDate);
        await OnDateRangeChanged.InvokeAsync((StartDate, EndDate));
    }

    #endregion

    #region Date Disable Helpers

    // Disable start date calendar dates that fall after the current end date
    private bool IsStartDateDisabled(DateTime date) =>
        EndDate.HasValue && date.Date > EndDate.Value.Date;

    // Disable end date calendar dates that fall before the current start date
    private bool IsEndDateDisabled(DateTime date) =>
        StartDate.HasValue && date.Date < StartDate.Value.Date;

    #endregion
}
