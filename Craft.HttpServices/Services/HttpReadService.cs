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
public class HttpReadService<T, TKey> : HttpServiceBase, IHttpReadService<T, TKey> where T : class, IEntity<TKey>, IModel<TKey>, new()
{
    protected readonly ILogger _logger;

    public HttpReadService(Uri apiURL, HttpClient httpClient, ILogger<HttpReadService<T, TKey>> logger)
        : base(apiURL, httpClient)
    {
        _logger = logger;
    }

    public virtual async Task<HttpServiceResult<IReadOnlyList<T>?>> GetAllAsync(bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpReadService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetAllAsync\"]");

        var uri = new Uri($"{_apiURL}/{includeDetails}");

        var result = await GetAndParseAsync<List<T>>(
            ct => _httpClient.GetAsync(uri, ct),
            async (content, ct) => (await content.ReadFromJsonAsync<List<T>>(cancellationToken: ct).ConfigureAwait(false)) ?? [],
            cancellationToken
        );

        // Convert to IReadOnlyList<T>
        return new HttpServiceResult<IReadOnlyList<T>?>
        {
            Data = result.Data,
            Success = result.Success,
            Errors = result.Errors,
            StatusCode = result.StatusCode
        };
    }

    public virtual async Task<HttpServiceResult<T?>> GetAsync(TKey id, bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpReadService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetAsync\"]");

        var uri = new Uri($"{_apiURL}/{id}/{includeDetails}");

        return await GetAndParseAsync(
            ct => _httpClient.GetAsync(uri, ct),
            (content, ct) => content.ReadFromJsonAsync<T>(cancellationToken: ct),
            cancellationToken
        );
    }

    public virtual async Task<HttpServiceResult<long>> GetCountAsync(CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpReadService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetCountAsync\"]");

        var uri = new Uri($"{_apiURL}/count");

        return await GetAndParseAsync(
            ct => _httpClient.GetAsync(uri, ct),
            (content, ct) => content.ReadFromJsonAsync<long>(cancellationToken: ct),
            cancellationToken
        );
    }

    public virtual async Task<HttpServiceResult<PageResponse<T>?>> GetPagedListAsync(int page, int pageSize, 
        bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page, nameof(page));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize, nameof(pageSize));

        var uri = new Uri($"{_apiURL}/items?page={page}&pageSize={pageSize}&includeDetails={includeDetails}");

        return await GetAndParseAsync(
            ct => _httpClient.GetAsync(uri, ct),
            async (content, ct) =>
            {
                var str = await content.ReadAsStringAsync(ct).ConfigureAwait(false);
                return JsonSerializer.Deserialize<PageResponse<T>>(str);
            },
            cancellationToken
        );
    }

    public virtual async Task<HttpServiceResult<PageResponse<TResult>?>> GetPagedListAsync<TResult>(int page, int pageSize,bool includeDetails = false, 
        CancellationToken cancellationToken = default) where TResult : class, new()
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page, nameof(page));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize, nameof(pageSize));

        var uri = new Uri($"{_apiURL}/items?page={page}&pageSize={pageSize}&includeDetails={includeDetails}");

        return await GetAndParseAsync(
            ct => _httpClient.GetAsync(uri, ct),
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

