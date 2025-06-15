using Microsoft.AspNetCore.Builder;
using Craft.Logging.Extensions;

namespace Craft.Logging.Tests.Extensions;

public class LoggingExtensionsTests
{
    [Fact]
    public void AddLogging_ReturnsSameBuilderInstance()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act
        var result = builder.AddLogging();

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void AddLogging_DoesNotThrow_ForValidBuilder()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act & Assert
        var exception = Record.Exception(() => builder.AddLogging());
        Assert.Null(exception);
    }

    [Fact]
    public void AddLogging_ThrowsArgumentNullException_WhenBuilderIsNull()
    {
        // Arrange
        WebApplicationBuilder? builder = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => LoggingExtensions.AddLogging(builder!));
    }
}
