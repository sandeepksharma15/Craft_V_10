namespace Craft.Extensions.Tests.System;

public class OtherExtensionsTests
{
    [Theory]
    [InlineData(null, null)] // Null input should return null
    [InlineData(new byte[0], "")] // Empty byte array should return an empty string
    [InlineData(new byte[] { 0x01, 0xAB, 0xFF }, "01ABFF")] // Typical byte values
    [InlineData(new byte[] { 0x00, 0x0F, 0xFF }, "000FFF")] // Byte values with leading zeros
    public void BytesToHex_ReturnsExpectedResult(byte[]? inputBytes, string? expectedResult)
    {
        // Act
        var result = inputBytes!.BytesToHex();

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("InvalidHex")]
    [InlineData("12345")]
    [InlineData("ABCDEF012G")]
    public void HexToBytes_InvalidInput_ThrowsFormatException(string input)
    {
        // Act & Assert
        Assert.Throws<FormatException>(() => input.HexToBytes());
    }

    [Theory]
    [InlineData("48656C6C6F", new byte[] { 72, 101, 108, 108, 111 })]
    [InlineData("010203", new byte[] { 1, 2, 3 })]
    [InlineData("", new byte[0])]
    [InlineData(null, new byte[0])]
    public void HexToBytes_ValidInput_ReturnsExpectedByteArray(string? input, byte[] expected)
    {
        // Act
        var result = input?.HexToBytes() ?? [];

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HexToBytes_WhitespaceOnly_ReturnsEmptyArray()
    {
        // Act
        var result = "   ".HexToBytes();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void HexToBytes_ParsesUpperAndLowerCase()
    {
        // Act
        var upper = "aBcDeF".HexToBytes();
        var lower = "abcdef".HexToBytes();

        // Assert
        Assert.Equal(new byte[] { 0xAB, 0xCD, 0xEF }, upper);
        Assert.Equal(new byte[] { 0xAB, 0xCD, 0xEF }, lower);
    }

    [Fact]
    public void HexToBytes_TrimsWhitespace()
    {
        // Act
        var result = "  01ABFF  ".HexToBytes();

        // Assert
        Assert.Equal(new byte[] { 0x01, 0xAB, 0xFF }, result);
    }

    [Theory]
    [InlineData(0, "0%")]
    [InlineData(0.1234, "12.34%")]
    [InlineData(-0.1234, "-12.34%")]
    [InlineData(1, "100%")]
    [InlineData(-1, "-100%")]
    [InlineData(0.005, "0.5%")]
    [InlineData(123.456, "12345.6%")]
    [InlineData(-123.456, "-12345.6%")]
    public void ToPercentage_ShouldConvertDecimalToPercentage(decimal input, string expected)
    {
        // Act
        var result = input.ToPercentage();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0, "0%")]
    [InlineData(0.1234, "12.34%")]
    [InlineData(-0.1234, "-12.34%")]
    [InlineData(1, "100%")]
    [InlineData(-1, "-100%")]
    [InlineData(0.005, "0.5%")]
    [InlineData(123.456, "12345.6%")]
    [InlineData(-123.456, "-12345.6%")]
    public void ToPercentage_ShouldConvertDoubleToPercentage(double input, string expected)
    {
        // Act
        var result = input.ToPercentage();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0f, "0%")]
    [InlineData(0.1234f, "12.34%")]
    [InlineData(-0.1234f, "-12.34%")]
    [InlineData(1f, "100%")]
    [InlineData(-1f, "-100%")]
    [InlineData(0.005f, "0.5%")]
    [InlineData(123.456f, "12345.6%")]
    [InlineData(-123.456f, "-12345.6%")]
    public void ToPercentage_ShouldConvertFloatToPercentage(float input, string expected)
    {
        // Act
        var result = input.ToPercentage();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0.9999, "99.99%")]
    [InlineData(0.995, "99.5%")]
    public void ToPercentage_RoundingEdgeCases(decimal input, string expected)
    {
        var result = input.ToPercentage();
        Assert.Equal(expected, result);
    }
}
