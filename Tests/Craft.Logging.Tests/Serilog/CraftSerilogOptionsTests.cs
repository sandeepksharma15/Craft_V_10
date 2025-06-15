using Craft.Logging.Serilog;

namespace Craft.Logging.Tests.Serilog;

public class CraftSerilogOptionsTests
{
    [Fact]
    public void EnricherNames_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var options = new CraftSerilogOptions();

        // Assert
        Assert.NotNull(options.EnricherNames);
        Assert.Equal("_TENANT_ID_", options.EnricherNames.TenantId);
        Assert.Equal("_USER_ID_", options.EnricherNames.UserId);
        Assert.Equal("_CLIENT_ID_", options.EnricherNames.ClientId);
    }

    [Fact]
    public void EnricherNames_ShouldAllowCustomValues()
    {
        // Arrange
        var options = new CraftSerilogOptions();

        // Act
        options.EnricherNames.TenantId = "CustomTenant";
        options.EnricherNames.UserId = "CustomUser";
        options.EnricherNames.ClientId = "CustomClient";

        // Assert
        Assert.Equal("CustomTenant", options.EnricherNames.TenantId);
        Assert.Equal("CustomUser", options.EnricherNames.UserId);
        Assert.Equal("CustomClient", options.EnricherNames.ClientId);
    }

    [Fact]
    public void EnricherNames_ShouldNotBeNullAfterInitialization()
    {
        // Arrange & Act
        var options = new CraftSerilogOptions();

        // Assert
        Assert.NotNull(options.EnricherNames);
    }

    [Fact]
    public void CanReplaceEnricherNamesInstance()
    {
        // Arrange
        var options = new CraftSerilogOptions();
        var newNames = new CraftSerilogOptions.SerilogEnricherNames
        {
            TenantId = "T",
            UserId = "U",
            ClientId = "C"
        };

        // Act
        options.EnricherNames = newNames;

        // Assert
        Assert.Equal("T", options.EnricherNames.TenantId);
        Assert.Equal("U", options.EnricherNames.UserId);
        Assert.Equal("C", options.EnricherNames.ClientId);
    }
}
