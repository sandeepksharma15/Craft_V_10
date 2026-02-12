using Craft.Core;
using Craft.Extensions.HttpResponse;

namespace Craft.HttpServices;

/// <summary>
/// Abstract base class for HTTP service helpers.
/// </summary>
public abstract class HttpServiceBase
{
    protected readonly Uri _apiURL;
    protected readonly HttpClient _httpClient;

    protected HttpServiceBase(Uri apiURL, HttpClient httpClient)
    {
        _apiURL = apiURL;
        _httpClient = httpClient;
    }

    private static async Task<ServiceResult<TResult>> HandleHttpOperationAsync<TResult>(
        Func<CancellationToken, Task<HttpResponseMessage>> sendRequest,
        Func<HttpResponseMessage, CancellationToken, Task<TResult>> parseResult,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await sendRequest(cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var data = await parseResult(response, cancellationToken).ConfigureAwait(false);
                return ServiceResult<TResult>.Success(data, (int)response.StatusCode);
            }

            var errors = await response.TryReadErrors(cancellationToken);
            return ServiceResult<TResult>.Failure(errors ?? [], statusCode: (int)response.StatusCode);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            return ServiceResult<TResult>.Failure([ex.Message]);
        }
    }

    protected static Task<ServiceResult<TResult?>> GetAndParseAsync<TResult>(
        Func<CancellationToken, Task<HttpResponseMessage>> sendRequest,
        Func<HttpContent, CancellationToken, Task<TResult?>> parser,
        CancellationToken cancellationToken)
    {
        return HandleHttpOperationAsync<TResult?>(
            sendRequest,
            async (response, ct) => await parser(response.Content, ct),
            cancellationToken);
    }

    protected static async Task<ServiceResult<TResult?>> SendAndParseAsync<TResult>(
        Func<CancellationToken, Task<HttpResponseMessage>> sendRequest,
        Func<HttpContent, CancellationToken, Task<TResult?>> parser,
        CancellationToken cancellationToken)
    {
        if (sendRequest == null)
            return ServiceResult<TResult?>.Failure("sendRequest delegate is null.");

        if (parser == null)
            return ServiceResult<TResult?>.Failure("parser delegate is null.");

        try
        {
            return await HandleHttpOperationAsync<TResult?>(
                sendRequest,
                async (response, ct) =>
                {
                    if (response?.Content == null)
                        return default;

                    return await parser(response.Content, ct);
                },
                cancellationToken);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            return ServiceResult<TResult?>.Failure($"Exception in SendAndParseAsync: {ex.Message}");
        }
    }

    protected static Task<ServiceResult<bool>> SendAndParseNoContentAsync(
        Func<CancellationToken, Task<HttpResponseMessage>> sendRequest,
        CancellationToken cancellationToken)
    {
        return HandleHttpOperationAsync(
            sendRequest,
            (response, _) => Task.FromResult(response.IsSuccessStatusCode),
            cancellationToken);
    }

    /// <summary>
    /// Helper to flatten a paged result into a list result for GetAllAsync methods.
    /// </summary>
    public static async Task<ServiceResult<List<TItem>>> GetAllFromPagedAsync<TItem, TPaged>(
        Func<CancellationToken, Task<ServiceResult<TPaged>?>> getPaged,
        Func<TPaged, List<TItem>> extractItems,
        CancellationToken cancellationToken)
    {
        try
        {
            var pagedResult = await getPaged(cancellationToken).ConfigureAwait(false);

            if (pagedResult == null)
                return ServiceResult<List<TItem>>.Failure("No result returned");

            if (!pagedResult.IsSuccess)
                return ServiceResult<List<TItem>>.Failure(pagedResult.Errors ?? [], statusCode: pagedResult.StatusCode);

            var items = pagedResult.Value is not null ? extractItems(pagedResult.Value) : [];
            return ServiceResult<List<TItem>>.Success(items);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            return ServiceResult<List<TItem>>.Failure(ex.Message);
        }
    }
}

