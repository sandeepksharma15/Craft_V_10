using System.Net.Http.Json;
using Craft.Domain;
using Mapster;
using Microsoft.Extensions.Logging;

namespace Craft.HttpServices.Services;

public class HttpChangeService<T, ViewT, DataTransferT, TKey>(Uri apiURL, HttpClient httpClient, ILogger<HttpChangeService<T, ViewT, DataTransferT, TKey>> logger)
    : HttpReadService<T, TKey>(apiURL, httpClient, logger), IHttpChangeService<T, ViewT, DataTransferT, TKey>
        where T : class, IEntity<TKey>, IModel<TKey>, new()
        where ViewT : class, IModel<TKey>, new()
        where DataTransferT : class, IModel<TKey>, new()
{
    /// inheritdoc />
    public virtual async Task<HttpResponseMessage> AddAsync(ViewT model, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpChangeService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"AddAsync\"]");

        DataTransferT dto = model.Adapt<DataTransferT>();

        return await _httpClient
            .PostAsJsonAsync(_apiURL, dto, cancellationToken: cancellationToken);
    }

    /// inheritdoc />
    public virtual async Task<HttpResponseMessage> AddRangeAsync(IReadOnlyCollection<ViewT> models, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpChangeService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"AddRangeAsync\"]");

        IEnumerable<DataTransferT> dtos = models.Adapt<IEnumerable<DataTransferT>>();

        return await _httpClient
            .PostAsJsonAsync(new Uri($"{_apiURL}/addrange"), dtos, cancellationToken: cancellationToken);
    }

    /// inheritdoc />
    public virtual async Task<HttpResponseMessage> AddRangeAsync(IEnumerable<ViewT> models, CancellationToken cancellationToken = default)
        => await AddRangeAsync(models is IReadOnlyCollection<ViewT> c ? c : models.ToList(), cancellationToken);

    /// inheritdoc />
    public virtual async Task<HttpResponseMessage> DeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpChangeService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"DeleteAsync\"]");

        return await _httpClient
            .DeleteAsync(new Uri($"{_apiURL}/{id}"), cancellationToken);
    }

    /// inheritdoc />
    public virtual async Task<HttpResponseMessage> DeleteRangeAsync(IReadOnlyCollection<ViewT> models, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpChangeService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"DeleteRangeAsync\"]");

        IEnumerable<DataTransferT> dtos = models.Adapt<IEnumerable<DataTransferT>>();

        return await _httpClient
            .PutAsJsonAsync(new Uri($"{_apiURL}/RemoveRange"), dtos, cancellationToken: cancellationToken);
    }

    /// inheritdoc />
    public virtual async Task<HttpResponseMessage> DeleteRangeAsync(IEnumerable<ViewT> models, CancellationToken cancellationToken = default)
        => await DeleteRangeAsync(models is IReadOnlyCollection<ViewT> c ? c : models.ToList(), cancellationToken);

    /// inheritdoc />
    public virtual async Task<HttpResponseMessage> UpdateAsync(ViewT model, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpChangeService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"UpdateAsync\"]");

        DataTransferT dto = model.Adapt<DataTransferT>();

        return await _httpClient.PutAsJsonAsync(_apiURL, dto, cancellationToken: cancellationToken);
    }

    /// inheritdoc />
    public virtual async Task<HttpResponseMessage> UpdateRangeAsync(IReadOnlyCollection<ViewT> models, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpChangeService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"UpdateRangeAsync\"]");

        IEnumerable<DataTransferT> dtos = models.Adapt<IEnumerable<DataTransferT>>();

        return await _httpClient
            .PutAsJsonAsync(new Uri($"{_apiURL}/UpdateRange"), dtos, cancellationToken: cancellationToken);
    }

    /// inheritdoc />
    public virtual async Task<HttpResponseMessage> UpdateRangeAsync(IEnumerable<ViewT> models, CancellationToken cancellationToken = default)
        => await UpdateRangeAsync(models is IReadOnlyCollection<ViewT> c ? c : models.ToList(), cancellationToken);
}


public class HttpChangeService<T, ViewT, DataTransferT>(Uri apiURL, HttpClient httpClient, ILogger<HttpChangeService<T, ViewT, DataTransferT>> logger)
    : HttpChangeService<T, ViewT, DataTransferT, KeyType>(apiURL, httpClient, logger), IHttpChangeService<T, ViewT, DataTransferT>
        where T : class, IEntity, IModel, new()
        where ViewT : class, IModel, new()
        where DataTransferT : class, IModel, new();
