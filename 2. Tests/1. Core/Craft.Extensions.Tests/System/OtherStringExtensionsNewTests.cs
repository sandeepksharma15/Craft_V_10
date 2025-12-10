using Craft.Extensions.System;

namespace Craft.Extensions.Tests.System;

public class OtherStringExtensionsNewTests
{
    [Theory]
    [InlineData("hello world", 5, "hello")]
    [InlineData("hello", 10, "hello")]
    [InlineData("hello", 5, "hello")]
    [InlineData(null, 5, null)]
    [InlineData("", 5, "")]
    public void Truncate_ReturnsCorrectResult(string? input, int maxLength, string? expected)
    {
        // Act
        var result = input.Truncate(maxLength);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("hello world", 10, "...", "hello w...")]
    [InlineData("hello world", 8, "...", "hello...")]
    [InlineData("hello", 10, "...", "hello")]
    [InlineData("hello world", 2, "...", "he")]
    [InlineData(null, 10, "...", null)]
    public void TruncateWithEllipsis_ReturnsCorrectResult(string? input, int maxLength, string ellipsis, string? expected)
    {
        // Act
        var result = input.TruncateWithEllipsis(maxLength, ellipsis);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("hello world", "HelloWorld")]
    [InlineData("hello_world", "HelloWorld")]
    [InlineData("hello-world", "HelloWorld")]
    [InlineData("HELLO WORLD", "HelloWorld")]
    [InlineData("hello", "Hello")]
    [InlineData(null, null)]
    [InlineData("", "")]
    public void ToPascalCase_ReturnsCorrectResult(string? input, string? expected)
    {
        // Act
        var result = input.ToPascalCase();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("hello world", "helloWorld")]
    [InlineData("hello_world", "helloWorld")]
    [InlineData("hello-world", "helloWorld")]
    [InlineData("HELLO WORLD", "helloWorld")]
    [InlineData("Hello", "hello")]
    [InlineData(null, null)]
    [InlineData("", "")]
    public void ToCamelCase_ReturnsCorrectResult(string? input, string? expected)
    {
        // Act
        var result = input.ToCamelCase();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("HelloWorld", "hello_world")]
    [InlineData("helloWorld", "hello_world")]
    [InlineData("hello world", "hello_world")]
    [InlineData("hello-world", "hello_world")]
    [InlineData("hello", "hello")]
    [InlineData(null, null)]
    [InlineData("", "")]
    public void ToSnakeCase_ReturnsCorrectResult(string? input, string? expected)
    {
        // Act
        var result = input.ToSnakeCase();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("HelloWorld", "hello-world")]
    [InlineData("helloWorld", "hello-world")]
    [InlineData("hello world", "hello-world")]
    [InlineData("hello_world", "hello-world")]
    [InlineData("hello", "hello")]
    [InlineData(null, null)]
    [InlineData("", "")]
    public void ToKebabCase_ReturnsCorrectResult(string? input, string? expected)
    {
        // Act
        var result = input.ToKebabCase();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("hello", "olleh")]
    [InlineData("abc", "cba")]
    [InlineData("a", "a")]
    [InlineData("", "")]
    [InlineData(null, null)]
    public void Reverse_ReturnsCorrectResult(string? input, string? expected)
    {
        // Act
        var result = input.Reverse();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("hello world hello", "hello", StringComparison.Ordinal, 2)]
    [InlineData("hello world", "hello", StringComparison.Ordinal, 1)]
    [InlineData("hello world", "xyz", StringComparison.Ordinal, 0)]
    [InlineData("HELLO hello HeLLo", "hello", StringComparison.OrdinalIgnoreCase, 3)]
    [InlineData(null, "hello", StringComparison.Ordinal, 0)]
    [InlineData("hello", null, StringComparison.Ordinal, 0)]
    public void CountOccurrences_ReturnsCorrectCount(string? input, string? substring, StringComparison comparison, int expected)
    {
        // Act
        var result = input.CountOccurrences(substring!, comparison);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("12345", true)]
    [InlineData("123abc", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("0", true)]
    public void IsNumeric_ReturnsCorrectResult(string? input, bool expected)
    {
        // Act
        var result = input.IsNumeric();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("hello", true)]
    [InlineData("Hello", true)]
    [InlineData("hello123", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsAlphabetic_ReturnsCorrectResult(string? input, bool expected)
    {
        // Act
        var result = input.IsAlphabetic();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("hello123", true)]
    [InlineData("Hello", true)]
    [InlineData("12345", true)]
    [InlineData("hello_123", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsAlphanumeric_ReturnsCorrectResult(string? input, bool expected)
    {
        // Act
        var result = input.IsAlphanumeric();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void TruncateWithEllipsis_WithCustomEllipsis_Works()
    {
        // Arrange
        var input = "hello world";

        // Act
        var result = input.TruncateWithEllipsis(10, "---");

        // Assert
        Assert.Equal("hello w---", result);
    }

    [Fact]
    public void CountOccurrences_WithOverlappingMatches_CountsCorrectly()
    {
        // Arrange
        var input = "aaaa";

        // Act
        var result = input.CountOccurrences("aa");

        // Assert
        Assert.Equal(2, result);
    }

    [Theory]
    [InlineData("HelloWorld123", "hello_world123")]
    [InlineData("HTTPSConnection", "httpsconnection")]
    [InlineData("IOError", "ioerror")]
    public void ToSnakeCase_WithAcronyms_HandlesCorrectly(string input, string expected)
    {
        // Act
        var result = input.ToSnakeCase();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("HelloWorld123", "hello-world123")]
    [InlineData("HTTPSConnection", "httpsconnection")]
    public void ToKebabCase_WithAcronyms_HandlesCorrectly(string input, string expected)
    {
        // Act
        var result = input.ToKebabCase();

        // Assert
        Assert.Equal(expected, result);
    }
}
