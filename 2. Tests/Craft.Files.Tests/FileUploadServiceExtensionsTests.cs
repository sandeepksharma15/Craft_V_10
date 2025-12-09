using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Craft.Files.Tests;

public class FileUploadServiceExtensionsTests
{
    [Fact]
    public void AddFileUploadServices_WithConfiguration_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["FileUploadOptions:Provider"] = "local",
                ["FileUploadOptions:BasePath"] = "Files",
                ["FileUploadOptions:DefaultMaxSizeMB"] = "20"
            })
            .Build();

        // Act
        services.AddFileUploadServices(configuration);
        var provider = services.BuildServiceProvider();

        // Assert
        var fileUploadService = provider.GetService<IFileUploadService>();
        var storageProvider = provider.GetService<IFileStorageProvider>();
        var options = provider.GetService<IOptions<FileUploadOptions>>();

        Assert.NotNull(fileUploadService);
        Assert.NotNull(storageProvider);
        Assert.NotNull(options);
        Assert.Equal("local", options.Value.Provider);
        Assert.Equal(20, options.Value.DefaultMaxSizeMB);
    }

    [Fact]
    public void AddFileUploadServices_WithAction_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddFileUploadServices(options =>
        {
            options.Provider = "local";
            options.BasePath = "CustomPath";
            options.DefaultMaxSizeMB = 50;
        });

        var provider = services.BuildServiceProvider();

        // Assert
        var options = provider.GetService<IOptions<FileUploadOptions>>();
        Assert.NotNull(options);
        Assert.Equal("CustomPath", options.Value.BasePath);
        Assert.Equal(50, options.Value.DefaultMaxSizeMB);
    }

    [Fact]
    public void AddFileUploadServices_WithTimeLimitedTokens_RegistersTokenService()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["FileUploadOptions:Provider"] = "local",
                ["FileUploadOptions:UseTimeLimitedTokens"] = "true"
            })
            .Build();

        // Act
        services.AddFileUploadServices(configuration);
        var provider = services.BuildServiceProvider();

        // Assert
        var tokenService = provider.GetService<IFileAccessTokenService>();
        Assert.NotNull(tokenService);
    }

    [Fact]
    public void AddFileStorageProvider_ReplacesDefaultProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddFileUploadServices(options => options.Provider = "local");

        // Act
        services.AddFileStorageProvider<CustomStorageProvider>();
        var provider = services.BuildServiceProvider();

        // Assert
        var storageProvider = provider.GetService<IFileStorageProvider>();
        Assert.NotNull(storageProvider);
        Assert.IsType<CustomStorageProvider>(storageProvider);
    }

    [Fact]
    public void AddVirusScanner_RegistersScanner()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddFileUploadServices(options => options.EnableVirusScanning = true);

        // Act
        services.AddVirusScanner<MockVirusScanner>();
        var provider = services.BuildServiceProvider();

        // Assert
        var scanner = provider.GetService<IVirusScanner>();
        Assert.NotNull(scanner);
        Assert.IsType<MockVirusScanner>(scanner);
    }

    [Fact]
    public void AddThumbnailGenerator_RegistersGenerator()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddFileUploadServices(options => options.EnableThumbnailGeneration = true);

        // Act
        services.AddThumbnailGenerator<MockThumbnailGenerator>();
        var provider = services.BuildServiceProvider();

        // Assert
        var generator = provider.GetService<IThumbnailGenerator>();
        Assert.NotNull(generator);
        Assert.IsType<MockThumbnailGenerator>(generator);
    }

    [Fact]
    public void AddFileUploadServices_ServiceLifetimes_AreCorrect()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddFileUploadServices(options => options.Provider = "local");

        // Act
        var serviceProvider = services.BuildServiceProvider();
        using var scope1 = serviceProvider.CreateScope();
        using var scope2 = serviceProvider.CreateScope();

        var service1 = scope1.ServiceProvider.GetService<IFileUploadService>();
        var service2 = scope2.ServiceProvider.GetService<IFileUploadService>();

        // Assert
        Assert.NotNull(service1);
        Assert.NotNull(service2);
        Assert.NotSame(service1, service2);
    }

    private class CustomStorageProvider : IFileStorageProvider
    {
        public string Name => "custom";

        public Task<string> UploadAsync(Stream stream, string fileName, string folderPath, CancellationToken cancellationToken = default)
            => Task.FromResult("custom/path");

        public Task<bool> ExistsAsync(string filePath, CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        public string? GetFullPath(string relativePath) => null;
    }

    private class MockVirusScanner : IVirusScanner
    {
        public Task<bool> ScanAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
            => Task.FromResult(true);
    }

    private class MockThumbnailGenerator : IThumbnailGenerator
    {
        public Task<string?> GenerateAsync(Stream sourceStream, string thumbnailPath, int width, int height, CancellationToken cancellationToken = default)
            => Task.FromResult<string?>("thumbnail/path");
    }
}
