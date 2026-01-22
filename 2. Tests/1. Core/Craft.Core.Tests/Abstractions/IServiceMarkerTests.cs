using Craft.Core;
using Xunit;

namespace Craft.Core.Tests.Abstractions;

public class IServiceMarkerTests
{
    private class DummyService : IService { }

    [Fact]
    public void DummyService_Implements_IService_MarkerInterface()
    {
        // Arrange
        var service = new DummyService();

        // Act & Assert
        Assert.IsType<IService>(service, exactMatch: false);
    }
}
