namespace Craft.Data.Tests.Abstractions;

/// <summary>
/// Unit tests for IReadOnlyDbContext marker interface.
/// </summary>
public class ReadOnlyDbContextTests
{
    private class TestReadOnlyDbContext : IReadOnlyDbContext
    {
    }

    [Fact]
    public void IReadOnlyDbContext_ShouldBeInterface()
    {
        // Arrange & Act
        var type = typeof(IReadOnlyDbContext);

        // Assert
        Assert.True(type.IsInterface);
    }

    [Fact]
    public void IReadOnlyDbContext_ShouldBeMarkerInterface()
    {
        // Arrange & Act
        var type = typeof(IReadOnlyDbContext);
        var members = type.GetMembers();

        // Assert - Marker interface should have no members except inherited ones
        Assert.DoesNotContain(members, m => m.DeclaringType == type);
    }

    [Fact]
    public void TestReadOnlyDbContext_ShouldImplementIReadOnlyDbContext()
    {
        // Arrange & Act
        var context = new TestReadOnlyDbContext();

        // Assert
        Assert.IsType<IReadOnlyDbContext>(context, exactMatch: false);
    }

    [Fact]
    public void IReadOnlyDbContext_CanBeUsedAsTypeConstraint()
    {
        // This test verifies that IReadOnlyDbContext can be used in generic constraints

        // Arrange & Act
        static bool IsReadOnly<T>() where T : IReadOnlyDbContext => true;
        var result = IsReadOnly<TestReadOnlyDbContext>();

        // Assert
        Assert.True(result);
    }
}
