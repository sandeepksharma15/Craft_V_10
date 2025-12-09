using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Craft.MultiTenant.Tests.StrategyTests;

public class BasePathStrategyTests
{
    private static HttpContext CreateHttpContextMock(string path, string pathBase = "/")
    {
        var mock = new Mock<HttpContext>();
        mock.SetupProperty<PathString>(c => c.Request.Path, path);
        mock.SetupProperty<PathString>(c => c.Request.PathBase, pathBase);
        mock.SetupProperty(c => c.RequestServices);
        return mock.Object;
    }

    [Fact]
    public async Task AppendTenantToExistingBase()
    {
        var services = new ServiceCollection();

        services.AddOptions().AddMultiTenant<Tenant>().WithBasePathStrategy().WithInMemoryStore(options =>
        {
            options.Tenants.Add(new Tenant
            {
                Id = 1,
                Identifier = "tenant",
                Name = "tenant"
            });
        });
        services.Configure<BasePathStrategyOptions>(options => options.UpdateRequestPath = true);
        var serviceProvider = services.BuildServiceProvider();
        var httpContext = CreateHttpContextMock("/tenant/path", "/base");
        httpContext.RequestServices = serviceProvider;

        Assert.Equal("/base", httpContext.Request.PathBase);
        Assert.Equal("/tenant/path", httpContext.Request.Path);

        // will trigger OnTenantFound event...
        var resolver = await serviceProvider.GetRequiredService<ITenantResolver>().ResolveAsync(httpContext);

        Assert.Equal("/base/tenant", httpContext.Request.PathBase);
        Assert.Equal("/path", httpContext.Request.Path);
    }

    [Fact]
    public async Task GetIdentifierAsync_EmptyPath_ReturnsNull()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/";

        var strategy = new BasePathStrategy();

        // Act
        var identifier = await strategy.GetIdentifierAsync(context);

        // Assert
        Assert.Null(identifier);
    }

    [Fact]
    public async Task GetIdentifierAsync_NullHttpContext_ThrowsException()
    {
        // Arrange
        HttpContext context = null!;

        var strategy = new BasePathStrategy();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => strategy.GetIdentifierAsync(context));
    }

    [Fact]
    public async Task GetIdentifierAsync_ValidHttpContext_ReturnsIdentifier()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/v1/users";

        var strategy = new BasePathStrategy();

        // Act
        var identifier = await strategy.GetIdentifierAsync(context);

        // Assert
        Assert.Equal("api", identifier);
    }

    [Fact]
    public async Task NotRebaseAspNetCoreBasePathIfOptionFalse()
    {
        var services = new ServiceCollection();
        services.AddOptions().AddMultiTenant<Tenant>().WithBasePathStrategy().WithInMemoryStore(options =>
        {
            options.Tenants.Add(new Tenant
            {
                Id = 1,
                Identifier = "base",
                Name = "base tenant"
            });
        });
        services.Configure<BasePathStrategyOptions>(options => options.UpdateRequestPath = false);
        var serviceProvider = services.BuildServiceProvider();
        var httpContext = CreateHttpContextMock("/base/notbase");
        httpContext.RequestServices = serviceProvider;

        Assert.Equal("/", httpContext.Request.PathBase);
        Assert.Equal("/base/notbase", httpContext.Request.Path);

        // will trigger OnTenantFound event...
        var resolver = await serviceProvider.GetRequiredService<ITenantResolver>().ResolveAsync(httpContext);

        Assert.Equal("/", httpContext.Request.PathBase);
        Assert.Equal("/base/notbase", httpContext.Request.Path);
    }

    [Fact]
    public async Task RebaseAspNetCoreBasePathIfOptionTrue()
    {
        var services = new ServiceCollection();
        services.AddOptions().AddMultiTenant<Tenant>().WithBasePathStrategy().WithInMemoryStore(options =>
        {
            options.Tenants.Add(new Tenant
            {
                Id = 1,
                Identifier = "base",
                Name = "base tenant"
            });
        });

        services.Configure<BasePathStrategyOptions>(options => options.UpdateRequestPath = true);
        var serviceProvider = services.BuildServiceProvider();
        var httpContext = CreateHttpContextMock("/base/notbase");
        httpContext.RequestServices = serviceProvider;

        Assert.Equal("/", httpContext.Request.PathBase);
        Assert.Equal("/base/notbase", httpContext.Request.Path);

        // will trigger OnTenantFound event...
        var resolver = await serviceProvider.GetRequiredService<ITenantResolver>().ResolveAsync(httpContext);

        Assert.Equal("/base", httpContext.Request.PathBase);
        Assert.Equal("/notbase", httpContext.Request.Path);
    }

    [Theory]
    [InlineData("/test", "test")] // single path
    [InlineData("/Test", "Test")] // maintain case
    [InlineData("", null)] // no path
    [InlineData("/", null)] // just trailing slash
    [InlineData("/initech/ignore/ignore", "initech")] // multiple path segments
    public async Task ReturnExpectedIdentifier(string path, string? expected)
    {
        var httpContext = CreateHttpContextMock(path);
        var strategy = new BasePathStrategy();

        var identifier = await strategy.GetIdentifierAsync(httpContext);

        Assert.Equal(expected, identifier);
    }
}
