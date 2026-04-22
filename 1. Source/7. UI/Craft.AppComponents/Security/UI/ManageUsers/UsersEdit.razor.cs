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

public partial class UsersEdit<TUser, TUserVM, TUserDTO, TRole>
    where TUser : class, ICraftUser, new()
    where TUserVM : class, ICraftUser, new()
    where TUserDTO : class, ICraftUser, new()
    where TRole : class, ICraftRole, new()
{
    /// <summary>The HTTP service used to load, create, and update users.</summary>
    [Parameter, EditorRequired] public IHttpService<TUser, TUserVM, TUserDTO, KeyType>? HttpService { get; set; }

    /// <summary>
    /// The HTTP service used to populate the RolesLookup dropdown.
    /// When null, the Role field is hidden.
    /// </summary>
    [Parameter] public IHttpService<TRole>? RoleHttpService { get; set; }

    /// <summary>The user ID to load for edit/view. When default, the form is in Add mode.</summary>
    [Parameter] public KeyType? UserId { get; set; }

    /// <summary>When true, all fields are read-only and Save is hidden.</summary>
    [Parameter] public bool IsViewMode { get; set; }

    /// <summary>Raised after a successful save. The parent page handles any post-save navigation.</summary>
    [Parameter] public EventCallback<TUser> OnSaveSuccess { get; set; }

    /// <summary>Raised when the user clicks Cancel or Back. The parent page handles navigation.</summary>
    [Parameter] public EventCallback OnCancelRequested { get; set; }

    /// <summary>
    /// Raised when the selected role changes during save, allowing the parent to assign
    /// the role to the user via its own role-assignment service.
    /// </summary>
    [Parameter] public EventCallback<KeyType?> OnRoleSelected { get; set; }

    [Inject] private ISnackbar? Snackbar { get; set; }
    [Inject] private ILogger<UsersEdit<TUser, TUserVM, TUserDTO, TRole>>? Logger { get; set; }

    private CustomValidator? _customValidator;
    private TUserVM? _viewModel;
    private KeyType? _selectedRoleId;
    private bool _isSpinnerVisible;
    private CancellationTokenSource? _cts;

    private bool IsNewUser => UserId is null || UserId == default(KeyType);

    protected override async Task OnInitializedAsync()
    {
        if (!IsNewUser)
            await LoadUserAsync();
        else
            _viewModel = new TUserVM { IsActive = true };
    }

    private async Task LoadUserAsync()
    {
        _cts = new CancellationTokenSource();

        try
        {
            var result = await HttpService!.GetAsync(id: UserId!.Value, cancellationToken: _cts.Token);

            if (result.IsSuccess && result.Value is not null)
                _viewModel = result.Value.Adapt<TUserVM>();
            else
            {
                var errors = string.Join(", ", result.Errors ?? []);
                Logger?.LogError("Failed to load User with ID {UserId}. Errors: {Errors}", UserId, errors);
            }
        }
        catch (OperationCanceledException)
        {
            Logger?.LogWarning("LoadUserAsync was cancelled for ID {UserId}", UserId);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error loading User with ID {UserId}", UserId);
        }
    }

    private async Task HandleSubmitAsync()
    {
        if (_viewModel is null)
        {
            Logger?.LogWarning("HandleSubmitAsync called with null ViewModel");
            return;
        }

        var operation = IsNewUser ? "Create" : "Update";

        try
        {
            _isSpinnerVisible = true;
            _customValidator?.ClearErrors();
            _cts = new CancellationTokenSource();

            // UserName mirrors Email per domain convention
            _viewModel.UserName = _viewModel.Email;

            var result = IsNewUser
                ? await HttpService!.AddAsync(_viewModel, _cts.Token)
                : await HttpService!.UpdateAsync(_viewModel, _cts.Token);

            if (result.IsSuccess)
            {
                var label = _viewModel.Email ?? "User";
                var message = IsNewUser ? $"{label} created" : $"{label} updated";

                Snackbar?.Add(message, Severity.Success);

                if (OnRoleSelected.HasDelegate)
                    await OnRoleSelected.InvokeAsync(_selectedRoleId);

                if (OnSaveSuccess.HasDelegate)
                    await OnSaveSuccess.InvokeAsync(result.Value);
            }
            else
            {
                Logger?.LogWarning("{Operation} failed for User. Errors: {Errors}",
                    operation, string.Join(", ", result.Errors ?? []));

                _customValidator?.DisplayErrors(result);
            }
        }
        catch (OperationCanceledException)
        {
            Logger?.LogWarning("{Operation} User was cancelled", operation);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error during {Operation} User", operation);
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
