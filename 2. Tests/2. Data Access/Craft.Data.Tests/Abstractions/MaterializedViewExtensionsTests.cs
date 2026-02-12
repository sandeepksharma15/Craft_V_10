using Microsoft.EntityFrameworkCore;
using Moq;

namespace Craft.Data.Tests.Abstractions;

/// <summary>
/// Unit tests for IMaterializedView extension methods.
/// </summary>
public class MaterializedViewExtensionsTests
{
    private class TestMaterializedView : IMaterializedView
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public async Task RefreshMaterializedViewAsync_WithNullContext_ShouldThrowArgumentNullException()
    {
        // Arrange
        DbContext context = null!;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            context.RefreshMaterializedViewAsync<TestMaterializedView>());
    }

    [Fact]
    public async Task RefreshMaterializedViewConcurrentlyAsync_WithNullContext_ShouldThrowArgumentNullException()
    {
        // Arrange
        DbContext context = null!;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            context.RefreshMaterializedViewConcurrentlyAsync<TestMaterializedView>());
    }

    [Fact]
    public async Task RefreshMaterializedViewAsync_WithEntityNotInModel_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<DbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        using var context = new DbContext(options);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            context.RefreshMaterializedViewAsync<TestMaterializedView>());

        Assert.Contains("not part of the model", exception.Message);
    }

    [Fact]
    public void IMaterializedView_ShouldBeInterface()
    {
        // Arrange & Act
        var type = typeof(IMaterializedView);

        // Assert
        Assert.True(type.IsInterface);
    }

    [Fact]
    public void TestMaterializedView_ShouldImplementIMaterializedView()
    {
        // Arrange & Act
        var view = new TestMaterializedView();

        // Assert
        Assert.IsType<IMaterializedView>(view, exactMatch: false);
    }

    [Fact]
    public void MaterializedViewExtensions_ShouldBeStatic()
    {
        // Arrange & Act
        var type = typeof(MaterializedViewExtensions);

        // Assert
        Assert.True(type.IsAbstract && type.IsSealed); // Static classes are abstract and sealed
    }
}
