using Craft.Core;
using Craft.Security;

namespace Craft.AppComponents.Security;

/// <summary>
/// Defines HTTP operations for the standard authentication API endpoints
/// (<c>login</c>, <c>refresh</c>, <c>logout</c>, <c>register</c>).
/// </summary>
/// <typeparam name="TUserVM">The view-model type used for user registration.</typeparam>
public interface IAuthHttpService<in TUserVM> where TUserVM : class
{
    /// <summary>
    /// Authenticates a user and returns JWT tokens.
    /// </summary>
    /// <param name="model">The login credentials.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result containing the JWT and refresh tokens on success.</returns>
    Task<ServiceResult<JwtAuthResponse>> LoginAsync(UserLoginRequest model, CancellationToken ct = default);

    /// <summary>
    /// Refreshes an expired JWT using a valid refresh token.
    /// </summary>
    /// <param name="model">The expired JWT and current refresh token.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result containing new JWT and refresh tokens on success.</returns>
    Task<ServiceResult<JwtAuthResponse>> RefreshAsync(RefreshTokenRequest model, CancellationToken ct = default);

    /// <summary>
    /// Logs out the current user by revoking the bearer token.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task<ServiceResult> LogoutAsync(CancellationToken ct = default);

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="model">The user view-model containing registration data.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<ServiceResult> RegisterAsync(TUserVM model, CancellationToken ct = default);
}
