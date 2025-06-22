using Craft.CryptKey;
using Craft.Domain.HashIdentityKey;
using Microsoft.Extensions.DependencyInjection;

namespace Craft.Domain.Tests.Keys;

public class ServiceCollectionExtensionsTests
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
