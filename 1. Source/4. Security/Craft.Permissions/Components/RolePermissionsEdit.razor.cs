using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MudBlazor;

namespace Craft.Permissions;

/// <summary>
/// Management UI component that displays all registered permissions grouped by their
/// <see cref="PermissionDefinition.Group"/> with checkboxes, and persists role-permission
/// assignments via <see cref="IRolePermissionHttpService"/>.
/// </summary>
public partial class RolePermissionsEdit : ComponentBase
{
    /// <summary>The role whose permissions are being managed.</summary>
    [Parameter, EditorRequired] public KeyType RoleId { get; set; }

    /// <summary>The HTTP service used to load and save role-permission assignments.</summary>
    [Parameter, EditorRequired] public IRolePermissionHttpService HttpService { get; set; } = null!;

    /// <summary>When <see langword="true"/> all checkboxes are read-only and Save is hidden.</summary>
    [Parameter] public bool IsViewMode { get; set; }

    /// <summary>Raised after permissions are saved successfully.</summary>
    [Parameter] public EventCallback OnSaveSuccess { get; set; }

    /// <summary>Raised when the user clicks Cancel.</summary>
    [Parameter] public EventCallback OnCancelRequested { get; set; }

    [Inject] private ILogger<RolePermissionsEdit>? Logger { get; set; }

    private bool _isLoading = true;
    private bool _isSaving;
    private HashSet<int> _selected = [];
    private Dictionary<string, List<PermissionDefinition>> _groups = [];

    protected override async Task OnInitializedAsync()
    {
        BuildGroups();
        await LoadCurrentPermissionsAsync();
        _isLoading = false;
    }

    private void BuildGroups()
    {
        _groups = Registry.GetAll()
            .GroupBy(d => d.Group ?? string.Empty)
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    private async Task LoadCurrentPermissionsAsync()
    {
        var result = await HttpService.GetPermissionsForRoleAsync(RoleId);

        if (result.IsSuccess && result.Value is not null)
            _selected = [.. result.Value];
        else
            Logger?.LogWarning("Could not load permissions for role {RoleId}: {Errors}", RoleId, result.Errors);
    }

    private void OnPermissionToggled(int code, bool isChecked)
    {
        if (isChecked)
            _selected.Add(code);
        else
            _selected.Remove(code);
    }

    private async Task SaveAsync()
    {
        _isSaving = true;

        try
        {
            var result = await HttpService.SetPermissionsForRoleAsync(RoleId, _selected);

            if (result.IsSuccess)
            {
                Snackbar.Add("Permissions saved successfully.", Severity.Success);
                await OnSaveSuccess.InvokeAsync();
            }
            else
            {
                Snackbar.Add($"Failed to save permissions: {string.Join(", ", result.Errors ?? [])}", Severity.Error);
            }
        }
        finally
        {
            _isSaving = false;
        }
    }
}
