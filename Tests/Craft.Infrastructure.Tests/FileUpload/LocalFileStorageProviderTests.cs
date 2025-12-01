using Craft.Infrastructure.FileUpload;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Craft.Infrastructure.Tests.FileUpload;

public class LocalFileStorageProviderTests : IDisposable
{
    private readonly string _testBasePath;
    private readonly LocalFileStorageProvider _provider;
    private readonly Mock<ILogger<LocalFileStorageProvider>> _loggerMock;

    public LocalFileStorageProviderTests()
    {
        _testBasePath = Path.Combine(Path.GetTempPath(), "FileUploadTests", Guid.NewGuid().ToString());
        
        var options = new FileUploadOptions { BasePath = _testBasePath };
        var optionsMock = new Mock<IOptions<FileUploadOptions>>();
        optionsMock.Setup(x => x.Value).Returns(options);

        _loggerMock = new Mock<ILogger<LocalFileStorageProvider>>();

        _provider = new LocalFileStorageProvider(optionsMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task UploadAsync_CreatesDirectoryIfNotExists()
    {
        // Arrange
        var fileName = "test.txt";
        var folderPath = "TestFolder";
        var content = "Test content"u8.ToArray();

        await using var stream = new MemoryStream(content);

        // Act
        var result = await _provider.UploadAsync(stream, fileName, folderPath);

        // Assert
        Assert.NotNull(result);
        var fullPath = _provider.GetFullPath(result);
        Assert.True(File.Exists(fullPath));
    }

    [Fact]
    public async Task UploadAsync_HandlesFileNameConflicts()
    {
        // Arrange
        var fileName = "duplicate.txt";
        var folderPath = "Duplicates";
        var content1 = "Content 1"u8.ToArray();
        var content2 = "Content 2"u8.ToArray();

        await using var stream1 = new MemoryStream(content1);
        await using var stream2 = new MemoryStream(content2);

        // Act
        var result1 = await _provider.UploadAsync(stream1, fileName, folderPath);
        stream2.Position = 0;
        var result2 = await _provider.UploadAsync(stream2, fileName, folderPath);

        // Assert
        Assert.NotEqual(result1, result2);
        Assert.Contains(" (1)", result2);
    }

    [Fact]
    public async Task UploadAsync_SanitizesFileName()
    {
        // Arrange
        var unsafeFileName = "test<>:file?.txt";
        var folderPath = "Sanitized";
        var content = "Test"u8.ToArray();

        await using var stream = new MemoryStream(content);

        // Act
        var result = await _provider.UploadAsync(stream, unsafeFileName, folderPath);

        // Assert
        Assert.NotNull(result);
        Assert.DoesNotContain("<", result);
        Assert.DoesNotContain(">", result);
        Assert.DoesNotContain(":", result);
        Assert.DoesNotContain("?", result);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsTrue_WhenFileExists()
    {
        // Arrange
        var fileName = "exists.txt";
        var folderPath = "Exists";
        var content = "Test"u8.ToArray();

        await using var stream = new MemoryStream(content);
        var relativePath = await _provider.UploadAsync(stream, fileName, folderPath);

        // Act
        var exists = await _provider.ExistsAsync(relativePath);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsFalse_WhenFileDoesNotExist()
    {
        // Arrange
        var nonExistentPath = "NonExistent/file.txt";

        // Act
        var exists = await _provider.ExistsAsync(nonExistentPath);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public void GetFullPath_ReturnsCorrectPath()
    {
        // Arrange
        var relativePath = Path.Combine("Folder", "file.txt");

        // Act
        var fullPath = _provider.GetFullPath(relativePath);

        // Assert
        Assert.NotNull(fullPath);
        Assert.Contains(_testBasePath, fullPath);
        Assert.Contains("Folder", fullPath);
        Assert.Contains("file.txt", fullPath);
    }

    [Fact]
    public void GetFullPath_ReturnsNull_ForEmptyPath()
    {
        // Arrange & Act
        var result = _provider.GetFullPath("");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UploadAsync_ThrowsArgumentNullException_ForNullStream()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _provider.UploadAsync(null!, "test.txt", "Folder"));
    }

    [Fact]
    public async Task UploadAsync_ThrowsArgumentException_ForEmptyFileName()
    {
        // Arrange
        await using var stream = new MemoryStream();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _provider.UploadAsync(stream, "", "Folder"));
    }

    [Fact]
    public async Task UploadAsync_ThrowsArgumentException_ForEmptyFolderPath()
    {
        // Arrange
        await using var stream = new MemoryStream();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _provider.UploadAsync(stream, "test.txt", ""));
    }

    public void Dispose()
    {
        if (Directory.Exists(_testBasePath))
            Directory.Delete(_testBasePath, recursive: true);
    }
}
