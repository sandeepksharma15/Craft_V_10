using System.Text;
using DocumentFormat.OpenXml.Packaging;

namespace Craft.Utilities.Helpers;

public static class TextExtractor
{
    /// <summary>
    /// Extracts text content from a PDF, DOC, or DOCX file.
    /// </summary>
    /// <param name="fileName">The path to the file.</param>
    /// <returns>The extracted text, or an empty string if extraction fails.</returns>
    /// <exception cref="ArgumentException">Thrown if fileName is null or empty.</exception>
    /// <exception cref="NotSupportedException">Thrown if file extension is not supported.</exception>
    /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
    public static string ExtractTextFromDocOrPdf(string fileName)
    {
        ArgumentException.ThrowIfNullOrEmpty(fileName);

        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        if (extension != ".pdf" && extension != ".doc" && extension != ".docx")
            throw new NotSupportedException("Only PDF, DOC and DOCX files are supported");

        if (!File.Exists(fileName))
            throw new FileNotFoundException("The file does not exist", fileName);

        using (var stream = File.OpenRead(fileName))
        {
            return extension == ".pdf" ? ExtractTextFromPdf(stream) : ExtractTextFromWordDocument(stream);
        }
    }

    private static string ExtractTextFromWordDocument(Stream stream)
    {
        try
        {
            using var document = WordprocessingDocument.Open(stream, false);

            var body = document?.MainDocumentPart?.Document.Body;

            if (body == null)
                return string.Empty;

            // Concatenate all text nodes for robustness
            var text = new StringBuilder();

            foreach (var textElement in body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>())
            {
                text.Append(textElement.Text);
                text.Append(' ');
            }

            return text.ToString().Trim();
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string ExtractTextFromPdf(Stream stream)
    {
        try
        {
            using var reader = new iText.Kernel.Pdf.PdfReader(stream);
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
}
