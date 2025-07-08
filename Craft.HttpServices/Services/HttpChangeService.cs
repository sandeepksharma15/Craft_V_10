using System.Net.Http.Json;
using Craft.Domain;
using Mapster;
using Microsoft.Extensions.Logging;
using Craft.Core.Common;
using Craft.Extensions.HttpResponse;

namespace Craft.HttpServices.Services;

/// <summary>
/// Provides HTTP change (CRUD) operations for entities, returning robust service results.
/// </summary>
public class HttpChangeService<T, ViewT, DataTransferT, TKey>(Uri apiURL, HttpClient httpClient, ILogger<HttpChangeService<T, ViewT, DataTransferT, TKey>> logger)
    : HttpReadService<T, TKey>(apiURL, httpClient, logger), IHttpChangeService<T, ViewT, DataTransferT, TKey>
        where T : class, IEntity<TKey>, IModel<TKey>, new()
        where ViewT : class, IModel<TKey>, new()
        where DataTransferT : class, IModel<TKey>, new()
{
    /// <inheritdoc />
    public virtual async Task<HttpServiceResult<T>> AddAsync(ViewT model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model, nameof(model));
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpChangeService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"AddAsync\"]");
        DataTransferT dto = model.Adapt<DataTransferT>();
        return await SendAndParseAsync<T>(
            () => _httpClient.PostAsJsonAsync(_apiURL, dto, cancellationToken: cancellationToken),
            (content, ct) => content.ReadFromJsonAsync<T>(cancellationToken: ct),
            cancellationToken
        );
    }

    /// <inheritdoc />
    public virtual async Task<HttpServiceResult<List<T>>> AddRangeAsync(IReadOnlyCollection<ViewT> models, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(models, nameof(models));
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpChangeService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"AddRangeAsync\"]");
        IEnumerable<DataTransferT> dtos = models.Adapt<IEnumerable<DataTransferT>>();
        return await SendAndParseAsync<List<T>>(
            () => _httpClient.PostAsJsonAsync(new Uri($"{_apiURL}/addrange"), dtos, cancellationToken: cancellationToken),
            async (content, ct) => (await content.ReadFromJsonAsync<List<T>>(cancellationToken: ct).ConfigureAwait(false)) ?? [],
            cancellationToken
        );
    }

    /// <inheritdoc />
    public virtual async Task<HttpServiceResult<List<T>>> AddRangeAsync(IEnumerable<ViewT> models, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(models, nameof(models));
        return await AddRangeAsync(models is IReadOnlyCollection<ViewT> c ? c : models.ToList(), cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<HttpServiceResult<bool>> DeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id, nameof(id));
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpChangeService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"DeleteAsync\"]");
        return await SendAndParseNoContentAsync(
            () => _httpClient.DeleteAsync(new Uri($"{_apiURL}/{id}"), cancellationToken),
            cancellationToken
        );
    }

    /// <inheritdoc />
    public virtual async Task<HttpServiceResult<bool>> DeleteRangeAsync(IReadOnlyCollection<ViewT> models, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(models, nameof(models));
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpChangeService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"DeleteRangeAsync\"]");
        IEnumerable<DataTransferT> dtos = models.Adapt<IEnumerable<DataTransferT>>();
        return await SendAndParseNoContentAsync(
            () => _httpClient.PutAsJsonAsync(new Uri($"{_apiURL}/RemoveRange"), dtos, cancellationToken: cancellationToken),
            cancellationToken
        );
    }

    /// <inheritdoc />
    public virtual async Task<HttpServiceResult<bool>> DeleteRangeAsync(IEnumerable<ViewT> models, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(models, nameof(models));
        return await DeleteRangeAsync(models is IReadOnlyCollection<ViewT> c ? c : models.ToList(), cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task<HttpServiceResult<T>> UpdateAsync(ViewT model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model, nameof(model));
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpChangeService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"UpdateAsync\"]");
        DataTransferT dto = model.Adapt<DataTransferT>();
        return await SendAndParseAsync<T>(
            () => _httpClient.PutAsJsonAsync(_apiURL, dto, cancellationToken: cancellationToken),
            (content, ct) => content.ReadFromJsonAsync<T>(cancellationToken: ct),
            cancellationToken
        );
    }

    /// <inheritdoc />
    public virtual async Task<HttpServiceResult<List<T>>> UpdateRangeAsync(IReadOnlyCollection<ViewT> models, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(models, nameof(models));
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpChangeService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"UpdateRangeAsync\"]");
        IEnumerable<DataTransferT> dtos = models.Adapt<IEnumerable<DataTransferT>>();
        return await SendAndParseAsync<List<T>>(
            () => _httpClient.PutAsJsonAsync(new Uri($"{_apiURL}/UpdateRange"), dtos, cancellationToken: cancellationToken),
            async (content, ct) => (await content.ReadFromJsonAsync<List<T>>(cancellationToken: ct).ConfigureAwait(false)) ?? [],
            cancellationToken
        );
    }

    /// <inheritdoc />
    public virtual async Task<HttpServiceResult<List<T>>> UpdateRangeAsync(IEnumerable<ViewT> models, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(models, nameof(models));
        return await UpdateRangeAsync(models is IReadOnlyCollection<ViewT> c ? c : models.ToList(), cancellationToken).ConfigureAwait(false);
    }
}

/// <summary>
/// Provides HTTP change (CRUD) operations for entities with default key type.
/// </summary>
public class HttpChangeService<T, ViewT, DataTransferT>(Uri apiURL, HttpClient httpClient, ILogger<HttpChangeService<T, ViewT, DataTransferT>> logger)
    : HttpChangeService<T, ViewT, DataTransferT, KeyType>(apiURL, httpClient, logger), IHttpChangeService<T, ViewT, DataTransferT>
        where T : class, IEntity, IModel, new()
        where ViewT : class, IModel, new()
        where DataTransferT : class, IModel, new();
