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
        // Arrange: Create a minimal valid DOCX file with "Hello World"
        var tempFile = Path.GetTempFileName() + ".docx";
        try
        {
            using (var doc = DocumentFormat.OpenXml.Packaging.WordprocessingDocument.Create(tempFile, DocumentFormat.OpenXml.WordprocessingDocumentType.Document))
            {
                var mainPart = doc.AddMainDocumentPart();
                mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document(
                    new DocumentFormat.OpenXml.Wordprocessing.Body(
                        new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                            new DocumentFormat.OpenXml.Wordprocessing.Run(
                                new DocumentFormat.OpenXml.Wordprocessing.Text("Hello World")
                            )
                        )
                    )
                );
            }

            // Act
            var result = TextExtractor.ExtractTextFromDocOrPdf(tempFile);

            // Assert
            Assert.Equal("Hello World", result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ExtractTextFromDocOrPdf_ReturnsText_ForValidPdf()
    {
        // Arrange: Create a minimal valid PDF file with "Hello World"
        var tempFile = Path.GetTempFileName() + ".pdf";
        try
        {
            // Create and close the PDF file before reading
            using (var writer = new iText.Kernel.Pdf.PdfWriter(tempFile))
            {
                using (var pdf = new iText.Kernel.Pdf.PdfDocument(writer))
                {
                    using (var doc = new iText.Layout.Document(pdf))
                    {
                        doc.Add(new iText.Layout.Element.Paragraph("Hello World"));
                    }
                }
            }

            // Act (after file is closed and all handles are released)
            var result = TextExtractor.ExtractTextFromDocOrPdf(tempFile);

            // Assert
            Assert.Equal("Hello World", result);
        }
        finally
        {
                try
                {
                    File.Delete(tempFile);
                }
                catch (Exception) {}
        }
    }
}
