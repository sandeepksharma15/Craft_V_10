using Microsoft.AspNetCore.Http;

namespace Craft.MultiTenant.Tests.StrategyTests;

public class HeaderStrategyTests
{
    [Fact]
    public async Task GetIdentifierAsync_HeaderNotFound_ReturnsNull()
    {
        // Arrange
        var context = new DefaultHttpContext();

        var strategy = new HeaderStrategy("Tenant-Token");

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

        var strategy = new HeaderStrategy("Tenant-Token");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => strategy.GetIdentifierAsync(context));
    }

    [Fact]
    public async Task GetIdentifierAsync_ValidHttpContext_ReturnsIdentifierFromHeader()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["Tenant-Token"] = "myidentifier";

        var strategy = new HeaderStrategy("Tenant-Token");

        // Act
        var identifier = await strategy.GetIdentifierAsync(context);

        // Assert
        Assert.Equal("myidentifier", identifier);
    }
}
