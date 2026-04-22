using Craft.QuerySpec;
using Craft.Security;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Craft.AppComponents.Security;

public partial class RolesLookup<TRole>
    where TRole : class, ICraftRole, new()
{
    private KeyType? _selectedRoleId;

    /// <summary>The HTTP service used to load roles for the dropdown.</summary>
    [Parameter, EditorRequired] public IHttpService<TRole>? HttpService { get; set; }

    /// <summary>The currently selected role ID.</summary>
    [Parameter]
    public KeyType? SelectedRoleId
    {
        get => _selectedRoleId;
        set
        {
            if (_selectedRoleId != value)
            {
                _selectedRoleId = value;
                SelectedRoleIdChanged.InvokeAsync(value);
            }
        }
    }

    /// <summary>Event callback invoked when the selected role changes.</summary>
    [Parameter] public EventCallback<KeyType?> SelectedRoleIdChanged { get; set; }

    /// <summary>Label for the dropdown.</summary>
    [Parameter] public string Label { get; set; } = "Role";

    /// <summary>Placeholder text when no role is selected.</summary>
    [Parameter] public string Placeholder { get; set; } = "Select a role";

    /// <summary>Variant for the MudSelect.</summary>
    [Parameter] public Variant Variant { get; set; } = Variant.Outlined;

    /// <summary>Whether the field is required.</summary>
    [Parameter] public bool Required { get; set; } = false;

    /// <summary>Whether the dropdown is read-only.</summary>
    [Parameter] public bool ReadOnly { get; set; } = false;

    /// <summary>Whether the dropdown is disabled.</summary>
    [Parameter] public bool Disabled { get; set; } = false;

    /// <summary>Additional CSS classes.</summary>
    [Parameter] public string? Class { get; set; }

    private IReadOnlyList<TRole>? _roles;

    protected override async Task OnInitializedAsync()
    {
        await LoadRolesAsync();
    }

    private async Task LoadRolesAsync()
    {
        try
        {
            var result = await HttpService!.GetAllAsync();

            _roles = result.IsSuccess && result.Value != null
                ? result.Value
                : [];
        }
        catch (Exception)
        {
            _roles = [];
        }
    }

    private string? GetRoleDisplayText(KeyType? roleId)
    {
        if (roleId == null || _roles == null)
            return null;

        var role = _roles.FirstOrDefault(r => r.Id == roleId);

        return role?.Name ?? roleId.ToString();
    }
}
