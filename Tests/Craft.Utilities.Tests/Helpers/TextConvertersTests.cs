using System;
using Xunit;
using Craft.Utilities.Helpers;

namespace Craft.Utilities.Tests.Helpers;

public class TextConvertersTests
{
    [Fact]
    public void ConvertMarkdownToRtf_NullOrEmpty_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentNullException>(() => TextConverters.ConvertMarkdownToRtf(null));
        Assert.Throws<ArgumentException>(() => TextConverters.ConvertMarkdownToRtf(""));
    }

    [Fact]
    public void ConvertMarkdownToRtf_BoldText_ConvertsToRtfBold()
    {
        var rtf = TextConverters.ConvertMarkdownToRtf("**bold**");
        Assert.Contains(@"\b ", rtf);
        Assert.Contains(@"\b0 ", rtf);
    }

    [Fact]
    public void ConvertMarkdownToRtf_ItalicText_ConvertsToRtfItalic()
    {
        var rtf = TextConverters.ConvertMarkdownToRtf("*italic*");
        Assert.Contains(@"\i ", rtf);
        Assert.Contains(@"\i0 ", rtf);
    }

    [Fact]
    public void ConvertMarkdownToRtf_Heading1_ConvertsToRtfHeading()
    {
        var rtf = TextConverters.ConvertMarkdownToRtf("# Heading 1");
        Assert.Contains(@"\b\fs36 ", rtf);
        Assert.Contains(@"\b0\fs24\par ", rtf);
    }

    [Fact]
    public void ConvertMarkdownToRtf_Paragraph_ConvertsToRtfParagraph()
    {
        var rtf = TextConverters.ConvertMarkdownToRtf("Paragraph\n\nAnother");
        Assert.Contains(@"\par ", rtf);
    }

    [Fact]
    public void ConvertMarkdownToRtf_List_ConvertsToRtfBullet()
    {
        var rtf = TextConverters.ConvertMarkdownToRtf("- item1\n- item2");
        Assert.Contains(@"\bullet\tab ", rtf);
    }

    [Fact]
    public void ConvertMarkdownToRtf_Hyperlink_ConvertsToRtfHyperlink()
    {
        var rtf = TextConverters.ConvertMarkdownToRtf("[mail](mailto:test@example.com)");
        Assert.Contains("HYPERLINK \"mailto:test@example.com\"", rtf);
        Assert.Contains("fldrslt mail", rtf);
    }

    [Fact]
    public void ConvertMarkdownToRtf_HandlesSpecialCharacters()
    {
        var rtf = TextConverters.ConvertMarkdownToRtf("&amp; &quot; – ---; &nbsp;");
        Assert.Contains("&", rtf);
        Assert.Contains("\"", rtf);
        Assert.Contains("\\'96", rtf); // en dash
        Assert.Contains(" ", rtf); // nbsp replaced by space
    }

    [Fact]
    public void ConvertMarkdownToRtf_MixedContent_AllowsMultipleFeatures()
    {
        var rtf = TextConverters.ConvertMarkdownToRtf("# Title\n\n**bold** and *italic* and [mail](mailto:test@example.com)\n- item");
        Assert.Contains(@"\b\fs36 ", rtf);
        Assert.Contains(@"\b ", rtf);
        Assert.Contains(@"\i ", rtf);
        Assert.Contains("HYPERLINK \"mailto:test@example.com\"", rtf);
        Assert.Contains(@"\bullet\tab ", rtf);
    }
}
