using System.Text.RegularExpressions;

namespace Craft.Extensions.Tests.System;

public class RegexExtensionsTests
{
    [Theory]
    [InlineData("Test123", @"\d+", true)]     // Positive match
    [InlineData("NoDigits", @"\d+", false)]   // No match
    [InlineData(null, @"\d+", false)]         // Null source
    public void RegexMatch_II_ShouldReturnExpectedResult(string? source, string? pattern, bool expectedSuccess)
    {
        // Arrange
        var regex = new Regex(pattern!);

        // Act
        var result = source!.RegexMatch(regex);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedSuccess, result.Success);
    }

    [Theory]
    [InlineData("Input123", "Input\\d+", true)]   // Valid match case
    [InlineData("Test123", "\\d{3}", true)]        // Valid match case
    [InlineData("NoMatch", "\\d+", false)]         // No match case
    [InlineData(null, "\\d+", false)]               // Null input case
    [InlineData("Input123", null, false)]           // Null regex pattern case
    public void RegexMatch_ShouldReturnExpectedResult(string? input, string? pattern, bool expectedResult)
    {
        // Arrange
        Regex? regex = pattern != null
            ? new Regex(pattern)
            : null;

        // Act
        Match result = regex!.RegexMatch(input!);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResult, result.Success);
    }
}
