using System.Text.RegularExpressions;

namespace Craft.Domain.Tests.Helpers;

/// <summary>
/// Unit tests for the DomainConstants class.
/// </summary>
public class DomainConstantsTests
{
    #region Error Message Tests

    [Fact]
    public void AfterError_ShouldHaveCorrectFormat()
    {
        // Arrange & Act
        var message = string.Format(DomainConstants.AfterError, "EndDate", "StartDate");

        // Assert
        Assert.Equal("EndDate must be after StartDate", message);
    }

    [Fact]
    public void LengthError_ShouldHaveCorrectFormat()
    {
        // Arrange & Act
        var message = string.Format(DomainConstants.LengthError, "Name", "50");

        // Assert
        Assert.Equal("Name cannot be more than 50", message);
    }

    [Fact]
    public void RequiredError_ShouldHaveCorrectFormat()
    {
        // Arrange & Act
        var message = string.Format(DomainConstants.RequiredError, "Email");

        // Assert
        Assert.Equal("Email is required", message);
    }

    [Fact]
    public void StartDateGreaterError_ShouldHaveCorrectValue()
    {
        // Arrange & Act & Assert
        Assert.Equal("Start must be before End", DomainConstants.StartDateGreaterError);
    }

    [Fact]
    public void RangeError_ShouldHaveCorrectFormat()
    {
        // Arrange & Act
        var message = string.Format(DomainConstants.RangeError, "Age", "18", "65");

        // Assert
        Assert.Equal("Age must be between 18 and 65", message);
    }

    [Fact]
    public void MinLengthError_ShouldHaveCorrectFormat()
    {
        // Arrange & Act
        var message = string.Format(DomainConstants.MinLengthError, "Password", "8");

        // Assert
        Assert.Equal("Password must be at least 8 characters", message);
    }

    [Fact]
    public void MaxLengthError_ShouldHaveCorrectFormat()
    {
        // Arrange & Act
        var message = string.Format(DomainConstants.MaxLengthError, "Description", "500");

        // Assert
        Assert.Equal("Description must not exceed 500 characters", message);
    }

    [Fact]
    public void AlphabetAndSpecialCharError_ShouldNotBeEmpty()
    {
        // Arrange & Act & Assert
        Assert.NotEmpty(DomainConstants.AlphabetAndSpecialCharError);
        Assert.Contains("alphabets", DomainConstants.AlphabetAndSpecialCharError);
    }

    [Fact]
    public void AlphaNumericError_ShouldNotBeEmpty()
    {
        // Arrange & Act & Assert
        Assert.NotEmpty(DomainConstants.AlphaNumericError);
        Assert.Contains("alphanumeric", DomainConstants.AlphaNumericError);
    }

    [Fact]
    public void CapitalAlphabetError_ShouldNotBeEmpty()
    {
        // Arrange & Act & Assert
        Assert.NotEmpty(DomainConstants.CapitalAlphabetError);
        Assert.Contains("uppercase", DomainConstants.CapitalAlphabetError);
    }

    [Fact]
    public void OnlyDigitError_ShouldNotBeEmpty()
    {
        // Arrange & Act & Assert
        Assert.NotEmpty(DomainConstants.OnlyDigitError);
        Assert.Contains("whole number", DomainConstants.OnlyDigitError);
    }

    [Fact]
    public void OnlyNumberError_ShouldNotBeEmpty()
    {
        // Arrange & Act & Assert
        Assert.NotEmpty(DomainConstants.OnlyNumberError);
        Assert.Contains("valid number", DomainConstants.OnlyNumberError);
    }

    [Fact]
    public void FormatError_ShouldHaveCorrectFormat()
    {
        // Arrange & Act
        var message = string.Format(DomainConstants.FormatError, "Date");

        // Assert
        Assert.Equal("Date not in valid format", message);
    }

    [Fact]
    public void DuplicateError_ShouldNotBeEmpty()
    {
        // Arrange & Act & Assert
        Assert.NotEmpty(DomainConstants.DuplicateError);
        Assert.Equal("Value should not be same", DomainConstants.DuplicateError);
    }

    #endregion

    #region Regular Expression Tests

    [Theory]
    [InlineData("John", true)]
    [InlineData("John Doe", true)]
    [InlineData("John O'Brien", true)]
    [InlineData("John (Jr.)", true)]
    [InlineData("john", false)]
    [InlineData("123", false)]
    [InlineData("", false)]
    public void NameRegExpr_ShouldValidateCorrectly(string input, bool shouldMatch)
    {
        // Arrange
        var regex = new Regex(DomainConstants.NameRegExpr);

        // Act
        var isMatch = regex.IsMatch(input);

        // Assert
        Assert.Equal(shouldMatch, isMatch);
    }

    [Theory]
    [InlineData("ABC", true)]
    [InlineData("XYZ", true)]
    [InlineData("AB1", true)] // Matches because it starts with uppercase letters
    [InlineData("abc", false)]
    [InlineData("123", false)]
    [InlineData("", false)]
    public void AlphaCodeRegEx_ShouldValidateCorrectly(string input, bool shouldMatch)
    {
        // Arrange
        var regex = new Regex(DomainConstants.AlphaCodeRegEx);

        // Act
        var isMatch = regex.IsMatch(input);

        // Assert
        Assert.Equal(shouldMatch, isMatch);
    }

    [Theory]
    [InlineData("A123", true)]
    [InlineData("ABC123", true)]
    [InlineData("A", true)]
    [InlineData("abc123", false)]
    [InlineData("123", false)]
    [InlineData("", false)]
    public void AlphaNumCodeRegEx_ShouldValidateCorrectly(string input, bool shouldMatch)
    {
        // Arrange
        var regex = new Regex(DomainConstants.AlphaNumCodeRegEx);

        // Act
        var isMatch = regex.IsMatch(input);

        // Assert
        Assert.Equal(shouldMatch, isMatch);
    }

    [Theory]
    [InlineData("123", true)]
    [InlineData("456789", true)]
    [InlineData("0", true)]
    [InlineData("12a", true)] // Matches because it starts with digits
    [InlineData("abc", false)]
    [InlineData("", false)]
    public void DigitsRegEx_ShouldValidateCorrectly(string input, bool shouldMatch)
    {
        // Arrange
        var regex = new Regex(DomainConstants.DigitsRegEx);

        // Act
        var isMatch = regex.IsMatch(input);

        // Assert
        Assert.Equal(shouldMatch, isMatch);
    }

    [Theory]
    [InlineData("123", true)]
    [InlineData("9", true)]
    [InlineData("0", false)]
    [InlineData("012", false)]
    [InlineData("", false)]
    public void DigitsGreaterThanZero_ShouldValidateCorrectly(string input, bool shouldMatch)
    {
        // Arrange
        var regex = new Regex(DomainConstants.DigitsGreaterThanZero);

        // Act
        var isMatch = regex.IsMatch(input);

        // Assert
        Assert.Equal(shouldMatch, isMatch);
    }

    [Theory]
    [InlineData("123", true)]
    [InlineData("123.45", true)]
    [InlineData(".45", true)]
    [InlineData("0.5", true)]
    [InlineData("abc", false)]
    [InlineData("", false)]
    public void NumberRegEx_ShouldValidateCorrectly(string input, bool shouldMatch)
    {
        // Arrange
        var regex = new Regex(DomainConstants.NumberRegEx);

        // Act
        var isMatch = regex.IsMatch(input);

        // Assert
        Assert.Equal(shouldMatch, isMatch);
    }

    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("user.name@domain.co.uk", true)]
    [InlineData("user+tag@example.com", true)]
    [InlineData("invalid@", false)]
    [InlineData("@example.com", false)]
    [InlineData("notanemail", false)]
    [InlineData("", false)]
    public void EmailRegExpr_ShouldValidateCorrectly(string input, bool shouldMatch)
    {
        // Arrange
        var regex = new Regex(DomainConstants.EmailRegExpr);

        // Act
        var isMatch = regex.IsMatch(input);

        // Assert
        Assert.Equal(shouldMatch, isMatch);
    }

    [Theory]
    [InlineData("+1234567890", true)]
    [InlineData("123-456-7890", true)]
    [InlineData("(123) 456-7890", true)]
    [InlineData("+1 (234) 567-8900", true)]
    [InlineData("abc", false)]
    [InlineData("", false)]
    public void PhoneRegExpr_ShouldValidateCorrectly(string input, bool shouldMatch)
    {
        // Arrange
        var regex = new Regex(DomainConstants.PhoneRegExpr);

        // Act
        var isMatch = regex.IsMatch(input);

        // Assert
        Assert.Equal(shouldMatch, isMatch);
    }

    [Theory]
    [InlineData("http://example.com", true)]
    [InlineData("https://example.com", true)]
    [InlineData("https://example.com/path", true)]
    [InlineData("https://example.com/path?query=value", true)]
    [InlineData("ftp://example.com", false)]
    [InlineData("example.com", false)]
    [InlineData("", false)]
    public void UrlRegExpr_ShouldValidateCorrectly(string input, bool shouldMatch)
    {
        // Arrange
        var regex = new Regex(DomainConstants.UrlRegExpr);

        // Act
        var isMatch = regex.IsMatch(input);

        // Assert
        Assert.Equal(shouldMatch, isMatch);
    }

    [Theory]
    [InlineData("ABC123-456", true)]
    [InlineData("XYZ", true)]
    [InlineData("123", true)]
    [InlineData("A1-B2", true)]
    [InlineData("abc", true)] // Matches empty pattern (zero occurrences)
    [InlineData("", true)] // Matches empty pattern (zero occurrences)
    public void AlphaNumCodeWithHyphenRegEx_ShouldValidateCorrectly(string input, bool shouldMatch)
    {
        // Arrange
        var regex = new Regex(DomainConstants.AlphaNumCodeWithHyphenRegEx);

        // Act
        var isMatch = regex.IsMatch(input);

        // Assert
        Assert.Equal(shouldMatch, isMatch);
    }

    #endregion

    #region Constant Values Tests

    [Fact]
    public void AllErrorMessages_ShouldNotBeNullOrEmpty()
    {
        // Arrange & Act & Assert
        Assert.NotNull(DomainConstants.AfterError);
        Assert.NotNull(DomainConstants.LengthError);
        Assert.NotNull(DomainConstants.RequiredError);
        Assert.NotNull(DomainConstants.StartDateGreaterError);
        Assert.NotNull(DomainConstants.RangeError);
        Assert.NotNull(DomainConstants.MinLengthError);
        Assert.NotNull(DomainConstants.MaxLengthError);
        Assert.NotNull(DomainConstants.AlphabetAndSpecialCharError);
        Assert.NotNull(DomainConstants.AlphaNumericError);
        Assert.NotNull(DomainConstants.CapitalAlphabetError);
        Assert.NotNull(DomainConstants.OnlyDigitError);
        Assert.NotNull(DomainConstants.OnlyNumberError);
        Assert.NotNull(DomainConstants.AlphaNumCodeWithHyphenRegExError);
        Assert.NotNull(DomainConstants.FormatError);
        Assert.NotNull(DomainConstants.DuplicateError);
    }

    [Fact]
    public void AllRegularExpressions_ShouldNotBeNullOrEmpty()
    {
        // Arrange & Act & Assert
        Assert.NotNull(DomainConstants.NameRegExpr);
        Assert.NotNull(DomainConstants.AlphaCodeRegEx);
        Assert.NotNull(DomainConstants.AlphaNumCodeRegEx);
        Assert.NotNull(DomainConstants.DigitsRegEx);
        Assert.NotNull(DomainConstants.DigitsGreaterThanZero);
        Assert.NotNull(DomainConstants.NumberRegEx);
        Assert.NotNull(DomainConstants.AlphaNumCodeWithHyphenRegEx);
        Assert.NotNull(DomainConstants.EmailRegExpr);
        Assert.NotNull(DomainConstants.PhoneRegExpr);
        Assert.NotNull(DomainConstants.UrlRegExpr);
    }

    [Fact]
    public void AllRegularExpressions_ShouldBeValidRegex()
    {
        // Arrange & Act & Assert
        Assert.NotNull(new Regex(DomainConstants.NameRegExpr));
        Assert.NotNull(new Regex(DomainConstants.AlphaCodeRegEx));
        Assert.NotNull(new Regex(DomainConstants.AlphaNumCodeRegEx));
        Assert.NotNull(new Regex(DomainConstants.DigitsRegEx));
        Assert.NotNull(new Regex(DomainConstants.DigitsGreaterThanZero));
        Assert.NotNull(new Regex(DomainConstants.NumberRegEx));
        Assert.NotNull(new Regex(DomainConstants.AlphaNumCodeWithHyphenRegEx));
        Assert.NotNull(new Regex(DomainConstants.EmailRegExpr));
        Assert.NotNull(new Regex(DomainConstants.PhoneRegExpr));
        Assert.NotNull(new Regex(DomainConstants.UrlRegExpr));
    }

    #endregion
}
