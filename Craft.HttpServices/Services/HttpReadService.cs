using System.Net.Http.Json;
using System.Text.Json;
using Craft.Core;
using Craft.Domain;
using Microsoft.Extensions.Logging;
using Craft.Core.Common;
using Craft.Extensions.HttpResponse;

namespace Craft.HttpServices.Services;

/// <summary>
/// Provides HTTP read operations for entities, returning robust service results.
/// </summary>
public class HttpReadService<T, TKey>(Uri apiURL, HttpClient httpClient, ILogger<HttpReadService<T, TKey>> logger)
    : IHttpReadService<T, TKey> where T : class, IEntity<TKey>, IModel<TKey>, new()
{
    protected readonly Uri _apiURL = apiURL;
    protected readonly HttpClient _httpClient = httpClient;
    protected readonly ILogger _logger = logger;

    /// <inheritdoc />
    public virtual async Task<HttpServiceResult<IReadOnlyList<T>>> GetAllAsync(bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpReadService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetAllAsync\"]");

        var result = new HttpServiceResult<IReadOnlyList<T>>();

        try
        {
            var response = await _httpClient
                .GetAsync(new Uri($"{_apiURL}/{includeDetails}"), cancellationToken)
                .ConfigureAwait(false);

            result.StatusCode = (int)response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                var data = await response
                    .Content
                    .ReadFromJsonAsync<List<T>>(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                result.Data = data ?? [];
                result.Success = true;
            }
            else
            {
                result.Errors = await response.TryReadErrors(cancellationToken);
                result.Success = false;
            }
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            result.Errors = [ex.Message];
            result.Success = false;
        }

        return result;
    }

    /// <inheritdoc />
    public virtual async Task<HttpServiceResult<T?>> GetAsync(TKey id, bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpReadService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetAsync\"]");

        var result = new HttpServiceResult<T?>();

        try
        {
            var response = await _httpClient
                .GetAsync(new Uri($"{_apiURL}/{id}/{includeDetails}"), cancellationToken)
                .ConfigureAwait(false);

            result.StatusCode = (int)response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                result.Data = await response
                    .Content
                    .ReadFromJsonAsync<T>(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                result.Success = true;
            }
            else
            {
                result.Errors = await response.TryReadErrors(cancellationToken);
                result.Success = false;
            }
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            result.Errors = [ex.Message];
            result.Success = false;
        }
        return result;
    }

    /// <inheritdoc />
    public virtual async Task<HttpServiceResult<long>> GetCountAsync(CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpReadService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetCountAsync\"]");

        var result = new HttpServiceResult<long>();

        try
        {
            var response = await _httpClient
                .GetAsync(new Uri($"{_apiURL}/count"), cancellationToken)
                .ConfigureAwait(false);

            result.StatusCode = (int)response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                result.Data = await response
                    .Content
                    .ReadFromJsonAsync<long>(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                result.Success = true;
            }
            else
            {
                result.Errors = await response.TryReadErrors(cancellationToken);
                result.Success = false;
            }
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            result.Errors = [ex.Message];
            result.Success = false;
        }
        return result;
    }

    /// <inheritdoc />
    public async Task<HttpServiceResult<PageResponse<T>>> GetPagedListAsync(int page, int pageSize, 
        bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page, nameof(page));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize, nameof(pageSize));

        var result = new HttpServiceResult<PageResponse<T>>();

        try
        {
            var requestUrl = new Uri($"{_apiURL}/items?page={page}&pageSize={pageSize}&includeDetails={includeDetails}");

            var response = await _httpClient
                .GetAsync(requestUrl, cancellationToken)
                .ConfigureAwait(false);

            result.StatusCode = (int)response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                var content = await response
                    .Content
                    .ReadAsStringAsync(cancellationToken)
                    .ConfigureAwait(false);

                var pagedList = JsonSerializer.Deserialize<PageResponse<T>>(content);
                result.Data = pagedList!;
                result.Success = true;
            }
            else
            {
                result.Errors = await response.TryReadErrors(cancellationToken);
                result.Success = false;
            }
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            result.Errors = [ex.Message];
            result.Success = false;
        }
        return result;
    }

    /// <inheritdoc />
    public async Task<HttpServiceResult<PageResponse<TResult>>> GetPagedListAsync<TResult>(int page, int pageSize,bool includeDetails = false, 
        CancellationToken cancellationToken = default) where TResult : class, new()
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page, nameof(page));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize, nameof(pageSize));

        var result = new HttpServiceResult<PageResponse<TResult>>();

        try
        {
            var requestUrl = new Uri($"{_apiURL}/items?page={page}&pageSize={pageSize}&includeDetails={includeDetails}");

            var response = await _httpClient
                .GetAsync(requestUrl, cancellationToken)
                .ConfigureAwait(false);

            result.StatusCode = (int)response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                var content = await response
                    .Content
                    .ReadAsStringAsync(cancellationToken)
                    .ConfigureAwait(false);

                var pagedResponse = JsonSerializer.Deserialize<PageResponse<TResult>>(content);
                result.Data = pagedResponse!;
                result.Success = true;
            }
            else
            {
                result.Errors = await response.TryReadErrors(cancellationToken);
                result.Success = false;
            }
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            result.Errors = [ex.Message];
            result.Success = false;
        }

        return result;
    }
}

/// <summary>
/// Provides HTTP read operations for entities with default key type.
/// </summary>
public class HttpReadService<T>(Uri apiURL, HttpClient httpClient, ILogger<HttpReadService<T>> logger)
    : HttpReadService<T, KeyType>(apiURL, httpClient, logger), IHttpReadService<T> where T : class, IEntity, IModel, new();

