using Craft.Security;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using MudBlazor;

namespace Craft.AppComponents.Security;

/// <summary>
/// A self-contained sign-in card that can be dropped into any Blazor app.
/// </summary>
/// <typeparam name="TUser">
/// The registration request type registered for <see cref="IAuthHttpService{TUserVM}"/> in DI.
/// Only used to resolve the correct service instance; login itself always uses
/// <see cref="UserLoginRequest"/>.
/// </typeparam>
public partial class LoginUser<TUser> : ComponentBase
    where TUser : class
{
    [Inject] private IAuthHttpService<TUser> AuthService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private NavigationManager Nav { get; set; } = null!;

    /// <summary>
    /// Fired after a successful login, before any internal navigation.
    /// Receives the full <see cref="JwtAuthResponse"/> so the host can store tokens or
    /// perform additional logic. When <see cref="SignInEndpoint"/> is also set, the
    /// component navigates after this callback returns.
    /// </summary>
    [Parameter] public EventCallback<JwtAuthResponse> OnSuccess { get; set; }

    /// <summary>
    /// Fired when login fails, after the internal error snackbar.
    /// Receives the error message for additional host-level handling or logging.
    /// </summary>
    [Parameter] public EventCallback<string> OnError { get; set; }

    /// <summary>
    /// When set, the component navigates to
    /// <c>{SignInEndpoint}?token=...&amp;returnUrl=...</c> with <c>forceLoad: true</c>
    /// after <see cref="OnSuccess"/> fires. Leave <see langword="null"/> to let the host
    /// handle all navigation inside <see cref="OnSuccess"/>.
    /// </summary>
    [Parameter] public string? SignInEndpoint { get; set; }

    /// <summary>
    /// Fallback redirect used when no <c>returnUrl</c> query-string value is present.
    /// Defaults to <c>"/"</c>.
    /// </summary>
    [Parameter] public string HomeHref { get; set; } = "/";

    /// <summary>Shows or hides the "Remember me" checkbox. Defaults to <see langword="true"/>.</summary>
    [Parameter] public bool ShowRememberMe { get; set; } = true;

    /// <summary>
    /// When set, a "Forgot password?" link is rendered after the password field.
    /// Leave <see langword="null"/> to hide the link.
    /// </summary>
    [Parameter] public string? ForgotPasswordHref { get; set; }

    /// <summary>Href for the "Register" link shown below the form. Defaults to <c>"/register"</c>.</summary>
    [Parameter] public string RegisterHref { get; set; } = "/register";

    /// <summary>Card header title. Defaults to <c>"Sign In"</c>.</summary>
    [Parameter] public string Title { get; set; } = "Sign In";

    /// <summary>
    /// Optional extra fields rendered inside the form before the submit button.
    /// Bind these fields to state on your host page and capture them in
    /// <see cref="OnSuccess"/> for any additional post-login logic.
    /// </summary>
    [Parameter] public RenderFragment? AdditionalFields { get; set; }

    private readonly UserLoginRequest _model = new();
    private bool _isProcessing;
    private bool _showPassword;
    private InputType _passwordInputType = InputType.Password;
    private string _passwordIcon = Icons.Material.Filled.VisibilityOff;
    private string? _returnUrl;

    protected override void OnInitialized()
    {
        var uri = Nav.ToAbsoluteUri(Nav.Uri);

        QueryHelpers.ParseQuery(uri.Query).TryGetValue("returnUrl", out var values);

        _returnUrl = values.FirstOrDefault();
    }

    private async Task HandleLoginAsync()
    {
        _isProcessing = true;

        try
        {
            var result = await AuthService.LoginAsync(_model);

            if (result.IsSuccess && result.Value is not null)
            {
                await OnSuccess.InvokeAsync(result.Value);

                if (!string.IsNullOrWhiteSpace(SignInEndpoint))
                {
                    var returnUrl = string.IsNullOrWhiteSpace(_returnUrl) ? HomeHref : _returnUrl;

                    Nav.NavigateTo(
                        $"{SignInEndpoint}?token={Uri.EscapeDataString(result.Value.JwtToken)}&returnUrl={Uri.EscapeDataString(returnUrl)}",
                        forceLoad: true);
                }
            }
            else
            {
                var errorMessage = result.Errors?.Count > 0
                    ? string.Join(", ", result.Errors)
                    : "Invalid email or password.";

                Snackbar.Add(errorMessage, Severity.Error);
                await OnError.InvokeAsync(errorMessage);
            }
        }
        finally
        {
            _isProcessing = false;
        }
    }

    private void TogglePasswordVisibility()
    {
        _showPassword = !_showPassword;

        if (_showPassword)
        {
            _passwordInputType = InputType.Text;
            _passwordIcon = Icons.Material.Filled.Visibility;
        }
        else
        {
            _passwordInputType = InputType.Password;
            _passwordIcon = Icons.Material.Filled.VisibilityOff;
        }
    }
}
