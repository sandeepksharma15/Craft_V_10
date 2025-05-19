namespace Craft.Extensions.Tests.System;

public class StandardStringExtensionsTests
{
    [Theory]
    [InlineData("abc", 'c', StringComparison.Ordinal, "abc")] // No change expected
    [InlineData("Hello", 'o', StringComparison.OrdinalIgnoreCase, "Hello")] // No change with case-insensitive comparison
    [InlineData("world", '!', StringComparison.Ordinal, "world!")] // Append character
    [InlineData("", 'a', StringComparison.Ordinal, "a")] // Empty string, append character
    [InlineData(null, 'x', StringComparison.Ordinal, null)] // Null string, no change expected
    public void EnsureEndsWith_ShouldEnsureCorrectEnding(string? source, char character,
        StringComparison comparisonType, string? expected)
    {
        // Act
        var result = source!.EnsureEndsWith(character, comparisonType);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("abc", 'a', StringComparison.Ordinal, "abc")] // No change expected
    [InlineData("world", 'h', StringComparison.OrdinalIgnoreCase, "hworld")] // Prepend character
    [InlineData("123", '0', StringComparison.Ordinal, "0123")] // Prepend character
    [InlineData("", 'a', StringComparison.Ordinal, "a")] // Empty string, prepend character
    [InlineData(null, 'x', StringComparison.Ordinal, null)] // Null string, no change expected
    public void EnsureStartsWith_ShouldEnsureCorrectStarting(string? source, char character,
        StringComparison comparisonType, string? expected)
    {
        // Act
        var result = source!.EnsureStartsWith(character, comparisonType);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("hello", "Hello")] // Normal case
    [InlineData("WORLD", "WORLD")] // All uppercase, no change expected
    [InlineData("123", "123")] // Numeric string, no change expected
    [InlineData("", "")] // Empty string, no change expected
    [InlineData(null, null)] // Null string, no change expected
    public void FirstCharToUpper_ShouldConvertFirstCharToUppercase(string? input, string? expected)
    {
        // Act
        var result = input!.FirstCharToUpper();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("example.file.txt", '.', "txt")]
    [InlineData("path/to/some/file.txt", '/', "file.txt")]
    [InlineData(null, '.', null)]
    [InlineData("no_delimiter", '.', "no_delimiter")]
    [InlineData("delimiter_at_the_end.", '.', "")]
    public void GetStringAfterLastDelimiter_WithVariousInputs_ReturnsExpectedResult(string? input, char delimiter, string? expected)
    {
        // Act
        string? result = input!.GetStringAfterLastDelimiter(delimiter);

        // Assert
        Assert.Equal(expected, result); 
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("   ", true)]
    [InlineData("abc", false)]
    [InlineData("  abc  ", false)]
    [InlineData("\t", true)]
    public void IsEmpty_ShouldReturnCorrectResult(string? input, bool expectedResult)
    {
        // Act
        var result = input!.IsEmpty();

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("abc", true)]
    [InlineData("  abc  ", true)]
    [InlineData("\t", false)]
    public void IsNonEmpty_ShouldReturnCorrectResult(string? input, bool expectedResult)
    {
        // Act
        var result = input!.IsNonEmpty();

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("   ", false)]
    [InlineData("abc", false)]
    [InlineData("  abc  ", false)]
    public void IsNullOrEmpty_ShouldReturnCorrectResult(string? input, bool expectedResult)
    {
        // Act
        var result = input!.IsNullOrEmpty();

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("   ", true)]
    [InlineData("abc", false)]
    [InlineData("  abc  ", false)]
    public void IsNullOrWhiteSpace_ShouldReturnCorrectResult(string? input, bool expectedResult)
    {
        // Act
        var result = input!.IsNullOrWhiteSpace();

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(null, 5, null)] // Null source should return null
    [InlineData("", 5, "")] // Empty source should return empty string
    [InlineData("abcdef", 0, "")] // Length 0 should return empty string
    [InlineData("abcdef", 5, "abcde")] // Length less than source length should return the left substring
    [InlineData("abcdef", 10, "abcdef")] // Length greater than or equal to source length should return the source string
    public void Left_ShouldReturnExpectedResult(string? source, int len, string? expectedResult)
    {
        // Act
        var result = source!.Left(len);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("123", 123)]
    [InlineData("456.78", 456.78)]
    [InlineData("0", 0)]
    [InlineData("-789", -789)]
    [InlineData("3.14", 3.14)]
    public void Parse_PositiveTestCases_ReturnsExpectedResult(string? input, float expectedResult)
    {
        // Act
        var result = input!.Parse<float>();

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    public void Parse_ShouldReturnExpectedValue(string? input, object? expected)
    {
        // Act
        var result = input!.Parse<object>();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, default(int))]
    [InlineData("123", 123)]
    [InlineData("abc", default(int))] // Parsing non-numeric value to int should return default
    [InlineData("456.78", default(int))] // Parsing non-integer value to int should return default
    public void Parse_ValidString_ReturnsExpectedResult(string? input, int expectedResult)
    {
        // Act
        var result = input!.Parse<int>();

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(null, 5, null)] // Null source should return null
    [InlineData("", 5, "")] // Empty source should return empty string
    [InlineData("abcdef", 0, "")] // Length 0 should return empty string
    [InlineData("abcdef", 5, "bcdef")] // Length less than source length should return the right substring
    [InlineData("abcdef", 10, "abcdef")] // Length greater than or equal to source length should return the source string
    public void Right_ShouldReturnExpectedResult(string? source, int len, string? expectedResult)
    {
        // Act
        var result = source!.Right(len);

        // Assert
        Assert.Equal(expectedResult, result);
    }
}
