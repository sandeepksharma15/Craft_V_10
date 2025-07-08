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
        return HandleHttpOperationAsync(_ => sendRequest(), async (response, ct) => await parser(response.Content, ct), cancellationToken);
    }

    protected static Task<HttpServiceResult<bool>> SendAndParseNoContentAsync(
        Func<Task<HttpResponseMessage>> sendRequest,
        CancellationToken cancellationToken)
    {
        return HandleHttpOperationAsync(_ => sendRequest(), (response, _) => Task.FromResult(response.IsSuccessStatusCode), cancellationToken);
    }
}
