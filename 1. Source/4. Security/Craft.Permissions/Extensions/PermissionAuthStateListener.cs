using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;

namespace Craft.Permissions;

/// <summary>
/// Scoped Blazor service that subscribes to <see cref="AuthenticationStateProvider.AuthenticationStateChanged"/>
/// and keeps the <see cref="IPermissionSessionCache"/> in sync with the current user's auth state.
/// <list type="bullet">
///   <item>On login (authenticated): fetches the user's permission codes from the API and populates the cache.</item>
///   <item>On logout (unauthenticated): clears the cache.</item>
/// </list>
/// Register it as a Scoped service and call <see cref="InitializeAsync"/> once from the root
/// layout or <c>App.razor</c> to start listening.
/// </summary>
public sealed class PermissionAuthStateListener : IAsyncDisposable
{
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly IUserPermissionsHttpService _userPermissionsHttpService;
    private readonly IPermissionSessionCache _cache;
    private readonly ILogger<PermissionAuthStateListener> _logger;

    public PermissionAuthStateListener(
        AuthenticationStateProvider authStateProvider,
        IUserPermissionsHttpService userPermissionsHttpService,
        IPermissionSessionCache cache,
        ILogger<PermissionAuthStateListener> logger)
    {
        _authStateProvider = authStateProvider;
        _userPermissionsHttpService = userPermissionsHttpService;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Loads the initial permission state and begins listening for future auth-state changes.
    /// Call this once from the root layout after the component has rendered.
    /// </summary>
    public async Task InitializeAsync()
    {
        _authStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;
        await LoadPermissionsAsync();
    }

    private void OnAuthenticationStateChanged(Task<AuthenticationState> authStateTask)
        => _ = HandleAuthStateChangedAsync(authStateTask);

    private async Task HandleAuthStateChangedAsync(Task<AuthenticationState> authStateTask)
    {
        try
        {
            var authState = await authStateTask.ConfigureAwait(false);

            if (authState.User.Identity?.IsAuthenticated is true)
                await LoadPermissionsAsync().ConfigureAwait(false);
            else
                _cache.Clear();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh permission cache after authentication state change.");
            _cache.Clear();
        }
    }

    private async Task LoadPermissionsAsync()
    {
        try
        {
            var result = await _userPermissionsHttpService
                .GetCurrentUserPermissionsAsync()
                .ConfigureAwait(false);

            if (result.IsSuccess && result.Value is not null)
                _cache.SetPermissions(result.Value);
            else
                _logger.LogWarning("Could not load user permissions: {Errors}", result.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while loading user permissions.");
        }
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        _authStateProvider.AuthenticationStateChanged -= OnAuthenticationStateChanged;
        return ValueTask.CompletedTask;
    }
}
