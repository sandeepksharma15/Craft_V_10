using Craft.Core.Common;
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

    private static async Task<HttpServiceResult<TResult>> HandleHttpOperationAsync<TResult>(
        Func<CancellationToken, Task<HttpResponseMessage>> sendRequest,
        Func<HttpResponseMessage, CancellationToken, Task<TResult>> parseResult,
        CancellationToken cancellationToken)
    {
        try
        {
            // Make the HTTP request
            var response = await sendRequest(cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                // Parse the result if the request was successful
                return new HttpServiceResult<TResult>
                {
                    Data = await parseResult(response, cancellationToken).ConfigureAwait(false),
                    IsSuccess = true,
                    StatusCode = (int)response.StatusCode
                };
            }
            else
            {
                // If the request failed, read the errors
                return new HttpServiceResult<TResult>
                {
                    IsSuccess = false,
                    Errors = await response.TryReadErrors(cancellationToken),
                    StatusCode = (int)response.StatusCode
                };
            }
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            return new HttpServiceResult<TResult>
            {
                IsSuccess = false,
                Errors = [ex.Message]
            };
        }
    }

    protected static Task<HttpServiceResult<TResult?>> GetAndParseAsync<TResult>(
        Func<CancellationToken, Task<HttpResponseMessage>> sendRequest,
        Func<HttpContent, CancellationToken, Task<TResult?>> parser,
        CancellationToken cancellationToken)
    {
        return HandleHttpOperationAsync(sendRequest, async (response, ct) => await parser(response.Content, ct), cancellationToken);
    }

    protected static Task<HttpServiceResult<TResult?>> SendAndParseAsync<TResult>(
        Func<Task<HttpResponseMessage>> sendRequest,
        Func<HttpContent, CancellationToken, Task<TResult?>> parser,
        CancellationToken cancellationToken)
    {
        // Defensive: Check for null delegates
        if (sendRequest == null)
        {
            var result = new HttpServiceResult<TResult?>
            {
                IsSuccess = false,
                Errors = ["sendRequest delegate is null."]
            };

            return Task.FromResult(result);
        }

        if (parser == null)
        {
            var result = new HttpServiceResult<TResult?>
            {
                IsSuccess = false,
                Errors = ["parser delegate is null."]
            };

            return Task.FromResult(result);
        }

        try
        {
            return HandleHttpOperationAsync(
                _ => sendRequest(),
                async (response, ct) =>
                {
                    if (response?.Content == null)
                    {
                        // Defensive: Null content
                        return default;
                    }

                    return await parser(response.Content, ct);
                },
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            var result = new HttpServiceResult<TResult?>
            {
                IsSuccess = false,
                Errors = [$"Exception in SendAndParseAsync: {ex.Message}"]
            };

            return Task.FromResult(result);
        }
    }

    protected static Task<HttpServiceResult<bool>> SendAndParseNoContentAsync(
        Func<Task<HttpResponseMessage>> sendRequest,
        CancellationToken cancellationToken)
    {
        return HandleHttpOperationAsync(_ => sendRequest(), (response, _) => Task.FromResult(response.IsSuccessStatusCode), cancellationToken);
    }

    /// <summary>
    /// Helper to flatten a paged result into a list result for GetAllAsync methods.
    /// </summary>
    public static async Task<HttpServiceResult<List<TItem>>> GetAllFromPagedAsync<TItem, TPaged>(
        Func<CancellationToken, Task<HttpServiceResult<TPaged>?>> getPaged,
        Func<TPaged, List<TItem>> extractItems,
        CancellationToken cancellationToken)
    {
        try
        {
            // Make The Call
            var pagedResult = await getPaged(cancellationToken).ConfigureAwait(false);

            return new HttpServiceResult<List<TItem>>
            {
                Data = pagedResult != null && pagedResult.Data != null ? extractItems(pagedResult.Data) : [],
                IsSuccess = pagedResult?.IsSuccess ?? false,
                Errors = pagedResult?.Errors,
                StatusCode = pagedResult?.StatusCode
            };
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            return new HttpServiceResult<List<TItem>>
            {
                IsSuccess = false,
                Errors = [ex.Message]
            };
        }
    }
}
