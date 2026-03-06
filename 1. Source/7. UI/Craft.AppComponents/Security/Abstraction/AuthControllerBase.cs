using System.Security.Claims;
using Craft.Core;
using Craft.Security;
using Craft.Security.Claims;
using Craft.Security.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Craft.AppComponents.Security;

/// <summary>
/// Wraps a user entity and its plaintext password for registration requests.
/// </summary>
/// <typeparam name="TUser">The application user entity type.</typeparam>
/// <param name="User">The user entity populated with registration data.</param>
/// <param name="Password">The plaintext password to hash and store.</param>
public sealed record UserRegistrationRequest<TUser>(TUser User, string Password);

/// <summary>
/// Reusable, overridable API controller that handles all standard authentication endpoints:
/// login, token refresh, logout, and registration.
/// </summary>
/// <typeparam name="TUser">The application user entity type, must extend <see cref="CraftUser{TKey}"/>.</typeparam>
/// <typeparam name="TKey">The primary key type of the user entity.</typeparam>
/// <remarks>
/// Derive from this class and supply concrete type arguments to produce a ready-to-use auth controller.
/// All endpoints are marked <c>virtual</c> so derived controllers can override individual actions.
///
/// <para>
/// <b>EF Core note:</b> This base class interacts with <see cref="RefreshToken{TKey}"/> and
/// <see cref="LoginHistory{TKey}"/> via <see cref="IDbContext.Set{TEntity}"/> rather than the
/// narrower <c>IRefreshTokenDbContext</c> / <c>ILoginHistoryDbContext</c> interfaces, because those
/// interfaces expose the non-generic concrete types (<c>RefreshToken</c>, <c>LoginHistory</c>) while
/// EF Core registers the generic types (<c>RefreshToken&lt;TKey&gt;</c>, <c>LoginHistory&lt;TKey&gt;</c>)
/// in the model — making the narrower interfaces incompatible with the actual DB context setup.
/// </para>
/// </remarks>
[Route("api/auth")]
[ApiController]
public abstract class AuthControllerBase<TUser, TKey> : ControllerBase
    where TUser : CraftUser<TKey>
    where TKey : IEquatable<TKey>
{
    private readonly UserManager<TUser> _userManager;
    private readonly ITokenManager _tokenManager;
    private readonly ITokenBlacklist _tokenBlacklist;
    private readonly IDbContext _dbContext;
    private readonly ILogger<AuthControllerBase<TUser, TKey>> _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="AuthControllerBase{TUser, TKey}"/>.
    /// </summary>
    /// <param name="userManager">The Identity user manager for the application user type.</param>
    /// <param name="tokenManager">Generates and validates JWT access and refresh tokens.</param>
    /// <param name="tokenBlacklist">Stores revoked tokens so middleware can reject them.</param>
    /// <param name="dbContext">
    /// The application DB context used to persist <see cref="RefreshToken{TKey}"/> and
    /// <see cref="LoginHistory{TKey}"/> records.
    /// </param>
    /// <param name="logger">Logger for this controller.</param>
    protected AuthControllerBase(
        UserManager<TUser> userManager,
        ITokenManager tokenManager,
        ITokenBlacklist tokenBlacklist,
        IDbContext dbContext,
        ILogger<AuthControllerBase<TUser, TKey>> logger)
    {
        _userManager = userManager;
        _tokenManager = tokenManager;
        _tokenBlacklist = tokenBlacklist;
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user by e-mail and password, returning a JWT access token and refresh token.
    /// </summary>
    /// <param name="request">Login credentials including e-mail, password, and optional IP address.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    /// <returns>
    /// <see cref="OkObjectResult"/> containing a <see cref="JwtAuthResponse"/> on success;
    /// <see cref="UnauthorizedResult"/> when the credentials are invalid or the account is inactive.
    /// </returns>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(JwtAuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public virtual async Task<IActionResult> LoginAsync(
        [FromBody] UserLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await _userManager.FindByEmailAsync(request.Email!);

        if (user is null || !user.IsActive)
        {
            _logger.LogWarning("[AuthController] Login attempt failed for {Email}: user not found or inactive", request.Email);
            return Unauthorized();
        }

        if (!await _userManager.CheckPasswordAsync(user, request.Password!))
        {
            _logger.LogWarning("[AuthController] Login attempt failed for {Email}: invalid password", request.Email);
            return Unauthorized();
        }

        var roles = await _userManager.GetRolesAsync(user);

        var claims = BuildClaims(user, roles);
        var authResponse = _tokenManager.GenerateJwtTokens(claims);

        _dbContext.Set<RefreshToken<TKey>>().Add(new RefreshToken<TKey>
        {
            Token = authResponse.RefreshToken,
            ExpiryTime = authResponse.RefreshTokenExpiryTime,
            UserId = user.Id
        });

        _dbContext.Set<LoginHistory<TKey>>().Add(new LoginHistory<TKey>
        {
            LastIpAddress = request.IpAddress,
            LastLoginOn = DateTime.UtcNow,
            UserId = user.Id
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("[AuthController] User {UserId} authenticated successfully", user.Id);

        return Ok(authResponse);
    }

    /// <summary>
    /// Exchanges an expired JWT access token and a valid refresh token for a new token pair.
    /// </summary>
    /// <param name="request">
    /// Contains the expired JWT access token, the refresh token, and the caller's IP address.
    /// </param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    /// <returns>
    /// <see cref="OkObjectResult"/> containing a new <see cref="JwtAuthResponse"/> on success;
    /// <see cref="UnauthorizedResult"/> when the tokens are invalid or the refresh token has expired.
    /// </returns>
    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(JwtAuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public virtual async Task<IActionResult> RefreshTokenAsync(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        ClaimsPrincipal principal;

        try
        {
            principal = _tokenManager.GetPrincipalFromExpiredToken(request.JwtToken);
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(ex, "[AuthController] Refresh failed: expired token is invalid");
            return Unauthorized("The supplied JWT token is invalid.");
        }

        var userIdStr = principal.FindFirstValue(CraftClaims.UserId);

        if (string.IsNullOrWhiteSpace(userIdStr))
        {
            _logger.LogWarning("[AuthController] Refresh failed: token contains no user identity claim");
            return Unauthorized("Token contains no user identity.");
        }

        TKey userId;

        try
        {
            userId = (TKey)Convert.ChangeType(userIdStr, typeof(TKey));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[AuthController] Refresh failed: cannot parse user id {UserId}", userIdStr);
            return Unauthorized("Invalid user identity in token.");
        }

        var refreshTokenSet = _dbContext.Set<RefreshToken<TKey>>();

        var existing = await refreshTokenSet
            .FirstOrDefaultAsync(r => r.UserId!.Equals(userId) && r.Token == request.RefreshToken, cancellationToken);

        if (existing is null || existing.ExpiryTime <= DateTime.UtcNow)
        {
            _logger.LogWarning("[AuthController] Refresh failed: token invalid or expired for user {UserId}", userId);
            return Unauthorized("The refresh token is invalid or has expired.");
        }

        var authResponse = _tokenManager.GenerateJwtTokens(principal.Claims);

        refreshTokenSet.Remove(existing);

        refreshTokenSet.Add(new RefreshToken<TKey>
        {
            Token = authResponse.RefreshToken,
            ExpiryTime = authResponse.RefreshTokenExpiryTime,
            UserId = userId
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("[AuthController] Refresh tokens rotated for user {UserId}", userId);

        return Ok(authResponse);
    }

    /// <summary>
    /// Revokes the caller's current JWT access token and removes their stored refresh tokens,
    /// effectively signing the user out on all devices.
    /// </summary>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    /// <returns><see cref="NoContentResult"/> on success.</returns>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public virtual async Task<IActionResult> LogoutAsync(CancellationToken cancellationToken = default)
    {
        var authHeader = HttpContext.Request.Headers.Authorization.ToString();
        var token = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authHeader["Bearer ".Length..].Trim()
            : string.Empty;

        if (!string.IsNullOrWhiteSpace(token))
        {
            // ITokenManager.RevokeTokenAsync delegates to ITokenBlacklist.AddAsync internally.
            await _tokenManager.RevokeTokenAsync(token, cancellationToken);
            _logger.LogInformation("[AuthController] Access token revoked for current user");
        }

        var userIdStr = User.FindFirstValue(CraftClaims.UserId);

        if (!string.IsNullOrWhiteSpace(userIdStr))
        {
            try
            {
                var userId = (TKey)Convert.ChangeType(userIdStr, typeof(TKey));
                var refreshTokenSet = _dbContext.Set<RefreshToken<TKey>>();

                var userTokens = await refreshTokenSet
                    .Where(r => r.UserId!.Equals(userId))
                    .ToListAsync(cancellationToken);

                refreshTokenSet.RemoveRange(userTokens);

                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("[AuthController] Removed {Count} refresh token(s) for user {UserId}", userTokens.Count, userId);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "[AuthController] Failed to clean up refresh tokens for user {UserId}", userIdStr);
            }
        }

        return NoContent();
    }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="request">
    /// A <see cref="UserRegistrationRequest{TUser}"/> containing the user entity (populated with
    /// profile data) and the plaintext password to hash.
    /// </param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    /// <returns>
    /// <see cref="CreatedAtActionResult"/> pointing to this action on success;
    /// <see cref="BadRequestObjectResult"/> containing Identity error details on failure.
    /// </returns>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(IEnumerable<IdentityError>), StatusCodes.Status400BadRequest)]
    public virtual async Task<IActionResult> RegisterAsync(
        [FromBody] UserRegistrationRequest<TUser> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await _userManager.CreateAsync(request.User, request.Password);

        if (!result.Succeeded)
        {
            _logger.LogWarning("[AuthController] Registration failed for {Email}: {Errors}",
                request.User.Email,
                string.Join(", ", result.Errors.Select(e => e.Description)));

            return BadRequest(result.Errors);
        }

        _logger.LogInformation("[AuthController] New user registered: {Email} (Id={UserId})", request.User.Email, request.User.Id);

        return CreatedAtAction(nameof(RegisterAsync), new { id = request.User.Id }, request.User);
    }

    /// <summary>
    /// Builds the claims list that will be embedded in the JWT access token.
    /// Override this method to add application-specific claims.
    /// </summary>
    /// <param name="user">The authenticated user.</param>
    /// <param name="roles">The roles assigned to the user.</param>
    /// <returns>A list of claims for the JWT.</returns>
    protected virtual List<Claim> BuildClaims(TUser user, IList<string> roles)
    {
        return
        [
            new Claim(CraftClaims.UserId, user.Id.ToString()!),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.GivenName, user.FirstName ?? string.Empty),
            new Claim(ClaimTypes.Surname, user.LastName ?? string.Empty),
            new Claim(CraftClaims.Role, string.Join(',', roles))
        ];
    }
}
