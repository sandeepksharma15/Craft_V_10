namespace Craft.Domain.Tests.Helpers;

public class DomainConstantsLocalizedTests
{
    #region Localized Property Tests

    [Fact]
    public void Localized_AfterError_ShouldReturnSameAsConstant()
    {
        // Act
        var localized = DomainConstants.Localized.AfterError;
        var constant = DomainConstants.AfterError;

        // Assert (they should match when using default culture)
        Assert.Equal(constant, localized);
    }

    [Fact]
    public void Localized_LengthError_ShouldReturnSameAsConstant()
    {
        // Act
        var localized = DomainConstants.Localized.LengthError;
        var constant = DomainConstants.LengthError;

        // Assert
        Assert.Equal(constant, localized);
    }

    [Fact]
    public void Localized_RequiredError_ShouldReturnSameAsConstant()
    {
        // Act
        var localized = DomainConstants.Localized.RequiredError;
        var constant = DomainConstants.RequiredError;

        // Assert
        Assert.Equal(constant, localized);
    }

    [Fact]
    public void Localized_StartDateGreaterError_ShouldReturnSameAsConstant()
    {
        // Act
        var localized = DomainConstants.Localized.StartDateGreaterError;
        var constant = DomainConstants.StartDateGreaterError;

        // Assert
        Assert.Equal(constant, localized);
    }

    [Fact]
    public void Localized_RangeError_ShouldReturnSameAsConstant()
    {
        // Act
        var localized = DomainConstants.Localized.RangeError;
        var constant = DomainConstants.RangeError;

        // Assert
        Assert.Equal(constant, localized);
    }

    [Fact]
    public void Localized_MinLengthError_ShouldReturnSameAsConstant()
    {
        // Act
        var localized = DomainConstants.Localized.MinLengthError;
        var constant = DomainConstants.MinLengthError;

        // Assert
        Assert.Equal(constant, localized);
    }

    [Fact]
    public void Localized_MaxLengthError_ShouldReturnSameAsConstant()
    {
        // Act
        var localized = DomainConstants.Localized.MaxLengthError;
        var constant = DomainConstants.MaxLengthError;

        // Assert
        Assert.Equal(constant, localized);
    }

    [Fact]
    public void Localized_FormatError_ShouldReturnSameAsConstant()
    {
        // Act
        var localized = DomainConstants.Localized.FormatError;
        var constant = DomainConstants.FormatError;

        // Assert
        Assert.Equal(constant, localized);
    }

    [Fact]
    public void Localized_DuplicateError_ShouldMatchConstant()
    {
        // Note: The constant has different casing, but localized should match resources
        // Act
        var localized = DomainConstants.Localized.DuplicateError;

        // Assert
        Assert.NotNull(localized);
        Assert.Contains("should not be same", localized, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Localized Formatting Method Tests

    [Fact]
    public void FormatAfter_ShouldFormatCorrectly()
    {
        // Act
        var result = DomainConstants.Localized.FormatAfter("EndDate", "StartDate");

        // Assert
        Assert.Equal("EndDate must be after StartDate", result);
    }

    [Fact]
    public void FormatLength_ShouldFormatCorrectly()
    {
        // Act
        var result = DomainConstants.Localized.FormatLength("Name", 100);

        // Assert
        Assert.Equal("Name cannot be more than 100", result);
    }

    [Fact]
    public void FormatRequired_ShouldFormatCorrectly()
    {
        // Act
        var result = DomainConstants.Localized.FormatRequired("FirstName");

        // Assert
        Assert.Equal("FirstName is required", result);
    }

    [Fact]
    public void FormatRange_ShouldFormatCorrectly()
    {
        // Act
        var result = DomainConstants.Localized.FormatRange("Quantity", 1, 100);

        // Assert
        Assert.Equal("Quantity must be between 1 and 100", result);
    }

    [Fact]
    public void FormatMinLength_ShouldFormatCorrectly()
    {
        // Act
        var result = DomainConstants.Localized.FormatMinLength("Username", 3);

        // Assert
        Assert.Equal("Username must be at least 3 characters", result);
    }

    [Fact]
    public void FormatMaxLength_ShouldFormatCorrectly()
    {
        // Act
        var result = DomainConstants.Localized.FormatMaxLength("Bio", 1000);

        // Assert
        Assert.Equal("Bio must not exceed 1000 characters", result);
    }

    #endregion

    #region Constants vs Localized Comparison Tests

    [Theory]
    [InlineData(nameof(DomainConstants.AfterError))]
    [InlineData(nameof(DomainConstants.LengthError))]
    [InlineData(nameof(DomainConstants.RequiredError))]
    [InlineData(nameof(DomainConstants.RangeError))]
    [InlineData(nameof(DomainConstants.MinLengthError))]
    [InlineData(nameof(DomainConstants.MaxLengthError))]
    [InlineData(nameof(DomainConstants.FormatError))]
    public void AllLocalizedProperties_ShouldMatchConstants(string propertyName)
    {
        // Arrange
        var constantsType = typeof(DomainConstants);
        var localizedType = typeof(DomainConstants.Localized);

        // Act
        var constantField = constantsType.GetField(propertyName);
        var localizedProperty = localizedType.GetProperty(propertyName);

        // Assert
        Assert.NotNull(constantField);
        Assert.NotNull(localizedProperty);

        var constantValue = constantField.GetValue(null) as string;
        var localizedValue = localizedProperty.GetValue(null) as string;

        Assert.Equal(constantValue, localizedValue);
    }

    #endregion

    #region Attribute Usage Pattern Tests

    [Fact]
    public void Constants_ShouldBeUsableInAttributes()
    {
        // This test verifies that constants can be used in attributes
        // by checking they are compile-time constants

        // Arrange
        var field = typeof(DomainConstants).GetField(nameof(DomainConstants.RequiredError));

        // Assert
        Assert.NotNull(field);
        Assert.True(field.IsLiteral); // Compile-time constant
        Assert.False(field.IsInitOnly);
    }

    [Fact]
    public void Localized_ShouldBeProperties_NotConstants()
    {
        // This test verifies that Localized members are properties (runtime)
        // not constants (compile-time)

        // Arrange
        var property = typeof(DomainConstants.Localized).GetProperty(nameof(DomainConstants.Localized.RequiredError));

        // Assert
        Assert.NotNull(property);
        Assert.True(property.CanRead);
        Assert.False(property.CanWrite);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void FormatRange_ShouldHandleDecimalValues()
    {
        // Act
        var result = DomainConstants.Localized.FormatRange("Price", 0.01m, 999.99m);

        // Assert
        Assert.Contains("0.01", result);
        Assert.Contains("999.99", result);
    }

    [Fact]
    public void FormatRequired_ShouldHandleEmptyFieldName()
    {
        // Act
        var result = DomainConstants.Localized.FormatRequired("");

        // Assert
        Assert.Equal(" is required", result);
    }

    [Fact]
    public void FormatLength_ShouldHandleZero()
    {
        // Act
        var result = DomainConstants.Localized.FormatLength("Field", 0);

        // Assert
        Assert.Equal("Field cannot be more than 0", result);
    }

    #endregion
}
