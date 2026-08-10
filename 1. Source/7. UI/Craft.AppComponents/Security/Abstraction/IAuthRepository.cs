using Craft.Repositories;
using Craft.Security;

namespace Craft.AppComponents.Security;

public interface IAuthRepository : IBaseRepository<RefreshToken, KeyType>
{
    /// <summary>Authenticates a user by e-mail and password, returning a JWT token pair.</summary>
    Task<JwtAuthResponse?> LoginAsync(IUserLoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>Exchanges an expired access token and refresh token for a new token pair.</summary>
    Task<JwtAuthResponse?> RefreshTokenAsync(IRefreshTokenRequest request, CancellationToken cancellationToken = default);

    /// <summary>Revokes the caller's access token and removes all stored refresh tokens for the user.</summary>
    Task LogoutAsync(string? jwtToken, string? userId, CancellationToken cancellationToken = default);

    /// <summary>Registers a new user account and returns the user-facing outcome of the registration flow.</summary>
    Task<AuthFlowResponse> RegisterUserAsync(ICreateUserRequest request, CancellationToken cancellationToken = default);

    /// <summary>Confirms a user's e-mail address using the token issued during registration.</summary>
    Task<AuthFlowResponse> ConfirmEmailAsync(IEmailConfirmationRequest request, CancellationToken cancellationToken = default);

    /// <summary>Changes the password for an authenticated user.</summary>
    Task ChangePasswordAsync(IPasswordChangeRequest request, CancellationToken cancellationToken = default);

    /// <summary>Initiates a forgot-password flow and returns the user-facing outcome of that flow.</summary>
    Task<AuthFlowResponse> ForgotPasswordAsync(IPasswordForgotRequest request, CancellationToken cancellationToken = default);

    /// <summary>Resets a user's password using the token issued by a forgot-password flow.</summary>
    Task ResetPasswordAsync(IPasswordResetRequest request, CancellationToken cancellationToken = default);
}
