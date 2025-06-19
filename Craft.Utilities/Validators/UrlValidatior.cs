namespace Craft.Utilities.Validators;

public static class UrlValidations
{
    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(5)
    };

    // <summary>
    /// Validates if the given string is a well-formed URL.
    /// </summary>
    /// <param name="url">The URL to validate.</param>
    /// <returns>True if the URL is valid; otherwise, false.</returns>
    public static bool IsValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;

        return Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }

    /// <summary>
    /// Checks if a specified web address is reachable by sending a HEAD request. Returns a boolean indicating the
    /// success of the request. The Url MUST not require Auithentication
    /// </summary>
    /// <param name="url">The web address to check for reachability.</param>
    /// <returns>A boolean value indicating whether the web address is reachable.</returns>
    public static async Task<bool> IsUrlReachableAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Head, url);
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if a given URL is accessible by sending a HEAD request, and falls back to a GET request if necessary.
    /// The url MAY or MAY NOT require Authentication
    /// </summary>
    /// <param name="url">The web address to verify for existence and accessibility.</param>
    /// <returns>Returns true if the URL is reachable or indicates a valid response; otherwise, returns false.</returns>
    public static async Task<bool> IsUrlExistingAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Head, url);
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            // Fallback to GET if HEAD is not allowed
            if (response.StatusCode == System.Net.HttpStatusCode.MethodNotAllowed)
            {
                request = new HttpRequestMessage(HttpMethod.Get, url);
                response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            }

            return response.IsSuccessStatusCode ||
                   response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                   response.StatusCode == System.Net.HttpStatusCode.Forbidden;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Removes invalid URLs from the provided list.
    /// </summary>
    /// <param name="urls">The list of URLs to validate.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A list of valid URLs.</returns>
    public static async Task<List<string?>> RemoveInvalidUrls(List<string?>? urls, CancellationToken cancellationToken = default)
    {
        if (urls == null) return [];

        var validationTasks = urls.Select(async url =>
        {
            var isValid = IsValidUrl(url!) && await IsUrlReachableAsync(url!, cancellationToken);
            return (url, isValid);
        });

        var results = await Task.WhenAll(validationTasks);

        return [.. results
            .Where(result => result.isValid)
            .Select(result => result.url)];
    }
}
