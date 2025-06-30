using System.Text;
using DocumentFormat.OpenXml.Packaging;

namespace Craft.Utilities.Helpers;

public static class TextExtractor
{
    public static string ExtractTextFromDocOrPdf(string fileName)
    {
        ArgumentException.ThrowIfNullOrEmpty(fileName);

        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        if (extension != ".pdf" && extension != ".doc" && extension != ".docx")
            throw new NotSupportedException("Only PDF, DOC and DOCX files are supported");

        if (!File.Exists(fileName))
            throw new FileNotFoundException("The file does not exist", fileName);

        if (extension == ".pdf")
        {
            // Use file path directly for PDF
            return ExtractTextFromPdf(fileName);
        }
        else
        {
            using (var stream = File.OpenRead(fileName))
            {
                return ExtractTextFromWordDocument(stream);
            }
        }
    }

    private static string ExtractTextFromPdf(string fileName)
    {
        try
        {
            using var reader = new iText.Kernel.Pdf.PdfReader(fileName);
            using var pdfDoc = new iText.Kernel.Pdf.PdfDocument(reader);

            var text = new StringBuilder();

            for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                var page = pdfDoc.GetPage(i);
                text.Append(iText.Kernel.Pdf.Canvas.Parser.PdfTextExtractor.GetTextFromPage(page));
                text.Append(' ');
            }

            return text.ToString().Trim();
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string ExtractTextFromWordDocument(Stream stream)
    {
        try
        {
            using var doc = WordprocessingDocument.Open(stream, false);
            return doc?.MainDocumentPart?.Document?.Body?.InnerText!;
        }
        catch
        {
            return string.Empty;
        }
    }
}
