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

    protected async Task<HttpServiceResult<TResult>> GetAndParseAsync<TResult>(
        Uri requestUri,
        Func<HttpContent, CancellationToken, Task<TResult?>> parser,
        CancellationToken cancellationToken)
    {
        var result = new HttpServiceResult<TResult>();
        try
        {
            var response = await _httpClient.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);
            result.StatusCode = (int)response.StatusCode;
            if (response.IsSuccessStatusCode)
            {
                result.Data = await parser(response.Content, cancellationToken).ConfigureAwait(false);
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

    // Shared helper for POST/PUT/DELETE requests with response parsing
    protected async Task<HttpServiceResult<TResult>> SendAndParseAsync<TResult>(
        Func<Task<HttpResponseMessage>> sendRequest,
        Func<HttpContent, CancellationToken, Task<TResult?>> parser,
        CancellationToken cancellationToken)
    {
        var result = new HttpServiceResult<TResult>();
        try
        {
            var response = await sendRequest().ConfigureAwait(false);
            result.StatusCode = (int)response.StatusCode;
            if (response.IsSuccessStatusCode)
            {
                result.Data = await parser(response.Content, cancellationToken).ConfigureAwait(false);
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

    // Shared helper for POST/PUT/DELETE requests with no content to parse
    protected async Task<HttpServiceResult<bool>> SendAndParseNoContentAsync(
        Func<Task<HttpResponseMessage>> sendRequest,
        CancellationToken cancellationToken)
    {
        var result = new HttpServiceResult<bool>();
        try
        {
            var response = await sendRequest().ConfigureAwait(false);
            result.StatusCode = (int)response.StatusCode;
            if (response.IsSuccessStatusCode)
            {
                result.Data = true;
                result.Success = true;
            }
            else
            {
                result.Data = false;
                result.Errors = await response.TryReadErrors(cancellationToken);
                result.Success = false;
            }
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            result.Data = false;
            result.Errors = [ex.Message];
            result.Success = false;
        }
        return result;
    }

    /// <inheritdoc />
    public virtual async Task<HttpServiceResult<IReadOnlyList<T>>> GetAllAsync(bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpReadService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetAllAsync\"]");

        var uri = new Uri($"{_apiURL}/{includeDetails}");
        return await GetAndParseAsync<IReadOnlyList<T>>(
            uri,
            async (content, ct) => (await content.ReadFromJsonAsync<List<T>>(cancellationToken: ct).ConfigureAwait(false)) ?? [],
            cancellationToken
        );
    }

    /// <inheritdoc />
    public virtual async Task<HttpServiceResult<T?>> GetAsync(TKey id, bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpReadService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetAsync\"]");

        var uri = new Uri($"{_apiURL}/{id}/{includeDetails}");
        return await GetAndParseAsync<T?>(
            uri,
            (content, ct) => content.ReadFromJsonAsync<T>(cancellationToken: ct),
            cancellationToken
        );
    }

    /// <inheritdoc />
    public virtual async Task<HttpServiceResult<long>> GetCountAsync(CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpReadService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetCountAsync\"]");

        var uri = new Uri($"{_apiURL}/count");
        return await GetAndParseAsync<long>(
            uri,
            (content, ct) => content.ReadFromJsonAsync<long>(cancellationToken: ct),
            cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<HttpServiceResult<PageResponse<T>>> GetPagedListAsync(int page, int pageSize, 
        bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page, nameof(page));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize, nameof(pageSize));

        var uri = new Uri($"{_apiURL}/items?page={page}&pageSize={pageSize}&includeDetails={includeDetails}");
        return await GetAndParseAsync<PageResponse<T>>(
            uri,
            async (content, ct) =>
            {
                var str = await content.ReadAsStringAsync(ct).ConfigureAwait(false);
                return JsonSerializer.Deserialize<PageResponse<T>>(str);
            },
            cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<HttpServiceResult<PageResponse<TResult>>> GetPagedListAsync<TResult>(int page, int pageSize,bool includeDetails = false, 
        CancellationToken cancellationToken = default) where TResult : class, new()
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page, nameof(page));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize, nameof(pageSize));

        var uri = new Uri($"{_apiURL}/items?page={page}&pageSize={pageSize}&includeDetails={includeDetails}");
        return await GetAndParseAsync<PageResponse<TResult>>(
            uri,
            async (content, ct) =>
            {
                var str = await content.ReadAsStringAsync(ct).ConfigureAwait(false);
                return JsonSerializer.Deserialize<PageResponse<TResult>>(str);
            },
            cancellationToken
        );
    }
}

/// <summary>
/// Provides HTTP read operations for entities with default key type.
/// </summary>
public class HttpReadService<T>(Uri apiURL, HttpClient httpClient, ILogger<HttpReadService<T>> logger)
    : HttpReadService<T, KeyType>(apiURL, httpClient, logger), IHttpReadService<T> where T : class, IEntity, IModel, new();

