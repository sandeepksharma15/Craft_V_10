using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Craft.Data.Tests.Services;

/// <summary>
/// Unit tests for DatabaseConnectionValidator hosted service.
/// </summary>
public class DatabaseConnectionValidatorTests
{
    [Fact]
    public async Task StartAsync_WithValidConnection_ShouldLogSuccess()
    {
        // Arrange
        var options = new DatabaseOptions
        {
            ConnectionString = "Server=localhost;Database=test;",
            DbProvider = DbProviderKeys.SqlServer
        };
        var optionsMock = new Mock<IOptions<DatabaseOptions>>();
        optionsMock.Setup(o => o.Value).Returns(options);

        var providerMock = new Mock<IDatabaseProvider>();
        providerMock.Setup(p => p.CanHandle(DbProviderKeys.SqlServer)).Returns(true);
        providerMock.Setup(p => p.ValidateConnection(It.IsAny<string>())).Returns(true);

        var loggerMock = new Mock<ILogger<DatabaseConnectionValidator>>();

        var validator = new DatabaseConnectionValidator(
            optionsMock.Object,
            new[] { providerMock.Object },
            loggerMock.Object);

        // Act
        await validator.StartAsync(CancellationToken.None);

        // Assert
        providerMock.Verify(p => p.ValidateConnection(options.ConnectionString), Times.Once);
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("validated successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task StartAsync_WithInvalidConnection_ShouldLogError()
    {
        // Arrange
        var options = new DatabaseOptions
        {
            ConnectionString = "Invalid connection string",
            DbProvider = DbProviderKeys.SqlServer
        };
        var optionsMock = new Mock<IOptions<DatabaseOptions>>();
        optionsMock.Setup(o => o.Value).Returns(options);

        var providerMock = new Mock<IDatabaseProvider>();
        providerMock.Setup(p => p.CanHandle(DbProviderKeys.SqlServer)).Returns(true);
        providerMock.Setup(p => p.ValidateConnection(It.IsAny<string>())).Returns(false);

        var loggerMock = new Mock<ILogger<DatabaseConnectionValidator>>();

        var validator = new DatabaseConnectionValidator(
            optionsMock.Object,
            new[] { providerMock.Object },
            loggerMock.Object);

        // Act
        await validator.StartAsync(CancellationToken.None);

        // Assert
        providerMock.Verify(p => p.ValidateConnection(options.ConnectionString), Times.Once);
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("FAILED")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task StartAsync_WithNoMatchingProvider_ShouldLogWarning()
    {
        // Arrange
        var options = new DatabaseOptions
        {
            ConnectionString = "Server=localhost;Database=test;",
            DbProvider = "unsupported"
        };
        var optionsMock = new Mock<IOptions<DatabaseOptions>>();
        optionsMock.Setup(o => o.Value).Returns(options);

        var providerMock = new Mock<IDatabaseProvider>();
        providerMock.Setup(p => p.CanHandle(It.IsAny<string>())).Returns(false);

        var loggerMock = new Mock<ILogger<DatabaseConnectionValidator>>();

        var validator = new DatabaseConnectionValidator(
            optionsMock.Object,
            new[] { providerMock.Object },
            loggerMock.Object);

        // Act
        await validator.StartAsync(CancellationToken.None);

        // Assert
        providerMock.Verify(p => p.ValidateConnection(It.IsAny<string>()), Times.Never);
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No database provider found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task StartAsync_WithException_ShouldLogError()
    {
        // Arrange
        var options = new DatabaseOptions
        {
            ConnectionString = "Server=localhost;Database=test;",
            DbProvider = DbProviderKeys.SqlServer
        };
        var optionsMock = new Mock<IOptions<DatabaseOptions>>();
        optionsMock.Setup(o => o.Value).Returns(options);

        var providerMock = new Mock<IDatabaseProvider>();
        providerMock.Setup(p => p.CanHandle(DbProviderKeys.SqlServer)).Returns(true);
        providerMock.Setup(p => p.ValidateConnection(It.IsAny<string>()))
            .Throws(new InvalidOperationException("Connection failed"));

        var loggerMock = new Mock<ILogger<DatabaseConnectionValidator>>();

        var validator = new DatabaseConnectionValidator(
            optionsMock.Object,
            new[] { providerMock.Object },
            loggerMock.Object);

        // Act
        await validator.StartAsync(CancellationToken.None);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Exception occurred")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task StopAsync_ShouldCompleteSuccessfully()
    {
        // Arrange
        var options = new DatabaseOptions
        {
            ConnectionString = "Server=localhost;Database=test;",
            DbProvider = DbProviderKeys.SqlServer
        };
        var optionsMock = new Mock<IOptions<DatabaseOptions>>();
        optionsMock.Setup(o => o.Value).Returns(options);

        var providerMock = new Mock<IDatabaseProvider>();
        var loggerMock = new Mock<ILogger<DatabaseConnectionValidator>>();

        var validator = new DatabaseConnectionValidator(
            optionsMock.Object,
            new[] { providerMock.Object },
            loggerMock.Object);

        // Act
        await validator.StopAsync(CancellationToken.None);

        // Assert - Should complete without errors
        Assert.True(true);
    }

    [Fact]
    public void Constructor_ShouldImplementIHostedService()
    {
        // Arrange
        var options = new DatabaseOptions();
        var optionsMock = new Mock<IOptions<DatabaseOptions>>();
        optionsMock.Setup(o => o.Value).Returns(options);

        var providerMock = new Mock<IDatabaseProvider>();
        var loggerMock = new Mock<ILogger<DatabaseConnectionValidator>>();

        // Act
        var validator = new DatabaseConnectionValidator(
            optionsMock.Object,
            new[] { providerMock.Object },
            loggerMock.Object);

        // Assert
        Assert.IsAssignableFrom<IHostedService>(validator);
    }
}
