namespace Craft.Core.Tests.Common;

public class ErrorTypeTests
{
    [Fact]
    public void ErrorType_None_HasValueZero()
    {
        // Assert
        Assert.Equal(0, (int)ErrorType.None);
    }

    [Fact]
    public void ErrorType_Validation_HasValue1()
    {
        // Assert
        Assert.Equal(1, (int)ErrorType.Validation);
    }

    [Fact]
    public void ErrorType_NotFound_HasValue2()
    {
        // Assert
        Assert.Equal(2, (int)ErrorType.NotFound);
    }

    [Fact]
    public void ErrorType_Unauthorized_HasValue3()
    {
        // Assert
        Assert.Equal(3, (int)ErrorType.Unauthorized);
    }

    [Fact]
    public void ErrorType_Forbidden_HasValue4()
    {
        // Assert
        Assert.Equal(4, (int)ErrorType.Forbidden);
    }

    [Fact]
    public void ErrorType_Conflict_HasValue5()
    {
        // Assert
        Assert.Equal(5, (int)ErrorType.Conflict);
    }

    [Fact]
    public void ErrorType_Internal_HasValue6()
    {
        // Assert
        Assert.Equal(6, (int)ErrorType.Internal);
    }

    [Fact]
    public void ErrorType_Timeout_HasValue7()
    {
        // Assert
        Assert.Equal(7, (int)ErrorType.Timeout);
    }

    [Fact]
    public void ErrorType_HasExpectedNumberOfValues()
    {
        // Arrange
        var values = Enum.GetValues<ErrorType>();

        // Assert
        Assert.Equal(8, values.Length);
    }

    [Theory]
    [InlineData(ErrorType.None, "None")]
    [InlineData(ErrorType.Validation, "Validation")]
    [InlineData(ErrorType.NotFound, "NotFound")]
    [InlineData(ErrorType.Unauthorized, "Unauthorized")]
    [InlineData(ErrorType.Forbidden, "Forbidden")]
    [InlineData(ErrorType.Conflict, "Conflict")]
    [InlineData(ErrorType.Internal, "Internal")]
    [InlineData(ErrorType.Timeout, "Timeout")]
    public void ErrorType_ToString_ReturnsExpectedName(ErrorType errorType, string expectedName)
    {
        // Act
        var name = errorType.ToString();

        // Assert
        Assert.Equal(expectedName, name);
    }

    [Theory]
    [InlineData("None", ErrorType.None)]
    [InlineData("Validation", ErrorType.Validation)]
    [InlineData("NotFound", ErrorType.NotFound)]
    [InlineData("Unauthorized", ErrorType.Unauthorized)]
    [InlineData("Forbidden", ErrorType.Forbidden)]
    [InlineData("Conflict", ErrorType.Conflict)]
    [InlineData("Internal", ErrorType.Internal)]
    [InlineData("Timeout", ErrorType.Timeout)]
    public void ErrorType_Parse_ReturnsExpectedValue(string name, ErrorType expected)
    {
        // Act
        var parsed = Enum.Parse<ErrorType>(name);

        // Assert
        Assert.Equal(expected, parsed);
    }

    [Fact]
    public void ErrorType_TryParse_ReturnsFalseForInvalidValue()
    {
        // Act
        var success = Enum.TryParse<ErrorType>("InvalidError", out var result);

        // Assert
        Assert.False(success);
        Assert.Equal(default, result);
    }

    [Fact]
    public void ErrorType_IsDefined_ReturnsTrueForValidValues()
    {
        // Assert
        Assert.True(Enum.IsDefined(typeof(ErrorType), 0));
        Assert.True(Enum.IsDefined(typeof(ErrorType), 1));
        Assert.True(Enum.IsDefined(typeof(ErrorType), 7));
    }

    [Fact]
    public void ErrorType_IsDefined_ReturnsFalseForInvalidValues()
    {
        // Assert
        Assert.False(Enum.IsDefined(typeof(ErrorType), 99));
        Assert.False(Enum.IsDefined(typeof(ErrorType), -1));
    }

    [Fact]
    public void ErrorType_CanBeUsedInSwitch()
    {
        // Arrange
        var errorType = ErrorType.NotFound;

        // Act
        var result = errorType switch
        {
            ErrorType.None => "No error",
            ErrorType.Validation => "Validation error",
            ErrorType.NotFound => "Not found",
            ErrorType.Unauthorized => "Unauthorized",
            ErrorType.Forbidden => "Forbidden",
            ErrorType.Conflict => "Conflict",
            ErrorType.Internal => "Internal error",
            ErrorType.Timeout => "Timeout",
            _ => "Unknown"
        };

        // Assert
        Assert.Equal("Not found", result);
    }
}
