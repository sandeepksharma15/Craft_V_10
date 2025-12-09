using System.Net.Http.Json;

namespace Craft.Extensions.HttpResponse;

public static class HttpResponseExtensions
{
    /// <summary>
    /// Attempts to read error messages from an HTTP response.
    /// </summary>
    /// <remarks>If the response content cannot be deserialized into a list of strings, the method attempts to
    /// read the content as a plain string. If the content is empty or whitespace, a default error message is
    /// returned.</remarks>
    /// <param name="response">The HTTP response message from which to read errors.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of error messages extracted
    /// from the response. If no errors are found, the list contains a single message indicating the HTTP status code
    /// and reason phrase.</returns>
    public static async Task<List<string>> TryReadErrors(this HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var errors = await response
                .Content
                .ReadFromJsonAsync<List<string>>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            return errors ?? [$"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}"];
        }
        catch
        {
            var text = await response
                .Content
                .ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);

            return [string.IsNullOrWhiteSpace(text) ? $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}" : text];
        }
    }
}
