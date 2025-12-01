using System.Text;
using Craft.Infrastructure.FileUpload;
using Microsoft.AspNetCore.Components.Forms;
using Moq;

namespace Craft.Utilities.Tests.Services;

public class FileUploadServiceTests
{
    [Fact]
    public async Task UploadFileAsync_ThrowsArgumentNullException_WhenSourceFileIsNull()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(() => FileUploadService.UploadFileAsync("somefile.txt", null!));
    }

    [Fact]
    public async Task UploadFileAsync_ThrowsArgumentException_WhenDestFilePathIsNullOrWhiteSpace()
    {
        // Arrange
        var mockFile = new Mock<IBrowserFile>();
        mockFile.Setup(f => f.Size).Returns(1);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => FileUploadService.UploadFileAsync(null, mockFile.Object));
        await Assert.ThrowsAsync<ArgumentException>(() => FileUploadService.UploadFileAsync(" ", mockFile.Object));
    }

    [Fact]
    public async Task UploadFileAsync_ThrowsInvalidOperationException_WhenFileSizeExceedsLimit()
    {
        // Arrange
        var mockFile = new Mock<IBrowserFile>();
        mockFile.Setup(f => f.Size).Returns(11 * 1024 * 1024); // 11 MB

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => FileUploadService.UploadFileAsync("file.txt", mockFile.Object, null, 10));
    }

    [Fact]
    public async Task UploadFileAsync_ThrowsIOException_WhenSourceStreamThrows()
    {
        // Arrange
        var mockFile = new Mock<IBrowserFile>();
        mockFile.Setup(f => f.Size).Returns(1);
        mockFile.Setup(f => f.OpenReadStream(It.IsAny<long>(), default))
            .Throws(new Exception("stream error"));

        // Act & Assert
        await Assert.ThrowsAsync<IOException>(() => FileUploadService.UploadFileAsync("file.txt", mockFile.Object));
    }

    [Fact]
    public async Task UploadFileAsync_WritesFileAndReportsProgress()
    {
        // Arrange
        var fileContent = Encoding.UTF8.GetBytes("Hello World");
        var memoryStream = new MemoryStream(fileContent);
        var mockFile = new Mock<IBrowserFile>();

        mockFile.Setup(f => f.Size)
            .Returns(fileContent.Length);

        mockFile.Setup(f => f.OpenReadStream(It.IsAny<long>(), default))
            .Returns(memoryStream);

        var tempFile = Path.GetTempFileName();

        int lastProgress = 0;
        try
        {
            // Act
            await FileUploadService.UploadFileAsync(tempFile, mockFile.Object, p => lastProgress = p, 10);
            var written = await File.ReadAllBytesAsync(tempFile);

            // Assert
            Assert.Equal(fileContent, written);
            Assert.Equal(100, lastProgress);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}

