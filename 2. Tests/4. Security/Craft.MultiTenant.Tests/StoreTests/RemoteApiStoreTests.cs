using System.Net;
using System.Reflection;
using System.Text.Json;
using Craft.MultiTenant.Stores;
using Moq;

namespace Craft.MultiTenant.Tests.StoreTests;

public class RemoteApiStoreTests : TenantStoreTestBase
{
    protected override ITenantStore<Tenant> CreateTestStore()
    {
        var client = new HttpClient(new TestHandler());
        var clientFactory = new Mock<IHttpClientFactory>();

        clientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

        var typedClient = new RemoteApiStoreClient<Tenant>(clientFactory.Object);

        return new RemoteApiStore<Tenant>(typedClient, "http://example.com");
    }

    protected override ITenantStore<Tenant> PopulateTestStore(ITenantStore<Tenant> store)
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void AppendTenantToTemplateIfMissing()
    {
        var clientFactory = new Mock<IHttpClientFactory>();
        var client = new RemoteApiStoreClient<Tenant>(clientFactory.Object);
        var store = new RemoteApiStore<Tenant>(client, "http://example.com/");

        var field = store.GetType().GetField("_endpointTemplate", BindingFlags.NonPublic | BindingFlags.Instance);
        var endpointTemplate = field?.GetValue(store);

        Assert.Equal($"http://example.com/{RemoteApiStore<Tenant>.EndpointIdentifierToken}", endpointTemplate);
    }

    [Fact]
    public void AppendTenantWithSlashToTemplateIfMissing()
    {
        var clientFactory = new Mock<IHttpClientFactory>();
        var client = new RemoteApiStoreClient<Tenant>(clientFactory.Object);
        var store = new RemoteApiStore<Tenant>(client, "http://example.com");

        var field = store.GetType().GetField("_endpointTemplate", BindingFlags.NonPublic | BindingFlags.Instance);
        var endpointTemplate = field?.GetValue(store);

        Assert.Equal($"http://example.com/{RemoteApiStore<Tenant>.EndpointIdentifierToken}", endpointTemplate);
    }

    [Fact]
    public override void GetTenantFromStoreByIdentifier()
    {
        base.GetTenantFromStoreByIdentifier();
    }

    [Fact]
    public override void ReturnNullWhenGettingByIdentifierIfTenantNotFound()
    {
        base.ReturnNullWhenGettingByIdentifierIfTenantNotFound();
    }

    [Theory]
    [InlineData("null")]
    [InlineData("")]
    [InlineData("invalidUri")]
    [InlineData("file://nothttp")]
    public void ThrowIfEndpointTemplateIsNotWellFormed(string uri)
    {
        var clientFactory = new Mock<IHttpClientFactory>();
        var client = new RemoteApiStoreClient<Tenant>(clientFactory.Object);
        Assert.Throws<ArgumentException>(() => new RemoteApiStore<Tenant>(client, uri));
    }

    [Fact]
    public void ThrowIfHttpClientFactoryIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new RemoteApiStoreClient<Tenant>(null!));
    }

    [Fact]
    public void ThrowIfTypedClientParamIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new RemoteApiStore<Tenant>(null!, "http://example.com"));
    }

    public class TestHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var result = new HttpResponseMessage();

            var numSegments = request.RequestUri!.Segments.Length;

            if (string.Equals(request.RequestUri.Segments[numSegments - 1], "initech", StringComparison.OrdinalIgnoreCase))
            {
                var tenantInfo = new Tenant { Id = 1, Identifier = "initech" };
                var json = JsonSerializer.Serialize(tenantInfo);
                result.StatusCode = HttpStatusCode.OK;
                result.Content = new StringContent(json);
            }
            else
                result.StatusCode = HttpStatusCode.NotFound;

            return Task.FromResult(result);
        }
    }
}
