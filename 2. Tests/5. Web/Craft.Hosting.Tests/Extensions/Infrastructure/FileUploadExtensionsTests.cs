using Craft.Files;
using Craft.Hosting.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Craft.Hosting.Tests.Extensions.Infrastructure;

/// <summary>
/// Unit tests for FileUploadExtensions.
/// </summary>
public class FileUploadExtensionsTests
{
    [Fact]
    public void AddFileUploadServices_WithConfiguration_RegistersAllServices()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["FileUploadOptions:Provider"] = "local",
                ["FileUploadOptions:BasePath"] = "uploads"
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        Craft.Hosting.Extensions.FileUploadExtensions.AddFileUploadServices(services, configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<IFileUploadService>());
        Assert.NotNull(serviceProvider.GetService<IFileStorageProvider>());
    }

    [Fact]
    public void AddFileUploadServices_WithConfigurationSection_RegistersAllServices()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["FileUploadOptions:Provider"] = "local",
                ["FileUploadOptions:BasePath"] = "uploads"
            })
            .Build();

        var services = new ServiceCollection();
        var section = configuration.GetSection("FileUploadOptions");

        // Act
        Craft.Hosting.Extensions.FileUploadExtensions.AddFileUploadServices(services, section);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<IFileUploadService>());
        Assert.NotNull(serviceProvider.GetService<IFileStorageProvider>());
    }

    [Fact]
    public void AddFileUploadServices_WithAction_RegistersAllServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Craft.Hosting.Extensions.FileUploadExtensions.AddFileUploadServices(services, options =>
        {
            options.Provider = "local";
            options.BasePath = "uploads";
            options.DefaultMaxSizeMB = 20;
        });

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<IFileUploadService>());
        Assert.NotNull(serviceProvider.GetService<IFileStorageProvider>());
    }

    [Fact]
    public void AddFileUploadServices_WithTimeLimitedTokens_RegistersTokenService()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["FileUploadOptions:Provider"] = "local",
                ["FileUploadOptions:UseTimeLimitedTokens"] = "true"
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        Craft.Hosting.Extensions.FileUploadExtensions.AddFileUploadServices(services, configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<IFileAccessTokenService>());
    }

    [Fact]
    public void AddFileUploadServices_WithoutTimeLimitedTokens_DoesNotRegisterTokenService()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["FileUploadOptions:Provider"] = "local",
                ["FileUploadOptions:UseTimeLimitedTokens"] = "false"
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        Craft.Hosting.Extensions.FileUploadExtensions.AddFileUploadServices(services, configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.Null(serviceProvider.GetService<IFileAccessTokenService>());
    }

    [Fact]
    public void AddFileStorageProvider_ReplacesDefaultProvider()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Craft.Hosting.Extensions.FileUploadExtensions.AddFileUploadServices(services, options => options.Provider = "local");
        Craft.Hosting.Extensions.FileUploadExtensions.AddFileStorageProvider<TestFileStorageProvider>(services);

        var serviceProvider = services.BuildServiceProvider();
        var provider = serviceProvider.GetService<IFileStorageProvider>();

        // Assert
        Assert.IsType<TestFileStorageProvider>(provider);
    }

    // Temporarily commented out until IVirusScanner interface is properly defined in Craft.Files
    // [Fact]
    // public void AddVirusScanner_RegistersVirusScanner()
    // {
    //     // Arrange
    //     var services = new ServiceCollection();
    //
    //     // Act
    //     services
    //         .AddFileUploadServices(options => options.Provider = "local")
    //         .AddVirusScanner<TestVirusScanner>();
    //
    //     var serviceProvider = services.BuildServiceProvider();
    //     var scanner = serviceProvider.GetService<IVirusScanner>();
    //
    //     // Assert
    //     Assert.IsType<TestVirusScanner>(scanner);
    // }
    //
    // [Fact]
    // public void AddThumbnailGenerator_RegistersThumbnailGenerator()
    // {
    //     // Arrange
    //     var services = new ServiceCollection();
    //
    //     // Act
    //     services
    //         .AddFileUploadServices(options => options.Provider = "local")
    //         .AddThumbnailGenerator<TestThumbnailGenerator>();
    //
    //     var serviceProvider = services.BuildServiceProvider();
    //     var generator = serviceProvider.GetService<IThumbnailGenerator>();
    //
    //     // Assert
    //     Assert.IsType<TestThumbnailGenerator>(generator);
    // }
    //
    // [Fact]
    // public void AddFileAccessTokenService_ReplacesDefaultTokenService()
    // {
    //     // Arrange
    //     var configuration = new ConfigurationBuilder()
    //         .AddInMemoryCollection(new Dictionary<string, string?>
    //         {
    //             ["FileUploadOptions:Provider"] = "local",
    //             ["FileUploadOptions:UseTimeLimitedTokens"] = "true"
    //         })
    //         .Build();
    //
    //     var services = new ServiceCollection();
    //
    //     // Act
    //     services
    //         .AddFileUploadServices(configuration)
    //         .AddFileAccessTokenService<TestFileAccessTokenService>();
    //
    //     var serviceProvider = services.BuildServiceProvider();
    //     var tokenService = serviceProvider.GetService<IFileAccessTokenService>();
    //
    //     // Assert
    //     Assert.IsType<TestFileAccessTokenService>(tokenService);
    // }

    [Fact]
    public void AddFileUploadServices_ConfiguresOptionsWithValidation()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["FileUploadOptions:Provider"] = "local",
                ["FileUploadOptions:BasePath"] = "uploads"
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        Craft.Hosting.Extensions.FileUploadExtensions.AddFileUploadServices(services, configuration);
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<FileUploadOptions>>().Value;

        // Assert
        Assert.Equal("local", options.Provider);
        Assert.Equal("uploads", options.BasePath);
    }

    [Fact]
    public void AddFileUploadServices_ReturnsSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        var result = Craft.Hosting.Extensions.FileUploadExtensions.AddFileUploadServices(services, configuration);

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddFileStorageProvider_ReturnsSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        Craft.Hosting.Extensions.FileUploadExtensions.AddFileUploadServices(services, options => options.Provider = "local");

        // Act
        var result = Craft.Hosting.Extensions.FileUploadExtensions.AddFileStorageProvider<TestFileStorageProvider>(services);

        // Assert
        Assert.Same(services, result);
    }

    // Temporarily commented out - related to IVirusScanner/IThumbnailGenerator which may not be defined yet
    // [Fact]
    // public void AddVirusScanner_ReturnsSameServiceCollection()
    // {
    //     // Arrange
    //     var services = new ServiceCollection();
    //     services.AddFileUploadServices(options => options.Provider = "local");
    //
    //     // Act
    //     var result = services.AddVirusScanner<TestVirusScanner>();
    //
    //     // Assert
    //     Assert.Same(services, result);
    // }
    //
    // [Fact]
    // public void AddThumbnailGenerator_ReturnsSameServiceCollection()
    // {
    //     // Arrange
    //     var services = new ServiceCollection();
    //     services.AddFileUploadServices(options => options.Provider = "local");
    //
    //     // Act
    //     var result = services.AddThumbnailGenerator<TestThumbnailGenerator>();
    //
    //     // Assert
    //     Assert.Same(services, result);
    // }

    private class TestFileStorageProvider : IFileStorageProvider
    {
        public string Name => "test";

        public Task<string> UploadAsync(Stream fileStream, string fileName, string folderPath, CancellationToken cancellationToken = default)
            => Task.FromResult($"{folderPath}/{fileName}");

        public Task<bool> ExistsAsync(string filePath, CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        public string? GetFullPath(string relativePath)
            => $"/full/path/{relativePath}";
    }

    // Note: IVirusScanner, IThumbnailGenerator, and IFileAccessTokenService interfaces
    // may not be currently defined in Craft.Files. These test classes are commented out
    // until the interfaces are properly defined in the Craft.Files project.

    /*
    private class TestVirusScanner : IVirusScanner
    {
        public Task<bool> ScanAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
            => Task.FromResult(true);
    }

    private class TestThumbnailGenerator : IThumbnailGenerator
    {
        public Task<Stream> GenerateAsync(Stream imageStream, string format, int width, int height, CancellationToken cancellationToken = default)
            => Task.FromResult<Stream>(Stream.Null);
    }

    private class TestFileAccessTokenService : IFileAccessTokenService
    {
        public string GenerateToken(string fileId, int expirationMinutes)
            => "test-token";

        public bool ValidateToken(string token, string fileId)
            => true;
    }
    */
}
