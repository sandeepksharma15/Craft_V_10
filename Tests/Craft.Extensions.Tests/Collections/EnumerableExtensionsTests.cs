using System.ComponentModel;

namespace Craft.Extensions.Tests.System;

public class EnumExtensionsTests
{
    // --- GetOrderedEnumValues ---
    [Fact]
    public void GetOrderedEnumValues_ReturnsOrderedList()
    {
        // Arrange
        var result = EnumExtensions.GetOrderedEnumValues<SimpleEnum>();

        // Act & Assert
        Assert.Equal([SimpleEnum.Zero, SimpleEnum.One, SimpleEnum.Two], result);
    }

    // --- GetExtremeEnumValue, GetHighestEnumValue, GetLowestEnumValue ---
    [Fact]
    public void GetExtremeEnumValue_ReturnsHighestAndLowest()
    {
        // Arrange & Act & Assert
        Assert.Equal(SimpleEnum.Two, EnumExtensions.GetExtremeEnumValue<SimpleEnum>(true));
        Assert.Equal(SimpleEnum.Zero, EnumExtensions.GetExtremeEnumValue<SimpleEnum>(false));
    }

    [Fact]
    public void GetHighestEnumValue_ReturnsHighest()
    {
        // Arrange & Act & Assert
        Assert.Equal(SimpleEnum.Two, EnumExtensions.GetHighestEnumValue<SimpleEnum>());
    }

    [Fact]
    public void GetLowestEnumValue_ReturnsLowest()
    {
        // Arrange & Act & Assert
        Assert.Equal(SimpleEnum.Zero, EnumExtensions.GetLowestEnumValue<SimpleEnum>());
    }

    // --- GetDescriptions, GetNames, GetValues ---
    [Fact]
    public void GetDescriptions_ReturnsDescriptions()
    {
        // Arrange
        var dict = EnumExtensions.GetDescriptions<DescriptionEnum>();

        // Act & Assert
        Assert.Equal("Alpha Desc", dict[DescriptionEnum.Alpha]);
        Assert.Equal("Beta Desc", dict[DescriptionEnum.Beta]);
        Assert.Equal("Gamma", dict[DescriptionEnum.Gamma]);
    }

    [Fact]
    public void GetNames_ReturnsNames()
    {
        // Arrange
        var dict = EnumExtensions.GetNames<SimpleEnum>();

        // Act & Assert
        Assert.Equal("Zero", dict[SimpleEnum.Zero]);
        Assert.Equal("One", dict[SimpleEnum.One]);
        Assert.Equal("Two", dict[SimpleEnum.Two]);
    }

    [Fact]
    public void GetValues_ReturnsAllValues()
    {
        // Arrange & Act
        var values = EnumExtensions.GetValues<SimpleEnum>();

        // Assert
        Assert.Contains(SimpleEnum.One, values);
        Assert.Equal(3, values.Length);
    }

    // --- GetNextEnumValue, GetPrevEnumValue ---
    [Fact]
    public void GetNextEnumValue_ReturnsNextOrFirst()
    {
        // Arrange & Act & Assert
        Assert.Equal(SimpleEnum.One, SimpleEnum.Zero.GetNextEnumValue());
        Assert.Equal(SimpleEnum.Zero, SimpleEnum.Two.GetNextEnumValue());
    }

    [Fact]
    public void GetPrevEnumValue_ReturnsPreviousOrLast()
    {
        // Arrange & Act & Assert
        Assert.Equal(SimpleEnum.Zero, SimpleEnum.One.GetPrevEnumValue());
        Assert.Equal(SimpleEnum.Two, SimpleEnum.Zero.GetPrevEnumValue());
    }

    // --- GetDescription ---
    [Fact]
    public void GetDescription_ReturnsDescriptionOrName()
    {
        // Arrange & Act & Assert
        Assert.Equal("Alpha Desc", DescriptionEnum.Alpha.GetDescription());
        Assert.Equal("Gamma", DescriptionEnum.Gamma.GetDescription());
    }

    // --- GetFlags ---
    [Fact]
    public void GetFlags_ReturnsSetFlags()
    {
        // Arrange & Act
        var flags = FlagsEnum.All.GetFlags();

        // Assert
        Assert.Contains(FlagsEnum.First, flags);
        Assert.Contains(FlagsEnum.Second, flags);
        Assert.Contains(FlagsEnum.Third, flags);
    }

    // --- IsSet ---
    [Fact]
    public void IsSet_ReturnsTrueIfFlagIsSet()
    {
        // Arrange & Act & Assert
        Assert.True((FlagsEnum.All).IsSet(FlagsEnum.First));
        Assert.False((FlagsEnum.First).IsSet(FlagsEnum.Second));
    }

    // --- GetName ---
    [Fact]
    public void GetName_ReturnsNameOrFlags()
    {
        // Arrange & Act & Assert
        Assert.Equal("First", FlagsEnum.First.GetName());
        Assert.Equal("None,First,Second", (FlagsEnum.First | FlagsEnum.Second).GetName());
    }

    // --- TryGetSingleDescription, TryGetSingleName ---
    [Fact]
    public void TryGetSingleDescription_ReturnsTrueAndDescription()
    {
        // Arrange & Act & Assert
        Assert.True(DescriptionEnum.Alpha.TryGetSingleDescription(out var desc));
        Assert.Equal("Alpha Desc", desc);
    }

    [Fact]
    public void TryGetSingleName_ReturnsTrueAndName()
    {
        // Arrange & Act & Assert
        Assert.True(DescriptionEnum.Beta.TryGetSingleName(out var name));
        Assert.Equal("Beta", name);
    }

    // --- ToStringInvariant ---
    [Fact]
    public void ToStringInvariant_ReturnsName()
    {
        // Arrange & Act & Assert
        Assert.Equal("Alpha", DescriptionEnum.Alpha.ToStringInvariant());
    }

    // --- ValidateEnumType ---
    [Fact]
    public void ValidateEnumType_ThrowsIfNotEnum()
    {
        // Arrange & Act & Assert
        Assert.Throws<Exception>(() => EnumExtensions.ValidateEnumType<int>());
    }

    // --- ToEnum (int) ---
    [Fact]
    public void ToEnum_IntToEnum_ReturnsEnumValue()
    {
        // Arrange & Act & Assert
        Assert.Equal(SimpleEnum.One, 1.ToEnum<SimpleEnum>());
    }

    // --- ToEnum (string) ---
    [Fact]
    public void ToEnum_StringToEnum_ReturnsEnumValue()
    {
        // Arrange & Act & Assert
        Assert.Equal(SimpleEnum.Two, "Two".ToEnum<SimpleEnum>());
        Assert.Equal(SimpleEnum.One, "1".ToEnum<SimpleEnum>());
    }

    [Fact]
    public void ToEnum_StringToEnum_ThrowsOnNullOrEmpty()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((string)null!).ToEnum<SimpleEnum>());
        Assert.Throws<ArgumentNullException>(() => "".ToEnum<SimpleEnum>());
    }

    // --- Contains ---
    [Fact]
    public void Contains_ReturnsTrueIfStringContainsFlagName()
    {
        // Arrange & Act & Assert
        Assert.True("First,Second".Contains(FlagsEnum.First));
        Assert.False("Third".Contains(FlagsEnum.First));
    }

    [Fact]
    public void Contains_ReturnsTrueIfStringContainsAnyFlag()
    {
        // Arrange & Act & Assert
        Assert.False("First,Second".Contains(FlagsEnum.All));
        Assert.True("First".Contains(FlagsEnum.First));
    }

    private enum SimpleEnum
    {
        Zero = 0,
        One = 1,
        Two = 2
    }

    [Flags]
    private enum FlagsEnum
    {
        None = 0,
        First = 1,
        Second = 2,
        Third = 4,
        All = First | Second | Third
    }

    private enum DescriptionEnum
    {
        [Description("Alpha Desc")]
        Alpha = 1,
        [Description("Beta Desc")]
        Beta = 2,
        Gamma = 3
    }
}
