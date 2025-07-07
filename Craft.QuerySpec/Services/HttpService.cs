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
    /// <summary>
    /// Deletes all entities that match the specified query criteria.
    /// </summary>
    /// <param name="query">A query containing filtering parameters.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The HTTP response message.</returns>
    /// <exception cref="ArgumentNullException">Thrown when query is null.</exception>
    public virtual async Task<HttpServiceResult<bool>> DeleteAsync(IQuery<T> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"DeleteAsync\"]");

        var response = await _httpClient
            .PostAsJsonAsync(new Uri($"{_apiURL}/delete"), query, cancellationToken)
            .ConfigureAwait(false);

        var result = new HttpServiceResult<bool> { StatusCode = (int)response.StatusCode };

        if (response.IsSuccessStatusCode)
        {
            result.Data = true;
            result.Success = true;
        }
        else
        {
            result.Data = false;
            result.Errors = await TryReadErrors(response, cancellationToken);
            result.Success = false;
        }
        return result;
    }

    /// <summary>
    /// Gets all entities that match the specified query criteria.
    /// </summary>
    /// <param name="query">A query containing filtering and sorting parameters.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A list of entities.</returns>
    /// <exception cref="ArgumentNullException">Thrown when query is null.</exception>
    public virtual async Task<List<T>> GetAllAsync(IQuery<T> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetAllAsync\"]");

        query.SetPage(1, int.MaxValue);

        var pagedResult = await GetPagedListAsync(query, cancellationToken)
            .ConfigureAwait(false);

        return pagedResult?.Items?.ToList() ?? [];
    }

    /// <summary>
    /// Gets all projected entities that match the specified query criteria.
    /// </summary>
    /// <typeparam name="TResult">The type to project to.</typeparam>
    /// <param name="query">A query containing filtering, sorting, and projection parameters.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A list of projected entities.</returns>
    /// <exception cref="ArgumentNullException">Thrown when query is null.</exception>
    public virtual async Task<List<TResult>> GetAllAsync<TResult>(IQuery<T, TResult> query, CancellationToken cancellationToken = default)
        where TResult : class, new()
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetAllAsync<{typeof(TResult).Name}>\"]");

        query.SetPage(1, int.MaxValue);

        var pagedResult = await GetPagedListAsync(query, cancellationToken)
            .ConfigureAwait(false);

        return pagedResult?.Items?.ToList() ?? [];
    }

    /// <summary>
    /// Gets a single entity that matches the specified query criteria.
    /// </summary>
    /// <param name="query">A query containing filtering parameters.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The entity or null if no match found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when query is null.</exception>
    /// <exception cref="HttpRequestException">Thrown when the request fails.</exception>
    public virtual async Task<T?> GetAsync(IQuery<T> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetAsync\"]");

        HttpResponseMessage response = await _httpClient
            .PostAsJsonAsync(new Uri($"{_apiURL}/find"), query, cancellationToken)
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        return await response
            .Content
            .ReadFromJsonAsync<T>(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a single projected entity that matches the specified query criteria.
    /// </summary>
    /// <typeparam name="TResult">The type to project to.</typeparam>
    /// <param name="query">A query containing filtering and projection parameters.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The projected entity or null if no match found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when query is null.</exception>
    /// <exception cref="HttpRequestException">Thrown when the request fails.</exception>
    public virtual async Task<TResult?> GetAsync<TResult>(IQuery<T, TResult> query, CancellationToken cancellationToken = default)
        where TResult : class, new()
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetAsync<{typeof(TResult).Name}>\"]");

        HttpResponseMessage response = await _httpClient
            .PostAsJsonAsync(new Uri($"{_apiURL}/findone"), query, cancellationToken)
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        return await response
            .Content
            .ReadFromJsonAsync<TResult>(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the count of entities that match the specified query criteria.
    /// </summary>
    /// <param name="query">A query containing filtering parameters.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The count of matching entities.</returns>
    /// <exception cref="ArgumentNullException">Thrown when query is null.</exception>
    /// <exception cref="HttpRequestException">Thrown when the request fails.</exception>
    public virtual async Task<long> GetCountAsync(IQuery<T> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetCountAsync\"]");

        HttpResponseMessage response = await _httpClient
            .PostAsJsonAsync(new Uri($"{_apiURL}/filtercount"), query, cancellationToken)
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        return await response
            .Content
            .ReadFromJsonAsync<long>(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a paginated list of entities that match the specified query criteria.
    /// </summary>
    /// <param name="query">A query containing filtering and sorting parameters.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A paginated response of entities.</returns>
    /// <exception cref="ArgumentNullException">Thrown when query is null.</exception>
    /// <exception cref="HttpRequestException">Thrown when the request fails.</exception>
    public virtual async Task<PageResponse<T>?> GetPagedListAsync(IQuery<T> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetPagedListAsync\"]");

        HttpResponseMessage response = await _httpClient
            .PostAsJsonAsync(new Uri($"{_apiURL}/search"), query, cancellationToken)
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        return await response
            .Content
            .ReadFromJsonAsync<PageResponse<T>>(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a paginated list of projected entities that match the specified query criteria.
    /// </summary>
    /// <typeparam name="TResult">The type to project to.</typeparam>
    /// <param name="query">A query containing filtering, sorting, and projection parameters.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A paginated response of projected entities.</returns>
    /// <exception cref="ArgumentNullException">Thrown when query is null.</exception>
    /// <exception cref="HttpRequestException">Thrown when the request fails.</exception>
    public virtual async Task<PageResponse<TResult>?> GetPagedListAsync<TResult>(IQuery<T, TResult> query, CancellationToken cancellationToken = default)
        where TResult : class, new()
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetPagedListAsync<{typeof(TResult).Name}>\"]");

        HttpResponseMessage response = await _httpClient
            .PostAsJsonAsync(new Uri($"{_apiURL}/select"), query, cancellationToken)
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        return await response
            .Content
            .ReadFromJsonAsync<PageResponse<TResult>>(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    private static async Task<List<string>> TryReadErrors(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var errors = await response.Content.ReadFromJsonAsync<List<string>>(cancellationToken: cancellationToken);
            return errors ?? [$"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}"];
        }
        catch
        {
            var text = await response.Content.ReadAsStringAsync(cancellationToken);
            return [string.IsNullOrWhiteSpace(text) ? $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}" : text];
        }
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
    : HttpService<T, T, T, KeyType>(apiURL, httpClient, logger), IHttpService<T>
        where T : class, IEntity, IModel, new();
