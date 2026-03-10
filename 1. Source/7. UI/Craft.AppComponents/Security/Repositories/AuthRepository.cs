using Craft.Security;

namespace Craft.AppComponents.Security;

internal class AuthRepository : IAuthRepository
{
    public Task<JwtAuthResponse?> LoginAsync(IUserLoginRequest request, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<JwtAuthResponse?> RefreshTokenAsync(IRefreshTokenRequest request, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task LogoutAsync(string? jwtToken, string? userId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<string> RegisterUserAsync(ICreateUserRequest request, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task ChangePasswordAsync(IPasswordChangeRequest request, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task ForgotPasswordAsync(IPasswordForgotRequest request, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task ResetPasswordAsync(IPasswordResetRequest request, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
