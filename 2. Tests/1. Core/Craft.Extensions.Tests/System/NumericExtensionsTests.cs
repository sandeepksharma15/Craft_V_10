namespace Craft.Extensions.Tests.System;

public class NumericExtensionsTests
{
    #region Clamp Tests

    [Theory]
    [InlineData(5, 0, 10, 5)]
    [InlineData(-5, 0, 10, 0)]
    [InlineData(15, 0, 10, 10)]
    [InlineData(0, 0, 10, 0)]
    [InlineData(10, 0, 10, 10)]
    public void Clamp_Int_ReturnsCorrectValue(int value, int min, int max, int expected)
    {
        // Act
        var result = value.Clamp(min, max);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(5.5, 0.0, 10.0, 5.5)]
    [InlineData(-5.5, 0.0, 10.0, 0.0)]
    [InlineData(15.5, 0.0, 10.0, 10.0)]
    public void Clamp_Decimal_ReturnsCorrectValue(double valueD, double minD, double maxD, double expectedD)
    {
        // Arrange
        var value = (decimal)valueD;
        var min = (decimal)minD;
        var max = (decimal)maxD;
        var expected = (decimal)expectedD;

        // Act
        var result = value.Clamp(min, max);

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion

    #region IsBetween Tests

    [Theory]
    [InlineData(5, 0, 10, true)]
    [InlineData(0, 0, 10, true)]
    [InlineData(10, 0, 10, true)]
    [InlineData(-1, 0, 10, false)]
    [InlineData(11, 0, 10, false)]
    public void IsBetween_Int_ReturnsCorrectResult(int value, int min, int max, bool expected)
    {
        // Act
        var result = value.IsBetween(min, max);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(5.5, 0.0, 10.0, true)]
    [InlineData(-0.1, 0.0, 10.0, false)]
    [InlineData(10.1, 0.0, 10.0, false)]
    public void IsBetween_Double_ReturnsCorrectResult(double value, double min, double max, bool expected)
    {
        // Act
        var result = value.IsBetween(min, max);

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion

    #region IsPositive Tests

    [Theory]
    [InlineData(5, true)]
    [InlineData(0, false)]
    [InlineData(-5, false)]
    public void IsPositive_Int_ReturnsCorrectResult(int value, bool expected)
    {
        // Act
        var result = value.IsPositive();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(5.5, true)]
    [InlineData(0.0, false)]
    [InlineData(-5.5, false)]
    public void IsPositive_Double_ReturnsCorrectResult(double value, bool expected)
    {
        // Act
        var result = value.IsPositive();

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion

    #region IsNegative Tests

    [Theory]
    [InlineData(-5, true)]
    [InlineData(0, false)]
    [InlineData(5, false)]
    public void IsNegative_Int_ReturnsCorrectResult(int value, bool expected)
    {
        // Act
        var result = value.IsNegative();

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion

    #region IsZero Tests

    [Theory]
    [InlineData(0, true)]
    [InlineData(5, false)]
    [InlineData(-5, false)]
    public void IsZero_Int_ReturnsCorrectResult(int value, bool expected)
    {
        // Act
        var result = value.IsZero();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0.0, true)]
    [InlineData(1e-11, true)]  // Less than default tolerance 1e-10
    [InlineData(0.001, false)]
    [InlineData(-1e-11, true)] // Less than default tolerance 1e-10
    public void IsZero_Double_WithTolerance_ReturnsCorrectResult(double value, bool expected)
    {
        // Act
        var result = value.IsZero();

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion

    #region IsEven/IsOdd Tests

    [Theory]
    [InlineData(0, true)]
    [InlineData(2, true)]
    [InlineData(4, true)]
    [InlineData(1, false)]
    [InlineData(3, false)]
    [InlineData(-2, true)]
    [InlineData(-1, false)]
    public void IsEven_ReturnsCorrectResult(int value, bool expected)
    {
        // Act
        var result = value.IsEven();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(3, true)]
    [InlineData(2, false)]
    [InlineData(4, false)]
    [InlineData(-1, true)]
    [InlineData(-2, false)]
    public void IsOdd_ReturnsCorrectResult(int value, bool expected)
    {
        // Act
        var result = value.IsOdd();

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion

    #region Formatting Tests

    [Theory]
    [InlineData(1000, "1,000")]
    [InlineData(1000000, "1,000,000")]
    [InlineData(0, "0")]
    public void ToFormattedString_Int_FormatsCorrectly(int value, string expected)
    {
        // Act
        var result = value.ToFormattedString();

        // Assert
        Assert.Contains(expected.Replace(",", ""), result.Replace(",", "").Replace(".", "").Replace(" ", ""));
    }

    [Fact]
    public void ToCurrency_FormatsAsExpected()
    {
        // Arrange
        decimal value = 1234.56m;

        // Act
        var result = value.ToCurrency();

        // Assert
        Assert.Contains("1", result);
        Assert.Contains("234", result);
        Assert.Contains("56", result);
    }

    #endregion

    #region RoundTo Tests

    [Theory]
    [InlineData(1.2345, 2, 1.23)]
    [InlineData(1.2355, 2, 1.24)]
    [InlineData(1.2, 2, 1.2)]
    public void RoundTo_Double_RoundsCorrectly(double value, int decimals, double expected)
    {
        // Act
        var result = value.RoundTo(decimals);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1.2345, 2, 1.23)]
    [InlineData(1.2355, 2, 1.24)]
    [InlineData(1.2, 2, 1.2)]
    public void RoundTo_Decimal_RoundsCorrectly(double valueD, int decimals, double expectedD)
    {
        // Arrange
        var value = (decimal)valueD;
        var expected = (decimal)expectedD;

        // Act
        var result = value.RoundTo(decimals);

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion

    #region Abs Tests

    [Theory]
    [InlineData(5, 5)]
    [InlineData(-5, 5)]
    [InlineData(0, 0)]
    public void Abs_Int_ReturnsAbsoluteValue(int value, int expected)
    {
        // Act
        var result = value.Abs();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(5.5, 5.5)]
    [InlineData(-5.5, 5.5)]
    [InlineData(0.0, 0.0)]
    public void Abs_Double_ReturnsAbsoluteValue(double value, double expected)
    {
        // Act
        var result = value.Abs();

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion

    #region Byte Conversion Tests

    [Theory]
    [InlineData(1024, 1.0)]
    [InlineData(2048, 2.0)]
    [InlineData(512, 0.5)]
    [InlineData(0, 0.0)]
    public void ToKilobytes_ConvertsCorrectly(long bytes, double expectedKB)
    {
        // Act
        var result = bytes.ToKilobytes();

        // Assert
        Assert.Equal(expectedKB, result);
    }

    [Theory]
    [InlineData(1048576, 1.0)]
    [InlineData(2097152, 2.0)]
    [InlineData(524288, 0.5)]
    [InlineData(0, 0.0)]
    public void ToMegabytes_ConvertsCorrectly(long bytes, double expectedMB)
    {
        // Act
        var result = bytes.ToMegabytes();

        // Assert
        Assert.Equal(expectedMB, result);
    }

    [Theory]
    [InlineData(1073741824, 1.0)]
    [InlineData(2147483648, 2.0)]
    [InlineData(0, 0.0)]
    public void ToGigabytes_ConvertsCorrectly(long bytes, double expectedGB)
    {
        // Act
        var result = bytes.ToGigabytes();

        // Assert
        Assert.Equal(expectedGB, result);
    }

    #endregion

    #region Long Clamp Tests

    [Theory]
    [InlineData(5L, 0L, 10L, 5L)]
    [InlineData(-5L, 0L, 10L, 0L)]
    [InlineData(15L, 0L, 10L, 10L)]
    public void Clamp_Long_ReturnsCorrectValue(long value, long min, long max, long expected)
    {
        // Act
        var result = value.Clamp(min, max);

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion

    #region Float Tests

    [Theory]
    [InlineData(5.5f, 0.0f, 10.0f, 5.5f)]
    [InlineData(-5.5f, 0.0f, 10.0f, 0.0f)]
    [InlineData(15.5f, 0.0f, 10.0f, 10.0f)]
    public void Clamp_Float_ReturnsCorrectValue(float value, float min, float max, float expected)
    {
        // Act
        var result = value.Clamp(min, max);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(5.5f, true)]
    [InlineData(0.0f, false)]
    [InlineData(-5.5f, false)]
    public void IsPositive_Float_ReturnsCorrectResult(float value, bool expected)
    {
        // Act
        var result = value.IsPositive();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(-5.5f, true)]
    [InlineData(0.0f, false)]
    [InlineData(5.5f, false)]
    public void IsNegative_Float_ReturnsCorrectResult(float value, bool expected)
    {
        // Act
        var result = value.IsNegative();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0.0f, true)]
    [InlineData(0.0000001f, true)]
    [InlineData(0.001f, false)]
    public void IsZero_Float_WithTolerance_ReturnsCorrectResult(float value, bool expected)
    {
        // Act
        var result = value.IsZero();

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion

    #region Decimal Tests

    [Theory]
    [InlineData(5, true)]
    [InlineData(0, false)]
    [InlineData(-5, false)]
    public void IsPositive_Decimal_ReturnsCorrectResult(int valueInt, bool expected)
    {
        // Arrange
        var value = (decimal)valueInt;

        // Act
        var result = value.IsPositive();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(-5, true)]
    [InlineData(0, false)]
    [InlineData(5, false)]
    public void IsNegative_Decimal_ReturnsCorrectResult(int valueInt, bool expected)
    {
        // Arrange
        var value = (decimal)valueInt;

        // Act
        var result = value.IsNegative();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(5, false)]
    [InlineData(-5, false)]
    public void IsZero_Decimal_ReturnsCorrectResult(int valueInt, bool expected)
    {
        // Arrange
        var value = (decimal)valueInt;

        // Act
        var result = value.IsZero();

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion

    #region Long Tests

    [Theory]
    [InlineData(5L, 0L, 10L, true)]
    [InlineData(-1L, 0L, 10L, false)]
    [InlineData(11L, 0L, 10L, false)]
    public void IsBetween_Long_ReturnsCorrectResult(long value, long min, long max, bool expected)
    {
        // Act
        var result = value.IsBetween(min, max);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0L, true)]
    [InlineData(2L, true)]
    [InlineData(1L, false)]
    public void IsEven_Long_ReturnsCorrectResult(long value, bool expected)
    {
        // Act
        var result = value.IsEven();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1L, true)]
    [InlineData(2L, false)]
    public void IsOdd_Long_ReturnsCorrectResult(long value, bool expected)
    {
        // Act
        var result = value.IsOdd();

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion
}
