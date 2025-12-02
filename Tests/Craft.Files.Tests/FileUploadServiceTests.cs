using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Craft.Files.Tests;

public class FileUploadServiceTests
{
    private readonly Mock<IFileStorageProvider> _storageProviderMock;
    private readonly Mock<ILogger<FileUploadService>> _loggerMock;
    private readonly FileUploadOptions _options;
    private readonly FileUploadService _service;

    public FileUploadServiceTests()
    {
        _storageProviderMock = new Mock<IFileStorageProvider>();
        _loggerMock = new Mock<ILogger<FileUploadService>>();
        _options = new FileUploadOptions();
        
        var optionsMock = new Mock<IOptions<FileUploadOptions>>();
        optionsMock.Setup(x => x.Value).Returns(_options);

        _service = new FileUploadService(
            optionsMock.Object,
            _storageProviderMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task UploadAsync_WithByteArray_Success()
    {
        // Arrange
        var fileData = "Test file content"u8.ToArray();
        var fileName = "test.txt";
        var uploadType = UploadType.Document;

        _storageProviderMock
            .Setup(x => x.UploadAsync(It.IsAny<Stream>(), fileName, It.IsAny<string>(), default))
            .ReturnsAsync("Documents/test.txt");

        // Act
        var result = await _service.UploadAsync(fileData, fileName, uploadType);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Documents/test.txt", result.FilePath);
        Assert.NotNull(result.Metadata);
        Assert.Equal(fileName, result.Metadata.FileName);
        Assert.Equal(".txt", result.Metadata.Extension);
        Assert.Equal(fileData.Length, result.Metadata.SizeInBytes);
    }

    [Fact]
    public async Task UploadAsync_WithStream_Success()
    {
        // Arrange
        var fileData = "Test file content"u8.ToArray();
        var fileName = "test.pdf";
        var uploadType = UploadType.Document;

        await using var stream = new MemoryStream(fileData);

        _storageProviderMock
            .Setup(x => x.UploadAsync(It.IsAny<Stream>(), fileName, It.IsAny<string>(), default))
            .ReturnsAsync("Documents/test.pdf");

        // Act
        var result = await _service.UploadAsync(stream, fileName, uploadType);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Documents/test.pdf", result.FilePath);
    }

    [Fact]
    public async Task UploadAsync_InvalidExtension_ReturnsFailure()
    {
        // Arrange
        var fileData = "Test file content"u8.ToArray();
        var fileName = "test.exe";
        var uploadType = UploadType.Document;

        // Act
        var result = await _service.UploadAsync(fileData, fileName, uploadType);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("not allowed", result.ErrorMessage);
    }

    [Fact]
    public async Task UploadAsync_FileSizeExceedsLimit_ReturnsFailure()
    {
        // Arrange
        var largeFileData = new byte[11 * 1024 * 1024];
        var fileName = "large.pdf";
        var uploadType = UploadType.Document;

        // Act
        var result = await _service.UploadAsync(largeFileData, fileName, uploadType);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("exceeds the allowed limit", result.ErrorMessage);
    }

    [Fact]
    public async Task UploadAsync_NullData_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.UploadAsync((byte[])null!, "test.txt", UploadType.Document));
    }

    [Fact]
    public async Task UploadAsync_EmptyFileName_ThrowsArgumentException()
    {
        // Arrange
        var fileData = "Test"u8.ToArray();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.UploadAsync(fileData, "", UploadType.Document));
    }

    [Fact]
    public async Task UploadBrowserFileAsync_Success()
    {
        // Arrange
        var fileContent = "Browser file content"u8.ToArray();
        var mockBrowserFile = new Mock<IBrowserFile>();
        
        mockBrowserFile.Setup(x => x.Name).Returns("browser.jpg");
        mockBrowserFile.Setup(x => x.Size).Returns(fileContent.Length);
        mockBrowserFile.Setup(x => x.ContentType).Returns("image/jpeg");
        mockBrowserFile.Setup(x => x.OpenReadStream(It.IsAny<long>(), default))
            .Returns(new MemoryStream(fileContent));

        _storageProviderMock
            .Setup(x => x.UploadAsync(It.IsAny<Stream>(), "browser.jpg", It.IsAny<string>(), default))
            .ReturnsAsync(@"Images\Assets\browser.jpg");

        // Act
        var result = await _service.UploadBrowserFileAsync(mockBrowserFile.Object, UploadType.Image);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(@"Images\Assets\browser.jpg", result.FilePath);
    }

    [Fact]
    public async Task UploadBrowserFileAsync_FileTooLarge_ReturnsFailure()
    {
        // Arrange
        var mockBrowserFile = new Mock<IBrowserFile>();
        mockBrowserFile.Setup(x => x.Name).Returns("large.jpg");
        mockBrowserFile.Setup(x => x.Size).Returns(10 * 1024 * 1024);

        // Act
        var result = await _service.UploadBrowserFileAsync(mockBrowserFile.Object, UploadType.Image);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("exceeds the allowed limit", result.ErrorMessage);
    }

    [Fact]
    public async Task UploadBrowserFileAsync_ReportsProgress()
    {
        // Arrange
        var fileContent = new byte[1024 * 100];
        var mockBrowserFile = new Mock<IBrowserFile>();
        
        mockBrowserFile.Setup(x => x.Name).Returns("progress.jpg");
        mockBrowserFile.Setup(x => x.Size).Returns(fileContent.Length);
        mockBrowserFile.Setup(x => x.ContentType).Returns("image/jpeg");
        mockBrowserFile.Setup(x => x.OpenReadStream(It.IsAny<long>(), default))
            .Returns(new MemoryStream(fileContent));

        _storageProviderMock
            .Setup(x => x.UploadAsync(It.IsAny<Stream>(), "progress.jpg", It.IsAny<string>(), default))
            .ReturnsAsync(@"Images\Assets\progress.jpg");

        var progressValues = new List<int>();

        // Act
        var result = await _service.UploadBrowserFileAsync(
            mockBrowserFile.Object,
            UploadType.Image,
            progressValues.Add);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(progressValues);
        Assert.Equal(100, progressValues.Last());
    }

    [Fact]
    public async Task UploadAsync_WithMultiTenancy_UsesTenantFolder()
    {
        // Arrange
        var tenant = new { Id = "tenant1", Identifier = "acme" };

        var optionsMock = new Mock<IOptions<FileUploadOptions>>();
        _options.EnableMultiTenancy = true;
        optionsMock.Setup(x => x.Value).Returns(_options);

        var service = new FileUploadService(
            optionsMock.Object,
            _storageProviderMock.Object,
            _loggerMock.Object,
            tenant: tenant);

        var fileData = "Test"u8.ToArray();
        var fileName = "test.jpg";

        _storageProviderMock
            .Setup(x => x.UploadAsync(
                It.IsAny<Stream>(),
                fileName,
                It.Is<string>(path => path.Contains("Tenants") && path.Contains("acme")),
                default))
            .ReturnsAsync(@"Tenants\acme\Images\Assets\test.jpg");

        // Act
        var result = await service.UploadAsync(fileData, fileName, UploadType.Image);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains("Tenants", result.FilePath!);
        Assert.Contains("acme", result.FilePath);
    }

    [Fact]
    public async Task UploadAsync_WithCurrentUser_StoresUploadedBy()
    {
        // Arrange
        var user = new { Id = "user123" };

        var optionsMock = new Mock<IOptions<FileUploadOptions>>();
        optionsMock.Setup(x => x.Value).Returns(_options);

        var service = new FileUploadService(
            optionsMock.Object,
            _storageProviderMock.Object,
            _loggerMock.Object,
            currentUser: user);

        var fileData = "Test"u8.ToArray();
        var fileName = "test.jpg";

        _storageProviderMock
            .Setup(x => x.UploadAsync(It.IsAny<Stream>(), fileName, It.IsAny<string>(), default))
            .ReturnsAsync(@"Images\Assets\test.jpg");

        // Act
        var result = await service.UploadAsync(fileData, fileName, UploadType.Image);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Metadata);
        Assert.Equal("user123", result.Metadata.UploadedBy);
    }

    [Fact]
    public async Task UploadAsync_WithVirusScanning_Success()
    {
        // Arrange
        var virusScannerMock = new Mock<IVirusScanner>();
        virusScannerMock
            .Setup(x => x.ScanAsync(It.IsAny<Stream>(), It.IsAny<string>(), default))
            .ReturnsAsync(true);

        var optionsMock = new Mock<IOptions<FileUploadOptions>>();
        _options.EnableVirusScanning = true;
        optionsMock.Setup(x => x.Value).Returns(_options);

        var service = new FileUploadService(
            optionsMock.Object,
            _storageProviderMock.Object,
            _loggerMock.Object,
            virusScanner: virusScannerMock.Object);

        var fileData = "Test"u8.ToArray();
        var fileName = "test.pdf";

        _storageProviderMock
            .Setup(x => x.UploadAsync(It.IsAny<Stream>(), fileName, It.IsAny<string>(), default))
            .ReturnsAsync("Documents/test.pdf");

        // Act
        var result = await service.UploadAsync(fileData, fileName, UploadType.Document);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Metadata?.VirusScanPassed);
        virusScannerMock.Verify(
            x => x.ScanAsync(It.IsAny<Stream>(), fileName, default),
            Times.Once);
    }

    [Fact]
    public async Task UploadAsync_VirusDetected_ReturnsFailure()
    {
        // Arrange
        var virusScannerMock = new Mock<IVirusScanner>();
        virusScannerMock
            .Setup(x => x.ScanAsync(It.IsAny<Stream>(), It.IsAny<string>(), default))
            .ReturnsAsync(false);

        var optionsMock = new Mock<IOptions<FileUploadOptions>>();
        _options.EnableVirusScanning = true;
        optionsMock.Setup(x => x.Value).Returns(_options);

        var service = new FileUploadService(
            optionsMock.Object,
            _storageProviderMock.Object,
            _loggerMock.Object,
            virusScanner: virusScannerMock.Object);

        var fileData = "Virus"u8.ToArray();
        var fileName = "virus.pdf";

        // Act
        var result = await service.UploadAsync(fileData, fileName, UploadType.Document);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Virus detected", result.ErrorMessage);
    }

    [Fact]
    public async Task UploadAsync_WithTimeLimitedTokens_GeneratesToken()
    {
        // Arrange
        var tokenServiceMock = new Mock<IFileAccessTokenService>();
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        tokenServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<int>()))
            .Returns(("test-token", expiresAt));

        var optionsMock = new Mock<IOptions<FileUploadOptions>>();
        _options.UseTimeLimitedTokens = true;
        _options.TokenExpirationMinutes = 60;
        optionsMock.Setup(x => x.Value).Returns(_options);

        var service = new FileUploadService(
            optionsMock.Object,
            _storageProviderMock.Object,
            _loggerMock.Object,
            tokenService: tokenServiceMock.Object);

        var fileData = "Test"u8.ToArray();
        var fileName = "test.pdf";

        _storageProviderMock
            .Setup(x => x.UploadAsync(It.IsAny<Stream>(), fileName, It.IsAny<string>(), default))
            .ReturnsAsync("Documents/test.pdf");

        // Act
        var result = await service.UploadAsync(fileData, fileName, UploadType.Document);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("test-token", result.AccessToken);
        Assert.Equal(expiresAt, result.TokenExpiresAt);
    }

    [Fact]
    public async Task FileExistsAsync_ReturnsTrue_WhenFileExists()
    {
        // Arrange
        var filePath = "test.txt";
        _storageProviderMock
            .Setup(x => x.ExistsAsync(filePath, default))
            .ReturnsAsync(true);

        // Act
        var exists = await _service.FileExistsAsync(filePath);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public void GetFullPath_ReturnsPathFromProvider()
    {
        // Arrange
        var relativePath = "Documents/test.pdf";
        var fullPath = "C:/Files/Documents/test.pdf";
        
        _storageProviderMock
            .Setup(x => x.GetFullPath(relativePath))
            .Returns(fullPath);

        // Act
        var result = _service.GetFullPath(relativePath);

        // Assert
        Assert.Equal(fullPath, result);
    }
}
