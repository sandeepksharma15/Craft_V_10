using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Craft.Security;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace Craft.AppComponents.Security;

/// <summary>
/// A self-contained change-password card that can be dropped into any Blazor app.
/// Reads the current user's ID from <see cref="ClaimTypes.NameIdentifier"/> automatically.
/// </summary>
/// <typeparam name="TUser">
/// The registration request type registered for <see cref="IAuthHttpService{TUserVM}"/> in DI.
/// Only used to resolve the correct service instance.
/// </typeparam>
public partial class ChangePasswordUser<TUser> : ComponentBase
    where TUser : class
{
    [Inject] private IAuthHttpService<TUser> AuthService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = null!;

    /// <summary>
    /// Fired after a successful password change, once the inline success panel is shown.
    /// Use this for additional host-level logic such as analytics or logging.
    /// </summary>
    [Parameter] public EventCallback OnSuccess { get; set; }

    /// <summary>
    /// Fired when the change fails, after the internal error snackbar.
    /// Receives the error message for additional host-level handling.
    /// </summary>
    [Parameter] public EventCallback<string> OnError { get; set; }

    /// <summary>
    /// Href shown as a "Cancel" link while the form is visible, and as a "Continue"
    /// button after a successful change. Leave <see langword="null"/> to hide both.
    /// Defaults to <c>"/"</c>.
    /// </summary>
    [Parameter] public string? ReturnHref { get; set; } = "/";

    /// <summary>Card header title. Defaults to <c>"Change Password"</c>.</summary>
    [Parameter] public string Title { get; set; } = "Change Password";

    private readonly ChangePasswordFormModel _model = new();
    private bool _isProcessing;
    private bool _submitted;
    private bool _showNewPassword;
    private InputType _newPasswordInputType = InputType.Password;
    private string _newPasswordIcon = Icons.Material.Filled.VisibilityOff;
    private long _userId;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var claim = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        long.TryParse(claim, out _userId);
    }

    private async Task HandleChangePasswordAsync()
    {
        _isProcessing = true;

        try
        {
            var request = new PasswordChangeRequest
            {
                Id = _userId,
                Password = _model.CurrentPassword,
                NewPassword = _model.NewPassword,
                ConfirmNewPassword = _model.ConfirmNewPassword
            };

            var result = await AuthService.ChangePasswordAsync(request);

            if (result.IsSuccess)
            {
                _submitted = true;
                await OnSuccess.InvokeAsync();
            }
            else
            {
                var errorMessage = result.Errors?.Count > 0
                    ? string.Join(", ", result.Errors)
                    : "Failed to change password. Please check your current password and try again.";

                Snackbar.Add(errorMessage, Severity.Error);
                await OnError.InvokeAsync(errorMessage);
            }
        }
        finally
        {
            _isProcessing = false;
        }
    }

    private void ToggleNewPasswordVisibility()
    {
        _showNewPassword = !_showNewPassword;

        if (_showNewPassword)
        {
            _newPasswordInputType = InputType.Text;
            _newPasswordIcon = Icons.Material.Filled.Visibility;
        }
        else
        {
            _newPasswordInputType = InputType.Password;
            _newPasswordIcon = Icons.Material.Filled.VisibilityOff;
        }
    }

    /// <summary>Internal form model with client-side validation for all three password fields.</summary>
    private sealed class ChangePasswordFormModel
    {
        [Required(ErrorMessage = "Current password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string? CurrentPassword { get; set; }

        [Required(ErrorMessage = "New password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters.")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string? NewPassword { get; set; }

        [Required(ErrorMessage = "Please confirm your new password.")]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        public string? ConfirmNewPassword { get; set; }
    }
}
