using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Craft.AppComponents.Auditing;

/// <summary>
/// A dropdown component for selecting an audit table name.
/// Includes an "All" option (null) at the top to indicate no table filtering.
/// The dropdown is disabled while table names are being loaded.
/// </summary>
public partial class SelectAuditTable
{
    private List<string>? _tableNames;

    /// <summary>The audit trail HTTP service used to load available table names.</summary>
    [Inject] public required IAuditTrailHttpService HttpService { get; set; }

    /// <summary>The currently selected table name. Null means "All" (no filter).</summary>
    [Parameter] public string? Value { get; set; }

    /// <summary>Callback invoked when the selected table name changes.</summary>
    [Parameter] public EventCallback<string?> ValueChanged { get; set; }

    /// <summary>Label displayed on the dropdown. Defaults to "Table".</summary>
    [Parameter] public string Label { get; set; } = "Table";

    /// <summary>Placeholder text shown when no value is selected.</summary>
    [Parameter] public string Placeholder { get; set; } = "Select a table";

    /// <summary>Input variant. Defaults to <see cref="Variant.Outlined"/>.</summary>
    [Parameter] public Variant Variant { get; set; } = Variant.Outlined;

    /// <summary>Whether the field is required.</summary>
    [Parameter] public bool Required { get; set; }

    /// <summary>Whether the dropdown is read-only.</summary>
    [Parameter] public bool ReadOnly { get; set; }

    /// <summary>Whether the dropdown is disabled.</summary>
    [Parameter] public bool Disabled { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadTableNamesAsync();
    }

    private async Task LoadTableNamesAsync()
    {
        var result = await HttpService.GetTableNamesAsync();

        _tableNames = result.IsSuccess && result.Value is not null
            ? result.Value
            : [];
    }

    // Translates the internal string sentinel back to null ("All") before notifying the parent.
    private Task OnValueChangedAsync(string value)
        => ValueChanged.InvokeAsync(string.IsNullOrEmpty(value) ? null : value);
}
