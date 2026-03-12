using Craft.Core;
using Craft.Security;

namespace Craft.AppComponents.Security;

/// <summary>
/// Defines HTTP operations for the standard authentication API endpoints
/// (<c>login</c>, <c>refresh</c>, <c>logout</c>, <c>register</c>,
/// <c>change-password</c>, <c>forgot-password</c>, <c>reset-password</c>).
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

    /// <summary>
    /// Changes the password for the currently authenticated user.
    /// </summary>
    /// <param name="model">The current and new password details.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<ServiceResult> ChangePasswordAsync(PasswordChangeRequest model, CancellationToken ct = default);

    /// <summary>
    /// Initiates a forgot-password flow by dispatching a reset link to the user's e-mail address.
    /// </summary>
    /// <param name="model">The user's e-mail and the client callback URI.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<ServiceResult> ForgotPasswordAsync(PasswordForgotRequest model, CancellationToken ct = default);

    /// <summary>
    /// Resets a user's password using the token issued by a forgot-password request.
    /// </summary>
    /// <param name="model">The reset token, new password, and user identity.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<ServiceResult> ResetPasswordAsync(ResetPasswordRequest model, CancellationToken ct = default);
}
