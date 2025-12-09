using Microsoft.AspNetCore.Http;

namespace Craft.MultiTenant.Tests.StrategyTests;

public class DelegateStrategyTests
{
    [Fact]
    public async Task BeAbleToReturnNull()
    {
        var strategy = new DelegateStrategy(async _ => await Task.FromResult<string>(null!));
        var result = await strategy.GetIdentifierAsync(new DefaultHttpContext());

        Assert.Null(result);
    }

    [Fact]
    public async Task CallDelegate()
    {
        int i = 0;
        var strategy = new DelegateStrategy(_ => Task.FromResult(i++.ToString())!);
        await strategy.GetIdentifierAsync(new DefaultHttpContext());

        Assert.Equal(1, i);
    }

    [Fact]
    public async Task GetIdentifierAsync_NullHttpContext_ThrowsException()
    {
        // Arrange
        HttpContext context = null!;

        var strategy = new DelegateStrategy(_ => Task.FromResult("myidentifier")!);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => strategy.GetIdentifierAsync(context));
    }

    [Fact]
    public async Task GetIdentifierAsync_ValidHttpContext_ReturnsIdentifier()
    {
        // Arrange
        var context = new DefaultHttpContext();

        var strategy = new DelegateStrategy(_ => Task.FromResult("myidentifier")!);

        // Act
        var identifier = await strategy.GetIdentifierAsync(context);

        // Assert
        Assert.Equal("myidentifier", identifier);
    }

    [Theory]
    [InlineData("initech-id")]
    [InlineData("")]
    [InlineData(null)]
    public async Task ReturnDelegateResult(string? identifier)
    {
        var strategy = new DelegateStrategy(async _ => await Task.FromResult(identifier));
        var result = await strategy.GetIdentifierAsync(new DefaultHttpContext());

        Assert.Equal(identifier, result);
    }
}
