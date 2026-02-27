using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Craft.AppComponents.Auditing;

/// <summary>
/// A dropdown component for selecting an audit user.
/// Includes an "All" option (null) at the top to indicate no user filtering.
/// The dropdown is disabled while users are being loaded.
/// </summary>
public partial class SelectAuditUser
{
    private List<AuditUserDTO>? _users;

    /// <summary>The audit trail HTTP service used to load audit users.</summary>
    [Inject] public required IAuditTrailHttpService HttpService { get; set; }

    /// <summary>The currently selected user ID. Null means "All" (no filter).</summary>
    [Parameter] public KeyType? Value { get; set; }

    /// <summary>Callback invoked when the selected user changes.</summary>
    [Parameter] public EventCallback<KeyType?> ValueChanged { get; set; }

    /// <summary>Label displayed on the dropdown. Defaults to "User".</summary>
    [Parameter] public string Label { get; set; } = "User";

    /// <summary>Placeholder text shown when no value is selected.</summary>
    [Parameter] public string Placeholder { get; set; } = "Select a user";

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
        await LoadUsersAsync();
    }

    private async Task LoadUsersAsync()
    {
        var result = await HttpService.GetAuditUsersAsync();

        _users = result.IsSuccess && result.Value is not null
            ? result.Value
            : [];
    }

    // Used by ToStringFunc to display the selected user's name in the input box.
    private string GetDisplayName(KeyType? userId)
    {
        if (userId is null)
            return "All";

        var user = _users?.FirstOrDefault(u => u.UserId == userId);

        return user?.UserName ?? userId.Value.ToString();
    }
}
