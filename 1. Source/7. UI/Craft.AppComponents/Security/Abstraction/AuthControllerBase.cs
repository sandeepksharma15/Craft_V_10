using System.Security.Claims;
using Craft.Security;
using Craft.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Craft.AppComponents.Security;

/// <summary>
/// Reusable, overridable API controller that handles all standard authentication endpoints:
/// login, token refresh, logout, registration, email confirmation, password change,
/// forgot-password, and password reset.
/// </summary>
/// <remarks>
/// Derive from this class to produce a ready-to-use auth controller.
/// All endpoints are marked <c>virtual</c> so derived controllers can override individual actions.
/// All operations are delegated to <see cref="IAuthRepository"/>, keeping this controller
/// free of infrastructure concerns.
/// </remarks>
[Route("api/auth")]
[ApiController]
public abstract class AuthControllerBase : ControllerBase
{
    private readonly IAuthRepository _authRepository;
    private readonly ILogger<AuthControllerBase> _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="AuthControllerBase"/>.
    /// </summary>
    /// <param name="authRepository">Handles all authentication and user-management operations.</param>
    /// <param name="logger">Logger for this controller.</param>
    protected AuthControllerBase(IAuthRepository authRepository, ILogger<AuthControllerBase> logger)
    {
        _authRepository = authRepository;
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
    public virtual async Task<IActionResult> LoginAsync([FromBody] UserLoginRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await _authRepository.LoginAsync(request, cancellationToken);

        if (response is null)
        {
            _logger.LogWarning("[AuthController] Login attempt failed for {Email}", request.Email);
            return Unauthorized(new List<string> { "Invalid email or password." });
        }

        _logger.LogInformation("[AuthController] User authenticated successfully: {Email}", request.Email);

        return Ok(response);
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
    public virtual async Task<IActionResult> RefreshTokenAsync([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await _authRepository.RefreshTokenAsync(request, cancellationToken);

        if (response is null)
        {
            _logger.LogWarning("[AuthController] Token refresh failed");
            return Unauthorized(new List<string> { "Session expired. Please sign in again." });
        }

        _logger.LogInformation("[AuthController] Tokens refreshed successfully");

        return Ok(response);
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

        var userId = User.FindFirstValue(CraftClaims.UserId);

        await _authRepository.LogoutAsync(token, userId, cancellationToken);

        _logger.LogInformation("[AuthController] User {UserId} logged out", userId);

        return NoContent();
    }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="request">
    /// A <see cref="CreateUserRequest"/> containing the profile data and plaintext password for the new account.
    /// </param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    /// <returns>
    /// A user-facing auth flow response on success;
    /// <see cref="BadRequestResult"/> on failure.
    /// </returns>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthFlowResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status400BadRequest)]
    public virtual async Task<IActionResult> RegisterUserAsync([FromBody] CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var response = await _authRepository.RegisterUserAsync(request, cancellationToken);

            _logger.LogInformation("[AuthController] New user registered: {Email}", request.Email);

            return StatusCode(StatusCodes.Status201Created, response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("[AuthController] Registration rejected for {Email}: {Message}", request.Email, ex.Message);
            return BadRequest(new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// Confirms a user's email address using the token issued during registration.
    /// </summary>
    /// <param name="request">The user's e-mail address and confirmation token.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    /// <returns>
    /// A user-facing auth flow response on success;
    /// <see cref="BadRequestResult"/> when the token is invalid or expired.
    /// </returns>
    [AllowAnonymous]
    [HttpPost("confirm-email")]
    [ProducesResponseType(typeof(AuthFlowResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status400BadRequest)]
    public virtual async Task<IActionResult> ConfirmEmailAsync([FromBody] EmailConfirmationRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var response = await _authRepository.ConfirmEmailAsync(request, cancellationToken);

            _logger.LogInformation("[AuthController] Email confirmed for {Email}", request.Email);

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("[AuthController] Email confirmation failed for {Email}: {Message}", request.Email, ex.Message);
            return BadRequest(new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// Changes the password for the currently authenticated user.
    /// </summary>
    /// <param name="request">
    /// A <see cref="PasswordChangeRequest"/> containing the current password and the desired new password.
    /// </param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    /// <returns>
    /// <see cref="NoContentResult"/> on success;
    /// <see cref="BadRequestResult"/> when the current password is wrong or validation fails.
    /// </returns>
    [Authorize]
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public virtual async Task<IActionResult> ChangePasswordAsync([FromBody] PasswordChangeRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            await _authRepository.ChangePasswordAsync(request, cancellationToken);

            _logger.LogInformation("[AuthController] Password changed for user {UserId}", request.Id);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("[AuthController] Password change rejected for user {UserId}: {Message}", request.Id, ex.Message);
            return BadRequest(new List<string> { "Password change failed. Please check your input and try again." });
        }
    }

    /// <summary>
    /// Initiates a forgot-password flow by sending a password-reset link to the user's e-mail address.
    /// </summary>
    /// <param name="request">
    /// A <see cref="PasswordForgotRequest"/> containing the user's e-mail and the client callback URI.
    /// </param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    /// <returns>A user-facing auth flow response on success.</returns>
    [AllowAnonymous]
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(AuthFlowResponse), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> ForgotPasswordAsync([FromBody] PasswordForgotRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var response = await _authRepository.ForgotPasswordAsync(request, cancellationToken);

            _logger.LogInformation("[AuthController] Forgot-password flow completed for {Email}", request.Email);

            return Ok(response);
        }
        catch (Exception ex)
        {
            // Never reveal whether the e-mail exists or whether sending failed.
            _logger.LogError(ex, "[AuthController] Forgot-password failed silently for {Email}", request.Email);

            return Ok(new AuthFlowResponse
            {
                UserMessage = "If an account exists for that email address, your request has been received."
            });
        }
    }

    /// <summary>
    /// Resets a user's password using the token issued by a forgot-password request.
    /// </summary>
    /// <param name="request">
    /// A <see cref="ResetPasswordRequest"/> containing the reset token, new password, and user identity.
    /// </param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    /// <returns>
    /// <see cref="NoContentResult"/> on success;
    /// <see cref="BadRequestResult"/> when the token is invalid or has expired.
    /// </returns>
    [AllowAnonymous]
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public virtual async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            await _authRepository.ResetPasswordAsync(request, cancellationToken);

            _logger.LogInformation("[AuthController] Password reset successfully for {Email}", request.Email);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("[AuthController] Password reset rejected for {Email}: {Message}", request.Email, ex.Message);
            return BadRequest(new List<string> { "Password reset failed. Please check your input and try again." });
        }
    }
}
