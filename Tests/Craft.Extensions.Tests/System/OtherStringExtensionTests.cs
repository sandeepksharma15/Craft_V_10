using Craft.Extensions.System;

namespace Craft.Extensions.Tests.System;

public class OtherStringExtensionTests
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("Test\r\nString", "Test\r\nString")]
    [InlineData("Multi\rLine\r\nText", "Multi\r\nLine\r\nText")]
    [InlineData("Carriage\rReturn", "Carriage\r\nReturn")]
    [InlineData("Line\nFeed", "Line\r\nFeed")]
    [InlineData("Mixed\r\n\rLine\nEndings", "Mixed\r\n\r\nLine\r\nEndings")]
    public void NormalizeLineEndings_ShouldNormalizeLineEndings(string? input, string? expectedOutput)
    {
        // Arrange
        // Act
        var result = input!.NormalizeLineEndings();

        // Assert
        Assert.Equal(expectedOutput, result);
    }

    [Theory]
    [InlineData("abcdef", 'c', 1, 2)]       // Second occurrence of 'c' is at index 2
    [InlineData("abcabc", 'c', 2, 5)]        // Third occurrence of 'c' is at index 4
    [InlineData("abcabc", 'd', 1, -1)]       // 'd' not found in the string
    [InlineData("abcabc", 'a', 1, 0)]        // First occurrence of 'a' is at index 0
    [InlineData("abcabc", 'a', 2, 3)]        // Second occurrence of 'a' is at index 3
    [InlineData("abcabc", 'a', 3, -1)]       // Third occurrence of 'a' not found
    [InlineData("abcabc", 'x', 1, -1)]       // 'x' not found in the string
    [InlineData("", 'a', 1, -1)]             // Empty string, character not found
    [InlineData(null, 'a', 1, -1)]           // Null string, character not found
    public void NthIndexOf_ShouldReturnExpectedIndex(string? input, char character, int occurrence, int expectedIndex)
    {
        // Arrange
        // Act
        int result = input!.NthIndexOf(character, occurrence);

        // Assert
        Assert.Equal(expectedIndex, result);
    }

    [Theory]
    [InlineData(null, null, null)]
    [InlineData("test", null, "test")]
    [InlineData(null, new string[] { "abc" }, null)]
    [InlineData("source", new string[] { }, "source")]
    [InlineData("apple,orange,apple,banana", new string[] { "apple", "banana" }, ",orange,,")]
    [InlineData("abcdef", new string[] { "abc", "def" }, "")]
    [InlineData("", new string[] { "abc" }, "")]
    public void RemoveAll_ShouldRemoveStringsCorrectly(string? source, string[]? stringsToRemove, string? expectedResult)
    {
        // Act
        var result = source.RemoveAll(stringsToRemove);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void RemoveExtraSpaces_NullString_ReturnsNull()
    {
        // Arrange
        const string? input = null;

        // Act
        var result = input!.RemoveExtraSpaces();

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("  single   space  ", "single space")]
    [InlineData("  multiple   spaces   between   words  ", "multiple spaces between words")]
    [InlineData("   leading   spaces", "leading spaces")]
    [InlineData("trailing   spaces   ", "trailing spaces")]
    [InlineData("   leading   and   trailing   spaces   ", "leading and trailing spaces")]
    public void RemoveExtraSpaces_ValidInput_ReturnsExpectedResult(string? input, string? expectedResult)
    {
        // Act
        var result = input!.RemoveExtraSpaces();

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("test123", "test", "123", "456", "789")]
    [InlineData("example.txt", "example", ".txt", ".TXT")]
    [InlineData("hello", "hello", "world")]
    [InlineData("", "", "suffix")]
    [InlineData(null, null, "suffix")]
    [InlineData("caseSensitive", "caseSen", "sitive")]
    public void RemovePostFix_II_ShouldRemovePostFixes(string? input, string? expected, params string[]? postfixes)
    {
        // Act
        var result = input.RemovePostFix(postfixes);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("test123", "test", StringComparison.Ordinal, "123", "456", "789")]
    [InlineData("example.txt", "example", StringComparison.OrdinalIgnoreCase, ".txt", ".TXT")]
    [InlineData("hello", "hello", StringComparison.Ordinal, "world")]
    [InlineData("", "", StringComparison.Ordinal, "suffix")]
    [InlineData(null, null, StringComparison.Ordinal, "suffix")]
    [InlineData("caseSensitive", "caseSen", StringComparison.Ordinal, "sitive")]
    public void RemovePostFix_ShouldRemovePostFixes(string? input, string? expected, StringComparison comparisonType, params string[]? postfixes)
    {
        // Act
        var result = input.RemovePostFix(comparisonType, postfixes);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("test123", "123", "test")]
    [InlineData("example.txt", ".txt", "example", ".TXT")]
    [InlineData("hello", "hello", "world")]
    [InlineData("", "", "suffix")]
    [InlineData(null, null, "suffix")]
    [InlineData("caseSensitive", "sitive", "caseSen")]
    public void RemovePreFix_II_ShouldRemovePreFixes(string? input, string? expected, params string[]? prefixes)
    {
        // Act
        var result = input?.RemovePreFix(prefixes!);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("test123", "123", StringComparison.Ordinal, "test")]
    [InlineData("example.txt", ".txt", StringComparison.OrdinalIgnoreCase, "example", ".TXT")]
    [InlineData("hello", "hello", StringComparison.Ordinal, "world")]
    [InlineData("", "", StringComparison.Ordinal, "prefix")]
    [InlineData(null, null, StringComparison.Ordinal, "prefix")]
    [InlineData("caseSensitive", "sitive", StringComparison.Ordinal, "caseSen")]
    public void RemovePreFix_ShouldRemovePreFixes(string? input, string? expected, StringComparison comparisonType, params string[]? prefixes)
    {
        // Act
        var result = input.RemovePreFix(comparisonType, prefixes);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("hello world", "hello", "hi", StringComparison.Ordinal, "hi world")]
    [InlineData("test123test", "123", "456", StringComparison.OrdinalIgnoreCase, "test456test")]
    [InlineData("test123test", "789", "456", StringComparison.OrdinalIgnoreCase, "test123test")]
    [InlineData("example.txt", ".txt", ".TXT", StringComparison.Ordinal, "example.TXT")]
    [InlineData("empty", "", "replacement", StringComparison.Ordinal, "empty")]
    [InlineData(null, "search", "replace", StringComparison.OrdinalIgnoreCase, null)]
    [InlineData("", "search", "replace", StringComparison.OrdinalIgnoreCase, "")]
    public void ReplaceFirst_ShouldReplaceFirstOccurrence(string? input, string? search, string? replace,
        StringComparison comparisonType, string? expected)
    {
        // Act
        var result = input.ReplaceFirst(search, replace, comparisonType);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("", "D41D8CD98F00B204E9800998ECF8427E")]
    [InlineData("hello world", "5EB63BBBE01EEED093CB22BB8F5ACDC3")]
    [InlineData("a", "0CC175B9C0F1B6A831C399E269772661")]
    public void ToMd5_ShouldCalculateMd5Hash(string? input, string? expected)
    {
        // Act
        var result = input?.ToMd5();

        // Assert
        Assert.Equal(expected, result);
    }
}
