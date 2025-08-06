using Microsoft.AspNetCore.Http;
using Moq;

namespace Craft.MultiTenant.Tests.StrategyTests;

public class HostStrategyTests
{
    private static HttpContext CreateHttpContextMock(string host)
    {
        var mock = new Mock<HttpContext>();
        mock.Setup(c => c.Request.Host).Returns(new HostString(host));

        return mock.Object;
    }

    [Fact]
    public async Task GetIdentifierAsync_HostNotMatched_ReturnsNull()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("example.org");

        var strategy = new HostStrategy("*.example.com");

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

        var strategy = new HostStrategy("*.example.com");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => strategy.GetIdentifierAsync(context));
    }

    [Fact]
    public async Task GetIdentifierAsync_ValidHttpContext_ReturnsIdentifierFromHost()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("myidentifier.example.com");

        var strategy = new HostStrategy("__TENANT__.example.com");

        // Act
        var identifier = await strategy.GetIdentifierAsync(context);

        // Assert
        Assert.Equal("myidentifier", identifier);
    }

    [Theory]
    [InlineData("", "__TENANT__", null)] // no host
    [InlineData("initech", "__TENANT__", "initech")] // basic match
    [InlineData("Initech", "__TENANT__", "Initech")] // maintain case
    [InlineData("abc.com.test.", "__TENANT__.", null)] // invalid pattern
    [InlineData("abc", "__TENANT__.", null)] // invalid pattern
    [InlineData("abc", ".__TENANT__", null)] // invalid pattern
    [InlineData("abc", ".__TENANT__.", null)] // invalid pattern
    [InlineData("abc-cool.org", "__TENANT__-cool.org", "abc")] // mixed segment
    [InlineData("abc.com.test", "__TENANT__.*", "abc")] // first segment
    [InlineData("abc", "__TENANT__.*", "abc")] // first and only segment
    [InlineData("www.example.test", "?.__TENANT__.?", "example")] // domain
    [InlineData("www.example.test", "?.__TENANT__.*", "example")] // 2nd segment
    [InlineData("www.example", "?.__TENANT__.*", "example")] // 2nd segment
    [InlineData("www.example.r", "?.__TENANT__.?.*", "example")] // 2nd segment of 3+
    [InlineData("www.example.r.f", "?.__TENANT__.?.*", "example")] // 2nd segment of 3+
    [InlineData("example.ok.test", "*.__TENANT__.?.?", "example")] // 3rd last segment
    [InlineData("w.example.ok.test", "*.?.__TENANT__.?.?", "example")] // 3rd last of 4+ segments
    [InlineData("example.com", "__TENANT__", "example.com")] // match entire domain (2.1)
    public async Task ReturnExpectedIdentifier(string host, string template, string? expected)
    {
        var httpContext = CreateHttpContextMock(host);
        var strategy = new HostStrategy(template);

        var identifier = await strategy.GetIdentifierAsync(httpContext);

        Assert.Equal(expected, identifier);
    }

    [Theory]
    [InlineData("*.__TENANT__.*")]
    [InlineData("*a.__TENANT__")]
    [InlineData("a*a.__TENANT__")]
    [InlineData("a*.__TENANT__")]
    [InlineData("*-.__TENANT__")]
    [InlineData("-*-.__TENANT__")]
    [InlineData("-*.__TENANT__")]
    [InlineData("__TENANT__.-?")]
    [InlineData("__TENANT__.-?-")]
    [InlineData("__TENANT__.?-")]
    [InlineData("")]
    [InlineData("     ")]
    [InlineData(null)]
    public void ThrowIfInvalidTemplate(string? template)
    {
        Assert.Throws<MultiTenantException>(() => new HostStrategy(template!));
    }
}
