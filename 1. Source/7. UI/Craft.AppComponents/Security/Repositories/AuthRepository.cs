using System.Security.Claims;
using Craft.Core;
using Craft.Repositories;
using Craft.Security;
using Craft.Security.Claims;
using Craft.Security.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Craft.AppComponents.Security;

internal class AuthRepository<TUser> : BaseRepository<RefreshToken, KeyType>, IAuthRepository
    where TUser : CraftUser<KeyType>, new()
{
    private readonly ILogger<AuthRepository<TUser>> _authLogger;
    private readonly UserManager<TUser> _userManager;
    private readonly SignInManager<TUser> _signInManager;
    private readonly ITokenManager _tokenManager;
    private readonly IEmailSender<TUser> _emailSender;

    public AuthRepository(
        IDbContext dbContext,
        ILoggerFactory loggerFactory,
        UserManager<TUser> userManager,
        SignInManager<TUser> signInManager,
        ITokenManager tokenManager,
        IEmailSender<TUser> emailSender)
        : base(dbContext, loggerFactory.CreateLogger<BaseRepository<RefreshToken, KeyType>>())
    {
        _authLogger = loggerFactory.CreateLogger<AuthRepository<TUser>>();
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenManager = tokenManager;
        _emailSender = emailSender;
    }

    /// <inheritdoc />
    public async Task<JwtAuthResponse?> LoginAsync(IUserLoginRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await _userManager.FindByEmailAsync(request.Email!);

        if (user is null || !user.IsActive || user.IsDeleted)
        {
            _authLogger.LogWarning("[AuthRepository] Login failed: user not found or inactive for {Email}", request.Email);
            return null;
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password!, lockoutOnFailure: true);

        if (!signInResult.Succeeded)
        {
            _authLogger.LogWarning("[AuthRepository] Login failed: invalid credentials for {Email}", request.Email);
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);
        var claims = BuildClaims(user, roles);
        var authResponse = _tokenManager.GenerateJwtTokens(claims);

        var oldTokens = await _dbSet
            .Where(rt => rt.UserId == user.Id && !rt.IsDeleted)
            .ToListAsync(cancellationToken);
        oldTokens.ForEach(rt => rt.IsDeleted = true);

        _dbSet.Add(new RefreshToken(authResponse.RefreshToken, authResponse.RefreshTokenExpiryTime, user.Id));

        _appDbContext.Set<LoginHistory>().Add(
            new LoginHistory(request.IpAddress ?? string.Empty, DateTime.UtcNow, user.Id));

        await SaveChangesAsync(cancellationToken);

        _authLogger.LogInformation("[AuthRepository] User {UserId} authenticated successfully", user.Id);

        return authResponse;
    }

    /// <inheritdoc />
    public async Task<JwtAuthResponse?> RefreshTokenAsync(IRefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        ClaimsPrincipal principal;

        try
        {
            principal = _tokenManager.GetPrincipalFromExpiredToken(request.JwtToken);
        }
        catch (Exception ex)
        {
            _authLogger.LogWarning(ex, "[AuthRepository] Token refresh failed: invalid JWT");
            return null;
        }

        var userIdString = principal.GetUserId();

        if (!KeyType.TryParse(userIdString, out var userId))
        {
            _authLogger.LogWarning("[AuthRepository] Token refresh failed: user ID claim missing or invalid");
            return null;
        }

        var stored = await _dbSet
            .FirstOrDefaultAsync(rt => rt.UserId == userId
                                       && rt.Token == request.RefreshToken
                                       && !rt.IsDeleted, cancellationToken);

        if (stored is null || stored.ExpiryTime <= DateTime.UtcNow)
        {
            _authLogger.LogWarning("[AuthRepository] Token refresh failed: refresh token invalid or expired for user {UserId}", userId);
            return null;
        }

        var user = await _userManager.FindByIdAsync(userIdString!);

        if (user is null || !user.IsActive || user.IsDeleted)
        {
            _authLogger.LogWarning("[AuthRepository] Token refresh failed: user {UserId} not found or inactive", userId);
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);
        var claims = BuildClaims(user, roles);
        var authResponse = _tokenManager.GenerateJwtTokens(claims);

        stored.Token = authResponse.RefreshToken;
        stored.ExpiryTime = authResponse.RefreshTokenExpiryTime;
        await SaveChangesAsync(cancellationToken);

        _authLogger.LogInformation("[AuthRepository] Tokens refreshed for user {UserId}", userId);

        return authResponse;
    }

    /// <inheritdoc />
    public async Task LogoutAsync(string? jwtToken, string? userId, CancellationToken cancellationToken = default)
    {
        if (!jwtToken.IsNullOrEmpty())
            await _tokenManager.RevokeTokenAsync(jwtToken!, cancellationToken);

        if (!userId.IsNullOrEmpty() && KeyType.TryParse(userId, out var parsedUserId))
        {
            var tokens = await _dbSet
                .Where(rt => rt.UserId == parsedUserId && !rt.IsDeleted)
                .ToListAsync(cancellationToken);

            tokens.ForEach(rt => rt.IsDeleted = true);
            await SaveChangesAsync(cancellationToken);
        }

        _authLogger.LogInformation("[AuthRepository] User {UserId} logged out", userId);
    }

    /// <inheritdoc />
    public async Task<string> RegisterUserAsync(ICreateUserRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = new TUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password!);

        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            _authLogger.LogWarning("[AuthRepository] Registration failed for {Email}: {Errors}", request.Email, errors);
            throw new InvalidOperationException($"User registration failed: {errors}");
        }

        _authLogger.LogInformation("[AuthRepository] New user registered: {Email} (Id={UserId})", request.Email, user.Id);

        return user.Id.ToString()!;
    }

    /// <inheritdoc />
    public async Task ChangePasswordAsync(IPasswordChangeRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await _userManager.FindByIdAsync(request.Id.ToString()!);

        if (user is null)
            throw new InvalidOperationException($"User {request.Id} not found.");

        var result = await _userManager.ChangePasswordAsync(user, request.Password!, request.NewPassword!);

        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            _authLogger.LogWarning("[AuthRepository] Password change failed for user {UserId}: {Errors}", request.Id, errors);
            throw new InvalidOperationException($"Password change failed: {errors}");
        }

        _authLogger.LogInformation("[AuthRepository] Password changed for user {UserId}", request.Id);
    }

    /// <inheritdoc />
    public async Task ForgotPasswordAsync(IPasswordForgotRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await _userManager.FindByEmailAsync(request.Email!);

        if (user is null || user.IsDeleted)
        {
            // Do not reveal whether the user exists
            _authLogger.LogDebug("[AuthRepository] Forgot-password request for unknown email {Email}", request.Email);
            return;
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = $"{request.ClientURI}?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(request.Email!)}";

        await _emailSender.SendPasswordResetLinkAsync(user, request.Email!, resetLink);

        _authLogger.LogInformation("[AuthRepository] Password reset link sent to {Email}", request.Email);
    }

    /// <inheritdoc />
    public async Task ResetPasswordAsync(IPasswordResetRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await _userManager.FindByEmailAsync(request.Email!);

        if (user is null)
            throw new InvalidOperationException($"User with email {request.Email} not found.");

        var result = await _userManager.ResetPasswordAsync(user, request.Token!, request.Password!);

        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            _authLogger.LogWarning("[AuthRepository] Password reset failed for {Email}: {Errors}", request.Email, errors);
            throw new InvalidOperationException($"Password reset failed: {errors}");
        }

        _authLogger.LogInformation("[AuthRepository] Password reset successfully for {Email}", request.Email);
    }

    private static List<Claim> BuildClaims(TUser user, IList<string> roles)
    {
        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, user.Id.ToString()!),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(CraftClaims.Fullname, user.FullName),
        ];

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        return claims;
    }
}
