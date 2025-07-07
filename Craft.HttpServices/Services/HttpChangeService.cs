using System.Net.Http.Json;
using Craft.Domain;
using Mapster;
using Microsoft.Extensions.Logging;
using Craft.Core.Common;

namespace Craft.HttpServices.Services;

public class HttpChangeService<T, ViewT, DataTransferT, TKey>(Uri apiURL, HttpClient httpClient, ILogger<HttpChangeService<T, ViewT, DataTransferT, TKey>> logger)
    : HttpReadService<T, TKey>(apiURL, httpClient, logger), IHttpChangeService<T, ViewT, DataTransferT, TKey>
        where T : class, IEntity<TKey>, IModel<TKey>, new()
        where ViewT : class, IModel<TKey>, new()
        where DataTransferT : class, IModel<TKey>, new()
{
    public virtual async Task<HttpServiceResult<T>> AddAsync(ViewT model, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpChangeService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"AddAsync\"]");

        DataTransferT dto = model.Adapt<DataTransferT>();
        var response = await _httpClient.PostAsJsonAsync(_apiURL, dto, cancellationToken: cancellationToken);
        var result = new HttpServiceResult<T> { StatusCode = (int)response.StatusCode };
        if (response.IsSuccessStatusCode)
        {
            result.Data = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
            result.Success = true;
        }
        else
        {
            result.Errors = await TryReadErrors(response, cancellationToken);
            result.Success = false;
        }
        return result;
    }

    public virtual async Task<HttpServiceResult<List<T>>> AddRangeAsync(IReadOnlyCollection<ViewT> models, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpChangeService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"AddRangeAsync\"]");

        IEnumerable<DataTransferT> dtos = models.Adapt<IEnumerable<DataTransferT>>();
        var response = await _httpClient.PostAsJsonAsync(new Uri($"{_apiURL}/addrange"), dtos, cancellationToken: cancellationToken);
        var result = new HttpServiceResult<List<T>> { StatusCode = (int)response.StatusCode };
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<List<T>>(cancellationToken: cancellationToken);
            result.Data = data ?? [];
            result.Success = true;
        }
        else
        {
            result.Errors = await TryReadErrors(response, cancellationToken);
            result.Success = false;
        }
        return result;
    }

    public virtual async Task<HttpServiceResult<List<T>>> AddRangeAsync(IEnumerable<ViewT> models, CancellationToken cancellationToken = default)
        => await AddRangeAsync(models is IReadOnlyCollection<ViewT> c ? c : models.ToList(), cancellationToken);

    public virtual async Task<HttpServiceResult<bool>> DeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpChangeService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"DeleteAsync\"]");

        var response = await _httpClient.DeleteAsync(new Uri($"{_apiURL}/{id}"), cancellationToken);
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

    public virtual async Task<HttpServiceResult<bool>> DeleteRangeAsync(IReadOnlyCollection<ViewT> models, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpChangeService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"DeleteRangeAsync\"]");

        IEnumerable<DataTransferT> dtos = models.Adapt<IEnumerable<DataTransferT>>();
        var response = await _httpClient.PutAsJsonAsync(new Uri($"{_apiURL}/RemoveRange"), dtos, cancellationToken: cancellationToken);
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

    public virtual async Task<HttpServiceResult<bool>> DeleteRangeAsync(IEnumerable<ViewT> models, CancellationToken cancellationToken = default)
        => await DeleteRangeAsync(models is IReadOnlyCollection<ViewT> c ? c : models.ToList(), cancellationToken);

    public virtual async Task<HttpServiceResult<T>> UpdateAsync(ViewT model, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpChangeService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"UpdateAsync\"]");

        DataTransferT dto = model.Adapt<DataTransferT>();
        var response = await _httpClient.PutAsJsonAsync(_apiURL, dto, cancellationToken: cancellationToken);
        var result = new HttpServiceResult<T> { StatusCode = (int)response.StatusCode };
        if (response.IsSuccessStatusCode)
        {
            result.Data = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
            result.Success = true;
        }
        else
        {
            result.Errors = await TryReadErrors(response, cancellationToken);
            result.Success = false;
        }
        return result;
    }

    public virtual async Task<HttpServiceResult<List<T>>> UpdateRangeAsync(IReadOnlyCollection<ViewT> models, CancellationToken cancellationToken = default)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
            _logger.LogDebug($"[HttpChangeService] Type: [\"{typeof(T).GetClassName()}\"] Method: [\"UpdateRangeAsync\"]");

        IEnumerable<DataTransferT> dtos = models.Adapt<IEnumerable<DataTransferT>>();
        var response = await _httpClient.PutAsJsonAsync(new Uri($"{_apiURL}/UpdateRange"), dtos, cancellationToken: cancellationToken);
        var result = new HttpServiceResult<List<T>> { StatusCode = (int)response.StatusCode };
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<List<T>>(cancellationToken: cancellationToken);
            result.Data = data ?? [];
            result.Success = true;
        }
        else
        {
            result.Errors = await TryReadErrors(response, cancellationToken);
            result.Success = false;
        }
        return result;
    }

    public virtual async Task<HttpServiceResult<List<T>>> UpdateRangeAsync(IEnumerable<ViewT> models, CancellationToken cancellationToken = default)
        => await UpdateRangeAsync(models is IReadOnlyCollection<ViewT> c ? c : models.ToList(), cancellationToken);

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

public class HttpChangeService<T, ViewT, DataTransferT>(Uri apiURL, HttpClient httpClient, ILogger<HttpChangeService<T, ViewT, DataTransferT>> logger)
    : HttpChangeService<T, ViewT, DataTransferT, KeyType>(apiURL, httpClient, logger), IHttpChangeService<T, ViewT, DataTransferT>
        where T : class, IEntity, IModel, new()
        where ViewT : class, IModel, new()
        where DataTransferT : class, IModel, new();
