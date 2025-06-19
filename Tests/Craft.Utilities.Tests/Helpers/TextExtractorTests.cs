using System;
using System.IO;
using Xunit;
using Craft.Utilities.Helpers;

namespace Craft.Utilities.Tests.Helpers;

public class TextExtractorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ExtractTextFromDocOrPdf_ThrowsArgumentException_WhenFileNameIsNullOrEmpty(string? fileName)
    {
        if (fileName is null)
            Assert.Throws<ArgumentNullException>(() => TextExtractor.ExtractTextFromDocOrPdf(fileName!));
        else
            Assert.Throws<ArgumentException>(() => TextExtractor.ExtractTextFromDocOrPdf(fileName));
    }

    [Theory]
    [InlineData("test.txt")]
    [InlineData("test.xlsx")]
    [InlineData("test.png")]
    public void ExtractTextFromDocOrPdf_ThrowsNotSupportedException_ForUnsupportedExtensions(string fileName)
    {
        Assert.Throws<NotSupportedException>(() => TextExtractor.ExtractTextFromDocOrPdf(fileName));
    }

    [Fact]
    public void ExtractTextFromDocOrPdf_ThrowsFileNotFoundException_WhenFileDoesNotExist()
    {
        Assert.Throws<FileNotFoundException>(() => TextExtractor.ExtractTextFromDocOrPdf("nonexistent.docx"));
    }

    [Fact]
    public void ExtractTextFromDocOrPdf_ReturnsEmptyString_ForCorruptedDocx()
    {
        var tempFile = Path.GetTempFileName() + ".docx";
        File.WriteAllText(tempFile, "not a real docx");
        try
        {
            var result = TextExtractor.ExtractTextFromDocOrPdf(tempFile);
            Assert.Equal(string.Empty, result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ExtractTextFromDocOrPdf_ReturnsEmptyString_ForCorruptedPdf()
    {
        var tempFile = Path.GetTempFileName() + ".pdf";
        File.WriteAllText(tempFile, "not a real pdf");
        try
        {
            var result = TextExtractor.ExtractTextFromDocOrPdf(tempFile);
            Assert.Equal(string.Empty, result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
     public void ExtractTextFromDocOrPdf_ReturnsText_ForValidDocx()
    {
        // Arrange
        string filePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData", "Test.docx");

        // Act
        var result = TextExtractor.ExtractTextFromDocOrPdf(filePath);

        // Assert
        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void ExtractTextFromDocOrPdf_ReturnsText_ForValidPdf()
    {
        // Arrange
        string filePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestData", "Test.pdf");

        // Act
        var result = TextExtractor.ExtractTextFromDocOrPdf(filePath);

        // Assert
        Assert.Equal("Hello World", result);
    }
}
