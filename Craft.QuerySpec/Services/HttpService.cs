using System.Net.Http.Json;
using Craft.Core;
using Craft.Core.Common;
using Craft.Domain;
using Craft.HttpServices.Services;
using Microsoft.Extensions.Logging;

namespace Craft.QuerySpec;

/// <summary>
/// Provides HTTP client operations for querying entities with advanced filtering capabilities.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
/// <typeparam name="ViewT">View model type for the entity.</typeparam>
/// <typeparam name="DataTransferT">Data transfer object type for the entity.</typeparam>
/// <typeparam name="TKey">Entity key type.</typeparam>
public class HttpService<T, ViewT, DataTransferT, TKey>(Uri apiURL, HttpClient httpClient, ILogger<HttpService<T, ViewT, DataTransferT, TKey>> logger)
    : HttpChangeService<T, ViewT, DataTransferT, TKey>(apiURL, httpClient, logger), IHttpService<T, ViewT, DataTransferT, TKey>
        where T : class, IEntity<TKey>, IModel<TKey>, new()
        where ViewT : class, IModel<TKey>, new()
        where DataTransferT : class, IModel<TKey>, new()
{
    /// <inheritdoc />
    public virtual async Task<HttpServiceResult<bool>> DeleteAsync(IQuery<T> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"DeleteAsync\"]");

        return await SendAndParseNoContentAsync(
            () => _httpClient.PostAsJsonAsync(new Uri($"{_apiURL}/delete"), query, cancellationToken),
            cancellationToken
        );
    }

    /// <inheritdoc />
    public virtual async Task<HttpServiceResult<List<T>>> GetAllAsync(IQuery<T> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetAllAsync\"]");

        query.SetPage(1, int.MaxValue);
        return await GetAllFromPagedAsync<T, PageResponse<T>>(
            ct => GetPagedListAsync(query, ct)!,
            paged => paged?.Items?.ToList() ?? [],
            cancellationToken
        );
    }

    /// <inheritdoc />
    public virtual async Task<HttpServiceResult<List<TResult>>> GetAllAsync<TResult>(IQuery<T, TResult> query, CancellationToken cancellationToken = default)
        where TResult : class, new()
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetAllAsync<{typeof(TResult).Name}>\"]");

        query.SetPage(1, int.MaxValue);

        return await GetAllFromPagedAsync<TResult, PageResponse<TResult>>(
            ct => GetPagedListAsync(query, ct)!,
            paged => {
                // Defensive: If paged is null or paged.Items is null or empty, return empty list
                if (paged == null || paged.Items == null)
                    return [];

                if (paged.Items is ICollection<TResult> items && items.Count == 0)
                    return [];

                // If it's not a collection, enumerate and check count
                var list = paged.Items.ToList();

                if (list.Count == 0)
                    return [];

                return list;
            },
            cancellationToken
        );
    }

    /// <inheritdoc />
    public virtual async Task<HttpServiceResult<T?>> GetAsync(IQuery<T> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetAsync\"]");

        return await SendAndParseAsync(
            () => _httpClient.PostAsJsonAsync(new Uri($"{_apiURL}/find"), query, cancellationToken),
            (content, ct) => content.ReadFromJsonAsync<T>(cancellationToken: ct),
            cancellationToken
        );
    }

    /// <inheritdoc />
    public virtual async Task<HttpServiceResult<TResult?>> GetAsync<TResult>(IQuery<T, TResult> query, CancellationToken cancellationToken = default)
        where TResult : class, new()
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetAsync<{typeof(TResult).Name}>\"]");

        return await SendAndParseAsync(
            () => _httpClient.PostAsJsonAsync(new Uri($"{_apiURL}/findone"), query, cancellationToken),
            (content, ct) => content.ReadFromJsonAsync<TResult>(cancellationToken: ct),
            cancellationToken
        );
    }

    /// <inheritdoc />
    public virtual async Task<HttpServiceResult<long>> GetCountAsync(IQuery<T> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetCountAsync\"]");

        return await SendAndParseAsync(
            () => _httpClient.PostAsJsonAsync(new Uri($"{_apiURL}/filtercount"), query, cancellationToken),
            (content, ct) => content.ReadFromJsonAsync<long>(cancellationToken: ct),
            cancellationToken
        );
    }

    /// <inheritdoc />
    public virtual async Task<HttpServiceResult<PageResponse<T>?>> GetPagedListAsync(IQuery<T> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetPagedListAsync\"]");

        return await SendAndParseAsync<PageResponse<T>?>(
            () => _httpClient.PostAsJsonAsync(new Uri($"{_apiURL}/search"), query, cancellationToken),
            (content, ct) => content.ReadFromJsonAsync<PageResponse<T>>(cancellationToken: ct),
            cancellationToken
        );
    }

    /// <inheritdoc />
    public virtual async Task<HttpServiceResult<PageResponse<TResult>?>> GetPagedListAsync<TResult>(IQuery<T, TResult> query, CancellationToken cancellationToken = default)
        where TResult : class, new()
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetPagedListAsync<{typeof(TResult).Name}>\"]");

        return await SendAndParseAsync<PageResponse<TResult>?>(
            () => _httpClient.PostAsJsonAsync(new Uri($"{_apiURL}/select"), query, cancellationToken),
            (content, ct) => content.ReadFromJsonAsync<PageResponse<TResult>>(cancellationToken: ct),
            cancellationToken
        );
    }
}

/// <summary>
/// Simplified version of HttpService for use with default key type.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
/// <typeparam name="ViewT">View model type for the entity.</typeparam>
/// <typeparam name="DataTransferT">Data transfer object type for the entity.</typeparam>
public class HttpService<T, ViewT, DataTransferT>(Uri apiURL, HttpClient httpClient, ILogger<HttpService<T, ViewT, DataTransferT>> logger)
    : HttpService<T, ViewT, DataTransferT, KeyType>(apiURL, httpClient, logger), IHttpService<T, ViewT, DataTransferT>
    where T : class, IEntity, IModel, new()
    where ViewT : class, IModel, new()
    where DataTransferT : class, IModel, new();

/// <summary>
/// Simplified version of HttpService for use with default entity, view, and DTO types.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
public class HttpService<T>(Uri apiURL, HttpClient httpClient, ILogger<HttpService<T>> logger)
    : HttpService<T, T, T, KeyType>(apiURL, httpClient, logger), IHttpService<T> where T : class, IEntity, IModel, new();
