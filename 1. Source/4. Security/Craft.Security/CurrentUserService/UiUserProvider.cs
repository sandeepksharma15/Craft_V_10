using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Craft.Security;

public class UiUserProvider(AuthenticationStateProvider authenticationStateProvider) : ICurrentUserProvider
{
    public ClaimsPrincipal GetUser()
    {
        var authenticationState = authenticationStateProvider.GetAuthenticationStateAsync().Result;

        return authenticationState.User;
    }
}
