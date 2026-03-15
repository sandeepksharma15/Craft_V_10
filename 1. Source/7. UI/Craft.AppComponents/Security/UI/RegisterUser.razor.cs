using Craft.Security;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Craft.AppComponents.Security;

/// <summary>
/// A self-contained registration card that can be dropped into any Blazor app.
/// </summary>
/// <typeparam name="TUser">
/// The request model sent to <see cref="IAuthHttpService{TUserVM}.RegisterAsync"/>.
/// Must implement <see cref="ICreateUserRequest"/> and have a public parameterless constructor.
/// </typeparam>
public partial class RegisterUser<TUser> : ComponentBase
    where TUser : class, ICreateUserRequest, new()
{
    [Inject] private IAuthHttpService<TUser> AuthService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    /// <summary>
    /// Optional factory that maps the completed <see cref="RegisterFormModel"/> to a
    /// <typeparamref name="TUser"/> instance. Use this when your user model has extra
    /// fields beyond <see cref="ICreateUserRequest"/> (e.g. phone number, username).
    /// When <see langword="null"/>, the component creates a <typeparamref name="TUser"/>
    /// via <c>new()</c> and copies the four standard properties automatically.
    /// </summary>
    [Parameter] public Func<RegisterFormModel, TUser>? CreateRequest { get; set; }

    /// <summary>
    /// Fired after a successful registration (after the internal success snackbar).
    /// Use this callback to navigate to the login page or perform any post-registration logic.
    /// </summary>
    [Parameter] public EventCallback OnSuccess { get; set; }

    /// <summary>
    /// Fired when registration fails (after the internal error snackbar).
    /// Receives the error message so the host can perform additional logging or display.
    /// </summary>
    [Parameter] public EventCallback<string> OnError { get; set; }

    /// <summary>
    /// Optional extra fields rendered inside the form after Confirm Password and before
    /// the submit button. Bind these fields to state on your host page and capture them
    /// in <see cref="CreateRequest"/> to include them in the server request.
    /// </summary>
    [Parameter] public RenderFragment? AdditionalFields { get; set; }

    /// <summary>Card header title. Defaults to <c>"Create Account"</c>.</summary>
    [Parameter] public string Title { get; set; } = "Create Account";

    /// <summary>
    /// Href for the "Sign In" link shown below the form. Defaults to <c>"/login"</c>.
    /// </summary>
    [Parameter] public string LoginHref { get; set; } = "/login";

    private readonly RegisterFormModel _model = new();
    private bool _isProcessing;
    private bool _showPassword;
    private InputType _passwordInputType = InputType.Password;
    private string _passwordIcon = Icons.Material.Filled.VisibilityOff;

    private async Task HandleRegisterAsync()
    {
        _isProcessing = true;

        try
        {
            TUser request = CreateRequest is not null
                ? CreateRequest(_model)
                : BuildDefaultRequest();

            var result = await AuthService.RegisterAsync(request);

            if (result.IsSuccess)
            {
                Snackbar.Add("Account created successfully. Please sign in.", Severity.Success);
                await OnSuccess.InvokeAsync();
            }
            else
            {
                var errorMessage = result.Errors?.Count > 0
                    ? string.Join(", ", result.Errors)
                    : "Registration failed. Please try again.";

                Snackbar.Add(errorMessage, Severity.Error);
                await OnError.InvokeAsync(errorMessage);
            }
        }
        finally
        {
            _isProcessing = false;
        }
    }

    /// <summary>
    /// Default mapping: creates a <typeparamref name="TUser"/> via <c>new()</c> and
    /// assigns the four <see cref="ICreateUserRequest"/> properties from the form model.
    /// </summary>
    private TUser BuildDefaultRequest() => new()
    {
        FirstName = _model.FirstName,
        LastName = _model.LastName,
        Email = _model.Email,
        Password = _model.Password
    };

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
