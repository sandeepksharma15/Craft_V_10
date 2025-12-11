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
        var result = new HttpServiceResult<TResult>();

        try
        {
            // Make the HTTP request
            var response = await sendRequest(cancellationToken).ConfigureAwait(false);

            result.StatusCode = (int)response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                // Parse the result if the request was successful
                result.Data = await parseResult(response, cancellationToken).ConfigureAwait(false);
                result.Success = true;
            }
            else
            {
                // If the request failed, read the errors
                result.Errors = await response.TryReadErrors(cancellationToken);
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
                Success = false,
                Errors = ["sendRequest delegate is null."]
            };

            return Task.FromResult(result);
        }

        if (parser == null)
        {
            var result = new HttpServiceResult<TResult?>
            {
                Success = false,
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
                Success = false,
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
        var result = new HttpServiceResult<List<TItem>>();

        try
        {
            // Make The Call
            var pagedResult = await getPaged(cancellationToken).ConfigureAwait(false);

            result.Data = pagedResult != null && pagedResult.Data != null ? extractItems(pagedResult.Data) : [];
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
}
