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

    protected static async Task<HttpServiceResult<TResult>> GetAndParseAsync<TResult>(
        Func<CancellationToken, Task<HttpResponseMessage>> sendRequest,
        Func<HttpContent, CancellationToken, Task<TResult?>> parser,
        CancellationToken cancellationToken)
    {
        var result = new HttpServiceResult<TResult>();

        try
        {
            var response = await sendRequest(cancellationToken).ConfigureAwait(false);

            result.StatusCode = (int)response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                result.Data = await parser(response.Content, cancellationToken).ConfigureAwait(false);
                result.Success = true;
            }
            else
            {
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

    protected static async Task<HttpServiceResult<TResult>> SendAndParseAsync<TResult>(
        Func<Task<HttpResponseMessage>> sendRequest,
        Func<HttpContent, CancellationToken, Task<TResult?>> parser,
        CancellationToken cancellationToken)
    {
        var result = new HttpServiceResult<TResult>();

        try
        {
            var response = await sendRequest().ConfigureAwait(false);

            result.StatusCode = (int)response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                result.Data = await parser(response.Content, cancellationToken).ConfigureAwait(false);
                result.Success = true;
            }
            else
            {
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

    protected static async Task<HttpServiceResult<bool>> SendAndParseNoContentAsync(
        Func<Task<HttpResponseMessage>> sendRequest,
        CancellationToken cancellationToken)
    {
        var result = new HttpServiceResult<bool>();
        try
        {
            var response = await sendRequest().ConfigureAwait(false);

            result.StatusCode = (int)response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                result.Data = true;
                result.Success = true;
            }
            else
            {
                result.Data = false;
                result.Errors = await response.TryReadErrors(cancellationToken);
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
}
