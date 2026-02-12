namespace Craft.Core.Tests.DependencyInjection;

public class DependencyInjectionMarkerTests
{
    #region IScopedDependency Tests

    [Fact]
    public void IScopedDependency_IsInterface()
    {
        // Arrange
        var type = typeof(IScopedDependency);

        // Assert
        Assert.True(type.IsInterface);
    }

    [Fact]
    public void IScopedDependency_HasNoMembers()
    {
        // Arrange
        var type = typeof(IScopedDependency);
        var members = type.GetMembers();

        // Assert - Only inherited members from object should exist
        Assert.Empty(members);
    }

    [Fact]
    public void IScopedDependency_CanBeImplemented()
    {
        // Arrange & Act
        var service = new TestScopedService();

        // Assert
        Assert.IsType<IScopedDependency>(service, exactMatch: false);
    }

    [Fact]
    public void IScopedDependency_IsInCraftCoreNamespace()
    {
        // Arrange
        var type = typeof(IScopedDependency);

        // Assert
        Assert.Equal("Craft.Core", type.Namespace);
    }

    #endregion

    #region ISingletonDependency Tests

    [Fact]
    public void ISingletonDependency_IsInterface()
    {
        // Arrange
        var type = typeof(ISingletonDependency);

        // Assert
        Assert.True(type.IsInterface);
    }

    [Fact]
    public void ISingletonDependency_HasNoMembers()
    {
        // Arrange
        var type = typeof(ISingletonDependency);
        var members = type.GetMembers();

        // Assert
        Assert.Empty(members);
    }

    [Fact]
    public void ISingletonDependency_CanBeImplemented()
    {
        // Arrange & Act
        var service = new TestSingletonService();

        // Assert
        Assert.IsType<ISingletonDependency>(service, exactMatch: false);
    }

    [Fact]
    public void ISingletonDependency_IsInCraftCoreNamespace()
    {
        // Arrange
        var type = typeof(ISingletonDependency);

        // Assert
        Assert.Equal("Craft.Core", type.Namespace);
    }

    #endregion

    #region ITransientDependency Tests

    [Fact]
    public void ITransientDependency_IsInterface()
    {
        // Arrange
        var type = typeof(ITransientDependency);

        // Assert
        Assert.True(type.IsInterface);
    }

    [Fact]
    public void ITransientDependency_HasNoMembers()
    {
        // Arrange
        var type = typeof(ITransientDependency);
        var members = type.GetMembers();

        // Assert
        Assert.Empty(members);
    }

    [Fact]
    public void ITransientDependency_CanBeImplemented()
    {
        // Arrange & Act
        var service = new TestTransientService();

        // Assert
        Assert.IsType<ITransientDependency>(service, exactMatch: false);
    }

    [Fact]
    public void ITransientDependency_IsInCraftCoreNamespace()
    {
        // Arrange
        var type = typeof(ITransientDependency);

        // Assert
        Assert.Equal("Craft.Core", type.Namespace);
    }

    #endregion

    #region IService Tests

    [Fact]
    public void IService_IsInterface()
    {
        // Arrange
        var type = typeof(IService);

        // Assert
        Assert.True(type.IsInterface);
    }

    [Fact]
    public void IService_HasNoMembers()
    {
        // Arrange
        var type = typeof(IService);
        var members = type.GetMembers();

        // Assert
        Assert.Empty(members);
    }

    [Fact]
    public void IService_CanBeImplementedWithLifetimeMarker()
    {
        // Arrange & Act
        var service = new TestServiceWithLifetime();

        // Assert
        Assert.IsType<IService>(service, exactMatch: false);
        Assert.IsType<IScopedDependency>(service, exactMatch: false);
    }

    #endregion

    #region Marker Interface Independence Tests

    [Fact]
    public void MarkerInterfaces_AreNotRelated()
    {
        // Arrange
        var scopedType = typeof(IScopedDependency);
        var singletonType = typeof(ISingletonDependency);
        var transientType = typeof(ITransientDependency);

        // Assert
        Assert.False(scopedType.IsAssignableFrom(singletonType));
        Assert.False(scopedType.IsAssignableFrom(transientType));
        Assert.False(singletonType.IsAssignableFrom(transientType));
    }

    [Fact]
    public void Service_CanImplementMultipleMarkers_ButShouldNotInPractice()
    {
        // This test documents that it's technically possible to implement multiple markers,
        // but this should be avoided in practice as it creates ambiguity

        // Arrange & Act
        var badService = new TestMultipleMarkerService();

        // Assert - All markers are satisfied (but this is bad practice)
        Assert.IsType<IScopedDependency>(badService, exactMatch: false);
        Assert.IsType<ISingletonDependency>(badService, exactMatch: false);
    }

    #endregion

    #region Test Service Classes

    private class TestScopedService : IScopedDependency
    {
    }

    private class TestSingletonService : ISingletonDependency
    {
    }

    private class TestTransientService : ITransientDependency
    {
    }

    private class TestServiceWithLifetime : IService, IScopedDependency
    {
    }

    // This class demonstrates bad practice - should only implement one lifetime marker
    private class TestMultipleMarkerService : IScopedDependency, ISingletonDependency
    {
    }

    #endregion
}
