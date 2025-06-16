using System.Text;
using DocumentFormat.OpenXml.Packaging;

namespace Craft.Utilities.Helpers;

public static class TextExtractor
{
    public static string? ExtractTextFromDocOrPdf(string fileName)
    {
        // Ensure File Name is not null or empty
        ArgumentException.ThrowIfNullOrEmpty(fileName);

        // Get file extension
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        // Ensure the extension is pdf, doc or docx
        if (extension != ".pdf" && extension != ".doc" && extension != ".docx")
            throw new NotSupportedException("Only PDF, DOC and DOCX files are supported");

        // Ensure The File Exists
        if (!File.Exists(fileName))
            throw new FileNotFoundException("The file does not exist", fileName);

        // Create A Stream From Document
        using var stream = File.OpenRead(fileName);

        if (extension == ".pdf")
            // Extract text from PDF
            return ExtractTextFromPdfAsync(stream);
        else
            // Extract text from Word Document
            return ExtractTextFromWordDocumentAsync(stream);
    }

    private static string? ExtractTextFromWordDocumentAsync(Stream stream)
    {
        using var document = WordprocessingDocument.Open(stream, false);

        var body = document?.MainDocumentPart?.Document.Body;

        return body?.InnerText;
    }

    private static string ExtractTextFromPdfAsync(Stream stream)
    {
        using var reader = new iText.Kernel.Pdf.PdfReader(stream);
        using var pdfDoc = new iText.Kernel.Pdf.PdfDocument(reader);

        var text = new StringBuilder();

        for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
        {
            var page = pdfDoc.GetPage(i);
            text.Append(iText.Kernel.Pdf.Canvas.Parser.PdfTextExtractor.GetTextFromPage(page));
        }

        return text.ToString();
    }
}
