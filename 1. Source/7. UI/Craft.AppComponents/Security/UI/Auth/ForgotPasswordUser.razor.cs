using System.ComponentModel.DataAnnotations;
using Craft.Security;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Craft.AppComponents.Security;

/// <summary>
/// A self-contained forgot-password card that can be dropped into any Blazor app.
/// Constructs the reset-password callback URI automatically from
/// <see cref="NavigationManager.BaseUri"/> + <see cref="ResetPasswordHref"/>.
/// </summary>
/// <typeparam name="TUser">
/// The registration request type registered for <see cref="IAuthHttpService{TUserVM}"/> in DI.
/// Only used to resolve the correct service instance.
/// </typeparam>
public partial class ForgotPasswordUser<TUser> : ComponentBase
    where TUser : class
{
    [Inject] private IAuthHttpService<TUser> AuthService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private NavigationManager Nav { get; set; } = null!;

    /// <summary>
    /// Relative path to the reset-password page (e.g. <c>"/reset-password"</c>).
    /// The component prepends <see cref="NavigationManager.BaseUri"/> to build the full
    /// callback URL embedded in the reset e-mail link.
    /// Defaults to <c>"/reset-password"</c>.
    /// </summary>
    [Parameter] public string ResetPasswordHref { get; set; } = "/reset-password";

    /// <summary>
    /// Fired after a successful submission, once the inline success panel is shown.
    /// Use this for additional host-level logic such as analytics or logging.
    /// </summary>
    [Parameter] public EventCallback OnSuccess { get; set; }

    /// <summary>
    /// Fired when the request fails, after the internal error snackbar.
    /// Receives the error message for additional host-level handling.
    /// </summary>
    [Parameter] public EventCallback<string> OnError { get; set; }

    /// <summary>Href for the "Sign In" link shown below the form. Defaults to <c>"/login"</c>.</summary>
    [Parameter] public string LoginHref { get; set; } = "/login";

    /// <summary>Card header title. Defaults to <c>"Forgot Password"</c>.</summary>
    [Parameter] public string Title { get; set; } = "Forgot Password";

    private readonly ForgotPasswordFormModel _model = new();
    private bool _isProcessing;
    private bool _submitted;

    private async Task HandleForgotPasswordAsync()
    {
        _isProcessing = true;

        try
        {
            var clientUri = Nav.BaseUri.TrimEnd('/') + ResetPasswordHref;

            var request = new PasswordForgotRequest
            {
                Email = _model.Email,
                ClientURI = clientUri
            };

            var result = await AuthService.ForgotPasswordAsync(request);

            if (result.IsSuccess)
            {
                _submitted = true;
                await OnSuccess.InvokeAsync();
            }
            else
            {
                var errorMessage = result.Errors?.Count > 0
                    ? string.Join(", ", result.Errors)
                    : "Failed to send reset link. Please try again.";

                Snackbar.Add(errorMessage, Severity.Error);
                await OnError.InvokeAsync(errorMessage);
            }
        }
        finally
        {
            _isProcessing = false;
        }
    }

    /// <summary>Internal form model — only the email field is needed for this flow.</summary>
    private sealed class ForgotPasswordFormModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "Email")]
        public string? Email { get; set; }
    }
}
