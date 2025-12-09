using System.Text.Json;
using Craft.Domain;

namespace Craft.MultiTenant.Stores;

public class RemoteApiStoreClient<T> where T : class, ITenant, IEntity, new()
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public RemoteApiStoreClient(IHttpClientFactory clientFactory)
    {
        ArgumentNullException.ThrowIfNull(clientFactory, nameof(clientFactory));

        _clientFactory = clientFactory;
        _jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    }

    public async Task<T?> GetByIdentifierAsync(string endpointTemplate, string identifier)
    {
        var client = _clientFactory.CreateClient(typeof(RemoteApiStoreClient<T>).FullName!);
        var uri = endpointTemplate.Replace(RemoteApiStore<T>.EndpointIdentifierToken, identifier);
        var response = await client.GetAsync(new Uri(uri));

        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<T>(json, _jsonSerializerOptions);
    }
}
