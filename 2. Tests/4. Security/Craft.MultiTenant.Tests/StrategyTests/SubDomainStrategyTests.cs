using Microsoft.AspNetCore.Http;

namespace Craft.MultiTenant.Tests.StrategyTests;

public class SubDomainStrategyTests
{
    [Fact]
    public async Task GetIdentifierAsync_WithSubdomain_ReturnsSubdomain()
    {
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("tenant1.example.com");
        
        var strategy = new SubDomainStrategy();
        var identifier = await strategy.GetIdentifierAsync(context);
        
        Assert.Equal("tenant1", identifier);
    }

    [Fact]
    public async Task GetIdentifierAsync_WithMultipleSubdomains_ReturnsFirstSubdomain()
    {
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("api.tenant1.example.com");
        
        var strategy = new SubDomainStrategy();
        var identifier = await strategy.GetIdentifierAsync(context);
        
        Assert.Equal("api", identifier);
    }

    [Fact]
    public async Task GetIdentifierAsync_WithNoSubdomain_ReturnsNull()
    {
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("example.com");
        
        var strategy = new SubDomainStrategy();
        var identifier = await strategy.GetIdentifierAsync(context);
        
        Assert.Null(identifier);
    }

    [Fact]
    public async Task GetIdentifierAsync_WithOnlyDomain_ReturnsNull()
    {
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("localhost");
        
        var strategy = new SubDomainStrategy();
        var identifier = await strategy.GetIdentifierAsync(context);
        
        Assert.Null(identifier);
    }

    [Theory]
    [InlineData("tenant1.example.com", "tenant1")]
    [InlineData("tenant2.mydomain.org", "tenant2")]
    [InlineData("test.sub.domain.com", "test")]
    [InlineData("mydomain.com", null)]
    public async Task GetIdentifierAsync_WithVariousHosts_ReturnsExpectedIdentifier(string host, string? expected)
    {
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString(host);
        
        var strategy = new SubDomainStrategy();
        var identifier = await strategy.GetIdentifierAsync(context);
        
        Assert.Equal(expected, identifier);
    }

    [Fact]
    public async Task GetIdentifierAsync_WithPort_IgnoresPort()
    {
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("tenant1.example.com", 8080);
        
        var strategy = new SubDomainStrategy();
        var identifier = await strategy.GetIdentifierAsync(context);
        
        Assert.Equal("tenant1", identifier);
    }

    [Fact]
    public async Task GetIdentifierAsync_WithNullContext_ThrowsArgumentNullException()
    {
        var strategy = new SubDomainStrategy();
        
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await strategy.GetIdentifierAsync(null!));
    }
}
