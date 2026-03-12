using System.Net.Http.Json;
using Craft.Core;
using Craft.HttpServices;
using Craft.Security;
using Microsoft.Extensions.Logging;

namespace Craft.AppComponents.Security;

/// <summary>
/// HTTP service that communicates with the API auth endpoints (<c>/api/auth/*</c>).
/// Inherits <see cref="HttpServiceBase"/> to reuse the standard
/// <c>SendAndParseAsync</c> / <c>SendAndParseNoContentAsync</c> helpers.
/// </summary>
/// <typeparam name="TUserVM">The view-model type used for user registration.</typeparam>
public class AuthHttpService<TUserVM>(Uri apiURL, HttpClient httpClient, ILogger<AuthHttpService<TUserVM>> logger)
    : HttpServiceBase(apiURL, httpClient), IAuthHttpService<TUserVM>
    where TUserVM : class
{
    protected readonly ILogger<AuthHttpService<TUserVM>> _logger = logger;

    /// <inheritdoc />
    public async Task<ServiceResult<JwtAuthResponse>> LoginAsync(UserLoginRequest model, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug("[AuthHttpService] Method: [\"LoginAsync\"] Email: [{Email}]", model.Email);

        var result = await SendAndParseAsync(
            token => _httpClient.PostAsJsonAsync(new Uri($"{_apiURL}/login"), model, cancellationToken: token),
            (content, token) => content.ReadFromJsonAsync<JwtAuthResponse>(cancellationToken: token),
            ct).ConfigureAwait(false);

        return ToRequired(result);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<JwtAuthResponse>> RefreshAsync(RefreshTokenRequest model, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug("[AuthHttpService] Method: [\"RefreshAsync\"]");

        var result = await SendAndParseAsync(
            token => _httpClient.PostAsJsonAsync(new Uri($"{_apiURL}/refresh"), model, cancellationToken: token),
            (content, token) => content.ReadFromJsonAsync<JwtAuthResponse>(cancellationToken: token),
            ct).ConfigureAwait(false);

        return ToRequired(result);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> LogoutAsync(CancellationToken ct = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug("[AuthHttpService] Method: [\"LogoutAsync\"]");

        var result = await SendAndParseNoContentAsync(
            token => _httpClient.PostAsync(new Uri($"{_apiURL}/logout"), content: null, cancellationToken: token),
            ct).ConfigureAwait(false);

        return ToNonGeneric(result);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> RegisterAsync(TUserVM model, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug("[AuthHttpService] Method: [\"RegisterAsync\"]");

        var result = await SendAndParseNoContentAsync(
            token => _httpClient.PostAsJsonAsync(new Uri($"{_apiURL}/register"), model, cancellationToken: token),
            ct).ConfigureAwait(false);

        return ToNonGeneric(result);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> ChangePasswordAsync(PasswordChangeRequest model, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug("[AuthHttpService] Method: [\"ChangePasswordAsync\"]");

        var result = await SendAndParseNoContentAsync(
            token => _httpClient.PostAsJsonAsync(new Uri($"{_apiURL}/change-password"), model, cancellationToken: token),
            ct).ConfigureAwait(false);

        return ToNonGeneric(result);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> ForgotPasswordAsync(PasswordForgotRequest model, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug("[AuthHttpService] Method: [\"ForgotPasswordAsync\"]");

        var result = await SendAndParseNoContentAsync(
            token => _httpClient.PostAsJsonAsync(new Uri($"{_apiURL}/forgot-password"), model, cancellationToken: token),
            ct).ConfigureAwait(false);

        return ToNonGeneric(result);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> ResetPasswordAsync(ResetPasswordRequest model, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug("[AuthHttpService] Method: [\"ResetPasswordAsync\"]");

        var result = await SendAndParseNoContentAsync(
            token => _httpClient.PostAsJsonAsync(new Uri($"{_apiURL}/reset-password"), model, cancellationToken: token),
            ct).ConfigureAwait(false);

        return ToNonGeneric(result);
    }

    /// <summary>
    /// Converts a nullable <see cref="ServiceResult{T}"/> to a non-nullable variant,
    /// returning a failure if the response body was empty.
    /// </summary>
    private static ServiceResult<TResult> ToRequired<TResult>(ServiceResult<TResult?> result)
    {
        if (result.IsSuccess && result.Value is not null)
            return ServiceResult<TResult>.Success(result.Value, result.StatusCode ?? 200);

        return ServiceResult<TResult>.Failure(result.Errors ?? [], statusCode: result.StatusCode);
    }

    /// <summary>
    /// Converts <see cref="ServiceResult{T}"/> of <see cref="bool"/> from
    /// <see cref="HttpServiceBase.SendAndParseNoContentAsync"/> into a non-generic <see cref="ServiceResult"/>.
    /// </summary>
    private static ServiceResult ToNonGeneric(ServiceResult<bool> result)
    {
        return result.IsSuccess
            ? ServiceResult.Success()
            : ServiceResult.Failure(result.Errors ?? [], statusCode: result.StatusCode);
    }
}
