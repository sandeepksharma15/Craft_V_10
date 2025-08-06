using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.TestHost;

namespace Craft.MultiTenant.Tests.StrategyTests;

public class RouteStrategyTests
{
    private static IWebHostBuilder GetTestHostBuilder(string identifier, string routePattern)
    {
        return new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddMultiTenant<Tenant>().WithRouteStrategy().WithInMemoryStore();
                services.AddMvc();
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseMultiTenant();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.Map(routePattern, async context =>
                    {
                        string? identifier = null;
                        try
                        {
                            var tenantContext = context.GetTenantContext<Tenant>();
                            identifier = tenantContext?.Tenant?.Identifier;
                        }
                        catch (InvalidOperationException)
                        {
                            // TenantContext is not available, leave identifier as null
                        }
                        await context.Response.WriteAsync(identifier ?? "");
                    });
                });

                var store = app.ApplicationServices.GetRequiredService<ITenantStore<Tenant>>();

                store.AddAsync(new Tenant { Id = 1, Identifier = identifier }).Wait();
            });
    }

    [Theory]
    [InlineData("/initech", "initech", "initech")]
    [InlineData("/", "initech", "")]
    public async Task ReturnExpectedIdentifier(string path, string identifier, string expected)
    {
        // Use the correct route parameter name for RouteStrategy (default is __TENANT__)
        IWebHostBuilder hostBuilder = GetTestHostBuilder(identifier, "{__TENANT__?}");

        using (var server = new TestServer(hostBuilder))
        {
            var client = server.CreateClient();
#pragma warning disable CA2234 // Pass system uri objects instead of strings
            var response = await client.GetStringAsync(path);
#pragma warning restore CA2234 // Pass system uri objects instead of strings
            Assert.Equal(expected, response);
        }
    }

    [Fact]
    public async Task ReturnNullIfNoRouteParamMatch()
    {
        IWebHostBuilder hostBuilder = GetTestHostBuilder("test_tenant", "{controller}");

        using (var server = new TestServer(hostBuilder))
        {
            var client = server.CreateClient();
#pragma warning disable CA2234 // Pass system uri objects instead of strings
            var response = await client.GetStringAsync("/test_tenant");
#pragma warning restore CA2234 // Pass system uri objects instead of strings
            Assert.Equal("", response);
        }
    }
}
