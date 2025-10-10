using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Craft.MultiTenant.Tests.StrategyTests;

public class RouteStrategyTests
{
    private static IHost CreateTestHost(string identifier, string routePattern)
    {
        var hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.ConfigureServices(services =>
                {
                    services.AddMultiTenant<Tenant>().WithRouteStrategy().WithInMemoryStore();
                    services.AddMvc();
                });
                webBuilder.Configure(app =>
                {
                    app.UseRouting();
                    app.UseMultiTenant();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.Map(routePattern, async context =>
                        {
                            string? tenantIdentifier = null;

                            try
                            {
                                var tenantContext = context.GetTenantContext<Tenant>();
                                tenantIdentifier = tenantContext?.Tenant?.Identifier;
                            }
                            catch (InvalidOperationException)
                            {
                                // TenantContext is not available, leave identifier as null
                            }

                            await context.Response.WriteAsync(tenantIdentifier ?? "");
                        });
                    });

                    var store = app.ApplicationServices.GetRequiredService<ITenantStore<Tenant>>();
                    store.AddAsync(new Tenant { Id = 1, Identifier = identifier }).Wait();
                });
            });

        return hostBuilder.Start();
    }

    [Theory]
    [InlineData("/initech", "initech", "initech")]
    [InlineData("/", "initech", "")]
    public async Task ReturnExpectedIdentifier(string path, string identifier, string expected)
    {
        // Arrange: Use the correct route parameter name for RouteStrategy (default is __TENANT__)
        using var host = CreateTestHost(identifier, "{__TENANT__?}");
        var server = host.GetTestServer();
        var client = server.CreateClient();

        // Act
        var response = await client.GetStringAsync(new Uri(path, UriKind.Relative));

        // Assert
        Assert.Equal(expected, response);
    }

    [Fact]
    public async Task ReturnNullIfNoRouteParamMatch()
    {
        // Arrange
        using var host = CreateTestHost("test_tenant", "{controller}");
        var server = host.GetTestServer();
        var client = server.CreateClient();

        // Act
        var response = await client.GetStringAsync(new Uri("/test_tenant", UriKind.Relative));

        // Assert
        Assert.Equal("", response);
    }
}
