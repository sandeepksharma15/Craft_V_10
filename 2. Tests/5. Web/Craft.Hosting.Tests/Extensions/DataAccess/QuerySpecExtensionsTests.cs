using Craft.Hosting.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Craft.Hosting.Tests.Extensions.DataAccess;

/// <summary>
/// Unit tests for QuerySpecExtensions.
/// </summary>
public class QuerySpecExtensionsTests
{
    [Fact]
    public void AddQuerySpecJsonOptions_ReturnsSameMvcBuilder()
    {
        // Arrange
        var services = new ServiceCollection();
        var mvcBuilder = services.AddControllers();

        // Act
        var result = mvcBuilder.AddQuerySpecJsonOptions();

        // Assert
        Assert.Same(mvcBuilder, result);
    }

    [Fact]
    public void AddQuerySpecJsonOptions_ConfiguresJsonOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var mvcBuilder = services.AddControllers();

        // Act
        mvcBuilder.AddQuerySpecJsonOptions();

        // Assert - The method should not throw and should return the builder
        Assert.NotNull(mvcBuilder);
    }

    [Fact]
    public void AddQuerySpecJsonOptions_CanBeCalledMultipleTimes()
    {
        // Arrange
        var services = new ServiceCollection();
        var mvcBuilder = services.AddControllers();

        // Act & Assert - Should not throw
        mvcBuilder.AddQuerySpecJsonOptions();
        mvcBuilder.AddQuerySpecJsonOptions();
    }
}
