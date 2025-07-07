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
        var result = new HttpServiceResult<bool>();
        try
        {
            var response = await _httpClient.PostAsJsonAsync(new Uri($"{_apiURL}/delete"), query, cancellationToken).ConfigureAwait(false);
            result.StatusCode = (int)response.StatusCode;
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
    public virtual async Task<HttpServiceResult<List<T>>> GetAllAsync(IQuery<T> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetAllAsync\"]");
        var result = new HttpServiceResult<List<T>>();
        try
        {
            query.SetPage(1, int.MaxValue);
            var pagedResult = await GetPagedListAsync(query, cancellationToken).ConfigureAwait(false);
            result.Data = pagedResult?.Data?.Items?.ToList() ?? [];
            result.Success = pagedResult?.Success ?? false;
            result.Errors = pagedResult?.Errors;
            result.StatusCode = pagedResult?.StatusCode;
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
    public virtual async Task<HttpServiceResult<List<TResult>>> GetAllAsync<TResult>(IQuery<T, TResult> query, CancellationToken cancellationToken = default)
        where TResult : class, new()
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetAllAsync<{typeof(TResult).Name}>\"]");
        var result = new HttpServiceResult<List<TResult>>();
        try
        {
            query.SetPage(1, int.MaxValue);
            var pagedResult = await GetPagedListAsync(query, cancellationToken).ConfigureAwait(false);
            result.Data = pagedResult?.Data?.Items?.ToList() ?? [];
            result.Success = pagedResult?.Success ?? false;
            result.Errors = pagedResult?.Errors;
            result.StatusCode = pagedResult?.StatusCode;
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
    public virtual async Task<HttpServiceResult<T?>> GetAsync(IQuery<T> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetAsync\"]");
        var result = new HttpServiceResult<T?>();
        try
        {
            var response = await _httpClient.PostAsJsonAsync(new Uri($"{_apiURL}/find"), query, cancellationToken).ConfigureAwait(false);
            result.StatusCode = (int)response.StatusCode;
            if (response.IsSuccessStatusCode)
            {
                result.Data = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken).ConfigureAwait(false);
                result.Success = true;
            }
            else
            {
                result.Errors = await TryReadErrors(response, cancellationToken);
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
    public virtual async Task<HttpServiceResult<TResult?>> GetAsync<TResult>(IQuery<T, TResult> query, CancellationToken cancellationToken = default)
        where TResult : class, new()
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetAsync<{typeof(TResult).Name}>\"]");
        var result = new HttpServiceResult<TResult?>();
        try
        {
            var response = await _httpClient.PostAsJsonAsync(new Uri($"{_apiURL}/findone"), query, cancellationToken).ConfigureAwait(false);
            result.StatusCode = (int)response.StatusCode;
            if (response.IsSuccessStatusCode)
            {
                result.Data = await response.Content.ReadFromJsonAsync<TResult>(cancellationToken: cancellationToken).ConfigureAwait(false);
                result.Success = true;
            }
            else
            {
                result.Errors = await TryReadErrors(response, cancellationToken);
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
    public virtual async Task<HttpServiceResult<long>> GetCountAsync(IQuery<T> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetCountAsync\"]");
        var result = new HttpServiceResult<long>();
        try
        {
            var response = await _httpClient.PostAsJsonAsync(new Uri($"{_apiURL}/filtercount"), query, cancellationToken).ConfigureAwait(false);
            result.StatusCode = (int)response.StatusCode;
            if (response.IsSuccessStatusCode)
            {
                result.Data = await response.Content.ReadFromJsonAsync<long>(cancellationToken: cancellationToken).ConfigureAwait(false);
                result.Success = true;
            }
            else
            {
                result.Errors = await TryReadErrors(response, cancellationToken);
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
    public virtual async Task<HttpServiceResult<PageResponse<T>?>> GetPagedListAsync(IQuery<T> query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetPagedListAsync\"]");
        var result = new HttpServiceResult<PageResponse<T>?>();
        try
        {
            var response = await _httpClient.PostAsJsonAsync(new Uri($"{_apiURL}/search"), query, cancellationToken).ConfigureAwait(false);
            result.StatusCode = (int)response.StatusCode;
            if (response.IsSuccessStatusCode)
            {
                result.Data = await response.Content.ReadFromJsonAsync<PageResponse<T>>(cancellationToken: cancellationToken).ConfigureAwait(false);
                result.Success = true;
            }
            else
            {
                result.Errors = await TryReadErrors(response, cancellationToken);
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
    public virtual async Task<HttpServiceResult<PageResponse<TResult>?>> GetPagedListAsync<TResult>(IQuery<T, TResult> query, CancellationToken cancellationToken = default)
        where TResult : class, new()
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"GetPagedListAsync<{typeof(TResult).Name}>\"]");
        var result = new HttpServiceResult<PageResponse<TResult>?>();
        try
        {
            var response = await _httpClient.PostAsJsonAsync(new Uri($"{_apiURL}/select"), query, cancellationToken).ConfigureAwait(false);
            result.StatusCode = (int)response.StatusCode;
            if (response.IsSuccessStatusCode)
            {
                result.Data = await response.Content.ReadFromJsonAsync<PageResponse<TResult>>(cancellationToken: cancellationToken).ConfigureAwait(false);
                result.Success = true;
            }
            else
            {
                result.Errors = await TryReadErrors(response, cancellationToken);
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
