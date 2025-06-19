using System.Text;
using System.Text.RegularExpressions;
using Markdig;

namespace Craft.Utilities.Helpers;

public static class TextConverters
{
    public static string? ConvertMarkdownToRtf(string? markdown)
    {
        ArgumentException.ThrowIfNullOrEmpty(markdown, nameof(markdown));

        // UTF-8 Text May be Misinterpreted as Windows-1252
        byte[] bytes = Encoding.Default.GetBytes(markdown);
        string correctedText = Encoding.UTF8.GetString(bytes);

        // Convert To HTML
        string html = Markdown.ToHtml(correctedText);

        // Return RTF After Simple Conversion
        return ConvertHtmlToRtf(html);
    }

    private static string ConvertHtmlToRtf(string html)
    {
        // Simple HTML-to-RTF conversion (basic)
        string rtf = html
            .Replace("<b>", @"\b ")  // Bold start
            .Replace("</b>", @"\b0 ")  // Bold end
            .Replace("<strong>", @"\b ")  // Bold start
            .Replace("</strong>", @"\b0 ")  // Bold end
            .Replace("<i>", @"\i ")  // Italics start
            .Replace("</i>", @"\i0 ")  // Italics end
            .Replace("<br>", @"\line ")  // Line break
            .Replace("<br />", @"\line ")  // Line break
            .Replace("<p>", @"\par ")  // Paragraph start
            .Replace("</p>", @"\par ")  // Paragraph end
            .Replace("<h1>", @"\b\fs36 ")
            .Replace("</h1>", @"\b0\fs24\par ")
            .Replace("<h2>", @"\b\fs32 ")
            .Replace("</h2>", @"\b0\fs24\par ")
            .Replace("<h3>", @"\b\fs28 ")
            .Replace("</h3>", @"\b0\fs24\par ")
            .Replace("<h4>", @"\b\fs24 ")
            .Replace("</h4>", @"\b0\fs24\par ")
            .Replace("<em>", @"\i ")
            .Replace("</em>", @"\i0 ")
            .Replace("<li>", @"\bullet\tab ")
            .Replace("</li>", @"\par ")
            .Replace("<ul>", "")
            .Replace("</ul>", "")
            .Replace("<hr />", @"\par ")
            .Replace("–", @"\'96")
            .Replace("&quot;", "\"")
            .Replace("&amp;", "&")
            .Replace("---;", " ")       // May fix Right Align Issue (May introduce new ;-) )
            .Replace("&nbsp;", " ");

        // Replace <a href="mailto:email@example.com">text</a> with RTF format
        string pattern = @"<a\s+href=""mailto:(.*?)"">(.*?)<\/a>";
        string replacement = @"{\field{\*\fldinst{HYPERLINK ""mailto:$1""}}{\fldrslt $2}}";

        // Use regular expression to replace
        string correctedText = Regex.Replace(rtf, pattern, replacement);

        // Wrap in RTF structure
        return $@"{{\rtf1\ansi\deff0 {{\tx360 {correctedText}}}}}";
    }
}
