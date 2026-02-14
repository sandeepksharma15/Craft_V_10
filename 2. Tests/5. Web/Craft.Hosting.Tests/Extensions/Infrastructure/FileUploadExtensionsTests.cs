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
        services.AddFileUploadServices(configuration);
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
        services.AddFileUploadServices(section);
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
        services.AddFileUploadServices(options =>
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
        services.AddFileUploadServices(configuration);
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
        services.AddFileUploadServices(configuration);
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
        services
            .AddFileUploadServices(options => options.Provider = "local")
            .AddFileStorageProvider<TestFileStorageProvider>();

        var serviceProvider = services.BuildServiceProvider();
        var provider = serviceProvider.GetService<IFileStorageProvider>();

        // Assert
        Assert.IsType<TestFileStorageProvider>(provider);
    }

    [Fact]
    public void AddVirusScanner_RegistersVirusScanner()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services
            .AddFileUploadServices(options => options.Provider = "local")
            .AddVirusScanner<TestVirusScanner>();

        var serviceProvider = services.BuildServiceProvider();
        var scanner = serviceProvider.GetService<IVirusScanner>();

        // Assert
        Assert.IsType<TestVirusScanner>(scanner);
    }

    [Fact]
    public void AddThumbnailGenerator_RegistersThumbnailGenerator()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services
            .AddFileUploadServices(options => options.Provider = "local")
            .AddThumbnailGenerator<TestThumbnailGenerator>();

        var serviceProvider = services.BuildServiceProvider();
        var generator = serviceProvider.GetService<IThumbnailGenerator>();

        // Assert
        Assert.IsType<TestThumbnailGenerator>(generator);
    }

    [Fact]
    public void AddFileAccessTokenService_ReplacesDefaultTokenService()
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
        services
            .AddFileUploadServices(configuration)
            .AddFileAccessTokenService<TestFileAccessTokenService>();

        var serviceProvider = services.BuildServiceProvider();
        var tokenService = serviceProvider.GetService<IFileAccessTokenService>();

        // Assert
        Assert.IsType<TestFileAccessTokenService>(tokenService);
    }

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
        services.AddFileUploadServices(configuration);
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
        var result = services.AddFileUploadServices(configuration);

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddFileStorageProvider_ReturnsSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddFileUploadServices(options => options.Provider = "local");

        // Act
        var result = services.AddFileStorageProvider<TestFileStorageProvider>();

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddVirusScanner_ReturnsSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddFileUploadServices(options => options.Provider = "local");

        // Act
        var result = services.AddVirusScanner<TestVirusScanner>();

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddThumbnailGenerator_ReturnsSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddFileUploadServices(options => options.Provider = "local");

        // Act
        var result = services.AddThumbnailGenerator<TestThumbnailGenerator>();

        // Assert
        Assert.Same(services, result);
    }

    private class TestFileStorageProvider : IFileStorageProvider
    {
        public Task<FileUploadResult> UploadAsync(Stream fileStream, string fileName, string? contentType = null, CancellationToken cancellationToken = default)
            => Task.FromResult(FileUploadResult.Success("test-id", "test-path"));

        public Task<FileDownloadResult> DownloadAsync(string fileId, CancellationToken cancellationToken = default)
            => Task.FromResult(FileDownloadResult.Success(Stream.Null, "test.txt", "text/plain"));

        public Task<bool> DeleteAsync(string fileId, CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        public Task<bool> ExistsAsync(string fileId, CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        public Task<FileMetadata?> GetMetadataAsync(string fileId, CancellationToken cancellationToken = default)
            => Task.FromResult<FileMetadata?>(null);
    }

    private class TestVirusScanner : IVirusScanner
    {
        public Task<ScanResult> ScanAsync(Stream fileStream, CancellationToken cancellationToken = default)
            => Task.FromResult(ScanResult.Clean());
    }

    private class TestThumbnailGenerator : IThumbnailGenerator
    {
        public Task<ThumbnailResult> GenerateAsync(Stream imageStream, int width, int height, CancellationToken cancellationToken = default)
            => Task.FromResult(ThumbnailResult.Success(Stream.Null));
    }

    private class TestFileAccessTokenService : IFileAccessTokenService
    {
        public string GenerateToken(string fileId, TimeSpan? expiresIn = null)
            => "test-token";

        public (bool IsValid, string? FileId) ValidateToken(string token)
            => (true, "test-id");
    }
}
