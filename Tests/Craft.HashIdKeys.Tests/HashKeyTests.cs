using Craft.HashIdKeys;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.Domain.Tests.Keys;

public class HashKeyTests
{
    [Fact]
    public void AddHashKeys_ResolvesHashKeysInstance()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHashKeys();

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var hashKeys = serviceProvider.GetService<IHashKeys>();

        // Assert
        Assert.NotNull(hashKeys);
        Assert.IsType<HashKeys>(hashKeys);
    }
}
