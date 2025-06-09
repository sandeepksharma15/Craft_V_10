using Microsoft.Extensions.FileProviders;
using Moq;

namespace Craft.Extensions.Tests.FileInfo;

public class FileInfoTests
{
    [Fact]
    public void ContentType_ShouldReturnGif_WhenImageExtensionIsGif()
    {
        // Arrange
        var fileInfo = new FakeFileInfo("Fake.Gif");

        // Act
        var result = fileInfo.ContentType();

        // Assert
        Assert.Equal("image/gif", result);
    }

    [Fact]
    public void ContentType_ShouldReturnJpeg_WhenImageExtensionIsJpg()
    {
        // Arrange
        var fileInfo = new FakeFileInfo("Fake.Jpg");

        // Act
        var result = fileInfo.ContentType();

        // Assert
        Assert.Equal("image/jpeg", result);
    }

    [Fact]
    public void ContentType_ShouldReturnOctetStream_WhenImageExtensionIsUnknown()
    {
        // Arrange
        var fileInfo = new FakeFileInfo("Fake.xsl");

        // Act
        var result = fileInfo.ContentType();

        // Assert
        Assert.Equal("application/octet-stream", result);
    }

    [Fact]
    public void ContentType_ShouldReturnPng_WhenImageExtensionIsPng()
    {
        // Arrange
        var fileInfo = new FakeFileInfo("Fake.Png");

        // Act
        var result = fileInfo.ContentType();

        // Assert
        Assert.Equal("image/png", result);
    }

    [Fact]
    public void ContentType_ShouldReturnSvg_WhenImageExtensionIsSvg()
    {
        // Arrange
        var fileInfo = new FakeFileInfo("Fake.Svg");

        // Act
        var result = fileInfo.ContentType();

        // Assert
        Assert.Equal("image/svg", result);
    }

    [Fact]
    public void Extension_ShouldReturnEmptyString_WhenNoExtensionExists()
    {
        // Arrange
        var fileInfo = new FakeFileInfo("file");

        // Act
        var result = fileInfo.Extension();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Extension_ShouldReturnLowerCaseExtension_WhenExtensionExists()
    {
        // Arrange
        var fileInfo = new FakeFileInfo("example.txt");

        // Act
        var result = fileInfo.Extension();

        // Assert
        Assert.Equal(".txt", result);
    }

    [Fact]
    public void Extension_ShouldReturnLowerCaseExtension_WhenExtensionExistsWithLeadingDot()
    {
        // Arrange
        var fileInfo = new FakeFileInfo(".document");

        // Act
        var result = fileInfo.Extension();

        // Assert
        Assert.Equal(".document", result);
    }

    [Fact]
    public void Extension_ShouldReturnLowerCaseExtension_WhenExtensionExistsWithMixedCase()
    {
        // Arrange
        var fileInfo = new FakeFileInfo("file.PDF");

        // Act
        var result = fileInfo.Extension();

        // Assert
        Assert.Equal(".pdf", result);
    }

    [Fact]
    public void Extension_ShouldReturnLowerCaseExtension_WhenMultipleDotsExist()
    {
        // Arrange
        var fileInfo = new FakeFileInfo("file.test.txt");

        // Act
        var result = fileInfo.Extension();

        // Assert
        Assert.Equal(".txt", result);
    }

    [Fact]
    public void GetFileExtensions()
    {
        // Arrange
        var txt = Mock.Of<IFileInfo>(f => f.Name == "test.txt");

        // Act & Assert
        Assert.Equal(".txt", txt.Extension());
    }

    [Fact]
    public void GetImageExtension_ShouldReturnGif_WhenExtensionIsGif()
    {
        // Arrange
        var fileInfo = new FakeFileInfo(".gif");

        // Act
        var result = fileInfo.GetImageExtension();

        // Assert
        Assert.Equal(ImageExtension.Gif, result);
    }

    [Fact]
    public void GetImageExtension_ShouldReturnJpg_WhenExtensionIsJpg()
    {
        // Arrange
        var fileInfo = new FakeFileInfo(".jpg");

        // Act
        var result = fileInfo.GetImageExtension();

        // Assert
        Assert.Equal(ImageExtension.Jpg, result);
    }

    [Fact]
    public void GetImageExtension_ShouldReturnPng_WhenExtensionIsPng()
    {
        // Arrange
        var fileInfo = new FakeFileInfo(".png");

        // Act
        var result = fileInfo.GetImageExtension();

        // Assert
        Assert.Equal(ImageExtension.Png, result);
    }

    [Fact]
    public void GetImageExtension_ShouldReturnSvg_WhenExtensionIsSvg()
    {
        // Arrange
        var fileInfo = new FakeFileInfo(".svg");

        // Act
        var result = fileInfo.GetImageExtension();

        // Assert
        Assert.Equal(ImageExtension.Svg, result);
    }

    [Fact]
    public void GetImageExtension_ShouldReturnUnknown_WhenExtensionIsNotRecognized()
    {
        // Arrange
        var fileInfo = new FakeFileInfo(".txt");

        // Act
        var result = fileInfo.GetImageExtension();

        // Assert
        Assert.Equal(ImageExtension.Unknown, result);
    }

    [Fact]
    public void PngImageExtensions()
    {
        // Arrange
        var png = Mock.Of<IFileInfo>(f => f.Name == "test.png");

        // Act & Assert
        Assert.Equal(".png", png.Extension());
        Assert.Equal(ImageExtension.Png, png.GetImageExtension());
        Assert.Equal("image/png", png.ContentType());
    }

    [Fact]
    public void SvgImageExtensions()
    {
        // Arrange
        var svg = Mock.Of<IFileInfo>(f => f.Name == "test.svg");

        // Act & Assert
        Assert.Equal(".svg", svg.Extension());
        Assert.Equal(ImageExtension.Svg, svg.GetImageExtension());
        Assert.Equal("image/svg", svg.ContentType());
    }

    [Fact]
    public void UnknownImageExtensions()
    {
        // Arrange
        var webp = Mock.Of<IFileInfo>(f => f.Name == "test.webp");

        // Act & Assert
        Assert.Equal(".webp", webp.Extension());
        Assert.Equal(ImageExtension.Unknown, webp.GetImageExtension());
        Assert.Equal("application/octet-stream", webp.ContentType());
    }
}

public class FakeFileInfo(string name) : IFileInfo
{
    public bool Exists => throw new NotImplementedException();

    public bool IsDirectory { get; }
    public DateTimeOffset LastModified { get; }
    public long Length => throw new NotImplementedException();

    public string Name { get; } = name;
    public string PhysicalPath => throw new NotImplementedException();

    public Stream CreateReadStream()
    {
        throw new NotImplementedException();
    }
}
