using System.Security.Claims;
using Craft.Security;
using Craft.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace Craft.AppComponents.Security;

/// <summary>
/// A self-contained navigation component that renders Login + Register links when the
/// user is not authenticated, and a collapsible user group with a Logout action when
/// authenticated. Designed to drop inside a <see cref="MudNavMenu"/> with zero host wiring.
/// </summary>
/// <typeparam name="TUser">
/// The registration request type registered for <see cref="IAuthHttpService{TUserVM}"/> in DI.
/// Only used to resolve the correct service instance for
/// <see cref="IAuthHttpService{TUserVM}.LogoutAsync"/>.
/// </typeparam>
public partial class NavAuthUser<TUser> : ComponentBase, IDisposable
    where TUser : class
{
    [Inject] private IAuthHttpService<TUser> AuthService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private NavigationManager Nav { get; set; } = null!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = null!;

    /// <summary>
    /// Href for the Login nav link. Also used as the post-logout redirect destination.
    /// Defaults to <c>"/login"</c>.
    /// </summary>
    [Parameter] public string LoginHref { get; set; } = "/login";

    /// <summary>Href for the Register nav link. Defaults to <c>"/register"</c>.</summary>
    [Parameter] public string RegisterHref { get; set; } = "/register";

    /// <summary>
    /// The cookie-clearing endpoint navigated to after <see cref="OnLogout"/> fires.
    /// Navigates to <c>{SignOutEndpoint}?returnUrl={LoginHref}</c> with <c>forceLoad: true</c>.
    /// Defaults to <c>"/auth/sign-out"</c>. Set to <see langword="null"/> to let the host
    /// handle all navigation inside <see cref="OnLogout"/>.
    /// </summary>
    [Parameter] public string? SignOutEndpoint { get; set; } = "/auth/sign-out";

    /// <summary>
    /// Fired after <see cref="IAuthHttpService{TUserVM}.LogoutAsync"/> completes and before
    /// any internal navigation. Use this for additional host-level cleanup or logging.
    /// </summary>
    [Parameter] public EventCallback OnLogout { get; set; }

    /// <summary>Icon for the Login nav link. Defaults to <see cref="Icons.Material.Filled.Login"/>.</summary>
    [Parameter] public string LoginIcon { get; set; } = Icons.Material.Filled.Login;

    /// <summary>Icon for the Register nav link. Defaults to <see cref="Icons.Material.Filled.PersonAdd"/>.</summary>
    [Parameter] public string RegisterIcon { get; set; } = Icons.Material.Filled.PersonAdd;

    /// <summary>Icon for the Logout nav link. Defaults to <see cref="Icons.Material.Filled.Logout"/>.</summary>
    [Parameter] public string LogoutIcon { get; set; } = Icons.Material.Filled.Logout;

    /// <summary>
    /// When set, a "Change Password" nav link is rendered above Logout in the authenticated group.
    /// Leave <see langword="null"/> to hide the link.
    /// </summary>
    [Parameter] public string? ChangePasswordHref { get; set; }

    /// <summary>Icon for the Change Password nav link. Defaults to <see cref="Icons.Material.Filled.Lock"/>.</summary>
    [Parameter] public string ChangePasswordIcon { get; set; } = Icons.Material.Filled.Lock;

    /// <summary>
    /// Icon for the authenticated user nav group.
    /// Defaults to <see cref="Icons.Material.Filled.AccountCircle"/>.
    /// </summary>
    [Parameter] public string UserGroupIcon { get; set; } = Icons.Material.Filled.AccountCircle;

    private bool _isAuthenticated;
    private string _displayName = string.Empty;
    private bool _isProcessing;

    protected override async Task OnInitializedAsync()
    {
        await RefreshAuthStateAsync();
        AuthStateProvider.AuthenticationStateChanged += OnAuthStateChanged;
    }

    private async Task RefreshAuthStateAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();

        _isAuthenticated = authState.User.Identity?.IsAuthenticated == true;
        _displayName = GetUserDisplayName(authState.User);
    }

    private async void OnAuthStateChanged(Task<AuthenticationState> authStateTask)
    {
        var authState = await authStateTask;

        _isAuthenticated = authState.User.Identity?.IsAuthenticated == true;
        _displayName = GetUserDisplayName(authState.User);

        await InvokeAsync(StateHasChanged);
    }

    private async Task HandleLogoutAsync()
    {
        if (_isProcessing)
            return;

        _isProcessing = true;

        try
        {
            // Best-effort server-side token revocation. We proceed regardless of the
            // outcome so the user is never left stuck in a broken authenticated state.
            await AuthService.LogoutAsync();
        }
        catch
        {
            // Intentionally swallowed — see comment above.
        }

        await OnLogout.InvokeAsync();

        if (!string.IsNullOrWhiteSpace(SignOutEndpoint))
        {
            Nav.NavigateTo(
                $"{SignOutEndpoint}?returnUrl={Uri.EscapeDataString(LoginHref)}",
                forceLoad: true);
        }

        // Only reached when SignOutEndpoint is null and the host owns all navigation.
        _isProcessing = false;
    }

    /// <summary>
    /// Resolves the display name from JWT claims in priority order:
    /// <c>fullName</c> claim → email → name → <c>"User"</c>.
    /// </summary>
    private static string GetUserDisplayName(ClaimsPrincipal user)
    {
        var fullName = user.FindFirst(CraftClaims.Fullname)?.Value;
        if (!string.IsNullOrWhiteSpace(fullName))
            return fullName;

        var email = user.FindFirst(ClaimTypes.Email)?.Value;
        if (!string.IsNullOrWhiteSpace(email))
            return email;

        var name = user.FindFirst(ClaimTypes.Name)?.Value;
        if (!string.IsNullOrWhiteSpace(name))
            return name;

        return "User";
    }

    public void Dispose()
    {
        AuthStateProvider.AuthenticationStateChanged -= OnAuthStateChanged;
    }
}
