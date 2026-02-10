using Craft.Core;
using System.Net.Http.Json;
using Craft.Core.Common;
using Craft.Domain;
using Mapster;
using Microsoft.Extensions.Logging;

namespace Craft.HttpServices.Services;

public class HttpChangeService<T, ViewT, DataTransferT, TKey> : HttpReadService<T, TKey>, IHttpChangeService<T, ViewT, DataTransferT, TKey>
    where T : class, IEntity<TKey>, IModel<TKey>, new()
    where ViewT : class, IModel<TKey>, new()
    where DataTransferT : class, IModel<TKey>, new()
{
    public HttpChangeService(Uri apiURL, HttpClient httpClient, ILogger<HttpChangeService<T, ViewT, DataTransferT, TKey>> logger)
        : base(apiURL, httpClient, logger) { }

    public virtual async Task<ServiceResult<T?>> AddAsync(ViewT model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model, nameof(model));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpChangeService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"AddAsync\"]");

        DataTransferT dto = model.Adapt<DataTransferT>();

        return await SendAndParseAsync(
            () => _httpClient.PostAsJsonAsync(_apiURL, dto, cancellationToken: cancellationToken),
            (content, ct) => content.ReadFromJsonAsync<T>(cancellationToken: ct),
            cancellationToken
        );
    }

    public virtual async Task<ServiceResult<List<T>?>> AddRangeAsync(IReadOnlyCollection<ViewT> models, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(models, nameof(models));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpChangeService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"AddRangeAsync\"]");

        IEnumerable<DataTransferT> dtos = models.Adapt<IEnumerable<DataTransferT>>();

        return await SendAndParseAsync(
            () => _httpClient.PostAsJsonAsync(new Uri($"{_apiURL}/addrange"), dtos, cancellationToken: cancellationToken),
            async (content, ct) => (await content.ReadFromJsonAsync<List<T>>(cancellationToken: ct).ConfigureAwait(false)) ?? [],
            cancellationToken
        );
    }

    public virtual async Task<ServiceResult<bool>> DeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id, nameof(id));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpChangeService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"DeleteAsync\"]");

        return await SendAndParseNoContentAsync(
            () => _httpClient.DeleteAsync(new Uri($"{_apiURL}/{id}"), cancellationToken),
            cancellationToken
        );
    }

    public virtual async Task<ServiceResult<bool>> DeleteRangeAsync(IReadOnlyCollection<ViewT> models, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(models, nameof(models));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpChangeService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"DeleteRangeAsync\"]");

        IEnumerable<DataTransferT> dtos = models.Adapt<IEnumerable<DataTransferT>>();

        return await SendAndParseNoContentAsync(
            () => _httpClient.PutAsJsonAsync(new Uri($"{_apiURL}/deleterange"), dtos, cancellationToken: cancellationToken),
            cancellationToken
        );
    }

    public virtual async Task<ServiceResult<T?>> UpdateAsync(ViewT model, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model, nameof(model));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpChangeService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"UpdateAsync\"]");

        DataTransferT dto = model.Adapt<DataTransferT>();

        return await SendAndParseAsync(
            () => _httpClient.PutAsJsonAsync(_apiURL, dto, cancellationToken: cancellationToken),
            (content, ct) => content.ReadFromJsonAsync<T>(cancellationToken: ct),
            cancellationToken
        );
    }

    public virtual async Task<ServiceResult<List<T>?>> UpdateRangeAsync(IReadOnlyCollection<ViewT> models, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(models, nameof(models));

        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpChangeService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"UpdateRangeAsync\"]");

        IEnumerable<DataTransferT> dtos = models.Adapt<IEnumerable<DataTransferT>>();

        return await SendAndParseAsync(
            () => _httpClient.PostAsJsonAsync(new Uri($"{_apiURL}/updaterange"), dtos, cancellationToken: cancellationToken),
            async (content, ct) => (await content.ReadFromJsonAsync<List<T>>(cancellationToken: ct).ConfigureAwait(false)) ?? [],
            cancellationToken
        );
    }
}

public class HttpChangeService<T, ViewT, DataTransferT>(Uri apiURL, HttpClient httpClient, ILogger<HttpChangeService<T, ViewT, DataTransferT>> logger)
    : HttpChangeService<T, ViewT, DataTransferT, KeyType>(apiURL, httpClient, logger), IHttpChangeService<T, ViewT, DataTransferT>
    where T : class, IEntity, IModel, new()
    where ViewT : class, IModel, new()
    where DataTransferT : class, IModel, new();


