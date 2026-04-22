using Craft.Components.Generic;
using Craft.Core;
using Craft.HttpServices;
using Craft.QuerySpec;
using Craft.Security;
using Mapster;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MudBlazor;

namespace Craft.AppComponents.Security;

public partial class RolesEdit<TRole, TRoleVM, TRoleDTO>
    where TRole : class, ICraftRole, new()
    where TRoleVM : class, ICraftRole, new()
    where TRoleDTO : class, ICraftRole, new()
{
    /// <summary>The HTTP service used to load, create, and update roles.</summary>
    [Parameter, EditorRequired] public IHttpService<TRole, TRoleVM, TRoleDTO, KeyType>? HttpService { get; set; }

    /// <summary>The role ID to load for edit/view. When default, the form is in Add mode.</summary>
    [Parameter] public KeyType? RoleId { get; set; }

    /// <summary>When true, all fields are read-only and Save is hidden.</summary>
    [Parameter] public bool IsViewMode { get; set; }

    /// <summary>Raised after a successful save. The parent page handles any post-save navigation.</summary>
    [Parameter] public EventCallback<TRole> OnSaveSuccess { get; set; }

    /// <summary>Raised when the user clicks Cancel or Back. The parent page handles navigation.</summary>
    [Parameter] public EventCallback OnCancelRequested { get; set; }

    [Inject] private ISnackbar? Snackbar { get; set; }
    [Inject] private ILogger<RolesEdit<TRole, TRoleVM, TRoleDTO>>? Logger { get; set; }

    private CustomValidator? _customValidator;
    private TRoleVM? _viewModel;
    private bool _isSpinnerVisible;
    private CancellationTokenSource? _cts;

    private bool IsNewRole => RoleId is null || RoleId == default(KeyType);

    protected override async Task OnInitializedAsync()
    {
        if (!IsNewRole)
            await LoadRoleAsync();
        else
            _viewModel = new TRoleVM { IsActive = true };
    }

    private async Task LoadRoleAsync()
    {
        _cts = new CancellationTokenSource();

        try
        {
            var result = await HttpService!.GetAsync(id: RoleId!.Value, cancellationToken: _cts.Token);

            if (result.IsSuccess && result.Value is not null)
                _viewModel = result.Value.Adapt<TRoleVM>();
            else
            {
                var errors = string.Join(", ", result.Errors ?? []);
                Logger?.LogError("Failed to load Role with ID {RoleId}. Errors: {Errors}", RoleId, errors);
            }
        }
        catch (OperationCanceledException)
        {
            Logger?.LogWarning("LoadRoleAsync was cancelled for ID {RoleId}", RoleId);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error loading Role with ID {RoleId}", RoleId);
        }
    }

    private async Task HandleSubmitAsync()
    {
        if (_viewModel is null)
        {
            Logger?.LogWarning("HandleSubmitAsync called with null ViewModel");
            return;
        }

        var operation = IsNewRole ? "Create" : "Update";

        try
        {
            _isSpinnerVisible = true;
            _customValidator?.ClearErrors();
            _cts = new CancellationTokenSource();

            var result = IsNewRole
                ? await HttpService!.AddAsync(_viewModel, _cts.Token)
                : await HttpService!.UpdateAsync(_viewModel, _cts.Token);

            if (result.IsSuccess)
            {
                var label = _viewModel.Name ?? "Role";
                var message = IsNewRole ? $"{label} created" : $"{label} updated";

                Snackbar?.Add(message, Severity.Success);

                if (OnSaveSuccess.HasDelegate)
                    await OnSaveSuccess.InvokeAsync(result.Value);
            }
            else
            {
                Logger?.LogWarning("{Operation} failed for Role. Errors: {Errors}",
                    operation, string.Join(", ", result.Errors ?? []));

                _customValidator?.DisplayErrors(result);
            }
        }
        catch (OperationCanceledException)
        {
            Logger?.LogWarning("{Operation} Role was cancelled", operation);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error during {Operation} Role", operation);
        }
        finally
        {
            _isSpinnerVisible = false;
        }
    }

    private async Task HandleCancelAsync()
    {
        if (OnCancelRequested.HasDelegate)
            await OnCancelRequested.InvokeAsync();
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}
