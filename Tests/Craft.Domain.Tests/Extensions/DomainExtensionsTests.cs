namespace Craft.Domain.Tests.Extensions;

public class DomainExtensionsTests
{
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
}
