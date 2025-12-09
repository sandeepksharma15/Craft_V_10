using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Craft.MultiTenant.Tests.StrategyTests;

public class SessionStrategyTests
{
    private static IHost CreateTestHost(string identifier, string sessionKey)
    {
        var hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.ConfigureServices(services =>
                {
                    services.AddDistributedMemoryCache();
                    services.AddSession(options =>
                    {
                        options.IdleTimeout = TimeSpan.FromSeconds(10);
                        options.Cookie.HttpOnly = true;
                        options.Cookie.IsEssential = true;
                    });

                    services.AddMultiTenant<Tenant>()
                        .WithSessionStrategy(sessionKey)
                        .WithInMemoryStore();
                    services.AddMvc();
                });
                webBuilder.Configure(app =>
                {
                    app.UseSession();
                    app.UseMultiTenant();
                    app.Run(async context =>
                    {
                        context.Session.SetString(sessionKey, identifier);
                        string? resolvedIdentifier = null;

                        try
                        {
                            var tenantContext = context.GetTenantContext<Tenant>();
                            resolvedIdentifier = tenantContext?.Tenant?.Identifier;
                        }
                        catch (InvalidOperationException)
                        {
                            // TenantContext is not available, leave resolvedIdentifier as null
                        }

                        await context.Response.WriteAsync(resolvedIdentifier ?? "");
                    });
                });
            });

        return hostBuilder.Start();
    }

    [Fact]
    public async Task ReturnNullIfNoSessionValue()
    {
        // Arrange
        using var host = CreateTestHost("test_tenant", "__TENANT__");
        var server = host.GetTestServer();
        var client = server.CreateClient();

        // Act
        var response = await client.GetStringAsync(new Uri("/test_tenant", UriKind.Relative));

        // Assert
        Assert.Equal("", response);
    }
}
