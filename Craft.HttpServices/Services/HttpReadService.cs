using System.Net.Http.Json;
using System.Text.Json;
using Craft.Core;
using Craft.Domain;
using Microsoft.Extensions.Logging;

namespace Craft.HttpServices.Services;

public class HttpReadService<T, TKey>(Uri apiURL, HttpClient httpClient, ILogger<HttpReadService<T, TKey>> logger)
    : IHttpReadService<T, TKey> where T : class, IEntity<TKey>, IModel<TKey>, new()
{
    protected readonly Uri _apiURL = apiURL;
    protected readonly HttpClient _httpClient = httpClient;
    protected readonly ILogger _logger = logger;

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpReadService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetAllAsync\"]");

        HttpResponseMessage response = await _httpClient
            .GetAsync(new Uri($"{_apiURL}/{includeDetails}"), cancellationToken)
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        return await response
            .Content
            .ReadFromJsonAsync<List<T>>(cancellationToken: cancellationToken)
            .ConfigureAwait(false) ?? new List<T>();
    }

    public virtual async Task<T?> GetAsync(TKey id, bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpReadService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetAsync\"]");

        HttpResponseMessage response = await _httpClient
            .GetAsync(new Uri($"{_apiURL}/{id}/{includeDetails}"), cancellationToken)
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        return await response
            .Content
            .ReadFromJsonAsync<T>(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    public virtual async Task<long> GetCountAsync(CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpReadService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetCountAsync\"]");

        HttpResponseMessage response = await _httpClient
            .GetAsync(new Uri($"{_apiURL}/count"), cancellationToken)
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        return await response
            .Content
            .ReadFromJsonAsync<long>(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<PageResponse<T>> GetPagedListAsync(int page, int pageSize, bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page, nameof(page));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize, nameof(pageSize));

        var requestUrl = new Uri($"{_apiURL}/items?page={page}&pageSize={pageSize}&includeDetails={includeDetails}");

        var response = await _httpClient
            .GetAsync(requestUrl, cancellationToken)
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        var content = await response
            .Content
            .ReadAsStringAsync(cancellationToken);

        var pagedList = JsonSerializer.Deserialize<PageResponse<T>>(content);
        return pagedList!;
    }

    public async Task<PageResponse<TResult>> GetPagedListAsync<TResult>(int page, int pageSize, bool includeDetails = false, CancellationToken cancellationToken = default) where TResult : class, new()
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page, nameof(page));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize, nameof(pageSize));

        var requestUrl = new Uri($"{_apiURL}/items?page={page}&pageSize={pageSize}&includeDetails={includeDetails}");

        var response = await _httpClient
            .GetAsync(requestUrl, cancellationToken)
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        var content = await response
            .Content
            .ReadAsStringAsync(cancellationToken);

        var pagedResponse = JsonSerializer.Deserialize<PageResponse<TResult>>(content);
        if (pagedResponse == null)
            throw new InvalidOperationException("Failed to deserialize the paged response.");
        return pagedResponse;
    }
}

public class HttpReadService<T>(Uri apiURL, HttpClient httpClient, ILogger<HttpReadService<T>> logger)
    : HttpReadService<T, KeyType>(apiURL, httpClient, logger), IHttpReadService<T>
    where T : class, IEntity, IModel, new();

